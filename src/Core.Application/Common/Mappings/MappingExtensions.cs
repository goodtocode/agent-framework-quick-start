using Goodtocode.AgentFramework.Core.Application.Common.Models;

namespace Goodtocode.AgentFramework.Core.Application.Common.Mappings;

public static class MappingExtensions
{
    public static Task<IPaginatedList<TDestination>> PaginatedListAsync<TDestination>(this IQueryable<TDestination> queryable, int pageNumber, int pageSize) where TDestination : class
    {
        var paginatedItems = new List<TDestination>();
        var paginatedList = new PaginatedList<TDestination>(paginatedItems, 0, pageNumber, pageSize);
        return paginatedList.CreateAsync(queryable.AsNoTracking(), pageNumber, pageSize);
    }

    public static IPaginatedList<T> PaginatedListFromEnumerable<T>(
        this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var totalCount = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
    }
}
