using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SensorPlatform.Application.Data;
using SensorPlatform.Application.Security;
using SensorPlatform.Domain.Entities;

namespace SensorPlatform.Application.Services;

public enum IngestStatus
{
    Accepted,
    DeviceNotFound,
    InvalidPayload,
    InvalidSignature,
    StaleTimestamp,
    ReplayDetected,
    DuplicateData
}

public sealed record IngestResult(
    IngestStatus Status,
    string Message,
    SensorReading? Reading = null);

public class IngestService
{
    private const int MaxClockSkewSeconds = 120;
    private readonly AppDbContext _db;

    public IngestService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IngestResult> ProcessAsync(
        string externalDeviceId,
        double value,
        long timestamp,
        string nonce,
        string signature,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalDeviceId) ||
            string.IsNullOrWhiteSpace(nonce) ||
            string.IsNullOrWhiteSpace(signature) ||
            double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            return new IngestResult(IngestStatus.InvalidPayload, "Payload is invalid.");
        }

        var device = await _db.Devices
            .SingleOrDefaultAsync(d => d.DeviceId == externalDeviceId && d.IsActive, cancellationToken);

        if (device is null)
        {
            return new IngestResult(IngestStatus.DeviceNotFound, "Device not found or inactive.");
        }

        var currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        // Avvisa gamla/för framtida meddelanden för att minska replay-risk.
        if (Math.Abs(currentUnixTime - timestamp) > MaxClockSkewSeconds)
        {
            await SaveIngestMessageAsync(device.Id, externalDeviceId, timestamp, nonce, signature, value, false, "Stale timestamp", cancellationToken);
            return new IngestResult(IngestStatus.StaleTimestamp, "Stale request timestamp.");
        }

        // Nonce får bara användas en gång per device.
        var replayExists = await _db.IngestMessages
            .AsNoTracking()
            .AnyAsync(m => m.DeviceId == device.Id && m.Nonce == nonce, cancellationToken);

        if (replayExists)
        {
            return new IngestResult(IngestStatus.ReplayDetected, "Replay detected (nonce already used).");
        }

        var payload = BuildSignablePayload(externalDeviceId, value, timestamp, nonce);
        var expectedSignature = HmacHelper.CreateSignature(payload, device.HmacSecret);

        // Jämför signaturer i konstant tid för att undvika timing-attacker.
        if (!SecureEquals(expectedSignature, signature))
        {
            await SaveIngestMessageAsync(device.Id, externalDeviceId, timestamp, nonce, signature, value, false, "Invalid signature", cancellationToken);
            return new IngestResult(IngestStatus.InvalidSignature, "Invalid HMAC signature.");
        }

        var readingTimestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
        var duplicateReadingExists = await _db.SensorReadings
            .AsNoTracking()
            .AnyAsync(r => r.DeviceId == device.Id && r.Timestamp == readingTimestamp && r.Value == value, cancellationToken);

        // Enkel dublettkontroll för identiska mätningar.
        if (duplicateReadingExists)
        {
            await SaveIngestMessageAsync(device.Id, externalDeviceId, timestamp, nonce, signature, value, false, "Duplicate reading", cancellationToken);
            return new IngestResult(IngestStatus.DuplicateData, "Duplicate reading ignored.");
        }

        var reading = new SensorReading
        {
            DeviceId = device.Id,
            Value = value,
            Timestamp = readingTimestamp,
            ReceivedAt = DateTime.UtcNow
        };

        _db.SensorReadings.Add(reading);
        _db.IngestMessages.Add(new IngestMessage
        {
            DeviceId = device.Id,
            Timestamp = timestamp,
            Nonce = nonce,
            Signature = signature,
            PayloadHash = ComputePayloadHash(payload),
            IsAccepted = true,
            ReceivedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);

        return new IngestResult(IngestStatus.Accepted, "Reading accepted.", reading);
    }

    public static string BuildSignablePayload(string deviceId, double value, long timestamp, string nonce)
        => $"{deviceId}|{value.ToString("G17", System.Globalization.CultureInfo.InvariantCulture)}|{timestamp}|{nonce}";

    private async Task SaveIngestMessageAsync(
        int deviceId,
        string externalDeviceId,
        long timestamp,
        string nonce,
        string signature,
        double value,
        bool isAccepted,
        string rejectReason,
        CancellationToken cancellationToken)
    {
        var payload = BuildSignablePayload(externalDeviceId, value, timestamp, nonce);
        _db.IngestMessages.Add(new IngestMessage
        {
            DeviceId = deviceId,
            Timestamp = timestamp,
            Nonce = nonce,
            Signature = signature,
            PayloadHash = ComputePayloadHash(payload),
            IsAccepted = isAccepted,
            RejectReason = rejectReason,
            ReceivedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string ComputePayloadHash(string payload)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes);
    }

    private static bool SecureEquals(string expected, string actual)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(actual);

        if (expectedBytes.Length != actualBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}
