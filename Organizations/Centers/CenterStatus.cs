using System.Text.Json;

namespace sip.Organizations.Centers;

public class CenterStatus(IOrganization organization, string submittedByNode, JsonElement configuration)
{
    public DateTime LastPing { get; set; }
    public DateTime LastChange { get; set; }

    public JsonElement Configuration { get; set; } = configuration;
    public IOrganization Organization { get; } = organization;
    public string SubmittedByNode { get; set; } = submittedByNode;

    public Dictionary<string, CenterNodeStatus> Nodes { get; } = new();
};