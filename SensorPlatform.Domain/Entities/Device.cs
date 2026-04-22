namespace SensorPlatform.Domain.Entities
{
    public class Device
    {
        public int Id { get; set; }
        public string DeviceId { get; set; } = "";  // Unique identifier for device
        public string Name { get; set; } = "";
        public string HmacSecret { get; set; } = "";  // Secret for HMAC signing
        public string SensorType { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Foreign key
        public int UserId { get; set; }
        
        // Navigation properties
        public User? User { get; set; }
        public ICollection<SensorReading> Readings { get; set; } = new List<SensorReading>();
    }
}
