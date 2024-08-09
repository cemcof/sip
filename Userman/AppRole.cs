using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace sip.Userman;

public class AppRole : ITreeItem<AppRole>, IEquatable<AppRole>
{
    public bool Equals(AppRole? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AppRole) obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(AppRole? left, AppRole? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AppRole? left, AppRole? right)
    {
        return !Equals(left, right);
    }

    [StringLength(128)] public string Id { get; set; } = null!;

    public string? ParentId { get; set; }
    public AppRole? Parent { get; set; }
    public ICollection<AppRole> Children { get; set; } = new List<AppRole>();

    public string Name { get; set; } = string.Empty;
    [NotMapped] public string DisplayName => string.IsNullOrEmpty(Name) ? Id.Humanize() : Name; 

    public string Description { get; set; } = string.Empty;

    public ICollection<UserInRole> UserInRoles { get; set; } = new List<UserInRole>();
}


public class AppRoleEntityTypeConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.HasOne(r => r.Parent)
            .WithMany(r => r.Children)
            .HasForeignKey(r => r.ParentId)
            .IsRequired(false);
    }
}