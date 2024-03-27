namespace sip.Forms;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public sealed class UploadFileAttribute : Attribute
{
    public string AcceptMimes { get; set; } = "*";
    public int BytesMin { get; set; } = 0;
    public int BytesMax { get; set; } = int.MaxValue;
    
    public int BytesPerFileMax { get; set; } = 1_000_000;
    public int BytesPerFileMin { get; set; } = 10;

    
    public int FilesMax { get; set; } = 10;
    public int FilesMin { get; set; } = 0;
}