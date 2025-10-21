namespace AutomatedInventory.Application.Models;

public sealed record UpsertCustomerRequest(
    string FullName,
    string Email,
    string PhoneNumber,
    string? LoyaltyTier);
