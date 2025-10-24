using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Extensions;
using AutomatedInventory.Application.Interfaces;
using AutomatedInventory.Domain.Entities;
using AutomatedInventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutomatedInventory.Infrastructure.Services;

public sealed class UserActivityService : IUserActivityService
{
    private readonly AutomatedInventoryDbContext _dbContext;

    public UserActivityService(AutomatedInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RecordAsync(Guid userId, string activityType, string description, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return;
        }

        var activity = new UserActivity
        {
            UserId = userId,
            ActivityType = activityType,
            Description = description,
            OccurredAtUtc = DateTime.UtcNow
        };

        _dbContext.UserActivities.Add(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserActivityDto>> GetRecentAsync(Guid userId, int limit, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return Array.Empty<UserActivityDto>();
        }

        limit = Math.Clamp(limit, 1, 100);

        var activities = await _dbContext.UserActivities
            .AsNoTracking()
            .Where(activity => activity.UserId == userId)
            .OrderByDescending(activity => activity.OccurredAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return activities.Select(activity => activity.ToDto()).ToArray();
    }
}
