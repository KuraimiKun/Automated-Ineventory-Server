namespace AutomatedInventory.Application.Models;

public sealed record UpsertSupplierRequest(
    string CompanyName,
    string ContactName,
    string Email,
    string PhoneNumber,
    string Address);
