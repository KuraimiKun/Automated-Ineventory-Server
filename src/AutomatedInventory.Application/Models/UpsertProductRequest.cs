namespace AutomatedInventory.Application.Models;

public sealed record UpsertProductRequest(
    string Name,
    string Sku,
    string Description,
    decimal UnitPrice,
    int StockQuantity,
    int ReorderLevel,
    bool IsActive);
