namespace SensorPlatform.Domain.Entities;

public class IngestMessage
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public long Timestamp { get; set; }
    public string Nonce { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string PayloadHash { get; set; } = string.Empty;
    public bool IsAccepted { get; set; }
    public string? RejectReason { get; set; }
    public DateTime ReceivedAt { get; set; }

    public Device? Device { get; set; }
}
