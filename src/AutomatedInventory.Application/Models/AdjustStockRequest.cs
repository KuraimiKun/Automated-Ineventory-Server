namespace AutomatedInventory.Application.Models;

public sealed record AdjustStockRequest(
    Guid ProductId,
    int Quantity,
    string Reason);
