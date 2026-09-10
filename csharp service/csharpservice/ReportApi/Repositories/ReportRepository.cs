
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ReportApi.Models;



namespace ReportApi.Repositories;


public class ReportRepository : IReportRepository

{
    private readonly ElasticsearchClient _client;
    private const string IndexName = "processed_records";

    public ReportRepository(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<Report>> SearchAsync(ReportSearchQuery query)
    {
        var response = await _client.SearchAsync<Report>(s => s
            .Index(IndexName)
            .Query(q => q
                .Bool(b =>
                {

                    if (!string.IsNullOrWhiteSpace(query.Text))
                    {
                        b.Must(m => m.Match(c => c.Field(f => f.Message).Query(query.Text)));
                    }


                    if (!string.IsNullOrWhiteSpace(query.Theater))
                    {
                        b.Filter(f => f.Term(t => t.Field(fld => fld.Theater).Value(query.Theater)));
                    }

                    if (!string.IsNullOrWhiteSpace(query.Sector))
                    {
                        b.Filter(f => f.Term(t => t.Field(fld => fld.Sector).Value(query.Sector)));
                    }

                    if (!string.IsNullOrWhiteSpace(query.Location))
                    {
                        b.Filter(f => f.Term(t => t.Field(fld => fld.Location).Value(query.Location)));
                    }

                    if (!string.IsNullOrWhiteSpace(query.ReportType))
                    {
                        b.Filter(f => f.Term(t => t.Field(fld => fld.ReportType).Value(query.ReportType)));
                    }


                    if (query.Priorities != null && query.Priorities.Any())
                    {
                        var priorityValues = query.Priorities.Select(p => (FieldValue)p).ToArray();
                        b.Filter(f => f.Terms(t => t.Field(fld => fld.Priority).Terms(new TermsQueryField(priorityValues))));
                    }

                    if (query.From.HasValue || query.To.HasValue)
                    {
                        b.Filter(f => f.Range(r => r
                            .DateRange(d => d
                                .Field(fld => fld.Timestamp)
                                .Gte(query.From)
                                .Lte(query.To)
                            )
                        ));
                    }
                }
            ))
        );

        return response.Documents;
    }



    public async Task<IEnumerable<Report>> GetBySubjectIdAsync(string subjectId)
    {
        var response = await _client.SearchAsync<Report>(s => s
            .Index(IndexName)
            .Query(q => q
                .Term(t => t.Field(f => f.SubjectId).Value(subjectId))
            )

            .Sort(so => so.Field(f => f.Timestamp, fso => fso.Order(SortOrder.Asc)))
        );

        return response.Documents;
    }

    public async Task<RawReportStats> GetAggregationsAsync()
    {
        var response = await _client.SearchAsync<Report>(s => s
            .Index(IndexName)
            .Size(0)
            .Aggregations(a => a
                .Add("by_priority", ag => ag.Terms(t => t.Field(f => f.Priority)))
                .Add("by_theater", ag => ag.Terms(t => t.Field(f => f.Theater)))
                .Add("by_report_type", ag => ag.Terms(t => t.Field(f => f.ReportType)))
            )
        );

        return new RawReportStats
        {
            TotalCount = response.Total,
            PriorityCounts = ExtractBuckets(response.Aggregations, "by_priority"),
            TheaterCounts = ExtractBuckets(response.Aggregations, "by_theater"),
            ReportTypeCounts = ExtractBuckets(response.Aggregations, "by_report_type")
        };
    }

    private static Dictionary<string, long> ExtractBuckets(IReadOnlyDictionary<string, IAggregate>? aggregations, string key)
    {
        if (aggregations != null && aggregations.TryGetValue(key, out var aggregate) && aggregate is StringTermsAggregate termsAgg)
        {
            return termsAgg.Buckets.ToDictionary(
                b => b.Key.ToString(),
                b => b.DocCount
            );
        }

        return new Dictionary<string, long>();
    }

    
    }

    
