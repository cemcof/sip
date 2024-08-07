using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sip.Userman;

// From old AppUsers to contact: 
// insert into Contact (ContactId, Email, Firstname, Lastname, IsPrimary, AppUserId) 
// SELECT uuid(), Email, Firstname, Lastname, 1, Id from AppUsers where 1

public class Contact : IStringFilter
{
    public Guid ContactId { get; set; }
    
    [StringLength(128), EmailAddress] 
    public string? Email { get; set; }
    public bool EmailVerified { get; set; }
    
    [MaxLength(256)] public string? Firstname { get; set; } 
    [MaxLength(256)] public string? Lastname { get; set; } 
    [MaxLength(256)] public string? Phone { get; set; }
    
    [Render(Tip =
        "User affiliation information in the following format (separate entry parts with a semi-colon): ; <Institute/Organization>; ")]
    [MaxLength(256)] public string? Affiliation { get; set; }
    [MaxLength(256)] public string? Position { get; set; }

    [NotMapped]
    public string? Fullname =>
        (!string.IsNullOrWhiteSpace(Firstname) && !string.IsNullOrWhiteSpace(Lastname)) ? Firstname + " " + Lastname : null;

    [NotMapped] public string? Fullcontact =>
        (!string.IsNullOrWhiteSpace(Email)) ? 
            !string.IsNullOrWhiteSpace(Fullname) ? $"{Fullname} <{Email}>" : $"<{Email}>"
            : null;
    
    public bool IsPrimary { get; set; } = false;

    public Guid AppUserId { get; set; }
    [JsonIgnore] public AppUser AppUser { get; set; } = null!;

    public bool IsFilterMatch(string? filter) => 
        StringUtils.IsFilterMatchAtLeastOneOf(filter, Firstname, Lastname, Email, Affiliation);
}