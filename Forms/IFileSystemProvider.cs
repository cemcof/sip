namespace sip.Forms;

public interface IFilesystemProvider
{
    public Task<List<FileSystemItemInfo>> RequestDirectoryInfoAsync(string path, string? scope = null, TimeSpan timeout = default, CancellationToken cts = default);
}