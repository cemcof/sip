using MimeTypes;

namespace sip.Utils;

public static class MimeHelper
{
    public static MimePart ToMimePart(this string str, string mimeType = "text/plain", string name = "")
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(str);
        writer.Flush();
        stream.Position = 0;
        
        return new MimePart(mimeType)
        {
            Content = new MimeContent(stream),
            FileName = name,
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64
        };  
    }
    public static MimePart FileToMimePart(string path, string name)
    {
        return new MimePart(MimeTypeMap.GetMimeType(name))
        {
            Content = new MimeContent(File.OpenRead(path)),
            FileName = name,
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64
        };  
    }

    /// <summary>
    /// Takes <see cref="IMimeContent" />, copies it to memory stream and converts it to string using UTF-8 encoding.
    /// </summary>
    /// <param name="mc"></param>
    /// <returns></returns>
    public static async Task<string> MimeContentToString(this IMimeContent mc)
    {
        await using var ms = new MemoryStream();
        await mc.DecodeToAsync(ms);
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    public static bool IsTextual(this ContentType ct) =>
         string.Equals(ct.MediaType, "text", StringComparison.OrdinalIgnoreCase);   
    public static bool IsPdf(this ContentType ct) =>
         string.Equals(ct.MimeType, "application/pdf", StringComparison.OrdinalIgnoreCase);
    
    public static bool IsImage(this ContentType ct) =>
        ct.MediaType.ToLower() == "image";
    
    public static bool IsMsWord(this ContentType ct) =>
        string.Equals(ct.MimeType, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.OrdinalIgnoreCase);


    public static string ToMimeTypeString(this ContentType contentType)
        => $"{contentType.MediaType}/{contentType.MediaSubtype}";
}