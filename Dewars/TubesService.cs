using sip.Core;
using sip.Organizations;

namespace sip.Dewars;

public class TubesService(IDbContextFactory<AppDbContext> dbContextFactory, ILogger<TubesService> logger)
{
    /// <summary>
    /// Search tubes using one search strings.
    /// This should search by all properties of tube: id, name and description.
    /// </summary>
    /// <param name="organization"></param>
    /// <param name="searchText"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Tube>> SearchTubesAsync(IOrganization organization, string searchText, CancellationToken ct = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        searchText = searchText.ToLower();
        // Get all tubes (there are not that many)
        var tubes = await dbContext.Set<Tube>()
            .Where(t => t.OrganizationId == organization.Id)
            .ToListAsync();
        
        // Now perform search
        return tubes.Where(t => t.Structure.ToLower().Contains(searchText) ||
                                t.User.ToLower().Contains(searchText) ||
                                t.Description.ToLower().Contains(searchText));
    }

    public async Task<IEnumerable<Tube>> GetAllTubesAsync(IOrganization organization, CancellationToken ct = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        var result = await dbContext.Set<Tube>()
            .Where(t => t.OrganizationId == organization.Id)
            .ToListAsync(cancellationToken: ct);
        
        return result;
    }

    public Task<Tube?> GetTube(
        IOrganization organization, 
        string dewar, 
        string holder, 
        string deck, 
        string position)
    {
        return GetTubeAsync(organization, $"{dewar}/{holder}/{deck}/{position}");
    }

    public async Task<Tube?> GetTubeAsync(IOrganization organization, string structure, CancellationToken ct = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        var tub = await dbContext.Set<Tube>()
            .FirstOrDefaultAsync(t => t.OrganizationId == organization.Id && t.Structure == structure, cancellationToken: ct);
        return tub;
    }

    public async Task UpdateTubeAsync(Tube tube, CancellationToken ct = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        tube.LastChange = DateTime.UtcNow;
        var trackedEntry = await dbContext.Set<Tube>().FindAsync(tube.Structure, tube.OrganizationId);
        if (trackedEntry is null)
        {
            dbContext.Set<Tube>().Add(tube);
            logger.LogInformation("Creating new tube entry {}, {}, {}", tube.Structure, tube.User, tube.Description);
        }
        else
        {
            logger.LogInformation("Updating tube entry from: {}, {}, {} \nto: {}, {}",
                trackedEntry.Structure, trackedEntry.User, trackedEntry.Description, tube.User, tube.Description);
            dbContext.Entry(trackedEntry).CurrentValues.SetValues(tube);
        }

        await dbContext.SaveChangesAsync(ct);
    }
}