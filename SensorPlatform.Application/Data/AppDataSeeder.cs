using Microsoft.EntityFrameworkCore;
using SensorPlatform.Application.Security;
using SensorPlatform.Domain.Entities;

namespace SensorPlatform.Application.Data;

public class AppDataSeeder
{
    private readonly AppDbContext _db;

    public AppDataSeeder(AppDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Skapa en enkel demoanvändare om den inte finns.
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == "demo", cancellationToken);
        if (user is null)
        {
            user = new User
            {
                Username = "demo",
                PasswordHash = PasswordHasher.Hash("demo123"),
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var seedDevices = new[]
        {
            (DeviceId: "sensor-temp-01", Name: "Temperature Office", SensorType: "temperature", Secret: "temp-01-secret"),
            (DeviceId: "sensor-hum-01", Name: "Humidity Office", SensorType: "humidity", Secret: "hum-01-secret"),
            (DeviceId: "sensor-co2-01", Name: "CO2 Lab", SensorType: "co2", Secret: "co2-01-secret"),
            (DeviceId: "sensor-press-01", Name: "Pressure Lab", SensorType: "pressure", Secret: "press-01-secret")
        };

        foreach (var seed in seedDevices)
        {
            var exists = await _db.Devices.AnyAsync(d => d.DeviceId == seed.DeviceId, cancellationToken);
            if (exists)
            {
                continue;
            }

            _db.Devices.Add(new Device
            {
                DeviceId = seed.DeviceId,
                Name = seed.Name,
                SensorType = seed.SensorType,
                HmacSecret = seed.Secret,
                IsActive = true,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        var hasReadings = await _db.SensorReadings.AnyAsync(cancellationToken);
        if (hasReadings)
        {
            return;
        }

        var devices = await _db.Devices.AsNoTracking().ToListAsync(cancellationToken);
        // Fast seed gör att mockdata blir reproducerbar mellan körningar.
        var rng = new Random(42);

        foreach (var device in devices)
        {
            for (var i = 0; i < 10; i++)
            {
                var timestamp = DateTime.UtcNow.AddMinutes(-(10 - i) * 5);
                _db.SensorReadings.Add(new SensorReading
                {
                    DeviceId = device.Id,
                    Value = CreateMockValue(device.SensorType, rng),
                    Timestamp = timestamp,
                    ReceivedAt = timestamp.AddSeconds(rng.Next(1, 4))
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static double CreateMockValue(string sensorType, Random rng)
    {
        return sensorType.ToLowerInvariant() switch
        {
            "temperature" => Math.Round(20 + rng.NextDouble() * 5, 2),
            "humidity" => Math.Round(40 + rng.NextDouble() * 20, 2),
            "co2" => Math.Round(500 + rng.NextDouble() * 250, 2),
            "pressure" => Math.Round(990 + rng.NextDouble() * 30, 2),
            _ => Math.Round(rng.NextDouble() * 100, 2)
        };
    }
}
