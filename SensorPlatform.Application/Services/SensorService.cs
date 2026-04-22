using SensorPlatform.Domain.Entities;
using SensorPlatform.Application.Data;
namespace SensorPlatform.Application.Services
{
    public class SensorService
    {
        private readonly AppDbContext _db;
        
        public SensorService(AppDbContext db)
        {
            _db = db;
        }

        public void Add(SensorReading reading)
        {
            _db.SensorReadings.Add(reading);
            _db.SaveChanges();
        }

        public List<SensorReading> GetAll()
        {
            return _db.SensorReadings.ToList();
        }
    }
}