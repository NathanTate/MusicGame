using Application.Models.Elastic;
using Domain.Enums;
using Elastic.Clients.Elasticsearch;
using Infrastructure.Context;
using Microsoft.Extensions.Logging;

namespace Application.Services.Elastic;
public class GenresElasticService : BaseElasticService<GenreDoc>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SongsElasticService> _logger;
    public GenresElasticService(ElasticsearchClient client, AppDbContext dbContext, ILogger<SongsElasticService> logger) : base(client)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async override Task<bool> ReindexAllAsync()
    {
        try
        {
            var genreDocs = _dbContext.Genres.Select((g => new GenreDoc
            {
                Id = g.GenreId.ToString(),
                Name = g.Name 
            }));

            await base.AddOrUpdateBulkAsync(genreDocs, ElasticIndex.GenresIndex);

            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, ex.Message);
            return false;
        }
    }
}
