using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Models;

namespace AutomatedInventory.Application.Interfaces;

public interface ICustomerService
{
    Task<IReadOnlyCollection<CustomerDto>> GetAsync(CancellationToken cancellationToken);
    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CustomerDto> CreateAsync(UpsertCustomerRequest request, CancellationToken cancellationToken);
    Task<CustomerDto?> UpdateAsync(Guid id, UpsertCustomerRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
