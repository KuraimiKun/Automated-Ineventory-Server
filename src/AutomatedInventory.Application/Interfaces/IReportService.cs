using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Models;

namespace AutomatedInventory.Application.Interfaces;

public interface IReportService
{
    Task<InventoryReportDto> GenerateInventoryReportAsync(InventoryReportFilter filter, CancellationToken cancellationToken);
}
