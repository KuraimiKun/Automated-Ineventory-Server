using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AutomatedInventory.Application.Constants;
using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using AutomatedInventory.Infrastructure.Identity;
using AutomatedInventory.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AutomatedInventory.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ITokenService tokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsRoleValid())
        {
            throw new InvalidOperationException($"Role {request.Role} is not supported.");
        }

        var existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var existingEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingEmail is not null)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            throw new InvalidOperationException($"Role {request.Role} does not exist.");
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        var roles = await _userManager.GetRolesAsync(user);
        return await CreateAuthResultAsync(user, roles);
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user is null)
        {
            throw new InvalidOperationException("Invalid username or password.");
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new InvalidOperationException("Invalid username or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return await CreateAuthResultAsync(user, roles);
    }

    private async Task<AuthResultDto> CreateAuthResultAsync(ApplicationUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
        var token = _tokenService.CreateToken(claims, expiresAtUtc);

        return new AuthResultDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            roles.ToArray(),
            token,
            expiresAtUtc);
    }
}
