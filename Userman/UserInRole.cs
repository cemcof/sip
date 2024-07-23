using Microsoft.EntityFrameworkCore.Metadata.Builders;
using sip.Organizations;

namespace sip.Userman;

public class UserInRole
{
    public Guid Id { get; set; }
    
    public string? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public string RoleId { get; set; } = null!;
    public AppRole Role { get; set; } = null!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public static UserInRole FromUser<TRole>(AppUser user, IOrganization? org)
    {
        return new UserInRole()
        {
            UserId = user.Id,
            OrganizationId = org?.Id,
            RoleId = typeof(TRole).Name
        };
    }
}

public class UserInRoleEntityTypeConfiguration : IEntityTypeConfiguration<UserInRole>
{
    public void Configure(EntityTypeBuilder<UserInRole> builder)
    {
        
    }
}