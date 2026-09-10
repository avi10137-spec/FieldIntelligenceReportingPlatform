using ReportApi.Repositories;
using ReportApi.Models;
namespace ReportApi.Services;
public class ReportService : IReportService
{
    private readonly IReportRepository _repository;

    public ReportService(IReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ReportDto>> SearchReportsAsync(ReportSearchQuery query)
    {
        
        if (query.From.HasValue && query.To.HasValue && query.From > query.To)
        {
            throw new ArgumentException("start cannot be after end");
        }

       
        var reports = await _repository.SearchAsync(query);

       
        return reports.Select(MapToDto);
    }

    public async Task<IEnumerable<ReportDto>> GetReportsBySubjectAsync(string subjectId)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            throw new ArgumentException("subject id cannot be empty");
        }

        var reports = await _repository.GetBySubjectIdAsync(subjectId);

     
        return reports.OrderBy(r => r.Timestamp).Select(MapToDto);
    }

    public async Task<ReportStatisticsDto> GetStatisticsAsync()
    {
        
        var rawStats = await _repository.GetAggregationsAsync();

      
        return new ReportStatisticsDto
        {
            TotalReports = rawStats.TotalCount,
            ByPriority = rawStats.PriorityCounts,
            ByTheater = rawStats.TheaterCounts,
            ByReportType = rawStats.ReportTypeCounts
        };
    }

    private static ReportDto MapToDto(Report entity) => new()
    {
        ReportId = entity.ReportId,
        Timestamp = entity.Timestamp,
        AgentId = entity.AgentId,
        Unit = entity.Unit,
        Theater = entity.Theater,
        Sector = entity.Sector,
        Location = entity.Location,
        ReportType = entity.ReportType,
        Priority = entity.Priority,
        SourceType = entity.SourceType,
        Message = entity.Message,
        SubjectId = entity.SubjectId,
        SubjectType = entity.SubjectType
    };
}
