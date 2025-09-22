using Application.Models.Elastic;
using Domain.Enums;
using Elastic.Clients.Elasticsearch;
using Infrastructure.Context;
using Microsoft.Extensions.Logging;

namespace Application.Services.Elastic;
public class UsersElasticService : BaseElasticService<UserDoc>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SongsElasticService> _logger;
    public UsersElasticService(ElasticsearchClient client, AppDbContext dbContext, ILogger<SongsElasticService> logger) : base(client)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async override Task<bool> ReindexAllAsync()
    {
        try
        {
            var userDocs = _dbContext.Users.Select((u => new UserDoc
            {
                Id = u.Id.ToString(),
                Name = u.DisplayName,
                Email = u.Email
            }));

            await base.AddOrUpdateBulkAsync(userDocs, ElasticIndex.UsersIndex);

            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, ex.Message);
            return false;
        }
    }
}
