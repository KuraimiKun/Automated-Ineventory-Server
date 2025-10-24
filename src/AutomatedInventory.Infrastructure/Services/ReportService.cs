using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Extensions;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Application.Models;
using AutomatedInventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutomatedInventory.Infrastructure.Services;

public sealed class ReportService : IReportService
{
    private readonly AutomatedInventoryDbContext _dbContext;

    public ReportService(AutomatedInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InventoryReportDto> GenerateInventoryReportAsync(InventoryReportFilter filter, CancellationToken cancellationToken)
    {
        var lowStockQuery = _dbContext.Products
            .AsNoTracking()
            .Where(product => product.StockQuantity <= product.ReorderLevel)
            .OrderBy(product => product.Name);

        var transactionsQuery = _dbContext.Transactions
            .AsNoTracking()
            .AsQueryable();

        if (filter.FromUtc.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(transaction => transaction.CreatedUtc >= filter.FromUtc.Value);
        }

        if (filter.ToUtc.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(transaction => transaction.CreatedUtc <= filter.ToUtc.Value);
        }

        if (filter.ProductId.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(transaction => transaction.ProductId == filter.ProductId.Value);
        }

        if (filter.SupplierId.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(transaction => transaction.SupplierId == filter.SupplierId.Value);
        }

        if (filter.CustomerId.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(transaction => transaction.CustomerId == filter.CustomerId.Value);
        }

        var lowStock = await lowStockQuery.ToListAsync(cancellationToken);

        var recentTransactions = await transactionsQuery
            .OrderByDescending(transaction => transaction.CreatedUtc)
            .Take(50)
            .Join(
                _dbContext.Products.AsNoTracking(),
                transaction => transaction.ProductId,
                product => product.Id,
                (transaction, product) => new TransactionSummaryDto(
                    transaction.Id,
                    product.Name,
                    transaction.Type,
                    transaction.Quantity,
                    Math.Round(transaction.UnitPrice * transaction.Quantity, 2),
                    transaction.CreatedUtc))
            .ToListAsync(cancellationToken);

        var filteredOrders = _dbContext.Orders
            .AsNoTracking()
            .Where(order => !order.IsCanceled)
            .Where(order => !filter.FromUtc.HasValue || order.OrderedAtUtc >= filter.FromUtc.Value)
            .Where(order => !filter.ToUtc.HasValue || order.OrderedAtUtc <= filter.ToUtc.Value);

        var orderTotalsQuery = _dbContext.OrderLines
            .AsNoTracking()
            .Join(
                filteredOrders,
                line => line.SalesOrderId,
                order => order.Id,
                (line, order) => new
                {
                    order.CustomerId,
                    order.CustomerName,
                    OrderId = order.Id,
                    line.Quantity,
                    line.UnitPrice
                })
            .GroupBy(entry => new { entry.CustomerId, entry.CustomerName, entry.OrderId })
            .Select(group => new
            {
                group.Key.CustomerId,
                group.Key.CustomerName,
                OrderTotal = group.Sum(item => item.UnitPrice * item.Quantity)
            });

        var orderTotals = await orderTotalsQuery.ToListAsync(cancellationToken);

        var customerSummaries = orderTotals
            .GroupBy(entry => new { entry.CustomerId, entry.CustomerName })
            .Select(group => new CustomerPurchaseSummaryDto(
                group.Key.CustomerId,
                group.Key.CustomerName,
                group.Sum(item => item.OrderTotal),
                group.Count()))
            .OrderByDescending(summary => summary.TotalSpent)
            .Take(10)
            .ToArray();

        return new InventoryReportDto(
            DateTime.UtcNow,
            lowStock.Select(product => product.ToDto()).ToArray(),
            recentTransactions,
            customerSummaries);
    }
}
