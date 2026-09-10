using Elastic.Clients.Elasticsearch;
using ReportApi.Infrastructure;
using ReportApi.Repositories;
using ReportApi.Services;

var builder = WebApplication.CreateBuilder(args);

var elasticUrl = builder.Configuration["ElasticsearchSettings:Url"] ?? "http://localhost:9200";
var defaultIndex = builder.Configuration["ElasticsearchSettings:IndexName"];
if (string.IsNullOrWhiteSpace(defaultIndex))
{
    defaultIndex = "processed_records";
}
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
var settings = new ElasticsearchClientSettings(new Uri(elasticUrl))
    .DefaultIndex(defaultIndex)
    .DisableDirectStreaming();

builder.Services.AddSingleton(new ElasticsearchClient(settings));
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();