namespace AutomatedInventory.Domain.Entities;

public sealed class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; private set; }
    public int ReorderLevel { get; set; } = 5;
    public bool IsActive { get; set; } = true;

    public void InitializeStock(int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Initial stock cannot be negative.");
        }

        StockQuantity = quantity;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        StockQuantity += quantity;
        MarkModified();
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (StockQuantity - quantity < 0)
        {
            throw new InvalidOperationException("Insufficient stock to fulfill the request.");
        }

        StockQuantity -= quantity;
        MarkModified();
    }
}
