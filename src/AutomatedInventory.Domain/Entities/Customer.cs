namespace AutomatedInventory.Domain.Entities;

public sealed class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? LoyaltyTier { get; set; }
}
