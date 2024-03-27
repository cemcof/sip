using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using sip.Userman;

namespace sip.Projects;

public class ProjectsFilterUserSettings
{

    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public ProjectsFilter ProjectsFilter { get; set; } = new();

}

public class ProjectsFilterEntityTypeConfiguration : IEntityTypeConfiguration<ProjectsFilterUserSettings>
{
    public void Configure(EntityTypeBuilder<ProjectsFilterUserSettings> builder)
    {
        builder.Property(p => p.ProjectsFilter)
            .HasConversion(
                v => JsonConvert.SerializeObject(v,
                    new JsonSerializerSettings() {TypeNameHandling = TypeNameHandling.All}),
                v => JsonConvert.DeserializeObject<ProjectsFilter>(v,
                    new JsonSerializerSettings() {TypeNameHandling = TypeNameHandling.All})!);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);

    }
}

