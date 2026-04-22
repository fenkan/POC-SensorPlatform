namespace SensorPlatform.Api.Models
{
    public class IngestRequest
    {
        public string DeviceId { get; set; } = "";  // Unique device identifier (e.g. "sensor-01")
        public double Value { get; set; }
        public long Timestamp { get; set; }
        public string Signature { get; set; } = "";
    }
}
