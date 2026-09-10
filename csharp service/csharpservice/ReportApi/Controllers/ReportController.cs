namespace ReportApi.Controllers;

using ReportApi.Models;
using ReportApi.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

  
    [HttpGet("reports/search")]
    public async Task<ActionResult<IEnumerable<ReportDto>>> Search([FromQuery] ReportSearchQuery query)
    {
        var reports = await _reportService.SearchReportsAsync(query);
        return Ok(reports);
    }

    
    [HttpGet("reports")]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetFiltered([FromQuery] ReportSearchQuery query)
    {
        var reports = await _reportService.SearchReportsAsync(query);
        return Ok(reports);
    }

    [HttpGet("reports/statistics")]
    public async Task<ActionResult<ReportStatisticsDto>> GetStatistics()
    {
        var stats = await _reportService.GetStatisticsAsync();
        return Ok(stats);
    }

    [HttpGet("subjects/{subjectId}/reports")]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetBySubject([FromRoute] string subjectId)
    {
        var reports = await _reportService.GetReportsBySubjectAsync(subjectId);
        return Ok(reports);
    }
}