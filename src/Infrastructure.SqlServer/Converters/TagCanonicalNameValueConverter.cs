using System.Text.Json;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Converters;

public class TagCanonicalNameValueConverter : ValueConverter<Dictionary<string, string>, string>
{
    public TagCanonicalNameValueConverter()
        : base(
            v => JsonSerializer.Serialize(v, JsonSerializerOptionsProvider.Default),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonSerializerOptionsProvider.Default) ?? new Dictionary<string, string>())
    { }
}
