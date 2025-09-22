using Application.Models.Elastic;
using Domain.Enums;
using Elastic.Clients.Elasticsearch;
using Infrastructure.Context;
using Microsoft.Extensions.Logging;


namespace Application.Services.Elastic;
public class SongsElasticService : BaseElasticService<SongDoc>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SongsElasticService> _logger;
    public SongsElasticService(ElasticsearchClient client, AppDbContext dbContext, ILogger<SongsElasticService> logger) : base(client)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async override Task<bool> ReindexAllAsync()
    {
        try
        {
            var songDocs = _dbContext.Songs.Select((s => new SongDoc
            {
                Id = s.SongId.ToString(),
                Name = s.Name,
                ArtistName = s.User.DisplayName,
                GenreNames = s.Genres.Select(g => g.Name).ToList()
            }));

            await base.AddOrUpdateBulkAsync(songDocs, ElasticIndex.SongsIndex);

            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, ex.Message);
            return false;
        }
    }
}
