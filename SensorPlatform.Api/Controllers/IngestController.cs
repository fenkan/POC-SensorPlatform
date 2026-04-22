using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SensorPlatform.Api.Models;
using SensorPlatform.Application.Data;
using SensorPlatform.Application.Security;
using SensorPlatform.Application.Services;
using SensorPlatform.Domain.Entities;

namespace SensorPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngestController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SensorService _service;

    public IngestController(AppDbContext db, SensorService service)
    {
        _db = db;
        _service = service;
    }

    // =========================
    // POST: ingest från device
    // =========================
    [HttpPost]
    public IActionResult Post([FromBody] IngestRequest request)
    {
        var device = _db.Devices
            .FirstOrDefault(d => d.DeviceId == request.DeviceId && d.IsActive);

        if (device == null)
            return NotFound("Device not found or inactive");

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (Math.Abs(now - request.Timestamp) > 60)
            return BadRequest("Stale request (possible replay attack)");

        var payload = JsonSerializer.Serialize(new
        {
            request.DeviceId,
            request.Value,
            request.Timestamp
        });

        var expectedSignature = HmacHelper.CreateSignature(payload, device.HmacSecret);

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(request.Signature)))
        {
            return Unauthorized("Invalid HMAC signature");
        }

        var reading = new SensorReading
        {
            DeviceId = device.Id,
            Value = request.Value,
            Timestamp = DateTimeOffset
                .FromUnixTimeSeconds(request.Timestamp)
                .UtcDateTime,
            ReceivedAt = DateTime.UtcNow
        };

        _service.Add(reading);

        return Ok(reading);
    }

    // =========================
    // GET: dashboard (FIX FÖR 405)
    // =========================
    [HttpGet]
    public IActionResult Get()
    {
        var data = _service.GetAll();
        return Ok(data);
    }
}