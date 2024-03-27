// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace sip.Forms;

public interface IFilesystemItemInfo
{
    public string Path { get; }
    public string? Name { get; }
    public bool IsDirectory { get; }
    public bool IsAccessible { get; }
}

public class FileSystemItemInfo : IFilesystemItemInfo
{
    public string Path { get; set; } = null!;
    public string? Name { get; set; }
    public bool IsDirectory { get; set; }

    public bool IsAccessible => true;

}