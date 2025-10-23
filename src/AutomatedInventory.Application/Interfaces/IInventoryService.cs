using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Models;

namespace AutomatedInventory.Application.Interfaces;

public interface IInventoryService
{
    Task<SupplyRecordDto?> RecordSupplyAsync(SupplyRecordRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SupplyRecordDto>> GetSupplyHistoryAsync(Guid productId, CancellationToken cancellationToken);
}

public sealed record SupplyRecordDto(
    Guid Id,
    Guid SupplierId,
    string SupplierName,
    Guid ProductId,
    string ProductName,
    int QuantityReceived,
    DateTime ReceivedDateUtc,
    string ReferenceNumber,
    string Notes);
