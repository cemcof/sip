using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using sip.Core;
using sip.Experiments.Logs;
using sip.Experiments.Model;
using sip.Messaging;
using sip.Messaging.Email;
using sip.Scheduling;
using sip.Utils;

namespace sip.Experiments;

public class ExperimentEngine(
        ILogger<ExperimentEngine>                logger,
        IDbContextFactory<AppDbContext>          dbContextFactory,
        IMemoryCache                             memoryCache,
        IOptions<AppOptions>                     appOptions,
        SmtpSender                               emailService,
        ISystemClock                             systemClock,
        ExperimentsService                       experimentsService,
        GeneralMessageBuilderProvider            messageBuilderProvider,
        IOptionsMonitor<ScheduledServiceOptions> schedOpts,
        IOptionsMonitor<ExperimentsOptions>            experimentsOptions,
        IOptionsMonitor<InstrumentsOptions>            instrumentOptions)
    : ScheduledService(schedOpts, systemClock, logger), IExperimentHandler
{
    private readonly IOptions<AppOptions> _appOptions   = appOptions;
    private readonly SmtpSender           _emailService = emailService;

    public event Action? ExperimentChanged;

    public Experiment CreateExperiment(string organization, string instrument, string technique)
    {
        // Create metadata for the experiment 
        var ex = new Experiment
        {
            InstrumentName = instrument,
            Technique      = technique,
            OrganizationId = organization,
            Storage        = new ExperimentStorage(),
            Publication    = new ExperimentPublication(),
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
        => experimentsService.GetExperimentAsync(id, true, cancellationToken);


    public async Task RequestStart(Experiment experiment)
    {
        // Requesting start - lets save experiment to the db
        await using var db = await dbContextFactory.CreateDbContextAsync();

        experiment.DtCreated = DateTime.UtcNow;

        // Set publication embargo date and expiration date
        experiment.Publication.DtEmbargo = (DateTime.UtcNow + experiment.Publication.EmbargoPeriod).Date;
        experiment.Storage.DtExpiration  = (DateTime.UtcNow + experiment.Storage.ExpirationPeriod).Date;


        // We have to ensure that the reference is same in case users are the same - otherwise EF complains
        if (experiment.User.Id == experiment.Operator.Id)
        {
            experiment.User = experiment.Operator;
        }

        // Set secondary ID 
        var guidPart = Guid.NewGuid().ToString().Replace("-", "")[24..].ToUpper();
        var sourceDir = PathLib.GetName(experiment.Storage.SourceDirectory);
        experiment.SecondaryId = $"{sourceDir}_{guidPart}";

        experiment.Processing.SerializeWorkflow();

        db.Attach(experiment);
        await db.SaveChangesAsync();

        await SubmitLogAsync(experiment, LogLevel.Information,
            "Experiment start requested, waiting for processing machines...");
        await ChangeStatusAsync(ExpState.StartRequested, experiment); // TODO - not overwrite
        // _logger.LogExp(LogLevel.Information, experiment, $"Requested experiment run.");
    }

    public async Task RequestStop(Experiment experiment)
    { 
        await SubmitLogAsync(experiment, LogLevel.Information,
            "Experiment stop requested, waiting for processing machines...");
        
        Logger.LogDebug("Requesting stop for experiment {Id} notify={NotifyUser}, notes={Notes}",
            experiment.Id, experiment.NotifyUser, experiment.Notes);
        
        await using (var dbctx = await dbContextFactory.CreateDbContextAsync())
        {
            dbctx.Entry(experiment).State = EntityState.Modified;
            await dbctx.SaveChangesAsync();
        }
        
        await ChangeStatusAsync(ExpState.StopRequested, experiment);
    }

    public async Task<Experiment?> GetRunningExperimentAsync(
        (string center, string instrument, string job) key,
        CancellationToken                              cancellationToken = default)
    {
        var cached = memoryCache.TryGetValue(key, out Guid activeExperiment);
        if (cached) return await GetExperimentAsync(activeExperiment, cancellationToken);

        var experiments = await experimentsService.GetExperimentsAsync(new ExperimentsFilter(
            ExpStates: new List<ExpState>() {ExpState.Active, ExpState.StartRequested, ExpState.StopRequested}
        ));
        
        var exp = experiments.Items.FirstOrDefault(e => e.KeyIdentif == key);
        if (exp is not null)
            memoryCache.Set(key, exp.Id);
        return exp;
    }

    public ExperimentOptions GetExpConfig(Experiment exp)
    {
        var expOpts = experimentsOptions.Get(exp.OrganizationId).InstrumentJobs;
        var instOpts = instrumentOptions.Get(exp.OrganizationId);
        var expo = expOpts[instOpts.GetInstrumentByName(exp.InstrumentName)][exp.Technique];
        return expo;
    }

    public async Task SendEmailNotificationAsync(Experiment exp, string subjectTemplate, string bodyTemplate)
    {
        
        var messageBuilder = messageBuilderProvider.CreateBuilder();
        

        messageBuilder.SubjectFromHbsStringTemplate(exp, subjectTemplate);
        messageBuilder.BodyFromHbsStringTemplate(exp, bodyTemplate);

        messageBuilder.AddRecipient(exp.User);
        messageBuilder.AddRecipient(exp.Operator, MessageRecipientType.Copy);

        Logger.LogDebug("Sending email notification for experiment {ExpId}", exp.Id);
        await messageBuilder.BuildAndSendAsync();
    }

    
    public Task ChangeStatusAsync(ExpState to, Experiment forExp)
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

    
    protected override async Task ExecuteRoundAsync(CancellationToken stoppingToken)
    {
        // Request expiration of experiments that are ready to be expired
        await experimentsService.ExpireExperiments(ct: stoppingToken);
        
        // Request publication of experiments that are ready to be published (after embargo period)
        await experimentsService.PublishExperiments(ct: stoppingToken);
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
        await using var context = await dbContextFactory.CreateDbContextAsync(ct);

        context.Set<Log>()
            .Add(new Log()
            {
                Id           = Guid.NewGuid(),
                Dt           = DateTime.UtcNow,
                ExperimentId = exp.Id,
                Message      = message,
                Origin       = "LIMS Webserver",
                Level        = level
            });

        await context.SaveChangesAsync(ct);
        // OnExperimentChanged();
    }

    public async Task SubmitLogsAsync(List<Log> logs, CancellationToken ct)
    {
        if (!logs.Any()) return;

        await using var context = await dbContextFactory.CreateDbContextAsync(ct);

        context.Set<Log>()
            .AddRange(logs);

        await context.SaveChangesAsync(ct);
        OnExperimentChanged(null);
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
        data.ApplyTo(exp);
        await context.SaveChangesAsync(ct);
        OnExperimentChanged(exp);
    }
}