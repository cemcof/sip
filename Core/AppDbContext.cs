using sip.Userman;
using sip.Utils;

namespace sip.Core;

public class AppDbContext(
        DbContextOptions<AppDbContext>    options,
        IEnumerable<ModelBuilderDelegate> modelBuilderDelegates)
    : IdentityUserContext<AppUser, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Let identity configure itself
        base.OnModelCreating(builder);
        
        // ===== Here is place for some very default model configurations - organizations, identity navigation properties, etc. ======
        // Configure base organization
        // Extension point for the db context - Invoke registered model configurations
        builder.Entity<AppUser>().ToTable("AppUsers")
            .Property(a => a.UserName).HasMaxLength(128);
        builder.Entity<AppUser>()
            .Property(a => a.NormalizedUserName).HasMaxLength(128);
        builder.Entity<AppUser>()
            .Property(a => a.Email).HasMaxLength(128);
        builder.Entity<AppUser>()
            .Property(a => a.NormalizedEmail).HasMaxLength(128);
        builder.Entity<AppUser>()
            .HasMany(a => a.IdentityUserLogins)
            .WithOne()
            .HasForeignKey(x => x.UserId);
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("AppUserClaims"); // .HasCharSet("ascii");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("AppUserTokens"); // .HasCharSet("ascii");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("AppUserLogins"); // .HasCharSet("ascii");
        
        
        foreach (var builderDelegate in modelBuilderDelegates)
        {
            builderDelegate(builder);
        }
        
        // Configure whole db to use UTC
        builder.UseUtc();
        
        // Configure whole db to use String enums
        builder.UseStringEnums();

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
    }
}

public static class ModelBuilderExtension
{
    public static IServiceCollection ConfigureDbModel(this IServiceCollection serviceCollection, ModelBuilderDelegate configure)
    {
        serviceCollection.AddSingleton(configure);
        return serviceCollection;
    }
    
    /// <summary>
    /// Find primary key values for given entity.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static object?[] GetPrimaryKey(this DbContext ctx, object entity)
    {
        var etype = entity.GetType();
        var key = ctx.Model.FindEntityType(etype)?.FindPrimaryKey();
        if (key is null) throw new InvalidOperationException("GetPrimaryKey: no key available");

        var names = key.Properties.Select(p => p.Name);
        var values = names.Select(n => etype.GetProperty(n)?.GetValue(entity)).ToArray();
        return values;
    }
}