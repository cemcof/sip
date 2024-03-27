using sip.Utils;

namespace sip.Organizations;

public interface IOrganization : ITreeItem<IOrganization>
{
    string Id { get; }
    string Name { get; }
    string Abbreviation { get; }
    string LinkId { get; }
}

// public record OrganizationRecord(string Id, string Name, string Abbreviation, string LinkId) : IOrganization;