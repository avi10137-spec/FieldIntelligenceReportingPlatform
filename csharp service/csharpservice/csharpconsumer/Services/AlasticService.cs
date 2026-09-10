using System;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using csharpconsumer.Models;

namespace csharpconsumer.Services
{
    public class ElasticsearchIndexService
    {
        public async Task EnsureIndexExistsAsync(ElasticsearchClient client, string indexName)
        {
            var existsResponse = await client.Indices.ExistsAsync(indexName);

            if (!existsResponse.Exists)
            {
                Console.WriteLine($"Index '{indexName}' does not exist. Creating...");

                var createResponse = await client.Indices.CreateAsync<Report>(indexName, r => r
                    .Mappings(m => m
                        .Properties(p => p
                            .Keyword(k => k.ReportId)
                            .Keyword(k => k.AgentId)
                            .Keyword(k => k.Unit)
                            .Keyword(k => k.Theater)
                            .Keyword(k => k.Sector)
                            .Keyword(k => k.Location)
                            .Keyword(k => k.ReportType)
                            .Keyword(k => k.Priority)
                            .Keyword(k => k.SourceType)
                            .Text(t => t.Message)
                            .Date(d => d.Timestamp)
                            .Date(d => d.ProcessedAt)
                            .Keyword(k => k.SubjectId)
                            .Keyword(k => k.SubjectType))
                        )
                    );

                if (createResponse.IsValidResponse)
                {
                    Console.WriteLine($"Index '{indexName}' created successfully with custom mappings.");
                }
                else
                {
                    Console.WriteLine($"Failed to create index: {createResponse.DebugInformation}");
                }
            }
            else
            {
                Console.WriteLine($"Index '{indexName}' already exists. Ready to consume.");
            }
        }
    }
}
