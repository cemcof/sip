namespace sip.Documents.Renderers.Pdf;

public class WeasyPrintPdfRendererOptions
{
    [Required] public string WeasyprintExecutable { get; set; } = null!;
    [Required] public string BaseRequestUrl { get; set; } = null!;
}