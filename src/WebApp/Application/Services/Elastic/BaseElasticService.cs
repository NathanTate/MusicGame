using Application.Errors;
using Application.Models.Elastic;
using Application.Models.Queries;
using Domain.Enums;
using Elastic.Clients.Elasticsearch;
using FluentResults;

namespace Application.Services.Elastic;
public abstract class BaseElasticService<T> where T : BaseDoc, new()
{
    protected readonly ElasticsearchClient _client;
    protected BaseElasticService(ElasticsearchClient client)
    {
        _client = client;
    }

    public virtual async Task<bool> CreateIndexIfNotExistsAsync(ElasticIndex index, CancellationToken cancellationToken = default)
    {
        var indexName = GetIndexName(index);

        var indexExistsResponse = await _client.Indices.ExistsAsync(indexName, cancellationToken);
        if (!indexExistsResponse.Exists)
        {
            await _client.Indices.CreateAsync(indexName, cancellationToken);
            return true;
        }

        return false;
    }

    public virtual async Task<bool> AddOrUpdateAsync(T item, ElasticIndex index, CancellationToken cancellationToken = default)
    {
        var indexName = GetIndexName(index);

        var response = await _client.IndexAsync(item, x => x.Index(indexName).OpType(OpType.Index).Id(item.Id), cancellationToken);
    
        return response.IsValidResponse;
    }

    public virtual async Task<bool> AddOrUpdateBulkAsync(IEnumerable<T> items, ElasticIndex index, CancellationToken cancellationToken = default)
    {
        var indexName = GetIndexName(index);

        var response = await _client.BulkAsync(
            x => x.Index(indexName)
            .UpdateMany(items, (itemDescr, item)
                => itemDescr.Doc(item).DocAsUpsert(true)));

        return response.IsValidResponse;
    }

    public virtual async Task<bool> Remove(string key, ElasticIndex index)
    {
        var indexName = GetIndexName(index);

        var response = await _client.DeleteByQueryAsync<T>(
            d => d.Indices(indexName));

        return response.IsValidResponse;
    }

    public virtual async Task<Result<List<T>>> SearchAsync(ElasticQuery query, ElasticIndex index)
    {
        var indexName = GetIndexName(index);

        var searchResponse = await _client.SearchAsync<T>(new SearchRequest(indexName)
        {
            From = (query.Page - 1) * query.PageSize,
            Size = query.PageSize,
            Query = new T().BuildQuery(query)
        });

        if (!searchResponse.IsValidResponse)
        {
            return new ValidationError($"Search failed: {searchResponse.DebugInformation}");
        }

        return FluentResults.Result.Ok(searchResponse.Documents.ToList());
    }

    public abstract Task<bool> ReindexAllAsync();

    protected string GetIndexName(ElasticIndex index)
    {
        return index switch
        {
            ElasticIndex.SongsIndex => "songs-index",
            ElasticIndex.PlaylistsIndex => "playlists-index",
            ElasticIndex.UsersIndex => "users-index",
            ElasticIndex.GenresIndex => "genres-index",
            _ => "songs-index"
        };
    }
}
