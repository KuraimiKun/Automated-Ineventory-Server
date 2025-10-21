using System.Linq;
using AutomatedInventory.Application.Constants;

namespace AutomatedInventory.Application.Models;

public sealed record RegisterUserRequest(
    string UserName,
    string Email,
    string Password,
    string FullName,
    string Role)
{
    public bool IsRoleValid() => RoleNames.All.Contains(Role);
}
