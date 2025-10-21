namespace AutomatedInventory.Domain.Entities;

public sealed class SalesOrder : BaseEntity
{
    private readonly List<SalesOrderLine> _lines = new();

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsCanceled { get; private set; }
    public IReadOnlyCollection<SalesOrderLine> Lines => _lines;
    public decimal TotalAmount => Math.Round(_lines.Sum(line => line.LineTotal), 2);

    public void AddLine(SalesOrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        line.SalesOrderId = Id;
        _lines.Add(line);
        MarkModified();
    }

    public void Cancel()
    {
        IsCanceled = true;
        MarkModified();
    }
}
