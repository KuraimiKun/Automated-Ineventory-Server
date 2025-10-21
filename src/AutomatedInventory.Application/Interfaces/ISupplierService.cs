using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Models;

namespace AutomatedInventory.Application.Interfaces;

public interface ISupplierService
{
    Task<IReadOnlyCollection<SupplierDto>> GetAsync(CancellationToken cancellationToken);
    Task<SupplierDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<SupplierDto> CreateAsync(UpsertSupplierRequest request, CancellationToken cancellationToken);
    Task<SupplierDto?> UpdateAsync(Guid id, UpsertSupplierRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
