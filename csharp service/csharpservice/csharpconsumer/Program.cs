using Confluent.Kafka;
using csharpconsumer.Models;
using csharpconsumer.Services;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace csharpconsumer;

class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    static async Task Main(string[] args)
    {
      
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables() 
            .Build();

        string elasticUrl = config["ElasticsearchSettings:Url"] ?? "http://localhost:9200";
        string indexName = config["ElasticsearchSettings:IndexName"] ?? "processed_records";
        string kafkaBroker = config["KafkaSettings:BootstrapServers"] ?? "localhost:9092";
        string groupId = config["KafkaSettings:GroupId"] ?? "report-consumer-group-v3";
        string topic = config["KafkaSettings:Topic"] ?? "raw-data-first";
        



        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        var validationService = new ReportValidationService();
        var indexService = new ElasticsearchIndexService();

        var settings = new ElasticsearchClientSettings(new Uri(elasticUrl))
            .DefaultIndex(indexName);

        var client = new ElasticsearchClient(settings);

      
        await WaitForElasticsearchAsync(indexService, client, indexName, cts.Token);

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = kafkaBroker,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(topic);

        Console.WriteLine($"C# Consumer running. Connected to Kafka ({kafkaBroker}) and ES ({elasticUrl}).");

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
              
                var consumeResult = consumer.Consume(cts.Token);

                if (consumeResult?.Message?.Value != null)
                {
                    string jsonString = consumeResult.Message.Value;

                    Report? record;
                    try
                    {
                        record = JsonSerializer.Deserialize<Report>(jsonString, JsonOptions);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Failed to deserialize JSON message: {ex.Message}");
                        consumer.Commit(consumeResult);
                        continue;
                    }

                    if (record != null)
                    {
                        var (isValid, errorMessage) = validationService.ValidateReport(record);
                        if (!isValid)
                        {
                            Console.WriteLine($"Report rejected. ReportId: {record.ReportId}, Reason: {errorMessage}");
                            consumer.Commit(consumeResult);
                            continue;
                        }

                        record.ProcessedAt = DateTime.UtcNow;

                        var existsResponse = await client.ExistsAsync(indexName, record.ReportId, cts.Token);
                        if (existsResponse.Exists)
                        {
                            Console.WriteLine($"Duplicate report detected and skipped. ReportId: {record.ReportId}");
                            consumer.Commit(consumeResult);
                            continue;
                        }

                        var indexResponse = await client.IndexAsync(record, idx => idx
                            .Index(indexName)
                            .Id(record.ReportId), cts.Token);

                        if (indexResponse.IsValidResponse)
                        {
                            Console.WriteLine($"Successfully inserted record {record.ReportId} into Elasticsearch.");
                            consumer.Commit(consumeResult);
                        }
                        else
                        {
                            Console.WriteLine($"Failed to index record {record.ReportId}: {indexResponse.DebugInformation}");
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Shutdown signal received.");
        }
        finally
        {
            consumer.Close();
            Console.WriteLine("Consumer closed gracefully.");
        }
    }

   
    private static async Task WaitForElasticsearchAsync(ElasticsearchIndexService indexService, ElasticsearchClient client, string indexName, CancellationToken cancellationToken)
    {
        int retries = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await indexService.EnsureIndexExistsAsync(client, indexName);
                Console.WriteLine("Connected to Elasticsearch successfully.");
                break;
            }
            catch (Exception ex)
            {
                retries++;
                Console.WriteLine($"[Attempt {retries}] Elasticsearch not ready yet ({ex.Message}). Retrying in 5 seconds...");
                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}