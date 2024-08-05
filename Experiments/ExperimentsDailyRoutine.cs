using sip.Experiments.Model;
using sip.Scheduling;

namespace sip.Experiments;

public class ExperimentsDailyRoutine(IOptionsMonitor<ScheduledServiceOptions> optionsMonitor, TimeProvider timeProvider, ILogger logger,
    ExperimentsService experimentsService, IOptionsMonitor<ExperimentsOptions> experimentsOptions) 
    : ScheduledService(optionsMonitor, timeProvider, logger)
{
    
    protected override async Task ExecuteRoundAsync(CancellationToken stoppingToken)
    {
        // Request expiration of experiments that are ready to be expired
        await _SafeExpireExperiments(stoppingToken);
        // Request publication of experiments that are ready to be published (after embargo period)
        await _SafePublishExperiments(stoppingToken);
        // Clean logs of experiments that are old enough
        await _SafeCleanLogs(stoppingToken);
    }

    private async Task _SafeCleanLogs(CancellationToken stoppingToken)
    {
        // Given experiment, determine when it is old enough (according to configuration) so that it's logs can be deleted  
        DateTime OlderThanProvider(Experiment e)
        {
            var delAfter = experimentsOptions.Get(e.OrganizationId)
                .FindExpOpts(e.InstrumentName, e.Technique)
                .CleanLogsAfter;
            return delAfter.HasValue ? timeProvider.DtUtcNow() - delAfter.Value : DateTime.MinValue;
        }

        try
        {
            await experimentsService.CleanLogsAsync(OlderThanProvider, ct: stoppingToken);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to clean logs");
        }
    }

    private async Task _SafePublishExperiments(CancellationToken stoppingToken)
    {
        try
        {
            await experimentsService.PublishExperiments(ct: stoppingToken);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to publish experiments");
        }
    }

    private async Task _SafeExpireExperiments(CancellationToken stoppingToken)
    {
        try
        {
            await experimentsService.ExpireExperiments(ct: stoppingToken);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to expire experiments");
        }
    }
}