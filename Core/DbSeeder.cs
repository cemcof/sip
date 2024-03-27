namespace sip.Core;

public delegate void DbContextDelegate(AppDbContext dbContext);

public enum DbSeedStrategy
{
    /// <summary>
    /// Leave database as-is
    /// </summary>
    None,
    /// <summary>
    /// Only add entities not found in the database
    /// </summary>
    AddOnly,
    /// <summary>
    /// Add not found entities and update existing entities
    /// </summary>
    Update,
    /// <summary>
    /// Purge the database and then init
    /// </summary>
    PurgeFirst,
}

public class DbSeedOptions
{
    public DbSeedStrategy Strategy { get; set; }

    public List<object> SeedEntities { get; set; } = new();
}

public interface IDbSeeder
{
    void SimpleDbSeed();
}

public class DbSeeder(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IEnumerable<DbContextDelegate>  ctxDelegates,
        IOptions<DbSeedOptions>         options,
        ILogger<DbSeeder>               logger)
    : IDbSeeder
{
    /// <summary>
    /// For simple db seeding, entities in <see cref="IOptions{DbSeedOptions}"/> are just tracked in db context and then saved.
    /// All db constrains (e.g. primary and foreign keys) must be satisfied on entities, or
    /// seed will get failed and reverted.
    /// </summary>
    public void SimpleDbSeed()
    {
        var opts = options.Value;
        if (opts.Strategy is DbSeedStrategy.None)
        {
            logger.LogInformation("Db init is disabled, skipping");
            return;
        }
        
        logger.LogInformation("Starting db seed with strategy {}", options.Value.Strategy);
        
        using var context = dbContextFactory.CreateDbContext();

        if (opts.Strategy is DbSeedStrategy.PurgeFirst)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
        
        foreach (var deleg in ctxDelegates)
        {
            deleg(context);
        }
        
        foreach (object valueSeedEntity in options.Value.SeedEntities)
        {
            var entity = context.Find(valueSeedEntity.GetType(), context.GetPrimaryKey(valueSeedEntity));

            if (opts.Strategy is DbSeedStrategy.Update)
            {
                if (entity is null)
                {
                    context.Add(valueSeedEntity);
                    context.SaveChanges();
                }
                else
                {
                    var entry = context.Entry(entity);
                    entry.CurrentValues.SetValues(valueSeedEntity);
                    entry.State = EntityState.Modified;
                    context.SaveChanges();
                }
            }

            if (opts.Strategy is DbSeedStrategy.AddOnly && entity is null)
            {
                context.Add(valueSeedEntity);
                context.SaveChanges();
            }
        }
        
        
        logger.LogInformation("Db successfully initialized");
    }
    
    
}