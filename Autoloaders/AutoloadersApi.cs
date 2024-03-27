using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using sip.Experiments;
using sip.Organizations;

namespace sip.Autoloaders;


// Autoloader incoming data is of following dictionary structure:
// [InstrumentName]:
//      [DataFileName]:
//          [TimeStamp]:
//              [StateKey]: [StateValue]
//              [StateKey]: [StateValue]
//              [StateKey]: [StateValue]
//              ...
//          [TimeStamp]:
//              ...
//          ...
//      [DataFileName]:
//          ...
// [InstrumentName]:
// ...
using AutoloadersRawData = Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>; // LOL

[ApiController]
[ServiceFilter(typeof(OrganizationActionFilter))]
[Route("/api/autoloaders")]
public class AutoloadersController(AutoloadersService autoloadersService, ILogger<AutoloadersController> logger)
    : OrganizationControllerBase
{
    [HttpPost]
    public Task<IActionResult> SubmitAutoloaderStatesAsync([FromBody] AutoloadersRawData data)
    {
        // Reformat the data to something more reasonable
        // The following code is kinda scary but it really just converts the raw dictionary structure into more reasonable
        // structure made of AutoloaderInstrumentData and AutoloaderStates records
        var parsedData = data.Select(
            kv => new AutoloaderInstrumentData(
                kv.Key, kv.Value.Select(s => new AutoloaderStates(
                    s.Value.ToDictionary(
                        x => DateTime.ParseExact(x.Key, "yyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                        x => x.Value
                    ))).ToList()));
        
        // Now we submit the converted data to our autoloader service
        autoloadersService.UpdateData(parsedData.ToList(), Organization);
        logger.LogDebug("Received autoloaders data");
        
        return Task.FromResult<IActionResult>(Ok());
    } 
}