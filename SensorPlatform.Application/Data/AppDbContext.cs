using Microsoft.EntityFrameworkCore;
using SensorPlatform.Domain.Entities;
namespace SensorPlatform.Application.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<IngestMessage> IngestMessages => Set<IngestMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User -> Device (1:N)
        modelBuilder.Entity<User>()
            .HasMany(u => u.Devices)
            .WithOne(d => d.User)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Device -> SensorReading (1:N)
        modelBuilder.Entity<Device>()
            .HasMany(d => d.Readings)
            .WithOne(r => r.Device)
            .HasForeignKey(r => r.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Device -> IngestMessage (1:N)
        modelBuilder.Entity<Device>()
            .HasMany(d => d.IngestMessages)
            .WithOne(m => m.Device)
            .HasForeignKey(m => m.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint on Username
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Unique constraint on DeviceId
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.DeviceId)
            .IsUnique();

        // Used to block replay of already used nonces per device
        modelBuilder.Entity<IngestMessage>()
            .HasIndex(m => new { m.DeviceId, m.Nonce })
            .IsUnique();

        modelBuilder.Entity<IngestMessage>()
            .HasIndex(m => new { m.DeviceId, m.PayloadHash });
    }
}
