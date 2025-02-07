using Application.Models.Elastic;
using Domain.Enums;
using Elastic.Clients.Elasticsearch;
using Infrastructure.Context;
using Microsoft.Extensions.Logging;

namespace Application.Services.Elastic;
public class PlaylistsElasticService : BaseElasticService<PlaylistDoc>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SongsElasticService> _logger;
    public PlaylistsElasticService(ElasticsearchClient client, AppDbContext dbContext, ILogger<SongsElasticService> logger) : base(client)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async override Task<bool> ReindexAllAsync()
    {
        try
        {
            var playlistDocs = _dbContext.Playlists.Select((p => new PlaylistDoc
            {
                Id = p.PlaylistId.ToString(),
                Name = p.Name,
                Description = p.Description,
            }));

            await base.AddOrUpdateBulkAsync(playlistDocs, ElasticIndex.PlaylistsIndex);

            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, ex.Message);
            return false;
        }
    }
}
