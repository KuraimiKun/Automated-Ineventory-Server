using System;
using System.Collections.Generic;

namespace AutomatedInventory.Application.DTOs;

public sealed record UserDetailDto(
    Guid Id,
    string UserName,
    string Email,
    string? FullName,
    IReadOnlyCollection<string> Roles,
    bool IsLockedOut,
    DateTime? LockoutEndUtc,
    string? PhoneNumber,
    IReadOnlyCollection<UserActivityDto> RecentActivities);
