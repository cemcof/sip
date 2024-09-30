using System.Collections.Concurrent;
using System.Text.Json;
using sip.Core;

namespace sip.Experiments.RemoteFilesystem;

// public class EntityFileSystemMaps : IEntityTypeConfiguration<FileSystemMap>
// {
//     public void Configure(EntityTypeBuilder<FileSystemMap> builder)
//     {
//     }
// }

public class DbRemoteFilesystemService(IDbContextFactory<AppDbContext> dbContextFactory) : IFilesystemProvider
{
    private readonly ConcurrentDictionary<Guid, (TaskCompletionSource<List<FileSystemItemInfo>>, CancellationToken)> _requests = new();


    public async Task<List<FileSystemItemInfo>> RequestDirectoryInfoAsync(string path, string? scope = null, TimeSpan timeout = default, CancellationToken cts = default)
    {
        await using var dbctx = await dbContextFactory.CreateDbContextAsync(cts);
        
        // Submit the path request and check if result is ready from previous request
        var fsMap = await dbctx.Set<FileSystemMap>()
            .FirstOrDefaultAsync(fsm => fsm.SourcePath == path && fsm.Scope == scope, cancellationToken: cts);

        var cached = false;
        
        if (fsMap is null)
        {
            fsMap = new FileSystemMap() {RequestSubmit = true, Scope = scope, SourcePath = path};
            dbctx.Add(fsMap);
        }
        else if (!fsMap.RequestSubmit) // TODO - caching by time
        {
            cached = true;
        }
        
        fsMap.RequestSubmit = true;
        await dbctx.SaveChangesAsync(cts);
        

        // We dont have a result - actively wait for the result to come - poll for it...
        if (timeout == default)
        {
            timeout = TimeSpan.FromSeconds(10);
        }
        
        var timeoutAfter = DateTime.UtcNow + timeout;
        
        do
        {
            await dbctx.Entry(fsMap).ReloadAsync(cts);
            if (!string.IsNullOrEmpty(fsMap.Results) && (!fsMap.RequestSubmit || cached))
            {
                // Deserialize and return
                return JsonSerializer.Deserialize<List<FileSystemItemInfo>>(fsMap.Results) ?? new List<FileSystemItemInfo>();
            }

            await Task.Delay(TimeSpan.FromMilliseconds(300), cts);
        } while (DateTime.UtcNow < timeoutAfter); // TODO - better loop handling?

        throw new TimeoutException();
    }
}