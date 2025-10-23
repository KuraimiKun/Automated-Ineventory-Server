using AutomatedInventory.Application.Constants;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.Admin + "," + RoleNames.InventoryManager)]
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
        var record = await _inventoryService.RecordSupplyAsync(request, cancellationToken);
        if (record is null)
        {
            return BadRequest("Unable to record supply. Ensure supplier and product exist.");
        }

        return CreatedAtAction(nameof(GetSupplyHistory), new { productId = record.ProductId }, record);
    }

    [HttpGet("history/{productId:guid}")]
    public async Task<IActionResult> GetSupplyHistory(Guid productId, CancellationToken cancellationToken)
    {
        var history = await _inventoryService.GetSupplyHistoryAsync(productId, cancellationToken);
        return Ok(history);
    }
}
