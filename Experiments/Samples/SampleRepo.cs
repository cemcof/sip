using sip.Core;

namespace sip.Experiments.Samples;

public class SampleRepo(IDbContextFactory<AppDbContext> dbfac)
{
    public Sample CreateItem()
    {
        return new Sample();
    }

    public async Task<ItemsResult<Sample>> LoadSamplesAsync(IFilter filter)
    {
        // Naive implementation - optimize
        await using var db = await dbfac.CreateDbContextAsync(filter.CancellationToken);
        var samples = await db.Set<Sample>()
            .ToListAsync(cancellationToken: filter.CancellationToken);

        if (!string.IsNullOrWhiteSpace(filter.FilterQuery))
            samples = samples.Where(u => u.IsFilterMatch(filter.FilterQuery)).ToList();

        return new ItemsResult<Sample>(samples
            .Skip(filter.Offset)
            .Take(filter.Count), 
            samples.Count);
    }
        
    public async Task PersistItem(Sample item)
    {
        await using var db = await dbfac.CreateDbContextAsync();
        db.Attach(item);
        await db.SaveChangesAsync();
    }
}