using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Extensions;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using AutomatedInventory.Domain.Entities;
using AutomatedInventory.Domain.Enums;
using AutomatedInventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutomatedInventory.Infrastructure.Services;

public sealed class OrderService : IOrderService
{
    private readonly AutomatedInventoryDbContext _dbContext;

    public OrderService(AutomatedInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order?.ToDto();
    }

    public async Task<IReadOnlyCollection<OrderDto>> GetRecentAsync(int limit, CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Orders
            .Include(o => o.Lines)
            .AsNoTracking()
            .OrderByDescending(o => o.OrderedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return orders.Select(order => order.ToDto()).ToArray();
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException("Customer not found.");
        }

        var order = new SalesOrder
        {
            CustomerId = customer.Id,
            CustomerName = customer.FullName
        };

        foreach (var line in request.Lines)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == line.ProductId, cancellationToken);
            if (product is null)
            {
                throw new InvalidOperationException($"Product with id {line.ProductId} not found.");
            }

            if (product.StockQuantity < line.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}.");
            }

            product.DecreaseStock(line.Quantity);

            order.AddLine(new SalesOrderLine
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = line.Quantity,
                UnitPrice = product.UnitPrice
            });

            _dbContext.Transactions.Add(new InventoryTransaction
            {
                ProductId = product.Id,
                CustomerId = customer.Id,
                Type = TransactionType.Sale,
                Quantity = line.Quantity,
                UnitPrice = product.UnitPrice,
                Notes = $"Order {order.Id}"
            });
        }

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return order.ToDto();
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            return false;
        }

        if (order.IsCanceled)
        {
            return true;
        }

        order.Cancel();

        foreach (var line in order.Lines)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == line.ProductId, cancellationToken);
            if (product is null)
            {
                continue;
            }

            product.IncreaseStock(line.Quantity);

            _dbContext.Transactions.Add(new InventoryTransaction
            {
                ProductId = product.Id,
                CustomerId = order.CustomerId,
                Type = TransactionType.Return,
                Quantity = line.Quantity,
                UnitPrice = product.UnitPrice,
                Notes = $"Cancellation of order {order.Id}"
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
