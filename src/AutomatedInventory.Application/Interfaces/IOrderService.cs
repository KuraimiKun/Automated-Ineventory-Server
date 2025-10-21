using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Models;

namespace AutomatedInventory.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OrderDto>> GetRecentAsync(int limit, CancellationToken cancellationToken);
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken);
}
