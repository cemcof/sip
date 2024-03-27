using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using sip.Experiments;
using sip.Organizations.Centers;
using sip.Utils;

namespace sip.Organizations;

/// <summary>
/// Determine and authorize request by organization.
/// If organization exists and is authorized, pass the organization to the underlying controller.
/// Otherwise reject the request.
/// </summary>
public class OrganizationActionFilter(CenterManager centerManager) : IAsyncActionFilter
{
    private const    string        ORGANISATION_KEY_PARAM_NAME = "lims-organization";


    public string ExtractSecretKey(HttpRequest request)
    {
        var orgKey = request.Query[ORGANISATION_KEY_PARAM_NAME];
        if (StringValues.IsNullOrEmpty(orgKey))
        {
            // No? Try headers.
            orgKey = request.Headers[ORGANISATION_KEY_PARAM_NAME];
        }
        
        if (string.IsNullOrWhiteSpace(orgKey)) 
            throw new NotAvailableException($"Organization key not found under {ORGANISATION_KEY_PARAM_NAME}");
        return orgKey!;
    }
    
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            var orgKey = ExtractSecretKey(context.HttpContext.Request);
            var org = await centerManager.GetFromKeyAsync(orgKey);
            if (context.Controller is OrganizationControllerBase cont)
            {
                cont.Organization = org.Organization;
            }
        }
        catch (NotAvailableException)
        {
            // Such organization is not available and we therefore reject the request
            context.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }
}