namespace AutomatedInventory.Application.Models;

public sealed record InventoryReportFilter(
    DateTime? FromUtc,
    DateTime? ToUtc,
    Guid? ProductId,
    Guid? SupplierId,
    Guid? CustomerId);
