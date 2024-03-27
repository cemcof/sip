using System.Collections.Concurrent;
using Microsoft.Extensions.Internal;
using sip.Forms;
using sip.Scheduling;

namespace sip.Experiments.RemoteFilesystem;

public class RemoteFilesystemApiService(
        IOptionsMonitor<ScheduledServiceOptions> optionsMonitor,
        ISystemClock                             systemClock,
        ILogger<RemoteFilesystemApiService>      logger)
    : ScheduledService(optionsMonitor, systemClock, logger), IFilesystemProvider
{
    private readonly ConcurrentDictionary<Guid, PathRequest> _requests = new();

    public record PathRequest(
        Guid                                           RequestId,
        CancellationToken                              CancellationToken,
        DateTime                                       NotAfter,
        TaskCompletionSource<List<FileSystemItemInfo>> ResultHandler,
        string                                         Path,
        string?                                        Scope);

    public Task<List<FileSystemItemInfo>> RequestDirectoryInfoAsync(
        string            path,
        string?           scope   = null,
        TimeSpan          timeout = default,
        CancellationToken cts     = default)
    {
        if (timeout == default)
        {
            timeout = TimeSpan.FromSeconds(10);
        }

        var reqid = Guid.NewGuid();

        // Save request to memory
        var pathRequest = new PathRequest(
            RequestId: reqid,
            CancellationToken: cts,
            NotAfter: DateTime.UtcNow + timeout,
            ResultHandler: new TaskCompletionSource<List<FileSystemItemInfo>>(),
            Path: path,
            Scope: scope
        );

        _requests[reqid] = pathRequest;
        return pathRequest.ResultHandler.Task;
    }

    public IDictionary<Guid, PathRequest> GetPendingPaths()
    {
        return _requests;
    }

    public void SubmitResults(Dictionary<Guid, List<FileSystemItemInfo>> results)
    {
        foreach (var (reqid, result) in results)
        {
            if (_requests.TryRemove(reqid, out var pathRequest))
            {
                pathRequest.ResultHandler.SetResult(result);
            }
        }
    }

    protected override Task ExecuteRoundAsync(CancellationToken stoppingToken)
    {
        // Check if any requests are expired and throw timeout exception on them, or if any are cancelled and cancel them
        foreach (var pathRequest in _requests.Values)
        {
            if (pathRequest.CancellationToken.IsCancellationRequested)
            {
                pathRequest.ResultHandler.SetCanceled(pathRequest.CancellationToken);
                _requests.TryRemove(pathRequest.RequestId, out _);
                continue;
            }

            if (pathRequest.NotAfter < DateTime.UtcNow)
            {
                pathRequest.ResultHandler.SetException(new TimeoutException());
                _requests.TryRemove(pathRequest.RequestId, out _);
            }
        }

        return Task.CompletedTask;
    }
}