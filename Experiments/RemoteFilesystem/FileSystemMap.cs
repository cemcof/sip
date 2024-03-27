namespace sip.Experiments.RemoteFilesystem;

public class FileSystemMap
{
    public Guid Id { get; set; }

    public string? Scope      { get; set; }
    public string  SourcePath { get; set; } = null!;
    public string? Results    { get; set; }

    public DateTime DtLastSubmitted { get; set; }
    public bool     RequestSubmit   { get; set; }
}