namespace sip.Utils.Items;

public record ItemsResult<TItem>(
    IEnumerable<TItem> Items,
    int TotalCount
    );