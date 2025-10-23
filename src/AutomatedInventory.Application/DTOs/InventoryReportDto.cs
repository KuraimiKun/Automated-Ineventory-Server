using AutomatedInventory.Domain.Enums;

namespace AutomatedInventory.Application.DTOs;

public sealed record InventoryReportDto(
    DateTime GeneratedAtUtc,
    IReadOnlyCollection<ProductDto> LowStockProducts,
    IReadOnlyCollection<TransactionSummaryDto> RecentTransactions,
    IReadOnlyCollection<CustomerPurchaseSummaryDto> TopCustomers);

public sealed record TransactionSummaryDto(
    Guid TransactionId,
    string ProductName,
    TransactionType Type,
    int Quantity,
    decimal TotalAmount,
    DateTime OccurredAtUtc);

public sealed record CustomerPurchaseSummaryDto(
    Guid CustomerId,
    string CustomerName,
    decimal TotalSpent,
    int OrdersCount);
