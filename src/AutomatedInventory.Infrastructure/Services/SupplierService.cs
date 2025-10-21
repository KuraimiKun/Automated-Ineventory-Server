using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Extensions;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using AutomatedInventory.Domain.Entities;
using AutomatedInventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutomatedInventory.Infrastructure.Services;

public sealed class SupplierService : ISupplierService
{
    private readonly AutomatedInventoryDbContext _dbContext;

    public SupplierService(AutomatedInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<SupplierDto>> GetAsync(CancellationToken cancellationToken)
    {
        var suppliers = await _dbContext.Suppliers
            .AsNoTracking()
            .OrderBy(supplier => supplier.CompanyName)
            .ToListAsync(cancellationToken);

        return suppliers.Select(supplier => supplier.ToDto()).ToArray();
    }

    public async Task<SupplierDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await _dbContext.Suppliers.AsNoTracking().FirstOrDefaultAsync(supplier => supplier.Id == id, cancellationToken);
        return supplier?.ToDto();
    }

    public async Task<SupplierDto> CreateAsync(UpsertSupplierRequest request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Suppliers.AnyAsync(supplier => supplier.Email == request.Email, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("A supplier with the same email already exists.");
        }

        var entity = new Supplier
        {
            CompanyName = request.CompanyName,
            ContactName = request.ContactName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address
        };

        _dbContext.Suppliers.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<SupplierDto?> UpdateAsync(Guid id, UpsertSupplierRequest request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Suppliers.FirstOrDefaultAsync(supplier => supplier.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var emailTaken = await _dbContext.Suppliers.AnyAsync(supplier => supplier.Email == request.Email && supplier.Id != id, cancellationToken);
        if (emailTaken)
        {
            throw new InvalidOperationException("A different supplier already uses this email.");
        }

        entity.CompanyName = request.CompanyName;
        entity.ContactName = request.ContactName;
        entity.Email = request.Email;
        entity.PhoneNumber = request.PhoneNumber;
        entity.Address = request.Address;
        entity.MarkModified();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Suppliers.FirstOrDefaultAsync(supplier => supplier.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _dbContext.Suppliers.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
