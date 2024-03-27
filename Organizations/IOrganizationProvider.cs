namespace sip.Organizations;

public interface IOrganizationProvider
{
    IEnumerable<Organization> GetAll();
    Organization GetFromString(string organization);
    Organization? GetFromStringOrDefault(string organization);
}