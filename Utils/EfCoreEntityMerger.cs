using sip.Core;

namespace sip.Utils;

public interface IEntityMerger<TDbContext>  where  TDbContext : DbContext
{
    Task MergeEntitiesAsync<TEntity>(TEntity from, TEntity to) where TEntity : class;
}

public class RawSqlEntityMerger<TDbContext>(
        IDbContextFactory<TDbContext>           dbContextFactory,
        ILogger<RawSqlEntityMerger<TDbContext>> logger)
    : IEntityMerger<TDbContext>
    where TDbContext : DbContext
{
    
    
    public async Task MergeEntitiesAsync<TEntity>(TEntity from, TEntity to) where TEntity : class
    {
        // Chatgpt 
        using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var entityType = dbContext.Model.FindEntityType(typeof(TEntity)) ?? throw new InvalidOperationException();
        var primaryKey = entityType.FindPrimaryKey() ?? throw new InvalidOperationException();
        var primaryKeyProperty = primaryKey.Properties.First() ?? throw new InvalidOperationException();
        var fromEntityKeyValue = primaryKeyProperty.PropertyInfo.GetValue(from);
        var toEntityKeyValue = primaryKeyProperty.PropertyInfo.GetValue(to);

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
                var tblName = dbContext.Model.FindEntityType(dependentEntityType).GetTableName();
                var schema = "public";
                var fkColumnName = fkProperty.GetColumnName();

                var sql = $"""UPDATE {schema}"{tblName}" SET {fkColumnName} = @toEntityKeyValue WHERE {fkColumnName} = @fromEntityKeyValue""";
                await dbContext.Database.ExecuteSqlRawAsync(sql, toEntityKeyValue, fromEntityKeyValue);
            }

            dbContext.Remove(from);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return;
        
        
        if (from.Equals(to)) throw new ArgumentException("Entities being merged cannot be equal");
        
        var db = await dbContextFactory.CreateDbContextAsync();
        var etype = typeof(TEntity);
        var fromPk = db.GetPrimaryKey(from).First() ?? throw new InvalidOperationException();
        var toPk = db.GetPrimaryKey(to).First() ?? throw new InvalidOperationException();
        
        var relMode = db.Model.GetRelationalModel();
        var eMeta = db.Model.FindEntityType(etype) ?? throw new InvalidOperationException();
        var tableName = eMeta.GetTableName() ?? throw new InvalidOperationException();
        var table = relMode.FindTable(tableName, null);

        logger.LogDebug("Running transaction for entity merge: {}={}, {}={}, {}={}, {}={}", nameof(etype), etype.FullName,
            nameof(fromPk), fromPk.ToString(), nameof(toPk), toPk.ToString(), nameof(tableName), tableName);
        await using var tsc = await db.Database.BeginTransactionAsync();
        
        foreach (var tb in relMode.Tables)
        {
            foreach (var fkc in tb.ForeignKeyConstraints)
            {
                if (fkc.PrincipalTable.Name != tableName) continue;

                var principalColName = fkc.PrincipalColumns.Single().Name;
                var referencingColName = fkc.Columns.Single().Name;
                var referencingTableName = "public." + tb.SchemaQualifiedName; // TODO
                var fromPkStr = fromPk.ToString();
                var toPkStr = toPk.ToString();
                // TODO - handle pk argument better
                var upd = $"UPDATE {referencingTableName} SET {referencingColName} = '{toPkStr}' WHERE {referencingColName} = '{fromPkStr}';";
                logger.LogDebug("Executing prepared command: {}", upd);
                
                await db.Database.ExecuteSqlRawAsync(upd);
            }
        }

        await tsc.CommitAsync();
        

        // Lastly and finally, delete the entity being merged
        db.Remove(from);
        await db.SaveChangesAsync();
    }
}