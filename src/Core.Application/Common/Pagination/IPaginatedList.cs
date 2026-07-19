namespace Goodtocode.AgentFramework.Core.Application.Common.Pagination;

public interface IPaginatedList<T>
{
    IReadOnlyCollection<T> Items { get; }
    int TotalCount { get; }
    int PageNumber { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}
