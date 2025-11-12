using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pgvector;
using project_service.Services.Interfaces;

namespace project_service.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmbeddingService> _logger;
    private readonly string _apiKey;
    private readonly string _apiUrl;

    public EmbeddingService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<EmbeddingService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _apiKey = _configuration["Gemini:ApiKey"]
            ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            ?? throw new InvalidOperationException("Gemini API key is not configured. Set Gemini:ApiKey in appsettings.json or GEMINI_API_KEY environment variable.");

        _apiUrl = _configuration["Gemini:EmbeddingApiUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/models/embedding-001:embedContent";
    }

    public async Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be null or empty", nameof(text));
        }

        try
        {
            var requestBody = new
            {
                model = "models/embedding-001",
                content = new
                {
                    parts = new[]
                    {
                        new { text = text }
                    }
                },
                taskType = "RETRIEVAL_DOCUMENT"
            };

            var url = $"{_apiUrl}?key={_apiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Gemini API returned {response.StatusCode}: {errorContent}");
            }

            var jsonDoc = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

            if (!jsonDoc.TryGetProperty("embedding", out var embeddingElement) ||
                !embeddingElement.TryGetProperty("values", out var valuesElement))
            {
                throw new InvalidOperationException("Gemini API response missing embedding values");
            }

            var embeddingArray = valuesElement.Deserialize<float[]>();
            if (embeddingArray == null || embeddingArray.Length == 0)
            {
                throw new InvalidOperationException("Gemini API returned empty embedding");
            }

            // Convert float array to Vector
            return new Vector(embeddingArray);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text: {Text}", text);
            throw;
        }
    }
}

