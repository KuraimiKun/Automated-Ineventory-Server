namespace AutomatedInventory.Domain.Entities;

public sealed class SupplyRecord : BaseEntity
{
    public Guid SupplierId { get; set; }
    public Guid ProductId { get; set; }
    public int QuantityReceived { get; set; }
    public DateTime ReceivedDateUtc { get; set; } = DateTime.UtcNow;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
