namespace sip.Utils;

public class EfCoreEntityContext<TDbContext, TEntity> : IEntityContext<TEntity> 
    where TDbContext : DbContext 
    where TEntity : class
{
    private readonly TDbContext _context;
    public TEntity Entity { get; init; }
    
    public EfCoreEntityContext(TEntity entity, TDbContext context)
    {
        Entity = entity;
        _context = context;
    }

    public EfCoreEntityContext(TEntity entity, IDbContextFactory<TDbContext> contextFactory)
    {
        Entity = entity;
        _context = contextFactory.CreateDbContext();
        _context.Attach(entity);
    }
    
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}