namespace sip.Utils.Items;

public interface IFilter
{
    string? FilterQuery { get; }
    int Count { get; }
    int Offset { get; }
    CancellationToken CancellationToken { get; }
}

public interface IFilteredResult<out TItem>
{
    IEnumerable<TItem> Items { get; }
    int TotalCount { get; }
}

public static class QueryExtensions
{
    public static IQueryable<T> ApplySkipTake<T>(this IQueryable<T> query, IFilter filter)
    {
        if (filter.Count > 0)
        {
            query = query.Skip(filter.Offset).Take(filter.Count);
        }

        return query;
    }
}