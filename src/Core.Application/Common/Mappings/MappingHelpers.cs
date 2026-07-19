using System.Text.Json;

namespace Goodtocode.AgentFramework.Core.Application.Common.Mappings;

public static class MappingHelpers
{
    public static TTarget MapBySerialization<TTarget>(object source)
    {
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<TTarget>(json)!;
    }

    public static async Task<TDestination?> MapBySerializationAsync<TDestination>(Task<object?> sourceTask)
    {
        var source = await sourceTask.ConfigureAwait(false);
        if (source == null) return default;
        return MapBySerialization<TDestination>(source);
    }

    public static async Task<TDestination?> MapBySerializationAsync<TSource, TDestination>(Task<TSource> sourceTask)
    {
        var source = await sourceTask.ConfigureAwait(false);
        if (source == null) return default;
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<TDestination>(json);
    }

    public static TDestination? MapBySerialization<TSource, TDestination>(this TSource source)
    {
        if (source == null) return default;
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<TDestination>(json);
    }
}
