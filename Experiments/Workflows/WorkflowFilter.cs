namespace sip.Experiments.Workflows;

public record WorkflowFilter(
    IOrganization Organization,
    TagFilter Tags);