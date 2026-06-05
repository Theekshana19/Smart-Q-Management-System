using Microsoft.AspNetCore.Identity;
using SmartQ.Application.Interfaces;
using SmartQ.Domain.Entities;

namespace SmartQ.Infrastructure.Services;

public class PasswordHashService : IPasswordHashService
{
    private readonly PasswordHasher<StaffUser> _hasher = new();

    public string HashPassword(string password) => _hasher.HashPassword(new StaffUser(), password);

    public bool VerifyPassword(string password, string passwordHash)
    {
        var result = _hasher.VerifyHashedPassword(new StaffUser(), passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
