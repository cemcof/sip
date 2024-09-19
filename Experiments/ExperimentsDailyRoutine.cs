using sip.Experiments.Model;
using sip.Messaging.Email;
using sip.Scheduling;

namespace sip.Experiments;

public class ExperimentsDailyRoutine(IOptionsMonitor<ScheduledServiceOptions> optionsMonitor, TimeProvider timeProvider, 
    ILogger<ExperimentsDailyRoutine> logger,
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
        // Sent email notification on experiments about to expire
        await _SafeNotifyExpiringExperiments(stoppingToken);
    }

    private async Task _SafeNotifyExpiringExperiments(CancellationToken ct)
    {
        EmailTemplateOptions? NotifyTodayProvider(Experiment e)
        {
            var expOpts = experimentsOptions.Get(e.OrganizationId)
                .FindExpOpts(e.InstrumentName, e.Technique);
            
            var notifyDays = expOpts.NotifyDaysBeforeExpiration;
            var today = TimeProvider.DtUtcNow().Date;
            var expDate = e.Storage.DtExpiration;
            var daysToExp = (int) (expDate - today).TotalDays;
            var shouldNotify = notifyDays.Any(d => d == daysToExp);
            logger.LogDebug("Exp {ExpId} {Instrument}/{Technique} should notify={ShouldNotify}, \n" +
                            "expDate={ExpDate}, \n" +
                            "daysToExp={DaysToExp}, \n" +
                            "notifyDays={@NotifyDays}", 
                e.SecondaryId, e.InstrumentName, e.Technique, shouldNotify, expDate, daysToExp, notifyDays);
            if (shouldNotify)
                return expOpts.ExpirationNotifyEmail ??
                       throw new InvalidOperationException("Expiration mail template not configured");
            return null;
        }

        try
        {
            var expsInIdleStorageState =
                await experimentsService.GetExperimentsAsync(new ExperimentsFilter(
                    StorageStates: [StorageState.Idle],
                    CustomFilter: e => !e.Storage.Archive));
            foreach (var exp in expsInIdleStorageState.Items)
            {
                var template = NotifyTodayProvider(exp);
                if (template is null) continue;
                await experimentsService.SendEmailNotificationAsync(exp, template, ct);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to notify expiring experiments");
        }

    }

    private async Task _SafeCleanLogs(CancellationToken stoppingToken)
    {
        // Given experiment, determine when it is old enough (according to configuration) so that it's logs can be deleted  
        DateTime OlderThanProvider(Experiment e)
        {
            var delAfter = experimentsOptions.Get(e.OrganizationId)
                .FindExpOpts(e.InstrumentName, e.Technique)
                .CleanLogsAfter;
            return delAfter.HasValue ? TimeProvider.DtUtcNow() - delAfter.Value : DateTime.MinValue;
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