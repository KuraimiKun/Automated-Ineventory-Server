using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutomatedInventory.Application.DTOs;

namespace AutomatedInventory.Application.Interfaces;

public interface IUserActivityService
{
    Task RecordAsync(Guid userId, string activityType, string description, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserActivityDto>> GetRecentAsync(Guid userId, int limit, CancellationToken cancellationToken);
}
