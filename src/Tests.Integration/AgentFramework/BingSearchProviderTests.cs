using System.Net;
using System.Text;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Goodtocode.AgentFramework.Tests.Integration.AgentFramework;

[TestClass]
public class BingSearchProviderTests
{
    [TestMethod]
    public async Task SearchAsyncReturnsNormalizedResults()
    {
        var handler = new QueueMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "webPages": {
                        "value": [
                          { "name": "OWASP Top 10", "url": "https://owasp.org/www-project-top-ten/", "snippet": "OWASP guidance", "siteName": "OWASP" },
                          { "name": "NVD CVE", "url": "https://nvd.nist.gov/", "snippet": "CVE detail", "siteName": "NIST" },
                          { "name": "MissingUrl" }
                        ]
                      }
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            });

        var sut = CreateSut(handler, new SearchProviderOptions
        {
            Provider = "Bing",
            Endpoint = "https://api.bing.microsoft.com",
            ApiKey = "test-key",
            DefaultResultCount = 5
        });

        var result = await sut.SearchAsync("owasp latest");

        Assert.AreEqual("owasp latest", result.Query);
        Assert.AreEqual(2, result.Results.Count);
        Assert.AreEqual("OWASP Top 10", result.Results[0].Title);
        Assert.AreEqual("https://owasp.org/www-project-top-ten/", result.Results[0].Url);
        Assert.AreEqual("OWASP", result.Results[0].Source);
    }

    [TestMethod]
    public async Task SearchAsyncRetriesOnTooManyRequestsAndSucceeds()
    {
        var retryResponse = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent("rate limit")
        };
        retryResponse.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromMilliseconds(1));

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                {
                  "webPages": {
                    "value": [
                      { "name": "Result", "url": "https://example.org", "snippet": "ok", "siteName": "Example" }
                    ]
                  }
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var handler = new QueueMessageHandler(retryResponse, successResponse);

        var sut = CreateSut(handler, new SearchProviderOptions
        {
            Provider = "Bing",
            Endpoint = "https://api.bing.microsoft.com",
            ApiKey = "test-key",
            DefaultResultCount = 3
        });

        var result = await sut.SearchAsync("retry query");

        Assert.AreEqual(2, handler.SendCount);
        Assert.AreEqual(1, result.Results.Count);
        Assert.AreEqual("Result", result.Results[0].Title);
    }

    [TestMethod]
    public async Task SearchAsyncReturnsEmptyResultWhenBingIsNotConfigured()
    {
        var handler = new QueueMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });

        var sut = CreateSut(handler, new SearchProviderOptions
        {
            Provider = "Bing",
            Endpoint = "https://api.bing.microsoft.com",
            ApiKey = string.Empty,
            DefaultResultCount = 5
        });

        var result = await sut.SearchAsync("fallback query");

        Assert.AreEqual("fallback query", result.Query);
        Assert.AreEqual(0, result.Results.Count);
        Assert.AreEqual(0, handler.SendCount);
    }

    [TestMethod]
    public async Task SearchAsyncThrowsForNonTransientFailure()
    {
        var handler = new QueueMessageHandler(
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("bad request")
            });

        var sut = CreateSut(handler, new SearchProviderOptions
        {
            Provider = "Bing",
            Endpoint = "https://api.bing.microsoft.com",
            ApiKey = "test-key",
            DefaultResultCount = 5
        });

        try
        {
            await sut.SearchAsync("bad query");
            Assert.Fail("Expected HttpRequestException.");
        }
        catch (HttpRequestException ex)
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, ex.StatusCode);
        }
    }

    private static BingSearchProvider CreateSut(HttpMessageHandler handler, SearchProviderOptions options)
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.bing.microsoft.com")
        };

        return new BingSearchProvider(client, Options.Create(options), NullLogger<BingSearchProvider>.Instance);
    }

    private sealed class QueueMessageHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);

        public int SendCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SendCount++;

            if (_responses.Count == 0)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("No response configured")
                });
            }

            return Task.FromResult(_responses.Dequeue());
        }
    }
}
