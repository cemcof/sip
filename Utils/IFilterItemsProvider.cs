namespace sip.Utils;

public interface IFilterItemsProvider<TItem>
{
    Task<IEnumerable<TItem>> GetItems(string? filter = null, CancellationToken ct = default);
}