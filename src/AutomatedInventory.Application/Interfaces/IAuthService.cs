using AutomatedInventory.Application.DTOs;
using AutomatedInventory.Application.Models;

namespace AutomatedInventory.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken);
    Task<AuthResultDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
