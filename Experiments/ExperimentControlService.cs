using sip.Core;
using sip.Experiments.Model;

namespace sip.Experiments;

public class ExperimentControlService(
    TimeProvider timeProvider,
    IDbContextFactory<AppDbContext> dbContextFactory,
    ExperimentsService experimentsService,
    ExperimentLoggingService experimentLoggingService,
    ILogger<ExperimentControlService> logger)
{
    public async Task RequestStart(Experiment experiment)
    {
        // Requesting start - lets save experiment to the db
        await using var db = await dbContextFactory.CreateDbContextAsync();

        experiment.DtCreated = DateTime.UtcNow;

        // Set publication embargo date and expiration date
        experiment.Publication.DtEmbargo = experiment.Publication.EmbargoPeriod switch
        {
            var period when period != default => (DateTime.UtcNow + period).Date,
            _ => default // Default if EmbargoPeriod is not set
        };

        experiment.Storage.DtExpiration = experiment.Storage.ExpirationPeriod switch
        {
            var period when period != default => (DateTime.UtcNow + period).Date,
            _ => default // Default if ExpirationPeriod is not set
        };


        // We have to ensure that the reference is same in case users are the same - otherwise EF complains
        if (experiment.User.Id == experiment.Operator.Id)
        {
            experiment.User = experiment.Operator;
        }

        // Set secondary ID and supath - refactor
        var sourceDir = PathLib.GetName(experiment.DataSource.SourceDirectory);
        // Check if sourceDir starts with at least two digits
        if ( ! ( sourceDir.Length > 2 && sourceDir[..2].All(char.IsDigit)) )
        {
            // If not, use current date as prefix
            sourceDir = timeProvider.DtUtcNow().ToString("yyMMdd") + "_" + sourceDir.Trim();
        }
        
        experiment.SecondaryId = sourceDir;
        string subPath;
        
        if (experiment.Project is not null)
        {
            var projN = (string.IsNullOrEmpty(experiment.Project.Acronym)) 
                ? experiment.Project.Title : experiment.Project.Acronym;
            projN = projN.ToSafeFilename();
            if (string.IsNullOrWhiteSpace(projN))
                throw new InvalidOperationException($"Project name or acronym is invalid, cannot be used in path, " +
                                                    $"{experiment.Project.Acronym} {experiment.Project.Title}");
            subPath = Path.Combine("projects", projN, experiment.SecondaryId);
        }
        else
        {
            var yearSubfolder = "DATA_" + timeProvider.DtUtcNow().ToString("yy");
            subPath = Path.Combine(yearSubfolder, experiment.SecondaryId);
        }
        
        if (experiment.OrganizationUser is not null)
        {
            subPath = Path.Combine(experiment.OrganizationUser.LinkId, subPath);
        }
        
        // Ensure unique subpath
        subPath = await experimentsService.GetUniqueSubpathAsync(
            subPath, 
            experiment.Id.ToString()[^7..], 
            experiment.Organization);
        
        experiment.Processing.SerializeWorkflow();
        experiment.Storage.SubPath  = subPath;
        experiment.Storage.DtLastUpdate = timeProvider.DtUtcNow();
        db.Attach(experiment);
        await db.SaveChangesAsync();

        await experimentLoggingService.SubmitLogAsync(experiment, LogLevel.Information,
            "Experiment start requested, waiting for processing machines...");
        await experimentsService.ChangeStatusAsync(ExpState.StartRequested, experiment); // TODO - not overwrite
        // _logger.LogExp(LogLevel.Information, experiment, $"Requested experiment run.");
    }

    public async Task RequestStop(Experiment experiment, ExperimentStopModel stopModel)
    { 
        await experimentLoggingService.SubmitLogAsync(experiment, LogLevel.Information,
            "Experiment stop requested, waiting for processing machines...");
        
        logger.LogDebug("Requesting stop for experiment {Id} notify={NotifyUser}, notes={Notes}",
            experiment.Id, experiment.NotifyUser, experiment.Notes);

        await using var dbctx = await dbContextFactory.CreateDbContextAsync();
        dbctx.Entry(experiment).State = EntityState.Unchanged;
        experiment.State = ExpState.StopRequested;
            
        await dbctx.SaveChangesAsync();
        experimentsService.OnExperimentChanged(experiment);
    }

}