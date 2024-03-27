
// ReSharper disable InconsistentNaming

namespace sip.RemoteAccess;

// public record GuacaNewTokenReponse
// {
//     public string? authToken { get; set; }
//     public string? dataSource { get; set; }
//     public List<string> availableDataSources { get; set; } = new();
// }

public record GuacaNewTokenReponse(
    string authToken,
    string dataSource,
    List<string> availableDataSources);

public record GuacamoleAuthModel(
    string username,
    string expires,
    Dictionary<string, GuacamoleAuthConnectionModel> connections);

public record GuacamoleAuthConnectionModel(
    string id,
    Dictionary<string, object> parameters
);

public record GuacamoleAuthConnectConnectionModel(
    string protocol,

    string id,
    Dictionary<string, object> parameters
    ) : GuacamoleAuthConnectionModel(id, parameters);

public record GuacamoleAuthJoinConnectionModel(
    string join,
    
    string id,
    Dictionary<string, object> parameters
    ) : GuacamoleAuthConnectionModel(id, parameters);