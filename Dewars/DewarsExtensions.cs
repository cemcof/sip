// ReSharper disable CollectionNeverUpdated.Global

using sip.Core;
using sip.Organizations;

namespace sip.Dewars;

public static class DewarsExtensions
{
    public static void AddDewars(this IServiceCollection services, IConfigurationRoot conf)
    {
        services.ConfigureDbModel(mb =>
        {
            var entity = mb.Entity<Tube>();
            entity.HasKey(nameof(Tube.Structure), nameof(Tube.OrganizationId));
            entity.HasOne<Organization>(e => e.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId);
            entity.Property(e => e.Structure)
                .HasMaxLength(64);

            entity.Ignore(x => x.Deck);
            entity.Ignore(x => x.Dewar);
            entity.Ignore(x => x.Holder);
            entity.Ignore(x => x.Position);
            entity.Ignore(x => x.TrimmedId);
            entity.Ignore(x => x.IsEmpty);
        });
        
        services.AddOptions<DewarsOptions>()
            .GetOrganizationOptionsBuilder(conf)
            .BindOrganizationConfiguration("Dewars");
        
        services.AddSingleton<TubesService>();
    }
}