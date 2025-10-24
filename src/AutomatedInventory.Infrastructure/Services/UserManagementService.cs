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
    private readonly IUserActivityService _userActivityService;
    private const int RecentActivityLimit = 20;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IUserActivityService userActivityService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userActivityService = userActivityService;
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

    public async Task<UserDetailDto?> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var isLockedOut = await _userManager.IsLockedOutAsync(user);
        var lockoutEnd = user.LockoutEnd?.UtcDateTime;
        var recentActivities = await _userActivityService.GetRecentAsync(user.Id, RecentActivityLimit, cancellationToken);

        return new UserDetailDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName,
            roles.ToArray(),
            isLockedOut,
            lockoutEnd,
            user.PhoneNumber,
            recentActivities);
    }

    public async Task<UserSummaryDto> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsRoleValid())
        {
            throw new InvalidOperationException($"Role {request.Role} is not supported.");
        }

        if (!IsManagedRole(request.Role))
        {
            throw new InvalidOperationException("Only InventoryManager and Cashier roles can be assigned through this endpoint.");
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            throw new InvalidOperationException($"Role {request.Role} does not exist.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var existingRoles = await _userManager.GetRolesAsync(user);
        EnsureNotAdmin(existingRoles);

        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailOwner = await _userManager.FindByEmailAsync(request.Email);
            if (emailOwner is not null && emailOwner.Id != user.Id)
            {
                throw new InvalidOperationException("Email already exists.");
            }

            var setEmailResult = await _userManager.SetEmailAsync(user, request.Email);
            if (!setEmailResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to update email: {FormatErrors(setEmailResult)}");
            }

            // Keep emails confirmed when managed by admins.
            user.EmailConfirmed = true;
        }

        if (!string.Equals(user.FullName, request.FullName, StringComparison.Ordinal))
        {
            user.FullName = request.FullName;
        }

        var managedRoles = existingRoles.Where(IsManagedRole).ToArray();
        var hasRequestedRole = existingRoles.Any(role => string.Equals(role, request.Role, StringComparison.OrdinalIgnoreCase));

        if (!hasRequestedRole)
        {
            if (managedRoles.Length > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, managedRoles);
                if (!removeResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to update roles: {FormatErrors(removeResult)}");
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!addResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to assign role: {FormatErrors(addResult)}");
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to update user profile: {FormatErrors(updateResult)}");
        }

        var roles = await _userManager.GetRolesAsync(user);
        await _userActivityService.RecordAsync(user.Id, "ProfileUpdated", "Account details updated by administrator.", cancellationToken);

        return ToSummary(user, roles);
    }

    public async Task<bool> SetUserLockoutAsync(Guid userId, bool lockout, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return false;
        }

        var roles = await _userManager.GetRolesAsync(user);
        EnsureNotAdmin(roles);

        if (lockout && !user.LockoutEnabled)
        {
            var enableResult = await _userManager.SetLockoutEnabledAsync(user, true);
            if (!enableResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to enable lockout: {FormatErrors(enableResult)}");
            }
        }

        var lockoutEnd = lockout ? DateTimeOffset.UtcNow.AddYears(1) : (DateTimeOffset?)null;
        var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
        if (!lockoutResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to update lockout state: {FormatErrors(lockoutResult)}");
        }

        if (!lockout)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        var activityType = lockout ? "AccountLocked" : "AccountUnlocked";
        var activityDescription = lockout ? "Account locked by administrator." : "Account unlocked by administrator.";
        await _userActivityService.RecordAsync(user.Id, activityType, activityDescription, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return false;
        }

        var roles = await _userManager.GetRolesAsync(user);
        EnsureNotAdmin(roles);

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to delete user: {FormatErrors(result)}");
        }

        await _userActivityService.RecordAsync(user.Id, "AccountDeleted", "Account deleted by administrator.", cancellationToken);
        return true;
    }

    private static bool IsManagedRole(string role) =>
        string.Equals(role, RoleNames.InventoryManager, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role, RoleNames.Cashier, StringComparison.OrdinalIgnoreCase);

    private static void EnsureNotAdmin(IEnumerable<string> roles)
    {
        if (roles.Any(role => string.Equals(role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Admin accounts cannot be managed through this endpoint.");
        }
    }

    private static string FormatErrors(IdentityResult result) =>
        string.Join(", ", result.Errors.Select(error => error.Description));

    private static UserSummaryDto ToSummary(ApplicationUser user, IEnumerable<string> roles) =>
        new(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName,
            roles.ToArray());
}
