using Application.Models.Queries;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace Application.Models.Elastic;
public class SongDoc : BaseDoc
{
    public string OwnerName { get; set; } = null!;
    public string ArtistName { get; set; } = null!;
    public List<string> GenreNames { get; set; } = [];

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
                    Boost = 4,
                    Fuzziness = new Fuzziness(1)
                },
                new MatchQuery(Infer.Field("artistName"))
                {
                    Query = query.SearchTerm,
                    Boost = 3,
                    Fuzziness = new Fuzziness(1)
                },
                new MatchQuery(Infer.Field("ownerName"))
                {
                    Query = query.SearchTerm,
                    Boost = 2,
                    Fuzziness = new Fuzziness(1)
                },

                new MatchQuery(Infer.Field("genreNames"))
                {
                    Query = query.SearchTerm,
                    Boost = 1,
                    Fuzziness = new Fuzziness(1)
                }
            },
            MinimumShouldMatch = 1
        };
    }
}
