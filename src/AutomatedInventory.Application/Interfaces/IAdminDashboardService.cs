using System.Threading;
using System.Threading.Tasks;
using AutomatedInventory.Application.DTOs;

namespace AutomatedInventory.Application.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken);
}
