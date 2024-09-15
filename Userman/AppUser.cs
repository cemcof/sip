using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using sip.Auth.Orcid;

namespace sip.Userman;


public interface IUserIdentification
{
    Guid Id { get; }
}

public interface IUserInfo : IUserIdentification
{
    string? EmailAddress { get; }
    string? Firstname { get;  }
    string? Lastname { get; }   
}

public interface IUserClientInfo : IUserInfo
{
    IPAddr IpAddress { get; }
}

public record UserInfo(Guid Id, string? EmailAddress, string? Firstname, string? Lastname) : IUserInfo;

public record ClientInfo
    (Guid Id, string? EmailAddress, string? Firstname, string? Lastname, IPAddr IpAddress) 
    : UserInfo(Id, EmailAddress, Firstname, Lastname), IUserClientInfo;



public sealed class AppUser : IdentityUser<Guid>, IStringFilter, IEquatable<AppUser>, IUserInfo
{
    #region EQUALS

        public bool Equals(AppUser? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Id == default) return false;
            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is AppUser other && Equals(other);
        }

        public override int GetHashCode()
        {
            // TODO - how to handle this warning
            return Id.GetHashCode();
        }

    #endregion

    public string? EmailAddress => PrimaryContact.Email;
    public string? Firstname => PrimaryContact.Firstname;
    public string? Lastname => PrimaryContact.Lastname;

    [YamlIgnore] public List<Contact> Contacts { get; set; } = new();
    [YamlIgnore] public Contact PrimaryContact => Contacts.FirstOrDefault(c => c.IsPrimary) ?? Contacts.First();

    [NotMapped] public string? Identifier => 
        IdentityUserLogins.FirstOrDefault(iu => iu.LoginProvider == OrcidDefaults.LOGIN_PROVIDER)?.ProviderKey; 

    [NotMapped] public string Fullname => Firstname + " " + Lastname;

    [NotMapped] public string Fullcontact => $"{Fullname} <{EmailAddress}>";

    [YamlIgnore] public List<UserInRole> UserInRoles { get; set; } = new();

    public DateTime DtCreated { get; set; }

    public AppUser()
    {
        UserName = Guid.NewGuid().ToString();
    }

    public override string ToString()
    {
        var result = Fullname + " ";
        if (!string.IsNullOrWhiteSpace(EmailAddress)) result += $"<{EmailAddress}>";
        if (string.IsNullOrWhiteSpace(result))
        {
            return $"Unidentified <{Id}>";
        }

        return result.Trim();
    }

    [YamlIgnore] public IEnumerable<(AppRole role, Organization? organization)> Roles
    {
        get
        {
            // TODO - make this distinct?
            var uirs = UserInRoles ?? throw NotLoadedException.ThrowForType<UserInRole>();
            foreach (var uir in uirs)
            {
                var roles = Tree<AppRole>.EnumerateToRoot(uir.Role);
                foreach (var r in roles)
                {
                    yield return (r, uir.Organization);
                }
            }
        }
    }


    public static AppUser NewEmpty()
    {
        var usr =  new AppUser()
        {
            UserName = Guid.NewGuid().ToString(),
            DtCreated = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        
        usr.Contacts.Add(new Contact() {IsPrimary = true});
        return usr;
    }
    
    [YamlIgnore, JsonIgnore] public List<IdentityUserLogin<Guid>> IdentityUserLogins { get; set; } = new();

    // Defined as property to simplify binding to it and serializing it
    [NotMapped]
    public string? Orcid
    {
        get => IdentityUserLogins.FirstOrDefault(iu => iu.LoginProvider == OrcidDefaults.LOGIN_PROVIDER)?.ProviderKey; 
        set
        {
            IdentityUserLogins.RemoveAll(iu => iu.LoginProvider == OrcidDefaults.LOGIN_PROVIDER);

            if (!string.IsNullOrEmpty(value))
            {
                IdentityUserLogins.Add(new IdentityUserLogin<Guid>()
                {
                    LoginProvider = OrcidDefaults.LOGIN_PROVIDER,
                    ProviderKey = value,
                    ProviderDisplayName = OrcidDefaults.DisplayName
                });
            }
        }
    }


    public bool IsFilterMatch(string? filter = null)
    {
        return StringUtils.IsFilterMatchAtLeastOneOf(filter, Fullcontact, PrimaryContact.Affiliation,
            Identifier ?? string.Empty);
    }

    public bool IsInRole<TRoleRef>(Organization organization)
        => IsInRole(typeof(TRoleRef).Name, organization);


    public bool IsInRole(string role, Organization? organization = null)
    {
        if (organization is null)
        {
            return UserInRoles.Any(ur => ur.RoleId == role && ur.OrganizationId == null);
        }

        // Does this organization or any in chain to parent have this role?
        return Tree<Organization>.EnumerateToRoot(organization).Any(
            o => UserInRoles.Any(ur => ur.RoleId == role && ur.OrganizationId == o.Id)
        );
    }

    /// <summary>
    /// Check if user is in all given roles for given organization
    /// To perform OR operation over the roles, include them in one string and separate them by comma
    /// </summary>
    /// <param name="roles"></param>
    /// <param name="organization"></param>
    /// <returns></returns>
    public bool IsInRole(IReadOnlyList<string> roles, Organization? organization = null)
    {
        return roles.All(
            r => r.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Any(rr => IsInRole(rr, organization))
        );
    }

}

public class AppUserEntityTypeConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        
    }
}