using Microsoft.Extensions.FileProviders;
using sip.Core;
using sip.Documents.Proposals;
using sip.Documents.Renderers;
using sip.Documents.Renderers.Handlebars;
using sip.Documents.Renderers.Pdf;
using sip.Documents.Renderers.Word;

namespace sip.Documents;

public class DocumentService(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IServiceProvider serviceProvider,
    ILogger<DocumentService> logger,
    HandlebarsService handlebarsService,
    MsWordRenderer msWordRenderer,
    EmbeddedFileProvider embeddedFilesProvider,
    TimeProvider timeProvider,
    WeasyPrintPdfRenderer pdfRenderer,
    ZipArchiver archiver,
    DocSelfHttp docSelfHttp)
{
    public HandlebarsService HandlebarsService { get; } = handlebarsService;

    private readonly ZipArchiver _archiver = archiver;


    public async Task<TDocument?> GetDocumentAsync<TDocument>(Guid id, CancellationToken ct = default)
        where TDocument : Document
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var p = await db.Set<TDocument>()
            .Include(d => d.Organization)
            .Include(d => d.FilesInDocuments)
            .ThenInclude(d => d.FileMetadata)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken: ct);

        return p;
    }

    public async Task LoadDocumentAsync<TDocument>(TDocument doc, CancellationToken ct = default)
        where TDocument : Document
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var docEntry = db.Attach(doc);
        // docEntry.State = EntityState.Unchanged;

        // doc.FilesInDocuments = new List<FileInDocument>();
        await docEntry // For some reason, filesindocument is not cleared
            .Collection(d => d.FilesInDocuments)
            .Query()
            .Include(fd => fd.FileMetadata)
            .LoadAsync(ct);
    }


    public async Task<TDocument?> GetProposalDocumentAsync<TDocument>(Guid id) where TDocument : Proposal
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var p = await db.Set<TDocument>()
            .Include(d => d.Organization)
            .Include(d => d.FilesInDocuments)
            .ThenInclude(d => d.FileMetadata)
            .Include(d => d.EvaluatedBy)
            .Include(d => d.ExpectedEvaluator)
            .FirstOrDefaultAsync(p => p.Id == id);

        return p;
    }

    public Task<string> ReadEmbeddedFileAsync(string target) => embeddedFilesProvider.ReadAsTextAsync(target);

    public async Task<IEnumerable<FileMetadata>> GetDocumentFilesAsync(
        Guid id,
        DocumentFileType? documentFileType = null)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var result = await db.Set<FileInDocument>()
            .Include(fm => fm.FileMetadata)
            .Where(fm => fm.DocumentId == id)
            .ToListAsync();

        if (documentFileType is not null)
            result = result.Where(f => f.DocumentFileType == documentFileType.Value).ToList();

        return result.Select(d => d.FileMetadata);
    }


    public IDocRenderer? GetRendererByFileName(string filename)
    {
        var ext = Path.GetExtension(filename);
        if (ext == ".hbs" || ext == ".html")
        {
            // For these we use handlebars renderer
            return HandlebarsService;
        }

        if (ContentType.Parse(MimeKit.MimeTypes.GetMimeType(filename)).IsMsWord())
        {
            // MS word renderer 
            return msWordRenderer;
        }

        return null;
    }

    public async Task<TDocument> CreateDocumentAsync<TDocument>() where TDocument : Document, new()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var propo = new TDocument();
        ctx.Set<TDocument>().Add(propo);
        await ctx.SaveChangesAsync();
        return propo;
    }

    /// <summary>
    /// Get type of a document that exist and is identified by given id
    /// </summary>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public async Task<Type> GetDocumentTypeAsync(Guid documentId)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var doc = await db.Set<Document>().FindAsync(documentId);
        return doc?.GetType() ?? throw new ArgumentException("Document does not exist");
    }

    public async Task UpdateDocument<TDocument>(TDocument document)
        where TDocument : Document
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var entr = db.Entry(document);
        entr.State = EntityState.Modified;
        await db.SaveChangesAsync();
    }

    public async Task<FileMetadata> GetFileAsync(Guid fileId)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var fmeta = await db.Set<FileMetadata>()
            .Include(fm => fm.FileData)
            .FirstAsync(fd => fd.Id == fileId);
        return fmeta;
    }

    public async Task<FileMetadata> SaveDocumentFileAsync(
        Document document,
        FileMetadata fileMetadata,
        Stream fileStream,
        DocumentFileType fileType,
        bool archiveExisting = true,
        bool appendMode = false
    )
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        // var docentry = db.Entry(document);
        var docentry = db.Attach(document);
        // TODO - is this correct approach?
        // docentry.State = EntityState.Unchanged;
        await docentry.Collection(d => d.FilesInDocuments)
            .Query()
            .Include(c => c.FileMetadata)
            .LoadAsync();
        
        foreach (var documentFilesInDocument in document.FilesInDocuments)
        {
            logger.LogDebug("FID state: {}", db.Entry(documentFilesInDocument));
        }

        logger.LogDebug("Finding doc meta: {} in {}", fileMetadata.FileName, 
            string.Join(',', document.FilesInDocuments.Select(f => f.FileMetadata.FileName)));
        var tf = document.FilesInDocuments.FirstOrDefault(
            f => f.FileMetadata.FileName == fileMetadata.FileName,
            new FileInDocument {FileMetadata = new()}
        );

        if (fileType == DocumentFileType.Primary && archiveExisting)
        {
            foreach (var primdoc in document.FilesInDocuments.Where(f =>
                         f.DocumentFileType == DocumentFileType.Primary))
            {
                // Archive other primary documents
                primdoc.DocumentFileType = DocumentFileType.Archived;
            }
        }
        
        var isNewFile = tf.Id == default;
        await using var memoryStream = new MemoryStream();
        if (!isNewFile)
        {
            await LoadFileData(tf.FileMetadata);
            if (appendMode)
            {
                var existingData = tf.FileMetadata.FileData!.Data;
                memoryStream.Write(existingData);
                logger.LogDebug("Appending to existing file ({ExistingBytes} bytes) {FileName} {Length} bytes",
                    existingData.Length, tf.FileMetadata.FileName, fileStream.Length);
            }
        }
        
        await fileStream.CopyToAsync(memoryStream);
        var fbytes = memoryStream.ToArray();

        tf.FileMetadata.FileName = fileMetadata.FileName;
        tf.FileMetadata.ContentType = fileMetadata.ContentType;

        tf.FileMetadata.DtModified = fileMetadata.DtModified == default
            ? timeProvider.DtUtcNow()
            : fileMetadata.DtModified;
        
        tf.FileMetadata.Length = fbytes.Length;
        tf.DocumentFileType = fileType;

        if (isNewFile)
        {
            tf.FileMetadata.DtCreated = fileMetadata.DtCreated == default
                ? timeProvider.DtUtcNow()
                : fileMetadata.DtCreated;
            tf.FileMetadata.FileData = new FileData() {Data = fbytes};
            document.FilesInDocuments.Add(tf);
        }
        else
        {
            tf.FileMetadata.FileData!.Data = fbytes;
        }
    
        logger.LogDebug("Saving file ({FileMetaId}) to database {FileName} {Length} bytes, DtModified={}, isNew={IsNew}", 
            tf.FileMetadata.Id, tf.FileMetadata.FileName, fbytes.Length, tf.FileMetadata.DtModified, isNewFile);
        logger.LogDebug("TF track state: {} {} orig={}, curr={}", db.Entry(tf), db.Entry(tf.FileMetadata),
            db.Entry(tf).OriginalValues, db.Entry(tf).CurrentValues);
        await db.SaveChangesAsync();
        return tf.FileMetadata;
    }

    public async Task LoadFileData(FileMetadata fileMetadata)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var entry = db.Entry(fileMetadata);
        entry.State = EntityState.Unchanged;
        await entry.Reference(f => f.FileData)
            .LoadAsync();
    }

    public async Task<FileMetadata> RenderToPdfAsync(Document doc, string? name = null, bool skipIfExists = true)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        // Do we have primary PDF file? 
        var existing = doc.ActivePrimaryFiles.FirstOrDefault(a => a.FileMetadata.ContentType.IsPdf());
        if (existing is not null)
        {
            var existingEntry = db.Entry(existing);
            existingEntry.State = EntityState.Unchanged;
            await existingEntry.Reference(e => e.FileMetadata).LoadAsync();
            name ??= existing.FileMetadata.FileName;
            if (skipIfExists)
            {
                await LoadFileData(existing.FileMetadata);
                return existing.FileMetadata;
            }
        }


        // Self request to own endpoint in order to fetch html for pdf rendering
        name ??= "document";
        var html = await docSelfHttp.GetDocHtml(doc.Id);
        logger.LogDebug("Passing following html for PDF rendering: \n{}", html);
        var pdfData = await pdfRenderer.RenderHtmlToPdfAsync(html);

        var resultingDocMeta = await SaveDocumentFileAsync(doc, new FileMetadata()
        {
            ContentType = new ContentType("application", "pdf"),
            DtCreated = timeProvider.DtUtcNow(), FileName = name
        }, pdfData, DocumentFileType.Primary);

        return resultingDocMeta;
    }

    public Task<FileMetadata> RenderToZipAsync(bool skipIfExists = true)
    {
        // _pdfRenderer.

        throw new NotImplementedException();
    }

    public DocumentRenderInfo? GetDocumentRenderInfo(Document document)
    {
        object rendInfoProvider =
            serviceProvider.GetService(typeof(IDocumentRenderInfoProvider<>).MakeGenericType(document.GetType()));
        if (rendInfoProvider is null) return null;
        dynamic result = rendInfoProvider.GetType()
            .GetMethod(nameof(IDocumentRenderInfoProvider<Document>.GetRenderInfo), new[] {document.GetType()})!
            .Invoke(rendInfoProvider, new object?[] {document})!;
        return result;
    }

    public async Task RemoveFileAsync(FileMetadata metadata)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        db.Entry(metadata).State = EntityState.Deleted;
        await db.SaveChangesAsync();

        var fileData = new FileData() {Id = metadata.FileDataId};
        db.Attach(fileData);
        db.Remove(fileData);
        await db.SaveChangesAsync();
    }
}