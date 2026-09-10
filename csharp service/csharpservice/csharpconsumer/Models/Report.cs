using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace csharpconsumer.Models
{
    public class Report
    {
        [Required]
        public string ReportId { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("@timestamp")] 
        public DateTime Timestamp { get; set; }

        [Required]
        public string AgentId { get; set; } = string.Empty;

        [Required]
        public string Unit { get; set; } = string.Empty;

        [Required]
        public string Theater { get; set; } = string.Empty;

        [Required]
        public string Sector { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public string ReportType { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = string.Empty;

        [Required]
        public string SourceType { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? SubjectId { get; set; }
        public string? SubjectType { get; set; }

        public DateTime? ProcessedAt { get; set; }
    }
}