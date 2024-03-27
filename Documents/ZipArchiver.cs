using System.IO.Compression;

namespace sip.Documents;

public class ZipArchiver
{
    public async Task<MimePart> Archive(IEnumerable<MimePart> mimeParts, string archiveName)
    {
        var archiveStream = new MemoryStream();
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true);
        
        // Now add each file to the archive
        foreach (var mp in mimeParts)
        {
            var archiveFile = archive.CreateEntry(mp.FileName, CompressionLevel.Optimal);
            await using var targetArchiveFileStream = archiveFile.Open();
            await mp.Content.DecodeToAsync(targetArchiveFileStream);
        }
        
        // Now zip is ready, wrap the stream in a mime part
        archiveStream.Position = 0;
        var result = new MimePart()
        {
            Content = new MimeContent(archiveStream),
            FileName = archiveName,
            ContentType = {MediaType = "application", MediaSubtype = "zip"}
        };

        return result;
    }
}