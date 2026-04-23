using System.ComponentModel.DataAnnotations;

namespace SensorPlatform.Api.Models;

public class LoginRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
