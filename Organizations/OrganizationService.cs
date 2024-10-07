using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.Caching.Memory;
using sip.Core;

namespace sip.Organizations;

public class OrganizationService(
        IDbContextFactory<AppDbContext>       dbContextFactory,
        IOptionsMonitor<OrganizationOptions> optionsMonitor,
        IMemoryCache cache,
        ILogger<OrganizationService> logger)
    : IOrganizationProvider
{
    // For easier usage in components
    public ValueTask<ItemsProviderResult<Organization>> GetOrganizationsAsync(
        ItemsProviderRequest request, 
        string? searchstring)
    {
        var result = GetAll().ToList();
        
        logger.LogTrace("GetOrganizationsAsync: all orgs count: {}, search: {} request start: {} request count {}", 
            result.Count, searchstring, request.StartIndex, request.Count);
        
        if (!string.IsNullOrWhiteSpace(searchstring))
        {
            result = result.Where(r => r.IsFilterMatch(searchstring)).ToList();
        }
        
        var count = result.Count;
        if (request.Count != -1)
        {
            result = result.Skip(request.StartIndex)
                            .Take(request.Count)
                            .ToList();
        }
        else
        {
            // Skip only
            result = result.Skip(request.StartIndex)
                            .ToList();
        }
        
        logger.LogTrace("GetOrganizationsAsync: {} items: {}", count,
            string.Join(", ", result.Select(r => r.Id)));
        var itemResult = new ItemsProviderResult<Organization>(result, count);
        return ValueTask.FromResult(itemResult);
    }


    public Organization GetOrganizationInfo(string getTargetOrg)
        => optionsMonitor.Get(getTargetOrg).OrganizationDetails;
    
    public Organization GetOrganizationInfo<T>()
        => GetOrganizationInfo(typeof(T).Name);

    public Tree<Organization> GetOrganizationTree()
    {
        if (cache.TryGetValue("__org_tree", out Tree<Organization>? cached))
            return cached!;
        
        var dbctx = dbContextFactory.CreateDbContext();
        var orgs = dbctx.Set<Organization>()
            .Include(o => o.Parent)
            .ToList();

        var rootOrg = new Organization("root", "root", "Root", "root");
        
        // Attach all organizations not having parent to the root org
        foreach (var org in orgs.Where(org => org.Parent is null))
        {
            rootOrg.Children.Add(org);
            org.Parent = rootOrg;
        }
        
        var tree = new Tree<Organization>(
            rootOrg
            );
        
        cache.Set("__org_tree", tree);
        return tree;
    }
    
    public IEnumerable<Organization> GetAll()
      => GetOrganizationTree().EnumerateBfs();
    
    public Organization GetFromString(string organization)
    {
        if (cache.TryGetValue(organization, out Organization? cached)) 
            return cached!;
        
        var org = GetOrganizationTree()
            .EnumerateBfs()
            .FirstOrDefault(o => o.Id == organization || o.LinkId == organization);

        if (org is null) throw new NotAvailableException($"Organization {organization} is not available");
        
        cache.Set(organization, org);
        return org;
    }

    public Organization? GetFromStringOrDefault(string organization)
    {
        try
        {
            return GetFromString(organization);
        }
        catch (NotAvailableException)
        {
            return null;
        }
    }
}