using Microsoft.Extensions.Internal;
using sip.Core;

namespace sip.Utils;

public interface IIdGenerator<TEntity>
{
    Task<string> GenerateNextIdAsync();
}

public class YearOrderIdGeneratorOptions
{
    public bool ShortYear { get; set; } = true;
    public int OrderLength { get; set; } = 4; 
    public string Postfix { get; set; } = "E";
}


/// <summary>
/// Generates IDs containing year, entity order and letter to further distinguish the entity, e.g. :
/// 210001I (first entity in year 21 of type I), 220125C (125. entity in year 22 of type C)
/// Id generated this way is always sortable. 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class YearOrderIdGeneratorService(IDbContextFactory<AppDbContext> dbContextFactory, ISystemClock systemClock)
{
    public async Task<string> GenerateNextIdAsync<TEntity>(YearOrderIdGeneratorOptions options)
        where TEntity : class, IStringIdentified 
    {
        await using var ctx  = await dbContextFactory.CreateDbContextAsync();
        var ent = await ctx.Set<TEntity>()
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync();

        return GenerateNextFromLast(ent?.Id, options);
    }

    private string GenerateNextFromLast(string? lastpid, YearOrderIdGeneratorOptions opts)
    {
        var yrlen = opts.ShortYear ? 2 : 4;
        
        var year = systemClock.UtcNow.ToString(opts.ShortYear ? "yy" : "yyyy");
        var order = 1;
        if (lastpid != null && lastpid.Substring(0, yrlen) != year)
        {
            // Thats a new year 
            lastpid = null;
        }

        if (lastpid != null)
        {
            var lastpidorder = int.Parse(lastpid.Substring(yrlen, opts.OrderLength));
            lastpidorder++;
            order = lastpidorder;
        }

        return year + order.ToString().PadLeft(opts.OrderLength, '0') + opts.Postfix;
    }
}