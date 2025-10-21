namespace AutomatedInventory.Application.DTOs;

public sealed record SupplierDto(
    Guid Id,
    string CompanyName,
    string ContactName,
    string Email,
    string PhoneNumber,
    string Address);
