using System.ClientModel;
using System.ClientModel.Primitives;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;

namespace Goodtocode.AgentStart.Infrastructure.AgentFramework.Embeddings;

/// <summary>
/// Implementation of IEmbeddingGenerator using Azure OpenAI (or Azure AI Foundry).
/// Generates configured-dimensional vectors, defaulting to text-embedding-3-small at 1536 dimensions.
/// Thread-safe; can be registered as scoped or singleton.
/// </summary>
public sealed class AzureOpenAiEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly EmbeddingClient _client;
    private readonly string _deploymentName;
    private readonly ILogger<AzureOpenAiEmbeddingGenerator> _logger;

    /// <inheritdoc/>
    public int Dimension { get; }

    /// <summary>
    /// Creates a new instance of the Azure OpenAI embedding generator.
    /// Configuration is read from appsettings:
    /// - AzureOpenAI:ApiKey (required)
    /// - AzureOpenAI:Endpoint (required)
    /// - AzureOpenAI:EmbeddingDeploymentName (optional, default: "embedding-standard")
    /// - AzureOpenAI:EmbeddingDimension (optional, default: 1536)
    /// </summary>
    /// <param name="options">Configuration provider.</param>
    /// <param name="logger">Structured logger.</param>
    /// <param name="httpClient">HTTP transport.</param>
    /// <exception cref="InvalidOperationException">If required configuration is missing.</exception>
    public AzureOpenAiEmbeddingGenerator(
        IOptions<AzureOpenAIOptions> options,
        ILogger<AzureOpenAiEmbeddingGenerator> logger,
        HttpClient httpClient)
    {
        var config = options?.Value ?? throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(config.ApiKey) || string.IsNullOrWhiteSpace(config.Endpoint))
            throw new InvalidOperationException("AzureOpenAI ApiKey and Endpoint are required for embedding generation.");
        if (config.EmbeddingDimension <= 0 || string.IsNullOrWhiteSpace(config.EmbeddingDeploymentName))
            throw new InvalidOperationException("AzureOpenAI embedding deployment and positive dimension are required.");
        Dimension = config.EmbeddingDimension;
        _deploymentName = config.EmbeddingDeploymentName;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(httpClient);
        _client = new OpenAIClient(new ApiKeyCredential(config.ApiKey), new OpenAIClientOptions
        {
            Endpoint = new Uri(ConfigureServices.NormalizeAzureOpenAIEndpoint(config.Endpoint)),
            Transport = new HttpClientPipelineTransport(httpClient)
        }).GetEmbeddingClient(config.EmbeddingDeploymentName);
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
        var textList = texts.Distinct(StringComparer.Ordinal).ToList();
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

            var startTime = DateTime.UtcNow;
            var response = await _client.GenerateEmbeddingsAsync(textList,
                new EmbeddingGenerationOptions { Dimensions = Dimension }, cancellationToken);
            var elapsed = DateTime.UtcNow - startTime;
            var result = new Dictionary<string, float[]>(StringComparer.Ordinal);
            if (response.Value.Count != textList.Count)
            {
                throw new InvalidOperationException("Embedding response count does not match the input count.");
            }
            foreach (var embedding in response.Value)
            {
                var vector = embedding.ToFloats().ToArray();
                if (embedding.Index < 0 || embedding.Index >= textList.Count || vector.Length != Dimension
                    || result.ContainsKey(textList[embedding.Index]))
                {
                    throw new InvalidOperationException("Embedding response contains an invalid index or vector dimension.");
                }
                result.Add(textList[embedding.Index], vector);
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
