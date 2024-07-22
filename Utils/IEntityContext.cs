namespace sip.Utils;

public interface IEntityContext<out TEntity> : IDisposable, IAsyncDisposable
{
    TEntity Entity { get; }
    Task SaveChangesAsync(CancellationToken ct = default);
}