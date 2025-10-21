namespace AutomatedInventory.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Sku,
    string Description,
    decimal UnitPrice,
    int StockQuantity,
    int ReorderLevel,
    bool IsActive);
