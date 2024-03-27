using System.Collections.Concurrent;

namespace sip.Utils;

public abstract class BackgroundQueue<TItem>(TimeSpan interval) : IHostedService, IDisposable
{
    protected readonly ConcurrentQueue<TItem> ConcurrentQueue = new();
    private            Timer?                 _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(_ => Process(), null, interval * 2, 
            interval);
        return Task.CompletedTask;
    }

    protected virtual void Add(TItem item)
    {
        ConcurrentQueue.Enqueue(item);
    }

    protected abstract void Process();

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        Process(); // Make one last processing before quitting
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}