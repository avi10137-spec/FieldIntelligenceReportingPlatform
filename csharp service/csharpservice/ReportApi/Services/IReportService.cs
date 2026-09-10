using ReportApi.Models;
namespace ReportApi.Services
{
    public interface IReportService
    {
        Task<IEnumerable<ReportDto>> SearchReportsAsync(ReportSearchQuery query);
        Task<IEnumerable<ReportDto>> GetReportsBySubjectAsync(string subjectId);
        Task<ReportStatisticsDto> GetStatisticsAsync();
    }
}
