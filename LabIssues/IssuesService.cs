using sip.Core;
using sip.Messaging.Email;
using sip.Scheduling.Obsolete;

namespace sip.LabIssues;

public class IssuesService(
        IDbContextFactory<AppDbContext> dbContextFactory,
        TimeProvider                  timeProvider,
        SmtpSender                      emailService,
        IOptions<DailySchedulerOptions> dsoptions,
        ILogger<IssuesService>          logger,
        IOptions<AppOptions>            appOptions)
    : DailyScheduler(dsoptions, logger), IFilterItemsProvider<Issue>
{
    
    public IssueUrgency AutoUrgencyHandler(Issue issue)
    {
        var timeDelta = timeProvider.DtUtcNow() - issue.DtAssigned;

        if (timeDelta.Days < 7) return IssueUrgency.Low;
        if (timeDelta.Days < 30) return IssueUrgency.Medium;
        return IssueUrgency.High;
    }
    
    public Issue CreateItem()
    {
        return new Issue()
        {
            DtObserved = DateTime.UtcNow,
            DtCreated = DateTime.UtcNow,
            Status = IssueStatus.InProgress
        };
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
            .OrderByDescending(i => i.DtLastChange)
            .ToListAsync(cancellationToken: ct);
        if (string.IsNullOrWhiteSpace(filter)) return issues;
        return issues.Where(u => u.IsFilterMatch(filter));
    }

    public async Task UpdateItem(Issue item)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        item.DtLastChange = DateTime.UtcNow;
        db.Entry(item).State = EntityState.Modified;
        await db.SaveChangesAsync();
    }
        
    public async Task PersistItem(Issue item)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        
        // Get reasonable id:
        var currYr = DateTime.Today.Year % 2000;
        var id = currYr + "001";
        var lastItem = await db.Set<Issue>()
            .OrderByDescending(i => i.Id)
            .FirstOrDefaultAsync();

        if (lastItem is not null)
        {
            var yr = int.Parse(lastItem.Id[..2]);
            var index = int.Parse(lastItem.Id[2..]);
            if (yr == currYr)
            {
                // Not a year reset
                id = currYr + (index + 1).ToString().PadLeft(3, '0');
            }
        }

        item.Id = id;
        item.DtLastChange = DateTime.UtcNow;
        item.DtCreated = DateTime.UtcNow;
        if (item.Responsible is not null)
        {
            item.DtAssigned = DateTime.UtcNow;
            item.Status = IssueStatus.InProgress;
            await NotifyAssignResponsible(item);
            item.DtLastNotified = DateTime.UtcNow;
        }
        db.Entry(item).State = EntityState.Added;
        await db.SaveChangesAsync();
    }

    public override async Task ExecAction()
    {
        // Check and notify all responsibles
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var issues = await db.Set<Issue>()
            .Include(i => i.Responsible)
            .ToListAsync();
        
        foreach (var issue in issues)
        {
            if (issue.DtAssigned == default || issue.Responsible is null)
                continue;

            var days = (DateTime.Today - issue.DtAssigned.Date).Days;
            // var notifyDays = (int) issue.NotifyIntervalDays == 0 ? 1 : (int) issue.NotifyIntervalDays;
            if (days > 0 && (days % (int)issue.NotifyIntervalDays) == 0 && issue.DtLastNotified.Date != DateTime.Today)
            {
                await NotifyRemiderResponsible(issue);
                issue.DtLastNotified = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync();
    }

    private async Task NotifyRemiderResponsible(Issue issue)
    {
        await emailService.SendOne(IssueRemiderMessage(issue, appOptions.Value));
    }
    
    private async Task NotifyAssignResponsible(Issue issue)
    {
        await emailService.SendOne(IssueAssignedMessage(issue, appOptions.Value));
    }
    
    public static MimeMessage IssueAssignedMessage(Issue issue, AppOptions appOptions)
    {
        var issueLink = appOptions.UrlBase + $"labissues/{issue.Id}";
        
        var message = new MimeMessage
        {
            Subject = $"[IID: {issue.Id}] New issue - {issue.Subject}",
            Body = new TextPart("plain")
            {
                Text = 
                    $"You have been made responsible for resolving a new issue: \n\n{issue.Description}\n\n" +
                    $"See the issue at {issueLink}\n\n" +
                    "# This email is generated automatically."
            },
        };

        if (issue.Responsible is null)
            throw new InvalidOperationException($"{nameof(IssueAssignedMessage)} - Null responsible");
        
        message.To.Add(MailboxAddress.Parse(issue.Responsible.Fullcontact));
        return message;
    }

    public static MimeMessage IssueRemiderMessage(Issue issue, AppOptions appOptions)
    {
        var issueLink = appOptions.UrlBase + $"/labissues/{issue.Id}";
        
        var message = new MimeMessage
        {
            Subject = $"[IID: {issue.Id}] Unresolved issue - {issue.Subject}",
            Body = new TextPart("plain")
            {
                Text = 
                    $"An issue has not been resolved for {(DateTime.Today - issue.DtAssigned).Days} days.\n\n" +
                    $"{issue.Subject}\n{issue.Description}\n\n" +
                    $"See the issue at {issueLink}\n\n" +
                    "# This email is generated automatically."
            },
        };

        if (issue.Responsible is null)
            throw new InvalidOperationException($"{nameof(IssueAssignedMessage)} - Null responsible");
        
        message.To.Add(MailboxAddress.Parse(issue.Responsible.Fullcontact));
        return message;
    }
}