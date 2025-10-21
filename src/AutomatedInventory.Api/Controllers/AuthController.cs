using AutomatedInventory.Application.Constants;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("register")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsRoleValid())
        {
            return BadRequest($"Role must be one of: {string.Join(", ", RoleNames.All)}");
        }

        var result = await _authService.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Login), new { username = result.UserName }, result);
    }
}
