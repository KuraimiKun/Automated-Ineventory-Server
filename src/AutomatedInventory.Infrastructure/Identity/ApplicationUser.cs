using Microsoft.AspNetCore.Identity;

namespace AutomatedInventory.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName { get; set; }
}
