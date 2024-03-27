using Microsoft.AspNetCore.Mvc;
using MimeTypes;
using sip.Core;
using sip.Experiments.Model;

// FIXME: Bad dependency!!!

namespace sip.Experiments;

public class ExperimentsDataController(IDbContextFactory<AppDbContext> dbContextFactory) : ControllerBase
{
    [HttpGet("/data/{expid:guid}")]
    public async Task<IActionResult> DownloadTransferArchive(Guid expid)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var exp = await db.Set<Experiment>().FirstAsync(e => e.Id == expid);
        // Prepare file path
        var path = exp.Storage.Target;
        if (path is null)
            throw new InvalidOperationException();
        var fname = Path.GetFileName(path);
        var ext = Path.GetExtension(path);

        var fstream = System.IO.File.OpenRead(path);
        return File(fstream, MimeTypeMap.GetMimeType(ext), fname);
    }
}