using System;
using System.Collections.Generic;

namespace AutomatedInventory.Application.DTOs;

public sealed record DashboardOrderSummaryDto(
    Guid Id,
    string CustomerName,
    DateTime OrderedAtUtc,
    decimal TotalAmount);

public sealed record AdminDashboardSummaryDto(
    int TotalProducts,
    int TotalCustomers,
    int TotalSuppliers,
    int TotalOrders,
    decimal TotalInventoryValue,
    IReadOnlyCollection<DashboardOrderSummaryDto> RecentOrders,
    IReadOnlyCollection<UserActivityDto> RecentUserActivities,
    IReadOnlyCollection<ProductDto> LowStockProducts);
