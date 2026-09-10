using System.Linq;
using csharpconsumer.Models;

namespace csharpconsumer.Services
{
    public class ReportValidationService
    {
        public (bool IsValid, string ErrorMessage) ValidateReport(Report report)
        {
            if (report == null)
                return (false, "Report payload is null");

            if (string.IsNullOrWhiteSpace(report.ReportId)) return (false, "Missing or empty reportId");
            if (string.IsNullOrWhiteSpace(report.AgentId)) return (false, "Missing or empty agentId");
            if (string.IsNullOrWhiteSpace(report.Sector)) return (false, "Missing or empty sector");
            if (string.IsNullOrWhiteSpace(report.Location)) return (false, "Missing or empty location");
            if (string.IsNullOrWhiteSpace(report.Unit)) return (false, "Missing or empty unit");
            if (string.IsNullOrWhiteSpace(report.Theater)) return (false, "Missing or empty theater");
            if (string.IsNullOrWhiteSpace(report.SourceType)) return (false, "Missing or empty sourceType");
            if (string.IsNullOrWhiteSpace(report.Message)) return (false, "Missing or empty message");

            var allowedPriorities = new[] { "Low", "Medium", "High", "Critical" };
            if (string.IsNullOrWhiteSpace(report.Priority) || !allowedPriorities.Contains(report.Priority))
                return (false, $"Invalid priority value: '{report.Priority}'");

            var allowedTypes = new[] { "Observation", "Movement", "Meeting", "Access", "Communication", "Logistics", "Incident" };
            if (string.IsNullOrWhiteSpace(report.ReportType) || !allowedTypes.Contains(report.ReportType))
                return (false, $"Invalid reportType value: '{report.ReportType}'");

            bool hasSubjectId = !string.IsNullOrWhiteSpace(report.SubjectId);
            bool hasSubjectType = !string.IsNullOrWhiteSpace(report.SubjectType);
            if (hasSubjectId != hasSubjectType)
                return (false, "subjectId and subjectType must both be present or both absent");

            return (true, string.Empty);
        }
    }
}
