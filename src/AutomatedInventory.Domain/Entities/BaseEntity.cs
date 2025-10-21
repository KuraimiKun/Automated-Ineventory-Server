namespace AutomatedInventory.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    public DateTime? ModifiedUtc { get; private set; }
    public void MarkModified() => ModifiedUtc = DateTime.UtcNow;
}
