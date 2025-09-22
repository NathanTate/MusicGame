using Application.Models.Queries;
using Domain.Enums;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace Application.Models.Elastic;

public abstract class BaseDoc
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;

    public abstract Query BuildQuery(ElasticQuery query);
}
