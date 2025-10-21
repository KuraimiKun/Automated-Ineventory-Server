namespace AutomatedInventory.Application.Models;

public sealed record CreateOrderRequest(
    Guid CustomerId,
    IReadOnlyCollection<CreateOrderLineRequest> Lines);

public sealed record CreateOrderLineRequest(
    Guid ProductId,
    int Quantity);
