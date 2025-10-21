using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Extensions;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using AutomatedInventory.Domain.Entities;
using AutomatedInventory.Domain.Enums;
using AutomatedInventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutomatedInventory.Infrastructure.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly AutomatedInventoryDbContext _dbContext;

    public InventoryService(AutomatedInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> RecordSupplyAsync(SupplyRecordRequest request, CancellationToken cancellationToken)
    {
        var supplier = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken);
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (supplier is null || product is null)
        {
            return false;
        }

        if (request.QuantityReceived <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.QuantityReceived), "Quantity received must be greater than zero.");
        }

        product.IncreaseStock(request.QuantityReceived);

        var supply = new SupplyRecord
        {
            SupplierId = supplier.Id,
            ProductId = product.Id,
            QuantityReceived = request.QuantityReceived,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes
        };

        var transaction = new InventoryTransaction
        {
            ProductId = product.Id,
            SupplierId = supplier.Id,
            Type = TransactionType.Restock,
            Quantity = request.QuantityReceived,
            UnitPrice = product.UnitPrice,
            Notes = $"Supply reference {request.ReferenceNumber}"
        };

        _dbContext.SupplyRecords.Add(supply);
        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyCollection<SupplyRecordDto>> GetSupplyHistoryAsync(Guid productId, CancellationToken cancellationToken)
    {
        var records = await _dbContext.SupplyRecords
            .Where(record => record.ProductId == productId)
            .OrderByDescending(record => record.ReceivedDateUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return records.Select(record => record.ToDto()).ToArray();
    }
}
