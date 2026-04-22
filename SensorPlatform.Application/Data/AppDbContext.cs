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

        // Unique constraint on Username
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Unique constraint on DeviceId
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.DeviceId)
            .IsUnique();
    }
}
