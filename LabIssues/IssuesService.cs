using sip.Core;
using sip.Messaging.Email;
using sip.Scheduling;

namespace sip.LabIssues;

public class IssuesService(
        IDbContextFactory<AppDbContext> dbContextFactory,
        TimeProvider                    timeProvider,
        GeneralMessageBuilderProvider   messageBuilderProvider,
        IOptionsMonitor<ScheduledServiceOptions> scheduleOptions,
        SmtpSender                      emailService,
        ILogger<IssuesService>          logger,
        IOptions<AppOptions>            appOptions)
    : ScheduledService(scheduleOptions, timeProvider, logger), IFilterItemsProvider<Issue>
{
    
    public IssueUrgency AutoUrgencyHandler(Issue issue)
    {
        var timeDelta = TimeProvider.DtUtcNow() - issue.DtAssigned;

        if (timeDelta.Days < 7) return IssueUrgency.Low;
        if (timeDelta.Days < 30) return IssueUrgency.Medium;
        return IssueUrgency.High;
    }
    
    public async Task<IEnumerable<Issue>> GetItems(string? filter = null, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var issuesSet = db.Set<Issue>();
        var issues = await issuesSet
            .Include(i => i.InitiatedBy)
                .ThenInclude(u => u!.Contacts)
            .Include(i => i.Responsible)
                .ThenInclude(u => u!.Contacts)
            .Include(i => i.Organization)
            .Include(i => i.IssueComments)
            .OrderByDescending(i => i.DtLastChange)
            .ToListAsync(cancellationToken: ct);
        if (string.IsNullOrWhiteSpace(filter)) return issues;
        return issues.Where(u => u.IsFilterMatch(filter));
    }

    private string _GenerateNextId(string? lastIssueId)
    {
        var currYr = DateTime.Today.Year % 2000;
        var id = currYr + "001";
        
        if (lastIssueId is not null)
        {
            var yr = int.Parse(lastIssueId[..2]);
            var index = int.Parse(lastIssueId[2..]);
            if (yr == currYr)
            {
                // Not a year reset
                id = currYr + (index + 1).ToString().PadLeft(4, '0');
            }
        }

        return id;
    }

    public async Task<Issue> CreateIssueAsync(IssueCreate issueCreate)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        
        var lastItem = await db.Set<Issue>()
            .AsNoTracking()
            .Where(i => i.OrganizationId == issueCreate.Organization.Id)
            .OrderByDescending(i => i.Id)
            .Select(i => i.Id)
            .FirstOrDefaultAsync();

        var id = _GenerateNextId(lastItem);
        
        var issue = new Issue(id, issueCreate);

        // Persist issue and optionally notify responsible
        // Just create issue and its comments
        db.Entry(issue).State = EntityState.Added;
        db.Set<IssueComment>()
            .AddRange(issue.IssueComments);
        
        await db.SaveChangesAsync();
        return issue;
    }

    public Task<IUnitOfWork<Issue>> GetIssueForUpdateAsync(Issue issue)
    {
        throw new NotImplementedException();
    }
    

    private Task SendToResponsible(Issue issue, string template) =>
        messageBuilderProvider.CreateBuilder()
            .BodyAndSubjectFromFileTemplate(issue, template)
            .AddRecipient(issue.Responsible ?? throw new InvalidOperationException("Responsible is not set"))
            .BuildAndSendAsync();
    
    private Task NotifyRemiderResponsible(Issue issue) =>
        SendToResponsible(issue, "IssueReminder.hbs");

    private Task NotifyAssignResponsible(Issue issue) =>
        SendToResponsible(issue, "IssueAssigned.hbs");

    protected override async Task ExecuteRoundAsync(CancellationToken ct) 
        => await _NotifyResponsibles(ct);

    private async Task _NotifyResponsibles(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var issues = await db.Set<Issue>()
            .Include(i => i.Responsible)
            .ToListAsync(cancellationToken: ct);
        
        foreach (var issue in issues)
        {
            if (issue.DtAssigned == default || issue.Responsible is null)
                continue;

            var days = (DateTime.Today - issue.DtAssigned.Date).Days;
            // var notifyDays = (int) issue.NotifyIntervalDays == 0 ? 1 : (int) issue.NotifyIntervalDays;
            if (days > 0 && (days % issue.NotifyIntervalDays) == 0 && issue.DtLastNotified.Date != DateTime.Today)
            {
                await NotifyRemiderResponsible(issue);
                issue.DtLastNotified = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync(ct);
    }
}