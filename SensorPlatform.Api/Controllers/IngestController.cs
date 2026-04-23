using Microsoft.AspNetCore.Mvc;
using SensorPlatform.Api.Models;
using SensorPlatform.Application.Services;

namespace SensorPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngestController : ControllerBase
{
    private readonly IngestService _ingestService;
    private readonly SensorService _sensorService;

    public IngestController(IngestService ingestService, SensorService sensorService)
    {
        _ingestService = ingestService;
        _sensorService = sensorService;
    }

    // =========================
    // POST: ingest från device
    // =========================
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] IngestRequest request, CancellationToken cancellationToken)
    {
        var result = await _ingestService.ProcessAsync(
            request.DeviceId,
            request.Value,
            request.Timestamp,
            request.Nonce,
            request.Signature,
            cancellationToken);

        return result.Status switch
        {
            IngestStatus.Accepted => Ok(result.Reading),
            IngestStatus.DeviceNotFound => NotFound(result.Message),
            IngestStatus.InvalidPayload => BadRequest(result.Message),
            IngestStatus.InvalidSignature => Unauthorized(result.Message),
            IngestStatus.StaleTimestamp => BadRequest(result.Message),
            IngestStatus.ReplayDetected => Conflict(result.Message),
            IngestStatus.DuplicateData => Conflict(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unhandled ingest status")
        };
    }

    // =========================
    // GET: dashboard (FIX FÖR 405)
    // =========================
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var data = await _sensorService.GetRecentAsync(200, cancellationToken);
        return Ok(data);
    }
}