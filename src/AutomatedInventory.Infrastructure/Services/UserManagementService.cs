using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutomatedInventory.Application.Constants;
using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using AutomatedInventory.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutomatedInventory.Infrastructure.Services;

public sealed class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<UserSummaryDto> CreateUserAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsRoleValid())
        {
            throw new InvalidOperationException($"Role {request.Role} is not supported.");
        }

        if (!IsManagedRole(request.Role))
        {
            throw new InvalidOperationException("Only InventoryManager and Cashier roles can be created through this endpoint.");
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            throw new InvalidOperationException($"Role {request.Role} does not exist.");
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

        return ToSummary(user, roles);
    }

    public async Task<UserSummaryDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return ToSummary(user, roles);
    }

    public async Task<IReadOnlyCollection<UserSummaryDto>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken)
    {
        if (!IsManagedRole(role))
        {
            throw new InvalidOperationException("Only InventoryManager and Cashier roles can be queried through this endpoint.");
        }

        var users = await _userManager.GetUsersInRoleAsync(role);
        var results = new List<UserSummaryDto>(users.Count);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            results.Add(ToSummary(user, roles));
        }

        return results;
    }

    private static bool IsManagedRole(string role) =>
        string.Equals(role, RoleNames.InventoryManager, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role, RoleNames.Cashier, StringComparison.OrdinalIgnoreCase);

    private static UserSummaryDto ToSummary(ApplicationUser user, IEnumerable<string> roles) =>
        new(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName,
            roles.ToArray());
}
