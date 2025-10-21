using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Models;

namespace AutomatedInventory.Application.Interfaces;

public interface IInventoryService
{
    Task<bool> RecordSupplyAsync(SupplyRecordRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SupplyRecordDto>> GetSupplyHistoryAsync(Guid productId, CancellationToken cancellationToken);
}

public sealed record SupplyRecordDto(
    Guid Id,
    Guid SupplierId,
    Guid ProductId,
    int QuantityReceived,
    DateTime ReceivedDateUtc,
    string ReferenceNumber,
    string Notes);
