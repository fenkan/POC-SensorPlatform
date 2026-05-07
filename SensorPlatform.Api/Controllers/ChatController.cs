using Microsoft.AspNetCore.Mvc;
using SensorPlatform.Application.Services;

namespace SensorPlatform.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly AiChatService _ai;

    public ChatController(AiChatService ai)
    {
        _ai = ai;
    }

    [HttpPost]
    public async Task<string> Chat([FromBody] string message)
    {
        return await _ai.AskAsync(message);
    }
}