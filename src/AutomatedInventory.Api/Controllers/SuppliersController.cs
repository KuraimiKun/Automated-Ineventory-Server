using AutomatedInventory.Application.Constants;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSuppliers(CancellationToken cancellationToken)
    {
        var suppliers = await _supplierService.GetAsync(cancellationToken);
        return Ok(suppliers);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSupplier(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.GetByIdAsync(id, cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] UpsertSupplierRequest request, CancellationToken cancellationToken)
    {
        var created = await _supplierService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetSupplier), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpsertSupplierRequest request, CancellationToken cancellationToken)
    {
        var updated = await _supplierService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSupplier(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _supplierService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
