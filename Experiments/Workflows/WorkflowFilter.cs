namespace sip.Experiments.Workflows;

public record WorkflowFilter(
    IOrganization Organization,
    string? Engine,
    string? Instrument,
    string? Technique
    );