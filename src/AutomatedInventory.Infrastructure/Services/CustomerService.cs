using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Extensions;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using AutomatedInventory.Domain.Entities;
using AutomatedInventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutomatedInventory.Infrastructure.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly AutomatedInventoryDbContext _dbContext;

    public CustomerService(AutomatedInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<CustomerDto>> GetAsync(CancellationToken cancellationToken)
    {
        var customers = await _dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.FullName)
            .ToListAsync(cancellationToken);

        return customers.Select(customer => customer.ToDto()).ToArray();
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);
        return customer?.ToDto();
    }

    public async Task<CustomerDto> CreateAsync(UpsertCustomerRequest request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Customers.AnyAsync(customer => customer.Email == request.Email, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("A customer with the same email already exists.");
        }

        var entity = new Customer
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            LoyaltyTier = request.LoyaltyTier
        };

        _dbContext.Customers.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<CustomerDto?> UpdateAsync(Guid id, UpsertCustomerRequest request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Customers.FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var emailTaken = await _dbContext.Customers.AnyAsync(customer => customer.Email == request.Email && customer.Id != id, cancellationToken);
        if (emailTaken)
        {
            throw new InvalidOperationException("A different customer already uses this email.");
        }

        entity.FullName = request.FullName;
        entity.Email = request.Email;
        entity.PhoneNumber = request.PhoneNumber;
        entity.LoyaltyTier = request.LoyaltyTier;
        entity.MarkModified();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Customers.FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _dbContext.Customers.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
