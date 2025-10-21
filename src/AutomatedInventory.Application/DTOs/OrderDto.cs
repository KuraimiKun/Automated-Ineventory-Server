namespace AutomatedInventory.Application.DTOs;

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    DateTime OrderedAtUtc,
    bool IsCanceled,
    IReadOnlyCollection<OrderLineDto> Lines,
    decimal TotalAmount);
