using Microsoft.AspNetCore.Components.Web.Virtualization;
using sip.Experiments.Model;

namespace sip.Experiments.Logs;

public interface IExperimentLogProvider
{
    ValueTask<ItemsProviderResult<Log>> GetLogsAsync(Experiment experiment, LogLevel minLevel, ItemsProviderRequest request, string? origin = null);
    ValueTask<List<(string, ItemsProviderDelegate<Log>)>> GetLogProvidersAsync(Experiment experiment, LogLevel minLevel, CancellationToken ct = default);
}