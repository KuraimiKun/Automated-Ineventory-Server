using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost("inventory")]
    public async Task<IActionResult> GenerateInventoryReport([FromBody] InventoryReportFilter filter, CancellationToken cancellationToken)
    {
        var report = await _reportService.GenerateInventoryReportAsync(filter, cancellationToken);
        return Ok(report);
    }
}
