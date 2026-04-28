using Microsoft.EntityFrameworkCore;
using SensorPlatform.Application.Data;
using SensorPlatform.Application.Security;
using SensorPlatform.Domain.Entities;

namespace SensorPlatform.Application.Services;

public class AuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim();
        var user = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Username == normalizedUsername, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return PasswordHasher.Verify(password, user.PasswordHash) ? user : null;
    }

    public async Task<(bool Success, string Error)> RegisterAsync(string username, string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim();
    var normalizedEmail = email.Trim().ToLowerInvariant();

    var usernameTaken = await _db.Users
        .AnyAsync(u => u.Username == normalizedUsername, cancellationToken);

    if (usernameTaken)
        return (false, "Username already taken.");

    var emailTaken = await _db.Users
        .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

    if (emailTaken)
        return (false, "Email already registered.");

    _db.Users.Add(new User
    {
        Username = normalizedUsername,
        Email = normalizedEmail,
        PasswordHash = PasswordHasher.Hash(password),
        CreatedAt = DateTime.UtcNow
    });

    await _db.SaveChangesAsync(cancellationToken);
    return (true, string.Empty);
    }
}
