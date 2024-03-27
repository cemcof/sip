using System.Diagnostics;
using System.Text.RegularExpressions;
using MimeTypes;
using sip.Core;

namespace sip.Documents.Renderers.Pdf;

public class WeasyPrintPdfRenderer(IOptions<WeasyPrintPdfRendererOptions> options, IOptions<AppOptions> appOptions)
    : IDocRenderer
{
    private readonly WeasyPrintPdfRendererOptions _options    = options.Value;

    public async Task<MimePart> Render(MimePart source, object context)
    {
        if (!SupportsType(source.ContentType))
            throw new FormatException($"{nameof(WeasyPrintPdfRenderer)}: Unsupported source {source.ContentType}");
            
        // TODO - replace hrefs - for pdf, all relative path must become absolute
        var sourceText = await source.Content.MimeContentToString();
        var baseurl = appOptions.Value.UrlBase; // TODO
        sourceText = Regex.Replace(sourceText, "( href=\")(/.*)(\")", e => e.Groups[1] + baseurl.ToString() + e.Groups[2] + e.Groups[3]); // TODO - caution


        // We will need to open up a new process
        ProcessStartInfo processStartInfo = new ProcessStartInfo(_options.WeasyprintExecutable, $"- - -u {_options.BaseRequestUrl}")
        {
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        using Process process = new Process() { StartInfo = processStartInfo };
        process.Start();
        await process.StandardInput.WriteAsync(sourceText);
        await process.StandardInput.FlushAsync();

        var outstream = new MemoryStream(); // This has to be disposed! 
        var errorOutput = await process.StandardError.ReadToEndAsync();
        await process.StandardOutput.BaseStream.CopyToAsync(outstream);
        await process.WaitForExitAsync();
        
        var resultMp = new MimePart(MimeTypeMap.GetMimeType(".pdf"))
        {
            Content = new MimeContent(outstream)
        };

        return resultMp;
    }

    public async Task<Stream> RenderHtmlToPdfAsync(string html)
    {
        var baseurl = appOptions.Value.UrlBase; // TODO
        html = Regex.Replace(html, "(a href=\")(/.*)(\")", e => e.Groups[1] + baseurl.ToString() + e.Groups[2] + e.Groups[3]); // TODO - caution

        // We will need to open up a new process
        ProcessStartInfo processStartInfo = new ProcessStartInfo(_options.WeasyprintExecutable, $"- - -u {_options.BaseRequestUrl}")
        {
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false 
        };

        using Process process = new Process() { StartInfo = processStartInfo };
        process.Start();
        await using (var stdIn = process.StandardInput)
        {
            await stdIn.WriteAsync(html);
        }

        var outstream = new MemoryStream(); // This has to be disposed!
        await using (var stdOut = process.StandardOutput.BaseStream)
        {
            await stdOut.CopyToAsync(outstream);
        }
        
        var errorOutput = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        outstream.Position = 0;
        return outstream;
    }
    
    public bool SupportsType(ContentType contentType) =>
        contentType.IsTextual(); 
    
}