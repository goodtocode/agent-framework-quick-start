using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;

/// <summary>
/// Implementation of IEmbeddingGenerator using Azure OpenAI (or Azure AI Foundry).
/// Generates 1536-dimensional vectors via text-embedding-3-small deployment.
/// Thread-safe; can be registered as scoped or singleton.
/// </summary>
public sealed class AzureOpenAiEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly string _deploymentName;
    private readonly ILogger<AzureOpenAiEmbeddingGenerator> _logger;
    private readonly HttpClient _httpClient;

    /// <inheritdoc/>
    public int Dimension => 1536;  // text-embedding-3-small standard dimension

    /// <summary>
    /// Creates a new instance of the Azure OpenAI embedding generator.
    /// Configuration is read from appsettings:
    /// - AzureOpenAI:ApiKey (required)
    /// - AzureOpenAI:Endpoint (required)
    /// - AzureOpenAI:EmbeddingDeploymentName (optional, default: "embedding-fast")
    /// </summary>
    /// <param name="config">Configuration provider.</param>
    /// <param name="logger">Structured logger.</param>
    /// <exception cref="InvalidOperationException">If required configuration is missing.</exception>
    public AzureOpenAiEmbeddingGenerator(
        IOptions<AzureOpenAIOptions> options,
        ILogger<AzureOpenAiEmbeddingGenerator> logger,
        HttpClient httpClient)
    {
        var config = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _apiKey = config.ApiKey;
        _endpoint = config.Endpoint;
        _deploymentName = config.EmbeddingDeploymentName;
        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_endpoint))
            throw new InvalidOperationException("AzureOpenAI ApiKey and Endpoint are required for embedding generation.");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc/>
    public async Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("Attempted to generate embedding for empty text");
            throw new ArgumentException("Text cannot be null or whitespace", nameof(text));
        }

        var result = await GenerateBatchAsync(new[] { text }, cancellationToken);
        if (!result.TryGetValue(text, out var vector))
        {
            _logger.LogError("Embedding generation failed for text: {Text}", text);
            throw new InvalidOperationException($"Failed to generate embedding for: {text}");
        }

        return vector;
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, float[]>> GenerateBatchAsync(
        IEnumerable<string> texts,
        CancellationToken cancellationToken)
    {
        var textList = texts.ToList();
        if (textList.Count == 0)
        {
            _logger.LogDebug("Empty text batch provided to GenerateBatchAsync; returning empty dictionary");
            return new Dictionary<string, float[]>();
        }

        try
        {
            _logger.LogInformation(
                "Generating embeddings for {Count} texts via deployment {Deployment}",
                textList.Count,
                _deploymentName);

            // Build request
            var requestBody = new
            {
                input = textList
            };
            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Build URL
            var url = $"{_endpoint.TrimEnd('/')}/openai/deployments/{_deploymentName}/embeddings?api-version=2024-02-15-preview";

            // Create request with auth header
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = httpContent
            };
            request.Headers.Add("api-key", _apiKey);

            // Call Azure OpenAI
            var startTime = DateTime.UtcNow;
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var elapsed = DateTime.UtcNow - startTime;

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Azure OpenAI embedding request failed: {StatusCode} {Reason}\nDetails: {ErrorContent}",
                    response.StatusCode,
                    response.ReasonPhrase,
                    errorContent);
                throw new HttpRequestException(
                    $"Azure OpenAI embedding failed: {response.StatusCode} {response.ReasonPhrase}");
            }

            // Parse response
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;

            var result = new Dictionary<string, float[]>(StringComparer.Ordinal);

            if (root.TryGetProperty("data", out var dataArray))
            {
                var embeddingIndex = 0;
                foreach (var item in dataArray.EnumerateArray())
                {
                    if (item.TryGetProperty("embedding", out var embeddingProp) &&
                        item.TryGetProperty("index", out var indexProp))
                    {
                        var index = indexProp.GetInt32();
                        if (index >= 0 && index < textList.Count)
                        {
                            var vector = embeddingProp
                                .EnumerateArray()
                                .Select(e => e.GetSingle())
                                .ToArray();

                            result[textList[index]] = vector;
                            embeddingIndex++;
                        }
                    }
                }
            }

            _logger.LogInformation(
                "Successfully generated {Count} embeddings in {ElapsedMs}ms via {Deployment}",
                result.Count,
                elapsed.TotalMilliseconds,
                _deploymentName);

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Embedding generation was cancelled");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during embedding generation");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during embedding generation");
            throw;
        }
    }
}
