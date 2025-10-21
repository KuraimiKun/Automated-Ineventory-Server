using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace AutomatedInventory.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(IEnumerable<Claim> claims, DateTime expiresAtUtc);
}
