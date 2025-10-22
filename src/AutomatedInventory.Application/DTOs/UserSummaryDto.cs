using System;
using System.Collections.Generic;

namespace AutomatedInventory.Application.DTOs;

public sealed record UserSummaryDto(
    Guid Id,
    string UserName,
    string Email,
    string? FullName,
    IReadOnlyCollection<string> Roles);
