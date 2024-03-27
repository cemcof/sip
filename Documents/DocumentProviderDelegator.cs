using sip.Core;

namespace sip.Documents;

public class DocumentProviderDelegator(
        IDbContextFactory<AppDbContext>    dbContextFactory,
        ILogger<DocumentProviderDelegator> logger,
        IServiceProvider                   serviceProvider)
    : IDocumentProvider
{
    public async Task<Document?> GetDocumentAsync(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        
        // TODO - it is important to test whether auto type determination will work
        // otherwise, we have to use discriminator manually
        var p = await db.Set<Document>().FirstOrDefaultAsync(p => p.Id == id);

        if (p is not null)
        {
            logger.LogDebug("Delegator loaded project of type {} for id {}", p.GetType().Name, id);
            
            var loader = serviceProvider.GetService(typeof(IDocumentProvider<>).MakeGenericType(p.GetType()));
            if (loader is not null)
            {
                dynamic task = loader.DynamicInvoke(nameof(IDocumentProvider<Document>.GetDocumentAsync),
                    typeof(Task<>).MakeGenericType(p.GetType()), new object[] {id})!;
                return await task;
            }
            else
            {
                // We just load the document normally - just itself and metadata from db
                var result= await db.Set<Document>()
                    .Include(d => d.FilesInDocuments)
                    .ThenInclude(df => df.FileMetadata)
                    .FirstOrDefaultAsync(d => d.Id == id);
                return result;
            }
        }

        return null;
    }
}