using sip.Experiments;

namespace sip.RemoteAccess;

public class RemoteAccessOptions
{
    public Dictionary<IInstrument, List<RemoteConnectionInfo>> Instruments { get; set; } = new();
}
