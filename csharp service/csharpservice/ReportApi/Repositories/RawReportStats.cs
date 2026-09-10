
using ReportApi.Models;
namespace ReportApi.Repositories
{

    public class RawReportStats
    {
        public long TotalCount { get; set; }
        public Dictionary<string, long> PriorityCounts { get; set; } = new();
        public Dictionary<string, long> TheaterCounts { get; set; } = new();
        public Dictionary<string, long> ReportTypeCounts { get; set; } = new();
    }
}