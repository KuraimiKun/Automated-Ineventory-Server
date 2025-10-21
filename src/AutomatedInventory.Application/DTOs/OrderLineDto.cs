namespace AutomatedInventory.Application.DTOs;

public sealed record OrderLineDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
