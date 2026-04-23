namespace SensorPlatform.Application.Models;

public sealed record SensorReadingListItem(
    string DeviceId,
    string DeviceName,
    string SensorType,
    double Value,
    DateTime Timestamp,
    DateTime ReceivedAt);

public sealed record SensorLatestItem(
    string DeviceId,
    string DeviceName,
    string SensorType,
    double LatestValue,
    DateTime Timestamp,
    DateTime ReceivedAt);
