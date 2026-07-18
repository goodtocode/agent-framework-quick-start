namespace Goodtocode.AgentFramework.Core.Application.Common.Models;

public class PaginatedList<T>(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize) : IPaginatedList<T>
{
    public IReadOnlyCollection<T> Items { get; } = items;
    public int PageNumber { get; } = pageNumber;
    public int TotalPages { get; } = (int)Math.Ceiling(count / (double)pageSize);
    public int TotalCount { get; } = count;

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public async Task<IPaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}

public class PaginatedListAdapter<T>(object concrete) : IPaginatedList<T>
{
    private readonly dynamic _concrete = concrete;

    public IReadOnlyCollection<T> Items => _concrete.Items;
    public int TotalCount => _concrete.TotalCount;
    public int PageNumber => _concrete.PageNumber;
    public int TotalPages => _concrete.TotalPages;
    public bool HasPreviousPage => _concrete.HasPreviousPage;
    public bool HasNextPage => _concrete.HasNextPage;
}
