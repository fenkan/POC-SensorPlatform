using Microsoft.EntityFrameworkCore;
using SensorPlatform.Domain.Entities;
using SensorPlatform.Application.Data;
using SensorPlatform.Application.Models;

namespace SensorPlatform.Application.Services
{
    public class SensorService
    {
        private readonly AppDbContext _db;
        
        public SensorService(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(SensorReading reading, CancellationToken cancellationToken = default)
        {
            await _db.SensorReadings.AddAsync(reading, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SensorReadingListItem>> GetRecentAsync(int take = 200, CancellationToken cancellationToken = default)
        {
            return await _db.SensorReadings
                .AsNoTracking()
                .Include(r => r.Device)
                .OrderByDescending(r => r.Timestamp)
                .Take(take)
                .Select(r => new SensorReadingListItem(
                    r.Device!.DeviceId,
                    r.Device.Name,
                    r.Device.SensorType,
                    r.Value,
                    r.Timestamp,
                    r.ReceivedAt))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SensorLatestItem>> GetLatestPerDeviceAsync(CancellationToken cancellationToken = default)
        {
            var orderedReadings = await _db.SensorReadings
                .AsNoTracking()
                .Include(r => r.Device)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync(cancellationToken);

            var latestByDevice = orderedReadings
                .GroupBy(r => r.DeviceId)
                .Select(group => group.First())
                .Select(r => new SensorLatestItem(
                    r.Device!.DeviceId,
                    r.Device.Name,
                    r.Device.SensorType,
                    r.Value,
                    r.Timestamp,
                    r.ReceivedAt))
                .OrderBy(x => x.DeviceId)
                .ToList();

            return latestByDevice;
        }

        public async Task<IReadOnlyList<SensorReadingListItem>> GetHistoryByDeviceIdAsync(
            string externalDeviceId,
            int take = 200,
            CancellationToken cancellationToken = default)
        {
            return await _db.SensorReadings
                .AsNoTracking()
                .Include(r => r.Device)
                .Where(r => r.Device!.DeviceId == externalDeviceId)
                .OrderByDescending(r => r.Timestamp)
                .Take(take)
                .Select(r => new SensorReadingListItem(
                    r.Device!.DeviceId,
                    r.Device.Name,
                    r.Device.SensorType,
                    r.Value,
                    r.Timestamp,
                    r.ReceivedAt))
                .ToListAsync(cancellationToken);
        }
    }
}