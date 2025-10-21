using AutomatedInventory.Domain.Enums;

namespace AutomatedInventory.Domain.Entities;

public sealed class InventoryTransaction : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SupplierId { get; set; }
    public TransactionType Type { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount => Math.Round(UnitPrice * Quantity, 2);
    public string Notes { get; set; } = string.Empty;
}
