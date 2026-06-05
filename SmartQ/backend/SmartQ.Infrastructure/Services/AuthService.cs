using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartQ.Application.Configuration;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;
using SmartQ.Domain.Enums;
using SmartQ.Infrastructure.Persistence;

namespace SmartQ.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly SmartQDbContext _db;
    private readonly IPasswordHashService _passwords;
    private readonly IStaffSessionService _sessions;
    private readonly JwtSettings _jwt;

    public AuthService(
        SmartQDbContext db,
        IPasswordHashService passwords,
        IStaffSessionService sessions,
        IOptions<JwtSettings> jwt)
    {
        _db = db;
        _passwords = passwords;
        _sessions = sessions;
        _jwt = jwt.Value;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? loginIp, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            throw new InvalidOperationException("Username and password are required.");

        var user = await _db.StaffUsers
            .FirstOrDefaultAsync(u => u.Username == request.Username.Trim(), ct)
            ?? throw new InvalidOperationException("Invalid username or password.");

        if (!user.IsActive)
            throw new InvalidOperationException("Account is inactive.");

        if (!_passwords.VerifyPassword(request.Password, user.PasswordHash))
            throw new InvalidOperationException("Invalid username or password.");

        var role = user.Role.ToString();
        var requiresCounterSelection = false;
        if (user.Role == StaffRole.STAFF)
        {
            var active = await _sessions.GetActiveSessionForStaffAsync(user.Id, ct);
            requiresCounterSelection = active is null;
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);
        var token = GenerateToken(user, expiresAt);

        return new LoginResponse(
            token,
            expiresAt,
            new AuthUserDto(user.Id, user.FullName, user.Username, role),
            requiresCounterSelection);
    }

    public async Task<MeResponse> GetMeAsync(int userId, CancellationToken ct = default)
    {
        var user = await _db.StaffUsers.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException("User not found.");

        ActiveCounterSessionDto? session = null;
        if (user.Role == StaffRole.STAFF)
            session = await _sessions.GetActiveSessionForStaffAsync(userId, ct);

        return new MeResponse(
            user.Id,
            user.FullName,
            user.Username,
            user.Role.ToString(),
            session);
    }

    public async Task LogoutAsync(int userId, string role, CancellationToken ct = default)
    {
        if (string.Equals(role, StaffRole.STAFF.ToString(), StringComparison.OrdinalIgnoreCase))
            await _sessions.EndSessionAsync(userId, ct);
    }

    private string GenerateToken(Domain.Entities.StaffUser user, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(_jwt.Secret))
            throw new InvalidOperationException("JWT secret is not configured.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("sub", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("username", user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim("fullName", user.FullName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
