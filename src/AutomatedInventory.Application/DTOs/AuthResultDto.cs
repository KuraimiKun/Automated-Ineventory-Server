using System;
using System.Collections.Generic;

namespace AutomatedInventory.Application.DTOs;

public sealed record AuthResultDto(
    Guid UserId,
    string UserName,
    string Email,
    IReadOnlyCollection<string> Roles,
    string AccessToken,
    DateTime ExpiresAtUtc);
