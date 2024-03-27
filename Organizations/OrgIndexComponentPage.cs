namespace sip.Organizations;

using Microsoft.AspNetCore.Components;

[Route("/{OrganizationId}")]
public class OrgIndexComponentPage : sip.Core.Index
{
    [Parameter] public string? OrganizationId { get; set; }
}