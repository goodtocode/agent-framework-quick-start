using System.Text.Json;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Converters;

public class TagCanonicalNameConverter : ValueConverter<ICollection<string>, string>
{
    public TagCanonicalNameConverter()
        : base(
            v => JsonSerializer.Serialize(v, JsonSerializerOptionsProvider.Default),
            v => JsonSerializer.Deserialize<List<string>>(v, JsonSerializerOptionsProvider.Default) ?? new List<string>())
    { }
}
