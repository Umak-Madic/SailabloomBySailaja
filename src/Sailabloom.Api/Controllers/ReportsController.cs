using Microsoft.AspNetCore.Mvc;
using Sailabloom.Application.Services;

namespace Sailabloom.Api.Controllers;

/// <summary>Sales reporting: headline NET numbers, monthly net, category share.</summary>
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports)
    {
        _reports = reports;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        return Ok(await _reports.GetSummaryAsync());
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> Monthly([FromQuery] int months = 12)
    {
        return Ok(await _reports.GetMonthlySalesAsync(months));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        return Ok(await _reports.GetCategoryShareAsync());
    }
}
