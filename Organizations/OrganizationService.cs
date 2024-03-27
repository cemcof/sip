using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.Caching.Memory;
using sip.Core;
using sip.Utils;

namespace sip.Organizations;

public class OrganizationService(
        IDbContextFactory<AppDbContext>       dbContextFactory,
        IOptionsMonitor<OrganizationOptions> optionsMonitor,
        IMemoryCache cache)
    : IOrganizationProvider
{
    // For easier usage in components
    public ValueTask<ItemsProviderResult<Organization>> GetOrganizationsAsync(
        ItemsProviderRequest request, 
        string? searchstring)
    {
        var result = GetAll().ToList();
        
        if (!string.IsNullOrWhiteSpace(searchstring))
        {
            result = result.Where(r => r.IsFilterMatch(searchstring)).ToList();
        }

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
        
        var itemResult = new ItemsProviderResult<Organization>(result, result.Count);
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

        var tree = new Tree<Organization>(
            orgs.Single(o => o.Parent is null)
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