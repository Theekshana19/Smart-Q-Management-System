using SmartQ.Application.DTOs;

namespace SmartQ.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string? loginIp, CancellationToken ct = default);
    Task<MeResponse> GetMeAsync(int userId, CancellationToken ct = default);
    Task LogoutAsync(int userId, string role, CancellationToken ct = default);
}
