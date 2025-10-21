using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Extensions;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using AutomatedInventory.Domain.Entities;
using AutomatedInventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutomatedInventory.Infrastructure.Services;

public sealed class ProductService : IProductService
{
    private readonly AutomatedInventoryDbContext _dbContext;

    public ProductService(AutomatedInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ProductDto>> GetAsync(CancellationToken cancellationToken)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return products.Select(product => product.ToDto()).ToArray();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
        return product?.ToDto();
    }

    public async Task<ProductDto> CreateAsync(UpsertProductRequest request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Products.AnyAsync(product => product.Sku == request.Sku, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("A product with the same SKU already exists.");
        }

        var entity = new Product
        {
            Name = request.Name,
            Sku = request.Sku,
            Description = request.Description,
            UnitPrice = request.UnitPrice,
            ReorderLevel = request.ReorderLevel,
            IsActive = request.IsActive
        };

        entity.InitializeStock(request.StockQuantity);

        _dbContext.Products.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpsertProductRequest request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var skuTaken = await _dbContext.Products.AnyAsync(product => product.Sku == request.Sku && product.Id != id, cancellationToken);
        if (skuTaken)
        {
            throw new InvalidOperationException("A different product already uses this SKU.");
        }

        entity.Name = request.Name;
        entity.Sku = request.Sku;
        entity.Description = request.Description;
        entity.UnitPrice = request.UnitPrice;
        entity.ReorderLevel = request.ReorderLevel;
        entity.IsActive = request.IsActive;
        entity.MarkModified();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _dbContext.Products.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AdjustStockAsync(AdjustStockRequest request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Products.FirstOrDefaultAsync(product => product.Id == request.ProductId, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        if (request.Quantity == 0)
        {
            return true;
        }

        if (request.Quantity > 0)
        {
            entity.IncreaseStock(request.Quantity);
        }
        else
        {
            entity.DecreaseStock(Math.Abs(request.Quantity));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
