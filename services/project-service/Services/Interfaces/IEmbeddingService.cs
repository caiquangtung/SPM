using Pgvector;

namespace project_service.Services.Interfaces;

public interface IEmbeddingService
{
    /// <summary>
    /// Generates embedding vector from text using Gemini Embedding API
    /// </summary>
    /// <param name="text">Text to generate embedding for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vector embedding (768 dimensions)</returns>
    Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}

