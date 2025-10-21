namespace AutomatedInventory.Application.Models;

public sealed record SupplyRecordRequest(
    Guid SupplierId,
    Guid ProductId,
    int QuantityReceived,
    string ReferenceNumber,
    string Notes);
