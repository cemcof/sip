using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Caching.Memory;
using sip.Core;
using sip.Experiments.Logs;
using sip.Experiments.Model;
using sip.Messaging.Email;
using sip.Scheduling;

namespace sip.Experiments;

public class ExperimentEngine(
        ILogger<ExperimentEngine>                logger,
        IDbContextFactory<AppDbContext>          dbContextFactory,
        IMemoryCache                             memoryCache,
        IOptions<AppOptions>                     appOptions,
        TimeProvider                             timeProvider,
        ExperimentsService                       experimentsService,
        IOptionsMonitor<ScheduledServiceOptions> schedOpts,
        IOptionsMonitor<ExperimentsOptions>            experimentsOptions,
        IOptionsMonitor<InstrumentsOptions>            instrumentOptions)
    : ScheduledService(schedOpts, timeProvider, logger), IExperimentHandler
{
    private readonly IOptions<AppOptions> _appOptions   = appOptions;

    public event Action? ExperimentChanged;
    public event Action<IReadOnlyCollection<Log>>? ExperimentLogAdded;

    public Experiment CreateExperiment(Organization organization, string instrument, string technique)
    {
        // Create metadata for the experiment 
        var ex = new Experiment
        {
            InstrumentName = instrument,
            Technique      = technique,
            OrganizationId = organization.Id,
            Organization   = organization,
            Storage        = new ExperimentStorage(),
            Publication    = new ExperimentPublication(),
            DataSource     = new ExperimentDataSource(), 
            Processing = new ExperimentProcessing()
            {
                ExperimentProcessingDocuments = new List<ExperimentProcessingDocument>()
                {
                    new() { Name = nameof(ExperimentProcessing.ResultReport) },
                    new() { Name = nameof(ExperimentProcessing.LogReport) },
                },
                
            }
        };
        return ex;
    }

    public Task<Experiment> GetExperimentAsync(Guid id, CancellationToken cancellationToken)
        => experimentsService.GetExperimentAsync(id, false, cancellationToken);


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
            sourceDir = TimeProvider.DtUtcNow().ToString("yyMMdd") + "_" + sourceDir.Trim();
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
            var yearSubfolder = "DATA_" + TimeProvider.DtUtcNow().ToString("yy");
            subPath = Path.Combine(yearSubfolder, experiment.SecondaryId);
        }
        
        if (experiment.OrganizationUser is not null)
        {
            subPath = Path.Combine(experiment.OrganizationUser.LinkId, subPath);
        }
        
        experiment.Processing.SerializeWorkflow();
        experiment.Storage.SubPath  = subPath;
        experiment.Storage.DtLastUpdate = TimeProvider.DtUtcNow();
        db.Attach(experiment);
        await db.SaveChangesAsync();

        await SubmitLogAsync(experiment, LogLevel.Information,
            "Experiment start requested, waiting for processing machines...");
        await ChangeStatusAsync(ExpState.StartRequested, experiment); // TODO - not overwrite
        // _logger.LogExp(LogLevel.Information, experiment, $"Requested experiment run.");
    }

    public async Task RequestStop(Experiment experiment, ExperimentStopModel stopModel)
    { 
        await SubmitLogAsync(experiment, LogLevel.Information,
            "Experiment stop requested, waiting for processing machines...");
        
        Logger.LogDebug("Requesting stop for experiment {Id} notify={NotifyUser}, notes={Notes}",
            experiment.Id, experiment.NotifyUser, experiment.Notes);

        await using var dbctx = await dbContextFactory.CreateDbContextAsync();
        dbctx.Entry(experiment).State = EntityState.Unchanged;
        experiment.State = ExpState.StopRequested;
            
        await dbctx.SaveChangesAsync();
        OnExperimentChanged(experiment);
    }

    public ExperimentOptions GetExpConfig(Experiment exp)
    {
        var expOpts = experimentsOptions.Get(exp.OrganizationId).InstrumentJobs;
        var instOpts = instrumentOptions.Get(exp.OrganizationId);
        var expo = expOpts[instOpts.GetInstrumentByName(exp.InstrumentName)][exp.Technique];
        return expo;
    }

    public Task SendEmailNotificationAsync(Experiment exp, EmailTemplateOptions template)
        => experimentsService.SendEmailNotificationAsync(exp, template);

    
    public  Task ChangeStatusAsync(ExpState to, Experiment forExp)
    {
        var jsonPatch = new JsonPatchDocument<Experiment>();
        jsonPatch.Replace(e => e.State, to);
        return PatchExperimentAsync(forExp, jsonPatch, CancellationToken.None);
    }

    
    public Task ChangeStorageStatusAsync(StorageState to, Experiment forExp)
    {   
        var jsonPatch = new JsonPatchDocument<Experiment>();
        jsonPatch.Replace(e => e.Storage.State, to);
        return PatchExperimentAsync(forExp, jsonPatch, CancellationToken.None);
    }
    
    public Task ChangeProcessingStatusAsync(ProcessingState to, Experiment forExp)
    {   
        var jsonPatch = new JsonPatchDocument<Experiment>();
        jsonPatch.Replace(e => e.Processing.State, to);
        return PatchExperimentAsync(forExp, jsonPatch, CancellationToken.None);
    }
    
    public Task ChangePublicationStatusAsync(PublicationState to, Experiment forExp)
    {   
        var jsonPatch = new JsonPatchDocument<Experiment>();
        jsonPatch.Replace(e => e.Publication.State, to);
        return PatchExperimentAsync(forExp, jsonPatch, CancellationToken.None);
    }

    public async Task ChangeStatusAsync(ExpState to, Guid expId, CancellationToken ct = default)
    {
        await ChangeStatusAsync(
            to,
            await GetExperimentAsync(expId, ct)
        );
    }

    
    protected override Task ExecuteRoundAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    protected virtual void OnExperimentChanged(Experiment? exp)
    {
        if (exp is not null)
        {
            // Invalidate caches
            memoryCache.Remove(exp.Id);
            memoryCache.Remove(exp.KeyIdentif);
        }
        
        ExperimentChanged?.Invoke();
    }

    // TODO - this should be probably refactored into asp net core logging infrastructure
    public async Task SubmitLogAsync(
        Experiment        exp,
        LogLevel          level,
        string            message,
        CancellationToken ct = default)
    {
        var log = new Log
        {
            Id           = Guid.NewGuid(),
            Dt           = DateTime.UtcNow,
            ExperimentId = exp.Id,
            Message      = message,
            Origin       = "LIMS Webserver",
            Level        = level
        };
        
        await SubmitLogsAsync([log], ct);
    }

    public async Task SubmitLogsAsync(List<Log> logs, CancellationToken ct)
    {
        if (!logs.Any()) return;

        await using var context = await dbContextFactory.CreateDbContextAsync(ct);
        
        // For now, UPSERT logs one by one 
        var logsSet = context.Set<Log>();
        
        foreach (var log in logs)
        {
            try
            {
                var hasLog = await logsSet.CountAsync(l => log.Id == l.Id, cancellationToken: ct);
                if (hasLog == 0)
                {
                    logsSet.Add(log);
                }
                else
                {
                    logsSet.Update(log);
                }

                await context.SaveChangesAsync(ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to save experiment log: {@Log}", log);       
            }
        }

        ExperimentLogAdded?.Invoke(logs);
    }

    public async Task RequestPublicationAsync(Experiment exp, CancellationToken ct = default)
    {
        var patch = new JsonPatchDocument<Experiment>();
        if (exp.Storage.State is not (StorageState.Archived or StorageState.Archiving))
        {
            // We must also request archivation
            patch.Replace(p => p.Storage.State, StorageState.ArchivationRequested);
        }
        
        patch.Replace(p => p.Publication.State, PublicationState.PublicationRequested);
        await PatchExperimentAsync(exp, patch, ct);
    }

    public async Task PatchExperimentAsync(Guid experimentId, JsonPatchDocument<Experiment> data, CancellationToken ct = default)
    {
        var exp = await GetExperimentAsync(experimentId, ct);
        await PatchExperimentAsync(exp, data, ct);
    }
    
    public async Task PatchExperimentAsync(Experiment exp, JsonPatchDocument<Experiment> data, CancellationToken ct)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(ct);
        context.Attach(exp);
        logger.LogDebug("Patching experiment {} by JsonPatch ApplyTo {}, \n state={}", 
            exp.SecondaryId, string.Join(';', data.Operations.Select(o => $"op={o.op} path={o.path} value={o.value}")), exp.State);
        data.ApplyTo(exp);
        logger.LogDebug("Patched experiment {}, state={}", exp.SecondaryId, exp.State);
        await context.SaveChangesAsync(ct);
        OnExperimentChanged(exp);
    }
}