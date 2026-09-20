using System.Text;

namespace Goodtocode.AgentFramework.Api.Clients;

public partial class BackendApiClient
{
    private const string IdempotencyHeaderName = "Idempotency-Key";
    private static readonly AsyncLocal<string?> CurrentIdempotencyKey = new();

    public IDisposable UseIdempotencyKey(string key)
    {
        var prior = CurrentIdempotencyKey.Value;
        CurrentIdempotencyKey.Value = key;
        return new RestoreIdempotencyKeyScope(prior);
    }

    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        => ApplyIdempotencyHeader(request);

    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder)
        => ApplyIdempotencyHeader(request);

    private static void ApplyIdempotencyHeader(HttpRequestMessage request)
    {
        if (request.Method != HttpMethod.Post
            && request.Method != HttpMethod.Patch
            && request.Method != HttpMethod.Delete)
        {
            return;
        }

        var key = CurrentIdempotencyKey.Value;
        if (string.IsNullOrWhiteSpace(key)
            || request.Headers.Contains(IdempotencyHeaderName))
        {
            return;
        }

        request.Headers.Add(IdempotencyHeaderName, key);
    }

    private sealed class RestoreIdempotencyKeyScope(string? prior) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            CurrentIdempotencyKey.Value = prior;
            _disposed = true;
        }
    }
}
