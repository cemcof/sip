using Microsoft.AspNetCore.Mvc;

namespace sip.Environment;

[ApiController]
public class EnvironmentController(EnvironmentService environmentService, ILogger<EnvironmentController> logger)
    : ControllerBase
{
    private readonly ILogger<EnvironmentController> _logger             = logger;


    [HttpGet("/environment/{variable}/graph")]
    // [Authorize(Policy = "LabOverview")]
    public IActionResult GraphImage(string variable)
    {
        var imgData = environmentService.Data!.Sensors
            .SelectMany(s => s.Variables.Values)
            .First(x => x.VarId == variable)
            .PngGraph;
        
        return File(imgData, MimeTypes.MimeTypeMap.GetMimeType("png"));
    }
    
}