using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace sip.Organizations;

public class OrganizationEntityDefinition : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder
            .HasOne(o => o.Parent)
            .WithMany(o => o.Children)
            .HasForeignKey(o => o.ParentId);

        builder.Property(o => o.ParentId)
            .HasMaxLength(128);
        
        builder.Property(o => o.Abbreviation)
            .HasMaxLength(32);
        
        builder.Property(o => o.DisplayName)
            .HasMaxLength(256);
        
        builder.Property(o => o.Description)
            .HasMaxLength(1024);

        builder
            .HasDiscriminator();

        builder
            .HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasMaxLength(128);
        
        builder.Property(p => p.LinkId)
            .HasMaxLength(32);
        builder.Property(p => p.Name)
            .HasMaxLength(128);
        builder.Property(p => p.Url)
            .HasMaxLength(256);
        

    }      
}