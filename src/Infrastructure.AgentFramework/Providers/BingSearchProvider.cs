using System.Net;
using System.Text.Json;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Providers;

public sealed class BingSearchProvider(
    HttpClient httpClient,
    IOptions<SearchProviderOptions> options,
    ILogger<BingSearchProvider> logger) : IWebSearchProvider
{
    private const int MaxAttempts = 3;
    private readonly HttpClient _httpClient = httpClient;
    private readonly SearchProviderOptions _options = options.Value;
    private readonly ILogger<BingSearchProvider> _logger = logger;

    public async Task<WebSearchResult> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query is required.", nameof(query));
        }

        if (!IsBingConfigured(_options, out var endpointUri))
        {
            _logger.LogInformation("Bing search is not configured. Returning no results for query '{Query}'.", query);
            return new WebSearchResult
            {
                Query = query,
                Results = []
            };
        }

        var resultCount = _options.DefaultResultCount <= 0 ? 5 : _options.DefaultResultCount;
        var requestUri = BuildSearchUri(endpointUri, query, resultCount);

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Ocp-Apim-Subscription-Key", _options.ApiKey);

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException ex) when (attempt < MaxAttempts)
            {
                _logger.LogWarning(ex,
                    "Bing search attempt {Attempt} failed for query '{Query}'. Retrying.",
                    attempt,
                    query);
                await Task.Delay(ComputeDelay(attempt, null), cancellationToken);
                continue;
            }

            using (response)
            {
                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    return await ParseResponseAsync(query, stream, cancellationToken);
                }

                if (IsTransient(response.StatusCode) && attempt < MaxAttempts)
                {
                    _logger.LogWarning(
                        "Bing search attempt {Attempt} returned transient status {StatusCode} for query '{Query}'. Retrying.",
                        attempt,
                        (int)response.StatusCode,
                        query);
                    await Task.Delay(ComputeDelay(attempt, response), cancellationToken);
                    continue;
                }

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Bing search failed with status {StatusCode} for query '{Query}'. Response: {ResponseBody}",
                    (int)response.StatusCode,
                    query,
                    body);

                throw new HttpRequestException(
                    $"Bing search failed with status code {(int)response.StatusCode}.",
                    null,
                    response.StatusCode);
            }
        }

        throw new InvalidOperationException("Bing search failed after retry attempts.");
    }

    private static bool IsBingConfigured(SearchProviderOptions options, out Uri endpointUri)
    {
        endpointUri = default!;

        if (string.IsNullOrWhiteSpace(options.ApiKey)
            || options.ApiKey.Equals("<secret>", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.Endpoint)
            || !Uri.TryCreate(options.Endpoint, UriKind.Absolute, out endpointUri))
        {
            return false;
        }

        return true;
    }

    private static Uri BuildSearchUri(Uri endpointUri, string query, int resultCount)
    {
        var baseUri = endpointUri.ToString().TrimEnd('/');
        var encodedQuery = Uri.EscapeDataString(query);
        return new Uri($"{baseUri}/v7.0/search?q={encodedQuery}&count={resultCount}&responseFilter=Webpages");
    }

    private static bool IsTransient(HttpStatusCode statusCode)
        => statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;

    private static TimeSpan ComputeDelay(int attempt, HttpResponseMessage? response)
    {
        if (response is not null
            && response.Headers.RetryAfter?.Delta is { } retryAfterDelta
            && retryAfterDelta > TimeSpan.Zero)
        {
            return retryAfterDelta;
        }

        return TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
    }

    private static async Task<WebSearchResult> ParseResponseAsync(
        string query,
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var items = new List<WebSearchItem>();

        if (document.RootElement.TryGetProperty("webPages", out var webPages)
            && webPages.TryGetProperty("value", out var values)
            && values.ValueKind == JsonValueKind.Array)
        {
            foreach (var value in values.EnumerateArray())
            {
                var title = value.TryGetProperty("name", out var nameElement)
                    ? nameElement.GetString()
                    : null;
                var url = value.TryGetProperty("url", out var urlElement)
                    ? urlElement.GetString()
                    : null;

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
                {
                    continue;
                }

                var snippet = value.TryGetProperty("snippet", out var snippetElement)
                    ? snippetElement.GetString()
                    : null;
                var source = value.TryGetProperty("siteName", out var sourceElement)
                    ? sourceElement.GetString()
                    : null;

                items.Add(new WebSearchItem
                {
                    Title = title,
                    Url = url,
                    Snippet = snippet,
                    Source = source
                });
            }
        }

        return new WebSearchResult
        {
            Query = query,
            Results = items
        };
    }
}
