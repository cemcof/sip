namespace sip.Utils.Items;

public interface IUnitOfWork<out TItem> : IDisposable
{
    TItem Item { get; }
    Task SaveAsync();
}

/// <summary>
/// Unit of work for Entity Framework Core.
/// </summary>
/// <param name="dbContext"></param>
/// <param name="item"></param>
/// <typeparam name="TItem"></typeparam>
public class EfUnitOfWork<TItem>(DbContext dbContext, TItem item) : IUnitOfWork<TItem>
    where TItem : class
{
    public TItem Item => item;

    public async Task SaveAsync()
    {
        await dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        dbContext.Dispose();
    }
}