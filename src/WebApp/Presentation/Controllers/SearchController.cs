using Application.Interfaces;
using Application.Models;
using Application.Models.Elastic;
using Application.Models.Queries;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.MSearch;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Route("api/search")]
public class SearchController : BaseApiController
{
    private readonly ISongService _songService;
    private readonly IPlaylistService _playlistService;
    private readonly IGenreService _genreService;
    private readonly IUserService _userService;
    private readonly ElasticsearchClient _client;
    private readonly ILogger<SearchController> _logger;
    public SearchController(ElasticsearchClient client, ILogger<SearchController> logger, ISongService songService, IPlaylistService playlistService, IGenreService genreService, IUserService userService)
    {
        _client = client;
        _songService = songService;
        _playlistService = playlistService;
        _genreService = genreService;
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> SearchData([FromQuery] SearchQuery query, CancellationToken cancellationToken)
    {
        var elasticQuery = new ElasticQuery()
        {
            Page = query.Page,
            PageSize = query.SongsPageSize,
            SearchTerm = query.SearchTerm,
        };

        var docTypes = new List<(BaseDoc Doc, string IndexName)>()
        {
            (new PlaylistDoc(), "playlists-index"),
            (new SongDoc(), "songs-index"),
            (new UserDoc(), "users-index"),
            (new GenreDoc(), "genres-index")
        };

        var listSearchRequestItems = new List<SearchRequestItem>();

        foreach (var item in docTypes)
        {
            listSearchRequestItems.Add(new SearchRequestItem(
                new MultisearchHeader
                {
                    Indices = item.IndexName
                },
                new MultisearchBody
                {
                    Query = item.Doc.BuildQuery(elasticQuery)
                }
            ));
        }

        var msr = new MultiSearchRequest()
        {
            Searches = listSearchRequestItems
        };

        var response = await _client.MultiSearchAsync<dynamic>(msr, cancellationToken).ConfigureAwait(false);
        var entityIdsByQuery = new Dictionary<string, List<(string? Id, double? Score)>>();

        foreach (var item in response.Responses)
        {
            item.Match(x =>
            {
                var ids = x.Hits.Select(hit => (hit.Id, hit.Score)).ToList();
                if (x.Hits.Count > 0)
                {
                    entityIdsByQuery[x.Hits.First().Index] = ids;
                }
            }, x =>
            {
                _logger.LogError(x.Error?.ToString());
            });
        }

        var songIds = entityIdsByQuery.ContainsKey("songs-index") ? entityIdsByQuery["songs-index"] : new List<(string? Id, double? Score)>();
        var playlistIds = entityIdsByQuery.ContainsKey("playlists-index") ? entityIdsByQuery["playlists-index"] : new List<(string? Id, double? Score)>();
        var userIds = entityIdsByQuery.ContainsKey("users-index") ? entityIdsByQuery["users-index"] : new List<(string? Id, double? Score)>();
        var genreIds = entityIdsByQuery.ContainsKey("genres-index") ? entityIdsByQuery["genres-index"] : new List<(string? Id, double? Score)>();

        var songs = await _songService.GetByIdsAsync(songIds.Select(x => Convert.ToInt32(x.Id)), new BaseQuery() { Page = query.Page, PageSize = query.SongsPageSize }, cancellationToken).ConfigureAwait(false);
        var playlists = await _playlistService.GetByIdsAsync(playlistIds.Select(x => Convert.ToInt32(x.Id)), new BaseQuery() { Page = query.Page, PageSize = query.SongsPageSize }, cancellationToken).ConfigureAwait(false);
        var users = await _userService.GetByIdsAsync(userIds.Select(x => x.Id!), new BaseQuery() { Page = query.Page, PageSize = query.SongsPageSize }, cancellationToken).ConfigureAwait(false);
        var genres = await _genreService.GetByIdsAsync(genreIds.Select(x => Convert.ToInt32(x.Id)), new GenresQuery() { Page = query.Page, PageSize = query.SongsPageSize }, cancellationToken).ConfigureAwait(false);


        var results = new List<(string? Id, string Type, double? Score)>();

        results.AddRange(songIds.Select(songTuple => (songTuple.Id, Type: "Song", songTuple.Score)));
        results.AddRange(playlistIds.Select(playlistTuple => (playlistTuple.Id, Type: "Playlist", playlistTuple.Score)));
        results.AddRange(userIds.Select(userTuple => (userTuple.Id, Type: "User", userTuple.Score)));
        results.AddRange(genreIds.Select(genreTuple => (genreTuple.Id, Type: "Genre", genreTuple.Score)));

        var bestFitTuple = results.OrderByDescending(x => x.Score).FirstOrDefault();

        object? bestFitItem = bestFitTuple.Type switch
        {
            "Song" => songs.Items.Find(x => x.SongId.ToString() == bestFitTuple.Id),
            "Playlist" => playlists.Items.Find(x => x.PlaylistId.ToString() == bestFitTuple.Id),
            "User" => users.Items.Find(x => x.UserId == bestFitTuple.Id),
            "Genre" => genres.Items.Find(x => x.GenreId.ToString() == bestFitTuple.Id),
            _ => null,
        };

        var searchResult = new SearchData
        {
            BestFitType = bestFitTuple.Type,
            BestFitItem = bestFitItem,
            Songs = songs,
            Playlists = playlists,
            Genres = genres,
            Users = users,
            HasItems = results.Any()
        };

        return Ok(searchResult);
    }
}
