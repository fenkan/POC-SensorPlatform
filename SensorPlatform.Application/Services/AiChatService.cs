using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace SensorPlatform.Application.Services;

public class AiChatService
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly string _baseUrl;

    public AiChatService(IConfiguration config)
    {
        _baseUrl = config["Ollama:BaseUrl"] ?? "http://localhost:11434";
        _model = config["Ollama:Model"] ?? "llama3.2";
        _http = new HttpClient { BaseAddress = new Uri(_baseUrl) };
    }

    public async Task<string> AskAsync(string message, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model = _model,
            messages = new[] { new { role = "user", content = message } },
            stream = false
        };

        var response = await _http.PostAsJsonAsync("/api/chat", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);
        return result?.Message?.Content ?? string.Empty;
    }

    private sealed class OllamaResponse
    {
        public OllamaMessage? Message { get; set; }
    }

    private sealed class OllamaMessage
    {
        public string Content { get; set; } = string.Empty;
    }
}