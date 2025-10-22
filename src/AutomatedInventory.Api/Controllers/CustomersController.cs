using AutomatedInventory.Application.Constants;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InventoryManager + "," + RoleNames.Cashier)]
    public async Task<IActionResult> GetCustomers(CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetAsync(cancellationToken);
        return Ok(customers);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InventoryManager + "," + RoleNames.Cashier)]
    public async Task<IActionResult> GetCustomer(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InventoryManager)]
    public async Task<IActionResult> CreateCustomer([FromBody] UpsertCustomerRequest request, CancellationToken cancellationToken)
    {
        var created = await _customerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCustomer), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InventoryManager)]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpsertCustomerRequest request, CancellationToken cancellationToken)
    {
        var updated = await _customerService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _customerService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
