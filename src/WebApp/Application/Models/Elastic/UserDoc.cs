using Application.Models.Queries;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Clients.Elasticsearch;

namespace Application.Models.Elastic;
public class UserDoc : BaseDoc
{
    public string Email { get; set; } = null!;

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
               new MatchQuery(Infer.Field("name"!))
               {
                    Query = query.SearchTerm,
                    Boost = 3,
                    Fuzziness = new Fuzziness(1)
               },
               new MatchQuery(Infer.Field("email"!))
               {
                    Query = query.SearchTerm,
                    Boost = 2,
                    Fuzziness = new Fuzziness(1)
               }
            },
            MinimumShouldMatch = 1
        };
    }
}
