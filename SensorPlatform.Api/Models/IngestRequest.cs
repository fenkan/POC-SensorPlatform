using System.ComponentModel.DataAnnotations;

namespace SensorPlatform.Api.Models
{
    public class IngestRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(64)]
        public string DeviceId { get; set; } = "";  // Unique device identifier (e.g. "sensor-01")

        [Range(-1000000, 1000000)]
        public double Value { get; set; }

        [Required]
        public long Timestamp { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string Nonce { get; set; } = "";

        [Required]
        [MinLength(16)]
        [MaxLength(512)]
        public string Signature { get; set; } = "";
    }
}
