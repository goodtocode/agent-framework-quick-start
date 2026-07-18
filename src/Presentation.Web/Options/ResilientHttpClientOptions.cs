namespace Goodtocode.AgentFramework.Presentation.Web.Options;

public class ResilientHttpClientOptions
{
    public Uri? BaseAddress { get; set; }
    public string? ClientName { get; set; }
    public int MaxRetry { get; set; } = 5;
}