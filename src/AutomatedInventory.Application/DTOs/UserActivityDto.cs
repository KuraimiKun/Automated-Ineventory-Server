using System;

namespace AutomatedInventory.Application.DTOs;

public sealed record UserActivityDto(
    Guid Id,
    string ActivityType,
    string Description,
    DateTime OccurredAtUtc);
