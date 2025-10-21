using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Domain.Entities;

namespace AutomatedInventory.Application.Extensions;

public static class MappingExtensions
{
    public static CustomerDto ToDto(this Customer customer) =>
        new(customer.Id, customer.FullName, customer.Email, customer.PhoneNumber, customer.LoyaltyTier);

    public static SupplierDto ToDto(this Supplier supplier) =>
        new(supplier.Id, supplier.CompanyName, supplier.ContactName, supplier.Email, supplier.PhoneNumber, supplier.Address);

    public static ProductDto ToDto(this Product product) =>
        new(product.Id, product.Name, product.Sku, product.Description, product.UnitPrice, product.StockQuantity, product.ReorderLevel, product.IsActive);

    public static OrderDto ToDto(this SalesOrder order) =>
        new(
            order.Id,
            order.CustomerId,
            order.CustomerName,
            order.OrderedAtUtc,
            order.IsCanceled,
            order.Lines.Select(line => new OrderLineDto(line.ProductId, line.ProductName, line.Quantity, line.UnitPrice)).ToArray(),
            order.TotalAmount);

    public static SupplyRecordDto ToDto(this SupplyRecord record) =>
        new(record.Id, record.SupplierId, record.ProductId, record.QuantityReceived, record.ReceivedDateUtc, record.ReferenceNumber, record.Notes);

    public static TransactionSummaryDto ToDto(this InventoryTransaction transaction, string productName) =>
        new(transaction.Id, productName, transaction.Quantity, transaction.TotalAmount, transaction.CreatedUtc);
}
