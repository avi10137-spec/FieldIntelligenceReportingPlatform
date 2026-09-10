namespace ReportApi.Models;

public class ReportStatisticsDto
{
  
    public long TotalReports { get; set; }

    public Dictionary<string, long> ByPriority { get; set; } = new();

    public Dictionary<string, long> ByTheater { get; set; } = new();

    public Dictionary<string, long> ByReportType { get; set; } = new();
}