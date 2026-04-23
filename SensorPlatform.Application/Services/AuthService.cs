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
}
