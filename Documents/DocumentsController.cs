using Microsoft.AspNetCore.Mvc;

namespace sip.Documents;


public class DocumentsController(
        DocumentService              documentService,
        IDocumentProvider            documentProvider,
        ILogger<DocumentsController> logger)
    : Controller
{
    [Route("/files/{fileMetadataId:guid}")]
    public async Task<IActionResult> RenderFile(Guid fileMetadataId)
    {
        var file = await documentService.GetFileAsync(fileMetadataId);
        
        return File(file.FileData!.Data, file.ContentType.MimeType);
    }
    
    [Route("/documents/{documentId:guid}")]
    public Task<IActionResult> RenderDocument(Guid documentId)
    {
        // If there is single primary document file, render it
        
        // If there are multiple primary files, zip them and send the zip
        
        // 

        throw new NotImplementedException();
    }
    
    
    [Route("/documents/{documentId:guid}/pdf")]
    public async Task<IActionResult> RenderDocumentPdf(Guid documentId)
    {
        var doc = await documentProvider.GetDocumentAsync(documentId);
        if (doc is null) return NotFound();
        
        var fmeta = await documentService.RenderToPdfAsync(doc);
        return File(fmeta.FileData!.Data, fmeta.ContentType.MimeType);
    }
    
    [Route("/documents/{documentId:guid}/zip")]
    public Task<IActionResult> RenderDocumentZip(Guid documentId)
    {
        throw new NotImplementedException();
    }
    
    [Route("/documents/{documentId:guid}/component")]
    public async Task<IActionResult> RenderDocumentComponent(Guid documentId)
    {
        // Get component render info for type of given document 
        var doc = await documentProvider.GetDocumentAsync(documentId);
        if (doc is null)
        {
            return BadRequest();
        }
        
        var reninfo = documentService.GetDocumentRenderInfo(doc);
        if (reninfo?.ViewRender is null)
        {
            return BadRequest();
        }
        
        var model = new DocumentHost() {RenderInfo = reninfo.Value.ViewRender.Value};
        return View("/Documents/DocumentHost.cshtml", model);
    }
    //
    // [HttpPost("/documents/{documentId:guid}/files")]
    // public async Task<IActionResult> UploadFileToDocument(Guid documentId, IFormFile file, List<IFormFile>? attachments = null)
    // { 
    //     var doc = await _documentProvider.GetDocumentAsync(documentId);
    //     if (doc is null) return NotFound();
    //
    //     async Task SaveFileHelper(IFormFile fileToSave, DocumentFileType type)
    //     {
    //         var fmeta = new FileMetadata(fileToSave.FileName, fileToSave.ContentType);
    //         await using var stream = fileToSave.OpenReadStream();
    //         await _documentService.SaveDocumentFileAsync(doc, fmeta, stream, type);
    //     }
    //     
    //     // Save primary file
    //     await SaveFileHelper(file, DocumentFileType.Primary);
    //     
    //     // Save attachments if any
    //     if (attachments is not null)
    //     {
    //         foreach (var attachment in attachments)
    //         {
    //             await SaveFileHelper(attachment, DocumentFileType.Attachment);
    //         }
    //     }
    //     
    //     return Ok();
    // }
    
    [HttpPost("/documents/{documentId:guid}/files")]
    [HttpPost("/api/documents/{documentId:guid}/files")]
    public async Task<IActionResult> UploadFileToDocument(Guid documentId, List<IFormFile> files, bool append = false)
    { 
        var doc = await documentProvider.GetDocumentAsync(documentId);
        if (doc is null) return NotFound();

        async Task SaveFileHelper(IFormFile fileToSave, DocumentFileType type)
        {
            // Content type is null? Guess it from the file extension
            var contentType = (string.IsNullOrWhiteSpace(fileToSave.ContentType)) ?
                MimeTypes.MimeTypeMap.GetMimeType(fileToSave.FileName) : 
                fileToSave.ContentType;
            
            var fmeta = new FileMetadata(fileToSave.FileName, contentType);
            await using var stream = fileToSave.OpenReadStream();
            await documentService.SaveDocumentFileAsync(doc, fmeta, stream, type, archiveExisting:false, appendMode:append);
        }
        
        // Save all files 
        logger.LogDebug("Preparing to save {} files", files.Count);
        foreach (var file in files)
        {
            logger.LogDebug("FormFile: cd={} fn={} ct={}", file.ContentDisposition, file.FileName, file.ContentType);
            await SaveFileHelper(file, file.ContentDisposition.Contains("attachment") ? DocumentFileType.Attachment : DocumentFileType.Primary);
        }
        
        return Ok();
    }
    

}