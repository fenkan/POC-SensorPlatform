using Microsoft.AspNetCore.Mvc;
using SensorPlatform.Api.Models;
using SensorPlatform.Api.Services;
using SensorPlatform.Application.Services;

namespace SensorPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(AuthService authService, JwtTokenService jwtTokenService)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _authService.ValidateCredentialsAsync(request.Username, request.Password, cancellationToken);
        if (user is null)
        {
            return Unauthorized("Invalid username or password.");
        }

        var (token, expiresAtUtc) = _jwtTokenService.CreateToken(user);
        return Ok(new LoginResponse(token, expiresAtUtc));
    }

    [HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
{
    var (success, error) = await _authService.RegisterAsync(
        request.Username,
        request.Email,
        request.Password,
        cancellationToken);

    if (!success)
        return Conflict(error);

    return Ok("User registered successfully.");
}

}
