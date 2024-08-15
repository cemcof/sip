namespace sip.Utils;

public interface IEntityMerger<TDbContext>  where  TDbContext : DbContext
{
    Task MergeEntitiesAsync<TEntity>(TEntity from, TEntity to) where TEntity : class;
}

public class EntityMergerOptions 
{
    public string MergeUpdateTemplate { get; set; } = "UPDATE \"{0}\" SET \"{1}\" = @p0 WHERE \"{1}\" = @p1";
}

public class RawSqlEntityMerger<TDbContext>(
        IDbContextFactory<TDbContext>           dbContextFactory,
        ILogger<RawSqlEntityMerger<TDbContext>> logger,
        IOptionsMonitor<EntityMergerOptions> options)
    : IEntityMerger<TDbContext>
    where TDbContext : DbContext
{
    
    
    public async Task MergeEntitiesAsync<TEntity>(TEntity from, TEntity to) where TEntity : class
    {
        using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var entityType = dbContext.Model.FindEntityType(typeof(TEntity)) ?? 
                         throw new InvalidOperationException($"Entity type {typeof(TEntity).FullName} not found in the model.");
        var primaryKey = entityType.FindPrimaryKey() ?? 
                         throw new InvalidOperationException($"Primary key not found for entity type {typeof(TEntity).FullName}.");
        var primaryKeyProperty = primaryKey.Properties.Single();
        var fromEntityKeyValue = primaryKeyProperty.PropertyInfo?.GetValue(from);
        if (fromEntityKeyValue is null)
            throw new InvalidOperationException($"Primary key value for FROM entity type {typeof(TEntity).FullName} must not be null.");
        var toEntityKeyValue = primaryKeyProperty.PropertyInfo?.GetValue(to);
        if (toEntityKeyValue is null)
            throw new InvalidOperationException($"Primary key value for TO entity type {typeof(TEntity).FullName} must not be null.");

        var foreignKeyProperties = dbContext.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .Where(fk => fk.PrincipalEntityType.ClrType == typeof(TEntity))
            .SelectMany(fk => fk.Properties)
            .ToList();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            foreach (var fkProperty in foreignKeyProperties)
            {
                var dependentEntityType = fkProperty.DeclaringEntityType.ClrType;
                var tblName = dbContext.Model.FindEntityType(dependentEntityType)?.GetTableName()
                              ?? throw new InvalidOperationException(
                                  $"Table name not found for entity type {dependentEntityType.FullName}.");
                
                var fkColumnName = fkProperty.GetColumnName();
                
                var sql = string.Format(options.CurrentValue.MergeUpdateTemplate, tblName, fkColumnName);
                await dbContext.Database.ExecuteSqlRawAsync(sql, toEntityKeyValue, fromEntityKeyValue);
            }

            dbContext.Entry(from).State = EntityState.Deleted;
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}