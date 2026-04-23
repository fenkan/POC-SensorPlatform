namespace SensorPlatform.Api.Models;

public sealed record LoginResponse(string Token, DateTime ExpiresAtUtc);
