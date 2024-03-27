
namespace sip.Organizations.Centers;

public interface ICenterProvider
{
    // InfrastructureConfiguration? GetCenter(string center);

    IEnumerable<CenterStatus> GetAvailableCenters();
}