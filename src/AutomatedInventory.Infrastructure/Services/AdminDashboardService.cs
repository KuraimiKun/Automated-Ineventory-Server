using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Extensions;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutomatedInventory.Infrastructure.Services;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly AutomatedInventoryDbContext _dbContext;

    public AdminDashboardService(AutomatedInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminDashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var totalProducts = await _dbContext.Products
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalCustomers = await _dbContext.Customers
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalSuppliers = await _dbContext.Suppliers
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalOrders = await _dbContext.Orders
            .AsNoTracking()
            .Where(order => !order.IsCanceled)
            .CountAsync(cancellationToken);

        var productStockValues = await _dbContext.Products
            .AsNoTracking()
            .Select(product => new { product.UnitPrice, product.StockQuantity })
            .ToListAsync(cancellationToken);

        var totalInventoryValue = productStockValues.Sum(entry => entry.UnitPrice * entry.StockQuantity);

        var recentOrderEntities = await _dbContext.Orders
            .AsNoTracking()
            .Where(order => !order.IsCanceled)
            .OrderByDescending(order => order.OrderedAtUtc)
            .Take(8)
            .Include(order => order.Lines)
            .ToListAsync(cancellationToken);

        var recentOrders = recentOrderEntities
            .Select(order => new DashboardOrderSummaryDto(
                order.Id,
                order.CustomerName,
                order.OrderedAtUtc,
                order.Lines.Sum(line => line.Quantity * line.UnitPrice)))
            .ToArray();

        var recentActivities = await _dbContext.UserActivities
            .AsNoTracking()
            .OrderByDescending(activity => activity.OccurredAtUtc)
            .Take(15)
            .Select(activity => activity.ToDto())
            .ToListAsync(cancellationToken);

        var lowStockEntities = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.StockQuantity <= product.ReorderLevel)
            .OrderBy(product => product.StockQuantity)
            .ThenBy(product => product.Name)
            .Take(10)
            .ToListAsync(cancellationToken);

        var lowStockProducts = lowStockEntities
            .Select(product => product.ToDto())
            .ToArray();

        return new AdminDashboardSummaryDto(
            totalProducts,
            totalCustomers,
            totalSuppliers,
            totalOrders,
            totalInventoryValue,
            recentOrders,
            recentActivities,
            lowStockProducts);
    }
}
