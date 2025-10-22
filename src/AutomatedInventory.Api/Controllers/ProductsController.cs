using AutomatedInventory.Application.Constants;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InventoryManager + "," + RoleNames.Cashier)]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InventoryManager + "," + RoleNames.Cashier)]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InventoryManager)]
    public async Task<IActionResult> CreateProduct([FromBody] UpsertProductRequest request, CancellationToken cancellationToken)
    {
        var created = await _productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InventoryManager)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpsertProductRequest request, CancellationToken cancellationToken)
    {
        var updated = await _productService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _productService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("adjust-stock")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InventoryManager)]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.AdjustStockAsync(request, cancellationToken);
        return result ? NoContent() : NotFound();
    }
}
