using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using sip.Experiments;
using sip.Utils;

namespace sip.Organizations.Centers;

[ApiController]
[Route("/api/centers")]
public class CenterController(OrganizationActionFilter organizationActionFilter, CenterManager centerManager, ILogger<CenterController> logger)
    : OrganizationControllerBase
{
    [HttpGet]
    [ServiceFilter(typeof(OrganizationActionFilter))]
    public async Task<IActionResult> GetCenterConfigurationAsync()
    {
        var cener = await centerManager.GetStatusFromStringAsync(Organization.Id);
        return new JsonResult(cener.Configuration);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitCenterConfigurationAsync([FromBody] JsonElement configJson, [FromQuery] string nodeSubmitter)
    {
        try
        {
            var organizationId = configJson
                .GetProperty("Center")
                .GetProperty("Identifier")
                .GetString();
            
            var kkey = organizationActionFilter.ExtractSecretKey(HttpContext.Request);
            var orgName = centerManager.KeyToOrgName(kkey);
            if (orgName != organizationId)
            {
                // Attempting to set configuration of different organization then that we are authorized for
                return BadRequest();
            }

            await centerManager.SubmitCenterConfigurationAsync(nodeSubmitter, organizationId, configJson);

        }
        catch (NullReferenceException)
        {
            return BadRequest();
        }
        catch (NotAvailableException)
        {
            return Unauthorized();
        }

        return Ok();
    }

    [HttpGet("ping/{nodeName}")]
    public async Task<IActionResult> PingCenterAsync(string nodeName)
    {
        try
        {
            var secretKey = organizationActionFilter.ExtractSecretKey(HttpContext.Request);
            var center = await centerManager.GetFromKeyAsync(secretKey);
            
            if (!center.Nodes.ContainsKey(nodeName))
            {
                center.Nodes[nodeName] = new CenterNodeStatus() {LastPing = DateTime.UtcNow};
            }
            else
            {
                center.Nodes[nodeName].LastPing = DateTime.UtcNow;
            }
            
            center.LastPing = DateTime.UtcNow;
            return Content(center.LastChange.ToPyCompatibleUtcIso());
            
        }
        catch (NotAvailableException e)
        {
            logger.LogDebug(e, "Not available, returning dt minvalue");
            return Content(DateTime.MinValue.ToPyCompatibleUtcIso());
        }
    }
}