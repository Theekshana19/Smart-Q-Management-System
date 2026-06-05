using Microsoft.EntityFrameworkCore;
using SmartQ.Application.Interfaces;
using SmartQ.Domain.Entities;
using SmartQ.Domain.Enums;

namespace SmartQ.Infrastructure.Persistence.Seed;

public static class AuthDataSeeder
{
    private static readonly (string Username, string Password, string FullName, string Email, StaffRole Role)[] DefaultUsers =
    [
        ("admin", "Admin@123", "Admin User", "admin@smartq.local", StaffRole.ADMIN),
        ("sarah", "Staff@123", "Officer Sarah", "sarah@smartq.local", StaffRole.STAFF)
    ];

    public static async Task SeedAsync(SmartQDbContext db, IPasswordHashService passwords, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        foreach (var (username, password, fullName, email, role) in DefaultUsers)
        {
            var existing = await db.StaffUsers.FirstOrDefaultAsync(u => u.Username == username, ct);
            if (existing == null)
            {
                db.StaffUsers.Add(new StaffUser
                {
                    FullName = fullName,
                    Username = username,
                    Email = email,
                    PasswordHash = passwords.HashPassword(password),
                    Role = role,
                    CounterId = role == StaffRole.STAFF ? 2 : null,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                var needsRehash = ShouldRehash(password, existing.PasswordHash, passwords);
                if (needsRehash)
                {
                    existing.PasswordHash = passwords.HashPassword(password);
                    existing.FullName = fullName;
                    existing.Email = email;
                    existing.Role = role;
                    existing.IsActive = true;
                    existing.UpdatedAt = now;
                }
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static bool ShouldRehash(string password, string passwordHash, IPasswordHashService passwords)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length < 20)
            return true;

        try
        {
            return !passwords.VerifyPassword(password, passwordHash);
        }
        catch (FormatException)
        {
            return true;
        }
    }
}
