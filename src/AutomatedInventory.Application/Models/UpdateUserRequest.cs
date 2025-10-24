using System;
using AutomatedInventory.Application.Constants;

namespace AutomatedInventory.Application.Models;

public sealed record UpdateUserRequest(
    string Email,
    string? FullName,
    string Role)
{
    public bool IsRoleValid() => Array.Exists(RoleNames.All, role => string.Equals(role, Role, StringComparison.OrdinalIgnoreCase));
}
