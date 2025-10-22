using System.Threading;
using System.Threading.Tasks;
using AutomatedInventory.Application.Constants;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedInventory.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public AdminUsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.CreateUserAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetUserById), new { userId = result.Id }, result);
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetUserById(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserByIdAsync(userId, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet("managers")]
    public async Task<IActionResult> GetManagers(CancellationToken cancellationToken)
    {
        var users = await _userManagementService.GetUsersByRoleAsync(RoleNames.InventoryManager, cancellationToken);
        return Ok(users);
    }

    [HttpGet("cashiers")]
    public async Task<IActionResult> GetCashiers(CancellationToken cancellationToken)
    {
        var users = await _userManagementService.GetUsersByRoleAsync(RoleNames.Cashier, cancellationToken);
        return Ok(users);
    }
}
