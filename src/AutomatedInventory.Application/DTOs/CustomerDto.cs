namespace AutomatedInventory.Application.DTOs;

public sealed record CustomerDto(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    string? LoyaltyTier);
