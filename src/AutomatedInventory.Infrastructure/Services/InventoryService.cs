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

    public async Task<SupplyRecordDto?> RecordSupplyAsync(SupplyRecordRequest request, CancellationToken cancellationToken)
    {
        var supplier = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken);
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (supplier is null || product is null)
        {
            return null;
        }

        if (request.QuantityReceived <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.QuantityReceived), "Quantity received must be greater than zero.");
        }

        product.IncreaseStock(request.QuantityReceived);

        var trimmedReference = request.ReferenceNumber?.Trim() ?? string.Empty;
        var trimmedNotes = request.Notes?.Trim() ?? string.Empty;

        var supply = new SupplyRecord
        {
            SupplierId = supplier.Id,
            ProductId = product.Id,
            QuantityReceived = request.QuantityReceived,
            ReferenceNumber = trimmedReference,
            Notes = trimmedNotes
        };

        var transaction = new InventoryTransaction
        {
            ProductId = product.Id,
            SupplierId = supplier.Id,
            Type = TransactionType.Restock,
            Quantity = request.QuantityReceived,
            UnitPrice = product.UnitPrice,
            Notes = string.IsNullOrWhiteSpace(trimmedReference)
                ? "Supply restock"
                : $"Supply reference {trimmedReference}"
        };

        _dbContext.SupplyRecords.Add(supply);
        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return supply.ToDto(supplier, product);
    }

    public async Task<IReadOnlyCollection<SupplyRecordDto>> GetSupplyHistoryAsync(Guid productId, CancellationToken cancellationToken)
    {
        var records = await _dbContext.SupplyRecords
            .AsNoTracking()
            .Where(record => record.ProductId == productId)
            .OrderByDescending(record => record.ReceivedDateUtc)
            .Join(
                _dbContext.Suppliers.AsNoTracking(),
                record => record.SupplierId,
                supplier => supplier.Id,
                (record, supplier) => new { record, supplier })
            .Join(
                _dbContext.Products.AsNoTracking(),
                combined => combined.record.ProductId,
                product => product.Id,
                (combined, product) => new SupplyRecordDto(
                    combined.record.Id,
                    combined.supplier.Id,
                    combined.supplier.CompanyName,
                    product.Id,
                    product.Name,
                    combined.record.QuantityReceived,
                    combined.record.ReceivedDateUtc,
                    combined.record.ReferenceNumber,
                    combined.record.Notes))
            .ToListAsync(cancellationToken);

        return records;
    }
}
