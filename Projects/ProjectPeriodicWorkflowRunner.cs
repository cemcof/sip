using sip.Core;
using sip.Scheduling;

namespace sip.Projects;

/// <summary>
/// Gathers all unclosed projects from the database and runs registered workflows on them, periodically (daily, but depends on configuration).
/// </summary>
public class ProjectPeriodicWorkflowRunner(
        IOptionsMonitor<ScheduledServiceOptions> optionsMonitor,
        TimeProvider                             timeProvider,
        ILogger<ProjectPeriodicWorkflowRunner>   logger,
        IDbContextFactory<AppDbContext>          dbContextFactory,
        IProjectLoader                           projectLoader,
        IServiceProvider                         serviceProvider)
    : ScheduledService(optionsMonitor, timeProvider, logger)
{
    protected override async Task ExecuteRoundAsync(CancellationToken stoppingToken)
    {
        // TODO - some fixing !
        // One cproject handler is registered three times, figure out why
        // Also refactor - obtain projects from some service rather than from here directly
        var db = await dbContextFactory.CreateDbContextAsync(stoppingToken);
        
        // Iterate all unclosed projects and perform some shit on them
        await foreach (var pid in db.Set<Project>().Where(p => !p.Closed)
                           .Select(p => p.Id)
                           .AsAsyncEnumerable().WithCancellation(stoppingToken))
        {
            Logger.LogDebug("About to perform periodic action on {}", pid);
            
            // Load this project
            var project = await projectLoader.LoadAsync(pid);
            if (project is null) continue;

            // Now we have the project type and we can get register services for reaction to this
            dynamic handlersList = serviceProvider.GetService(
                typeof(IEnumerable<>).MakeGenericType(
                    typeof(IProjectDailyActionHandler<>).MakeGenericType(project.GetType())))!;

            foreach (var handler in handlersList)
            {
                try
                {
                    Logger.LogDebug("Invoking registered periodic action handler on {}", pid);
                    await handler.HandleDailyActionAsync((dynamic)project); // Dynamic cast must be there, otherwise CS1502
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Unhandled error during periodic action");
                }
            }
        }
    }
}