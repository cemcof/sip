using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.Caching.Memory;
using sip.Core;
using sip.Experiments.Logs;
using sip.Experiments.Model;
using sip.Messaging.Email;

namespace sip.Experiments;

public record ExperimentsFilter(
    IOrganization? Organization = null,
    string? FilterQuery = null,
    List<ExpState>? ExpStates = null,
    List<StorageState>? StorageStates = null,
    List<ProcessingState>? ProcessingStates = null,
    List<PublicationState>? PublicationStates = null,
    Expression<Func<Experiment, bool>>? CustomFilter = null,
    int Count = -1,
    int Offset = 0,
    CancellationToken CancellationToken = default
) : IFilter;

public record ExperimentsResult(IReadOnlyCollection<Experiment> Experiments)
{
    public Experiment? ByKeyIdentif((string center, string instrument, string job) key)
        => Experiments.FirstOrDefault(e => e.KeyIdentif == key);

    public static ExperimentsResult Empty()
        => new(Array.Empty<Experiment>());
    
    public IEnumerable<Experiment> ForInstrument(string instrument)
        => Experiments.Where(e => e.InstrumentName == instrument);
}

public class ExperimentsService(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ILogger<ExperimentsService> logger,
    IMemoryCache memoryCache,
    GeneralMessageBuilderProvider messageBuilderProvider) : IExperimentLogProvider
{
    public ItemProviderRequestWithSearchDelegate<Experiment> GetFilteredExperimentsProviderByOrg(
        IOrganization organizationSubject)
        => (request, searchString) => GetFilteredExperiments(request, organizationSubject, searchString);
    
    public async Task<ExperimentsResult> GetRunningExperimentAsync(
        IOrganization organization,
        CancellationToken cancellationToken = default)
    {
        var experiments = await GetExperimentsAsync(new ExperimentsFilter(
            Organization: organization,
            ExpStates: new List<ExpState>() {ExpState.Active, ExpState.StartRequested, ExpState.StopRequested},
            CancellationToken: cancellationToken
        ));

        return new ExperimentsResult(experiments.Items.ToArray());
    }

    
    public async ValueTask<ItemsProviderResult<Experiment>> GetFilteredExperiments(
        ItemsProviderRequest request,
        IOrganization organizationSubject,
        string? searchstring)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(request.CancellationToken);
        logger.LogTrace("GetFiltered exps: {}, {}", request.Count, request.StartIndex);

        List<Experiment> resultItems;
        var query = db.Set<Experiment>()
            .Where(e => e.OrganizationId == organizationSubject.Id)
            .OrderByDescending(e => e.DtCreated)
            .Include(e => e.User)
            .ThenInclude(u => u.Contacts)
            .Include(e => e.Operator)
            .ThenInclude(u => u.Contacts)
            .Include(e => e.Sample)
            .Include(e => e.Project);

        // Search string 
        var searches =
            (searchstring ?? "").Split("&", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var totalCount = 0;

        if (searches.Length == 0)
        {
            // No searches means gimme all
            totalCount = await db.Set<Experiment>().Where(e => e.OrganizationId == organizationSubject.Id)
                .CountAsync();
            resultItems = await query.Skip(request.StartIndex)
                .Take(request.Count)
                .ToListAsync(request.CancellationToken);
        }
        else
        {
            // We are searching ... for now do it stupidly and fetch everything, then search in the result  
            resultItems =
                await query.ToListAsync(); // TODO - on many items this is so resource heavy and unacceptable...
            resultItems = resultItems.Where(x => StringUtils.IsFilterMatchAtLeastOneOf(searches,
                    x.Technique, x.User.Fullcontact, x.SecondaryId, x.Operator.Fullcontact, x.Sample.Name, x.Sample.KeywordsStr))
                .ToList();
            totalCount = resultItems.Count;
            resultItems = resultItems
                .Skip(request.StartIndex)
                .Take(request.Count)
                .ToList();
        }

        return new ItemsProviderResult<Experiment>(resultItems, totalCount);
    }

    public async Task<ItemsResult<Experiment>> GetExperimentsAsync(ExperimentsFilter filter, bool cacheResults = false)
    {
        logger.LogTrace("GettingExperimentAsync: cache={}", cacheResults);
        await using var db = await dbContextFactory.CreateDbContextAsync(filter.CancellationToken);
        var query = db.Set<Experiment>().AsQueryable();

        if (filter.CustomFilter is not null)
            query = query.Where(filter.CustomFilter);

        if (filter.Organization is not null)
            query = query.Where(e => e.OrganizationId == filter.Organization.Id);

        if (filter.ExpStates is not null && filter.ExpStates.Count > 0)
        {
            query = query.Where(e => filter.ExpStates.Contains(e.State));
        }

        if (filter.StorageStates is not null && filter.StorageStates.Count > 0)
        {
            query = query.Where(e => filter.StorageStates.Contains(e.Storage.State));
        }

        if (filter.ProcessingStates is not null && filter.ProcessingStates.Count > 0)
        {
            query = query.Where(e => filter.ProcessingStates.Contains(e.Processing.State));
        }

        if (filter.PublicationStates is not null && filter.PublicationStates.Count > 0)
        {
            query = query.Where(e => filter.PublicationStates.Contains(e.Publication.State));
        }

        query = _WithExperimentIncludes(query);
        query = query.OrderByDescending(e => e.DtCreated);
        var countTotal = await query.CountAsync(filter.CancellationToken); // TODO - count without includes?
        query = query.ApplySkipTake(filter);

        var result = await query.ToListAsync(filter.CancellationToken);

        foreach (var experiment in result)
        {
            experiment.Processing.DeserializeWorkflow();

            if (cacheResults)
            {
                memoryCache.Set(experiment.Id, experiment);
            }
        }

        return new ItemsResult<Experiment>(result, countTotal);
    }

    private IQueryable<Experiment> _WithExperimentIncludes(IQueryable<Experiment> query)
    {
        return query.Include(e => e.User)
            .ThenInclude(u => u.Contacts)
            .Include(e => e.User)
            .ThenInclude(u => u.IdentityUserLogins)
            .Include(e => e.Operator)
            .ThenInclude(e => e.Contacts)
            .Include(e => e.Operator)
            .ThenInclude(e => e.IdentityUserLogins)
            .Include(e => e.Storage)
            .Include(e => e.Publication)
            .Include(e => e.DataSource)
            .Include(e => e.Processing)
            .ThenInclude(ep => ep.ExperimentProcessingDocuments)
                .ThenInclude(epd => epd.FilesInDocuments)
                .ThenInclude(ep => ep.FileMetadata)
            .Include(e => e.Sample)
            .Include(e => e.Project)
            .Include(e => e.Organization);
    }


    /// <summary>
    /// Check which experiments should be expired and set expiration request on them.
    /// This is supposed to be called periodically (e.g. daily)
    /// </summary>
    /// <param name="dt">Which dt to consider when checking expiration - defaults to now</param>
    /// <param name="ct"></param>
    public async Task ExpireExperiments(DateTime dt = default, CancellationToken ct = default)
    {
        if (dt == default) dt = DateTime.UtcNow;

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var result = await dbContext.Set<ExperimentStorage>()
            .Where(e => e.State == StorageState.Idle && e.DtExpiration != default && e.DtExpiration < dt)
            .Include(e => e.Experiment)
            .ToListAsync(ct);

        // Depending on whether the experiment is marked for archivation, request either expiration or archivation
        result.ForEach(e =>
            e.State = e.Archive ? StorageState.ArchivationRequested : StorageState.ExpirationRequested);

        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Experiments {} have been requested for expiration/archivation",
            string.Join(", ", result.Select(e => e.Experiment.SecondaryId)));
    }

    public async Task<Experiment> GetExperimentAsync(Guid id, bool useCache = false,
        CancellationToken cancellationToken = default)
    {
        if (useCache)
        {
            var cached = memoryCache.TryGetValue(id, out Experiment? exp);
            if (cached) return exp!;
        }

        var expFilter = new ExperimentsFilter
        {
            CustomFilter = e => e.Id == id,
            Count = 1,
            CancellationToken = cancellationToken
        };
        var exps = await GetExperimentsAsync(expFilter, cacheResults: useCache);

        if (exps.TotalCount < 1)
        {
            throw new NotAvailableException($"Experiment with id {id} does not exist");
        }

        return exps.Items.First();
    }

    public async ValueTask<ItemsProviderResult<Log>> GetLogsAsync(
        Experiment exp,
        LogLevel minLevel,
        ItemsProviderRequest request,
        string? origin = null)
    {
        try
        {
            var db = await dbContextFactory.CreateDbContextAsync(request.CancellationToken);
            var countQuery = db.Set<Log>()
                .Where(l => l.ExperimentId == exp.Id)
                .Where(l => (int) l.Level >= (int) minLevel);

            if (origin is not null)
                countQuery = countQuery.Where(l => l.Origin == origin);

            var countTotal = await countQuery.CountAsync(request.CancellationToken);

            var query = db.Set<Log>()
                .Where(l => l.ExperimentId == exp.Id)
                .Where(l => (int) l.Level >= (int) minLevel);

            if (origin is not null) query = query.Where(l => l.Origin == origin);


            var result = await query
                .OrderByDescending(l => l.Dt)
                .Skip(request.StartIndex)
                .Take(request.Count)
                .ToListAsync(request.CancellationToken);

            logger.LogTrace("GetLogs: origin={} countTotal={} countResult={}, startIndex={}, amount={}", origin,
                countTotal, result.Count, request.StartIndex, request.Count);
            return new ItemsProviderResult<Log>(result, countTotal);
        }
        catch (OperationCanceledException)
        {
            request.CancellationToken
                .ThrowIfCancellationRequested(); // fixes the isuue, throws correct exception with corresponding token
            throw;
        }
    }

    public async Task PublishExperiments(CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var exps = await dbContext.Set<ExperimentPublication>()
            .Include(e => e.Experiment)
            .Where(e => e.State == PublicationState.DraftCreated && e.DtEmbargo < DateTime.UtcNow)
            .ToListAsync(ct);

        exps.ForEach(e => e.State = PublicationState.PublicationRequested);
        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Experiments {} have been requested for publication",
            string.Join(", ", exps.Select(e => e.Experiment.SecondaryId)));
    }

    public async ValueTask<List<(string, ItemsProviderDelegate<Log>)>> GetLogProvidersAsync(
        Experiment experiment,
        LogLevel minLevel,
        CancellationToken ct = default)
    {
        var db = await dbContextFactory.CreateDbContextAsync(ct);
        var logsByCategs = await db.Set<Log>()
            .Where(l => l.ExperimentId == experiment.Id)
            .GroupBy(l => l.Origin)
            .Select(g => new {g.Key, Count = g.Count()})
            .ToListAsync(cancellationToken: ct);

        var result = new List<(string, ItemsProviderDelegate<Log>)>();
        // Add one provider for everything
        result.Add(("All", request => GetLogsAsync(experiment, minLevel, request)));
        foreach (var logsByCateg in logsByCategs)
        {
            result.Add((logsByCateg.Key, (request) => GetLogsAsync(experiment, minLevel, request, logsByCateg.Key)));
        }

        return result;
    }
    

    public async Task CleanLogsAsync(Func<Experiment, DateTime> olderThanProvider, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var exp = db.Set<Experiment>();
        var exps = await exp.Where(e => e.Logs.Any())
            .ToArrayAsync(cancellationToken: ct);
        logger.LogDebug("Found {} experiments with logs", exps.Length);
        
        exps = exps.Where(e => e.DtCreated < olderThanProvider(e)).ToArray();
        logger.LogDebug("About to clean logs for {} experiments", exps.Length);
        
        foreach (var experiment in exps)
        {
            logger.LogDebug("Cleaning logs for experiment {}", experiment.SecondaryId);
            var deletedCount = await db.Set<Log>()
                .Where(l => l.ExperimentId == experiment.Id)
                .ExecuteDeleteAsync(ct);
            logger.LogDebug("Deleted {} logs for experiment {}", deletedCount, experiment.SecondaryId);
        }
    }
    
    /// <summary>
    /// Change status to given status only if the current status is "from" status
    /// </summary>
    /// <param name="fromState"></param>
    /// <param name="toStatus"></param>
    /// <param name="forExp"></param>
    public async Task<bool> ChangeStatusFromAsync(ExpState fromState, ExpState toStatus, Experiment forExp)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var updateResult = await context.Set<Experiment>()
            .Where(e => e.Id == forExp.Id && e.State == fromState)
            .ExecuteUpdateAsync(ps => ps.SetProperty(p=> p.State, toStatus));
        if (updateResult == 1)
        {
            forExp.State = toStatus;
            return true;
        }

        return false;
    }
    
    public async Task SendEmailNotificationAsync(
        Experiment exp, 
        EmailTemplateOptions template, 
        CancellationToken ct = default)
    {
        var messageBuilder = messageBuilderProvider.CreateBuilder();
        messageBuilder.SubjectFromHbsStringTemplate(exp, template.Subject);
        messageBuilder.BodyFromHbsStringTemplate(exp, template.Body);

        messageBuilder.AddRecipient(exp.User);
        messageBuilder.AddRecipient(exp.Operator, MessageRecipientType.Copy);

        logger.LogDebug("Sending email notification for experiment {ExpId}", exp.Id);
        await messageBuilder.BuildAndSendAsync();
    }

    public async Task UpdatePropertyAsync(Experiment exp, Expression<Func<Experiment, string?>> property)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var entry = context.Set<Experiment>().Entry(exp);
        entry.State = EntityState.Unchanged;
        entry.Property(property).IsModified = true;
        await context.SaveChangesAsync();
    }
}