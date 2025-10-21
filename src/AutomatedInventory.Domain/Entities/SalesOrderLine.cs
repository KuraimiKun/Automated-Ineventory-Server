namespace AutomatedInventory.Domain.Entities;

public sealed class SalesOrderLine : BaseEntity
{
    public Guid SalesOrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Math.Round(Quantity * UnitPrice, 2);
}
