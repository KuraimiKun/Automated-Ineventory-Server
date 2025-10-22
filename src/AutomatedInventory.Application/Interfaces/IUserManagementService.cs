using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Models;

namespace AutomatedInventory.Application.Interfaces;

public interface IUserManagementService
{
    Task<UserSummaryDto> CreateUserAsync(RegisterUserRequest request, CancellationToken cancellationToken);
    Task<UserSummaryDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserSummaryDto>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken);
}
