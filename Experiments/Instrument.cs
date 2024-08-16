using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace sip.Experiments;

public interface IInstrument
{
    Guid Id { get; }
    string Name { get; }
    string Alias { get; }
    
    /// <summary>
    /// An organization this instrument belongs to
    /// </summary>
    IOrganization Organization { get; }

    string DisplayName { get; }
    string DisplayTheme { get; }
}

public record InstrumentRecord(
    Guid Id,
    string Name,
    string Alias,
    IOrganization Organization,
    string DisplayName,
    string DisplayTheme
) : IInstrument
{
    public virtual bool Equals(InstrumentRecord? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Organization.Equals(other.Organization);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Organization);
    }
}

public class InstrumentsOptions
{
    public List<IInstrument> Instruments { get; set; } = new();

    public IInstrument GetInstrumentByName(string name)
    {
        return Instruments.First(i => i.Name == name);
    }
}
