using System;
using System.Linq;
using AutomatedInventory.Application.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AutomatedInventory.Infrastructure.Identity;

public sealed class IdentitySeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<IdentitySeeder> _logger;

    public IdentitySeeder(RoleManager<IdentityRole<Guid>> roleManager, ILogger<IdentitySeeder> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
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
    }
}
