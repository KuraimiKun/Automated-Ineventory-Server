using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost("supply")]
    public async Task<IActionResult> RecordSupply([FromBody] SupplyRecordRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.RecordSupplyAsync(request, cancellationToken);
        return result ? NoContent() : BadRequest("Unable to record supply. Ensure supplier and product exist.");
    }

    [HttpGet("history/{productId:guid}")]
    public async Task<IActionResult> GetSupplyHistory(Guid productId, CancellationToken cancellationToken)
    {
        var history = await _inventoryService.GetSupplyHistoryAsync(productId, cancellationToken);
        return Ok(history);
    }
}
