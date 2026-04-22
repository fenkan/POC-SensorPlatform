namespace SensorPlatform.Domain.Entities
{
    public class SensorReading
    {
        public int Id { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime ReceivedAt { get; set; }
        
        // Foreign key
        public int DeviceId { get; set; }
        
        // Navigation property
        public Device? Device { get; set; }
    }
}