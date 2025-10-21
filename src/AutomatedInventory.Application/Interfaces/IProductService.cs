using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Models;

namespace AutomatedInventory.Application.Interfaces;

public interface IProductService
{
    Task<IReadOnlyCollection<ProductDto>> GetAsync(CancellationToken cancellationToken);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductDto> CreateAsync(UpsertProductRequest request, CancellationToken cancellationToken);
    Task<ProductDto?> UpdateAsync(Guid id, UpsertProductRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> AdjustStockAsync(AdjustStockRequest request, CancellationToken cancellationToken);
}
