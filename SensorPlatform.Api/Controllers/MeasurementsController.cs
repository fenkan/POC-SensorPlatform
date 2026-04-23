using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SensorPlatform.Application.Services;

namespace SensorPlatform.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MeasurementsController : ControllerBase
{
    private readonly SensorService _sensorService;

    public MeasurementsController(SensorService sensorService)
    {
        _sensorService = sensorService;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> Latest(CancellationToken cancellationToken)
    {
        var latest = await _sensorService.GetLatestPerDeviceAsync(cancellationToken);
        return Ok(latest);
    }

    [HttpGet("sensors")]
    public async Task<IActionResult> Sensors(CancellationToken cancellationToken)
    {
        var latest = await _sensorService.GetLatestPerDeviceAsync(cancellationToken);
        var sensors = latest
            .Select(x => new { x.DeviceId, x.DeviceName, x.SensorType })
            .OrderBy(x => x.DeviceId)
            .ToList();

        return Ok(sensors);
    }

    [HttpGet("history/{deviceId}")]
    public async Task<IActionResult> History(string deviceId, [FromQuery] int take = 200, CancellationToken cancellationToken = default)
    {
        if (take is < 1 or > 1000)
        {
            return BadRequest("'take' must be between 1 and 1000.");
        }

        var history = await _sensorService.GetHistoryByDeviceIdAsync(deviceId, take, cancellationToken);
        return Ok(history);
    }
}
