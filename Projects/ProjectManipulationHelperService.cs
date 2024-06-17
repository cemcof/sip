using System.Linq.Expressions;
using sip.Core;
using sip.Documents;
using sip.Documents.Proposals;
using sip.Organizations;
using sip.Userman;
using sip.Utils;

namespace sip.Projects;

public class ProjectManipulationHelperService(IDbContextFactory<AppDbContext> dbContextFactory)
{
    public virtual async Task<TProject> CreateProjectAsync<TProject>(IIdGenerator<TProject> idGenerator) where  TProject : Project, new()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var proj = new TProject()
        {
            Id = await idGenerator.GenerateNextIdAsync(),
            AffiliationDetails = new AffiliationDetails()
        };

        ctx.Add(proj);
        await ctx.SaveChangesAsync();
        return proj;
    }

    public async Task<TProject> LoadProjectAsync<TProject>(string id, AppDbContext? db = null) where TProject : Project
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
    
        var result = await ctx.Set<TProject>()
            .Include(p => p.Children)
            .Include(p => p.ProjectStatuses)
            .ThenInclude(ps => ps.StatusInfo)
            .Include(p => p.ProjectStatuses)
            .ThenInclude(ps => ps.Organization)
            .ThenInclude(o => o.Parent)
            // .Include(p => p.ProjectDocuments)
            // .ThenInclude(pd => pd.FilesInDocuments)
            // .ThenInclude(pd => pd.FileMetadata)
            .Include(p => p.ProjectMembers)
            .ThenInclude(pm => pm.MemberUser)
            .ThenInclude(mu => mu.Contacts)
            .Include(p => p.ProjectOrganizationMessages)
            .SingleAsync(p => p.Id == id);


        await ctx.Set<Document>()
            .Include(d => d.FilesInDocuments)
            .ThenInclude(d => d.FileMetadata)
            .Include(d => (d as Proposal)!.ExpectedEvaluator)
            .ThenInclude(mu => mu!.Contacts)
            .Include(d => (d as Proposal)!.EvaluatedBy)
            .ThenInclude(mu => mu!.Contacts)
            .Where(d => d.ProjectId == id)
            .LoadAsync();
            
        return result;
    }

    public async Task AddProjectMemberAsync<TMember>(Project project, AppUser user, Organization organization)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var memtype = typeof(TMember).Name;
        if (!await ctx.Set<ProjectMember>().AnyAsync(p =>
                p.ProjectId == project.Id && p.OrganizationId == organization.Id && p.MemberType == memtype))
        {
            await ctx.Set<ProjectMember>()
                .AddAsync(new ProjectMember()
                {
                    OrganizationId = organization.Id,
                    ProjectId = project.Id,
                    MemberUserId = user.Id,
                    MemberType = memtype
                });
            
            await ctx.SaveChangesAsync();
        }
    }
    
    public Task<ProjectLoadResults> LoadProjectsAsyns(ProjectFilter filter) => LoadProjectsAsync<Project>(filter);
    public async Task<ProjectLoadResults> LoadProjectsAsync<TProject>(ProjectFilter filter) where TProject : Project
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();

        var result = await ctx.Set<TProject>()
            .Include(p => p.Children)
            .Include(p => p.ProjectStatuses)
            .ThenInclude(ps => ps.StatusInfo)
            .Include(p => p.ProjectStatuses)
            .ThenInclude(ps => ps.Organization)
            .ThenInclude(o => o.Parent)
            .Include(p => p.ProjectMembers)
            .ThenInclude(pm => pm.MemberUser)
            .ThenInclude(mu => mu.Contacts)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        var count = result.Count;
        
        if (!string.IsNullOrWhiteSpace(filter.FilterQuery))
        {
            result = result.Where(p => StringUtils.IsFilterMatchAtLeastOneOf(filter.FilterQuery,
                p.Id, p.Title, p.Acronym, 
                string.Join(" ", p.ProjectMembers.Select(pm => pm.MemberUser.Fullcontact)),
                string.Join(" ", p.Statuses.Select(ps => ps.StatusInfo.DisplayName)))).ToList();
        }

        return new ProjectLoadResults(Items: result.Cast<Project>().ToList(), TotalCount: count);
    }
    
    public async IAsyncEnumerable<Project> EnumerateProjectsAsync(Expression<Func<Project, bool>>? predicate = null)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var query = ctx.Set<Project>().AsQueryable();
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }
        
        await foreach (var projId in query.Select(p => p.Id)
                           .AsAsyncEnumerable())
        {
            // Fully load the project and yield it
            yield return await LoadProjectAsync<Project>(projId, ctx);
        }
    }


    public async Task RemoveProjectAsync(Project project)
    {
        // Firstly remove all documents
        await using var ctx = await dbContextFactory.CreateDbContextAsync();

        foreach (var projectProjectDocument in project.ProjectDocuments)
        {
            ctx.Remove(projectProjectDocument);
        }
        
        ctx.Remove(project);
        await ctx.SaveChangesAsync();
    }
    
    public TProject CreateEmpty<TProject>() where TProject : Project, new()
    {
        return new TProject()
        {
            Id = Guid.NewGuid().ToString(), AffiliationDetails = new AffiliationDetails()
        };
    }
    
    public async Task PersistProjectAsync(Project project, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        db.Entry(project).State = EntityState.Added;
        db.Entry(project.AffiliationDetails).State = EntityState.Added;
        await db.SaveChangesAsync(ct);
    }

}