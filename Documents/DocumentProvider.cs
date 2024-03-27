using sip.Core;

namespace sip.Documents;

public class DocumentProvider<TDocument>
    (IDbContextFactory<AppDbContext> dbContextFactory) : IDocumentProvider<TDocument>
    where TDocument : Document
{
    public async Task<TDocument?> GetDocumentAsync(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.Set<TDocument>()
            .Include(d => d.FilesInDocuments)
            .ThenInclude(fd => fd.FileMetadata)
            .FirstOrDefaultAsync(d => d.Id == id);
        
    }
}