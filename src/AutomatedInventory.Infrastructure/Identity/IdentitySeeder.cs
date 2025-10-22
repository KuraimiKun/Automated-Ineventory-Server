using System;
using System.Linq;
using AutomatedInventory.Application.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace AutomatedInventory.Infrastructure.Identity;

public sealed class IdentitySeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<IdentitySeeder> _logger;
    private readonly IConfiguration _configuration;

    public IdentitySeeder(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger<IdentitySeeder> logger,
        IConfiguration configuration)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Ensure roles exist
        foreach (var role in RoleNames.All)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create role {Role}: {Errors}", role, errors);
            }
        }

        // If there are no users, create an initial admin user from configuration or environment
        var anyUsers = await _userManager.Users.AnyAsync(cancellationToken);
        if (anyUsers)
        {
            return;
        }

        // Read admin user settings from configuration (AdminUser section) or environment variables as fallback
        var adminSection = _configuration.GetSection("AdminUser");
        var userName = adminSection.GetValue<string>("UserName") ?? Environment.GetEnvironmentVariable("ADMIN_USER_NAME");
        var email = adminSection.GetValue<string>("Email") ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var password = adminSection.GetValue<string>("Password") ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        var fullName = adminSection.GetValue<string>("FullName") ?? "Administrator";

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogInformation("Admin user not configured; skipping initial admin creation. Set AdminUser in appsettings or ADMIN_* env vars to enable.");
            return;
        }

        var adminUser = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(adminUser, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create initial admin user: {Errors}", errors);
            return;
        }

        // Ensure Admin role exists then add user to it
        if (!await _roleManager.RoleExistsAsync(RoleNames.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(RoleNames.Admin));
        }

        var addRoleResult = await _userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
        if (!addRoleResult.Succeeded)
        {
            var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to add initial admin user to role: {Errors}", errors);
        }

        _logger.LogInformation("Initial admin user '{User}' created.", userName);
    }
}
