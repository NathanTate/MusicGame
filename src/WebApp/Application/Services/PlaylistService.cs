using Application.Models.Playlists;
using Application.Errors;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Infrastructure.Context;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Application.Common.UserHelpers;
using AutoMapper.QueryableExtensions;
using Application.Models;
using Application.Models.Queries;
using Application.Models.Users;
using Application.Services.Elastic;
using Application.Models.Songs;
using Application.Models.Elastic;

namespace Application.Services;
internal class PlaylistService : IPlaylistService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IFileHandler _fileHandler;
    private readonly IUserContext _userContext;
    private readonly PlaylistsElasticService _elasticService;
    public PlaylistService(AppDbContext dbContext, IMapper mapper, IFileHandler fileHandler, IUserContext userContext, PlaylistsElasticService elasticService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _fileHandler = fileHandler;
        _userContext = userContext;
        _elasticService = elasticService;
    }

    public async Task<Result<bool>> ToggleLikeAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        bool liked = false;
        var like = await _dbContext.PlaylistLike.FindAsync([playlistId, currentUser.UserId], cancellationToken);

        if (like is not null)
        {
            _dbContext.Remove(like);
        }
        else
        {
            var playlist = await _dbContext.Playlists.AsNoTracking().FirstOrDefaultAsync(x => x.PlaylistId == playlistId);

            if (playlist is null)
            {
                return new ValidationError($"Can't add like to not existed playilist - {playlistId}");
            }
            if (playlist.UserId == currentUser.UserId)
            {
                return new ValidationError("You can't like your own playlists");
            }

            var playlistLike = new PlaylistLike
            {
                PlaylistId = playlistId,
                UserId = currentUser.UserId
            };

            _dbContext.PlaylistLike.Add(playlistLike);
            liked = true;
        }

        await _dbContext.SaveChangesAsync();
        return Result.Ok(liked);
    }

    public async Task<Result<PlaylistResponse>> CreatePlaylistAsync(CancellationToken cancellationToken = default)
    {
        int playlistCount = await _dbContext.Playlists.CountAsync(cancellationToken);
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        Playlist playlist = new Playlist()
        {
            Name = $"My Playlist {playlistCount + 1}",
            IsPrivate = false,
            TotalDuration = 0,
            UserId = currentUser.UserId,
        };

        _dbContext.Playlists.Add(playlist);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _elasticService.AddOrUpdateAsync(_mapper.Map<PlaylistDoc>(playlist), ElasticIndex.PlaylistsIndex, cancellationToken);

        return _mapper.Map<PlaylistResponse>(playlist);
    }

    public async Task<Result<PlaylistResponse>> GetPlaylistAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUser()?.UserId;

        var playlist = await _dbContext.Playlists
            .AsNoTracking()
            .Where(x => x.PlaylistId == playlistId && (!x.IsPrivate || x.UserId == userId))
            .ProjectTo<PlaylistResponse>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {playlistId} cannot be found");
        }

        List<int> songIds = playlist.Songs.Select(x => x.Song.SongId).ToList();

        var songsLikes = _dbContext.SongLike
            .AsNoTracking()
            .Where(x => songIds.Contains(x.SongId))
            .GroupBy(x => x.SongId)
            .Select(x => new
            {
                SongId = x.Key,
                LikesCount = x.Count(),
            });

        foreach (var item in songsLikes)
        {
            var matchingItem = playlist.Songs.Find(x => x.Song.SongId == item.SongId);
            if (matchingItem is not null)
            {
                matchingItem.Song.LikesCount = item.LikesCount;
            }
        }

        return Result.Ok(playlist);
    }

    public async Task<PagedList<PlaylistResponse>> GetPlaylistsAsync(PlaylistsQuery query, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUser()?.UserId;

        var playlistsQuery = _dbContext.Playlists
            .AsNoTracking()
            .Where(x => (!x.IsPrivate || x.UserId == userId));

        List<int> searchIds = new();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var elasticQuery = new ElasticQuery
            {
                Page = query.Page,
                PageSize = query.PageSize,
                SearchTerm = query.SearchTerm
            };

            var result = await _elasticService.SearchAsync(elasticQuery, ElasticIndex.PlaylistsIndex, cancellationToken);

            if (result.IsFailed)
            {
                throw new ArgumentNullException(result.Errors.Select(x => x.Message).FirstOrDefault());
            }

            searchIds = result.Value.Select(x => Convert.ToInt32(x.Id)).ToList();

            playlistsQuery = playlistsQuery.Where(x => searchIds.Contains(x.PlaylistId));
        }

        if (query.SortOrder == "desc")
        {
            playlistsQuery = playlistsQuery.OrderByDescending(query.GetSortProperty());
        }
        else
        {
            playlistsQuery = playlistsQuery.OrderBy(query.GetSortProperty());
        }

        var resultQuery = playlistsQuery.Select(x => new PlaylistResponse
        {
            PlaylistId = x.PlaylistId,
            Name = x.Name,
            Description = x.Description,
            IsPrivate = x.IsPrivate,
            TotalDuration = x.TotalDuration,
            LikesCount = x.LikesCount,
            SongsCount = x.Songs.Count,
            CreatedAt = x.CreatedAt,
            PhotoUrl = x.Photo == null ? null : x.Photo.URL,
            User = new ArtistResponse(x.User.Id, x.User.Email, x.User.DisplayName)
        });

        var pagedList = await PagedList<PlaylistResponse>.CreateAsync(resultQuery, query.Page, query.PageSize, cancellationToken);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var orderedItems = pagedList.Items.OrderBy(x => searchIds.IndexOf(x.PlaylistId)).ToList();

            return new PagedList<PlaylistResponse>(orderedItems, pagedList.Page, pagedList.PageSize, pagedList.TotalCount);
        }

        return pagedList;
    }

    public async Task<PagedList<PlaylistResponse>> GetByIdsAsync(IEnumerable<int> ids, BaseQuery query, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUser()?.UserId;

        var playlistsQuery = _dbContext.Playlists
            .AsNoTracking()
            .Where(x => (!x.IsPrivate || x.UserId == userId))
            .Where(x => ids.Contains(x.PlaylistId));

        var resultQuery = playlistsQuery.Select(x => new PlaylistResponse
        {
            PlaylistId = x.PlaylistId,
            Name = x.Name,
            Description = x.Description,
            IsPrivate = x.IsPrivate,
            TotalDuration = x.TotalDuration,
            LikesCount = x.LikesCount,
            SongsCount = x.Songs.Count,
            CreatedAt = x.CreatedAt,
            PhotoUrl = x.Photo == null ? null : x.Photo.URL,
            User = new ArtistResponse(x.User.Id, x.User.Email, x.User.DisplayName)
        });

        return await PagedList<PlaylistResponse>.CreateAsync(resultQuery, query.Page, query.PageSize, cancellationToken);
    }

    public async Task<Result<PlaylistResponse>> UpdatePlaylistAsync(UpdatePlaylistRequest model, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlist = await GetByIdAsync(model.PlaylistId, currentUser.UserId, cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {model.PlaylistId} cannot be found");
        }

        if (playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        _mapper.Map(model, playlist);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _elasticService.AddOrUpdateAsync(_mapper.Map<PlaylistDoc>(playlist), ElasticIndex.PlaylistsIndex, cancellationToken);

        return Result.Ok(_mapper.Map<PlaylistResponse>(playlist));
    }
    public async Task<Result> DeletePlaylistAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlist = await GetByIdAsync(playlistId, currentUser.UserId, cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {playlistId} cannot be found");
        }

        if (playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        _dbContext.Playlists.Remove(playlist);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _elasticService.Remove(currentUser.UserId, ElasticIndex.PlaylistsIndex, cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<PlaylistResponse>> UploadPhotoAsync(int playlistId, IFormFile photo, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlist = await GetByIdAsync(playlistId, currentUser.UserId, cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {playlistId} cannot be found");
        }

        if (playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        var photoUrl = playlist.Photo is null
            ? await _fileHandler.UploadFileAsync(photo, FileContainer.Photos, cancellationToken)
            : await _fileHandler.UpdateFileAsync(Path.GetFileName(playlist.Photo.URL), photo, FileContainer.Photos, cancellationToken);

        var playlistPhoto = new Photo()
        {
            URL = photoUrl,
            Size = photo.Length,
            ContentType = photo.ContentType,
        };

        playlist.Photo = playlistPhoto;
        await _dbContext.SaveChangesAsync();

        return Result.Ok(_mapper.Map<PlaylistResponse>(playlist));
    }
    public async Task<Result> DeletePhotoAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlist = await GetByIdAsync(playlistId, currentUser.UserId, cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {playlistId} cannot be found");
        }

        if (playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        if (playlist.Photo is null)
        {
            return Result.Ok();
        }

        var fileName = Path.GetFileName(playlist.Photo.URL);
        await _fileHandler.DeleteFileAsync(fileName, FileContainer.Photos, cancellationToken);

        var photo = await _dbContext.Photos.FindAsync([playlist.Photo.PhotoId], cancellationToken);

        if (photo is null)
        {
            return new NotFoundError($"Photo for playlist {playlist.Name} cannot be found");
        }

        _dbContext.Remove(photo);

        await _dbContext.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> AddSongToPlaylistAsync(UpsertSongPlaylistRequest model, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlist = await GetByIdAsync(model.PlaylistId, currentUser.UserId, cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {model.PlaylistId} cannot be found");
        }

        if (playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        var song = await _dbContext.Songs
        .Select(x => new Song
        {
            SongId = x.SongId,
            IsPrivate = x.IsPrivate,
            Duration = x.Duration
        })
        .FirstOrDefaultAsync(x => x.SongId == model.SongId, cancellationToken);

        if (song is null)
        {
            return new NotFoundError($"Song with id {model.SongId} cannot be found");
        }

        if (song.IsPrivate)
        {
            return new ValidationError($"Private songs cannot be added to playlists");
        }

        var playlistSong = await _dbContext.PlaylistSong
            .FirstOrDefaultAsync(x => x.SongId == model.SongId && x.PlaylistId == model.PlaylistId, cancellationToken);

        if (playlistSong is not null)
        {
            return new ValidationError($"Song with id {model.SongId} is already in the playlist");
        }

        var maxPosition = await _dbContext.PlaylistSong.Where(x => x.PlaylistId == model.PlaylistId).MaxAsync(x => (int?)x.Position);

        playlistSong = new PlaylistSong
        {
            PlaylistId = model.PlaylistId,
            SongId = model.SongId,
            Position = model.Position ?? maxPosition ?? 1,
        };

        _dbContext.PlaylistSong.Add(playlistSong);
        playlist.TotalDuration += song.Duration;

        await _dbContext.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> RemoveSongFromPlaylistAsync(int songId, int playlistId, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlistSong = await _dbContext.PlaylistSong
            .Select(x => new PlaylistSong
            {
                SongId = x.SongId,
                PlaylistId = x.PlaylistId,
                Song = new Song
                {
                    SongId = x.SongId,
                    Duration = x.Song.Duration
                },
                Playlist = new Playlist
                {
                    PlaylistId = x.PlaylistId,
                    TotalDuration = x.Playlist.TotalDuration,
                    UserId = x.Playlist.UserId
                }
            })
            .FirstOrDefaultAsync(x => x.SongId == songId && x.PlaylistId == playlistId, cancellationToken);

        if (playlistSong is null)
        {
            return new NotFoundError($"Song with id {songId} does not exist in Playlist with id {playlistId}");
        }

        if (playlistSong.Playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        _dbContext.PlaylistSong.Remove(playlistSong);
        playlistSong.Playlist.TotalDuration -= playlistSong.Song.Duration;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> UpdateSongPositionAsync(UpsertSongPlaylistRequest model, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlistSong = await _dbContext.PlaylistSong
            .Include(p => p.Playlist)
            .FirstOrDefaultAsync(x => x.SongId == model.SongId && x.PlaylistId == model.PlaylistId, cancellationToken);

        if (playlistSong is null)
        {
            return new NotFoundError($"Song with id {model.SongId} does not exist in Playlist with id {model.PlaylistId}");
        }

        if (playlistSong.Playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        _mapper.Map(model, playlistSong);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<bool> IsPlaylistNameAvailable(string name, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Playlists.AnyAsync(p => p.Name.ToUpper() == name.ToUpper(), cancellationToken);
        return !exists;
    }

    public async Task<Result<bool>> IsLiked(int playlistId, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }


        var like = await _dbContext.PlaylistLike.AnyAsync(x => x.PlaylistId == playlistId && x.UserId == currentUser.UserId, cancellationToken);

        return Result.Ok(like);
    }

    private async Task<Playlist?> GetByIdAsync(int playlistId, string? userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Playlists
            .Include(x => x.Photo)
            .Where(x => x.PlaylistId == playlistId && (!x.IsPrivate || x.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken);
    }

}
