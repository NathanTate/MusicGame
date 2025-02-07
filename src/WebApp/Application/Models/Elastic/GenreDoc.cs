using Application.Models.Queries;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Clients.Elasticsearch;

namespace Application.Models.Elastic;
public class GenreDoc : BaseDoc
{
    public override Query BuildQuery(ElasticQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            return Query.MatchAll(new MatchAllQuery());
        }


        return new BoolQuery
        {
            Should = new List<Query>
            {
               new MatchQuery(Infer.Field("name"))
               {
                    Query = query.SearchTerm,
                    Fuzziness = new Fuzziness(1)
               }
            },
            MinimumShouldMatch = 1
        };
    }
}
