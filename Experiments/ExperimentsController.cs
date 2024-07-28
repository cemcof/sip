using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using sip.Core;
using sip.Experiments.Logs;
using sip.Experiments.Model;
using sip.Messaging.Email;
using sip.Organizations;

namespace sip.Experiments;

public class OrganizationControllerBase : ControllerBase
{
    public IOrganization Organization { get; set; } = null!;
}


[ApiController]
[ServiceFilter(typeof(OrganizationActionFilter))]
[Route("/api/experiments")]
public class ExperimentsController(
        IDbContextFactory<AppDbContext> dbContextFactory,
        ExperimentEngine                experimentEngine,
        ExperimentsService            experimentsService,
        ILogger<ExperimentsController>  logger,
        IOptions<AppOptions>            appOptions)
    : OrganizationControllerBase
{
    [HttpGet("{experimentId:guid}")]
    public async Task<Experiment> GetExperimentAsync(Guid experimentId, CancellationToken cancellationToken)
    {
        var exp = await experimentEngine.GetExperimentAsync(experimentId, cancellationToken);
        return exp;
    }

    
    [HttpGet]
    public async Task<IActionResult> GetExperimentsAsync(string? storageState, string? expState, string? processingState, string? publicationState,
        CancellationToken cancellationToken)
    {
        List<T>? ParseStates<T> (string? states) where T : struct
        {
            if (states is null) return null;
            return states.Split(',').Select(Enum.Parse<T>).ToList();
        } 
        
        // Prepare filter
        var filter = new ExperimentsFilter(
            Organization,
            ExpStates: ParseStates<ExpState>(expState),
            StorageStates: ParseStates<StorageState>(storageState),
            ProcessingStates: ParseStates<ProcessingState>(processingState),
            PublicationStates: ParseStates<PublicationState>(publicationState),
            CancellationToken: cancellationToken
        );
        
        var exps = await experimentsService.GetExperimentsAsync(filter);
        return new JsonResult(exps.Items);
    }


    // DELME
    // [HttpPatch("{experimentId:guid}/published")]
    // public async Task<IActionResult> ExperimentPublishedAsync(Guid experimentId, [FromBody] ExperimentPublished data, CancellationToken ct)
    // {
    //     // DELME and send notification as another api request
    //     var exp = await experimentEngine.GetExperimentAsync(experimentId, ct);
    //     if (exp.Publication.PublicationState != PublicationState.PublicationRequested) return BadRequest("Incorrect experiment state");
    //     
    //     // Patch experiment 
    //     await using var context = await dbContextFactory.CreateDbContextAsync(ct);
    //     var entry = context.Entry(exp.Storage);
    //     entry.State = EntityState.Unchanged;
    //     exp.Publication.Doi = data.Doi;
    //     if (data.Target is not null) exp.Storage.Target = data.Target;
    //     await context.SaveChangesAsync(ct);
    //     
    //     // Update state
    //     await experimentEngine.ChangeStatusAsync(ExpState.Published, exp);
    //     
    //     // Send notification email
    //     if (data.Notification is not null)
    //     {
    //         await experimentEngine.SendEmailNotificationAsync(exp, data.Notification.Subject, data.Notification.Body);
    //     }
    //
    //     return Ok();
    // }
    
    [HttpPost("{experimentId:guid}/email")]
    public async Task<IActionResult> SendEmailNotificationAsync(Guid experimentId, [FromBody] EmailTemplateOptions data, CancellationToken ct)
    {
        var exp = await experimentEngine.GetExperimentAsync(experimentId, ct);
        await experimentEngine.SendEmailNotificationAsync(exp, data.Subject, data.Body);
        return Ok();
    }
    
    [HttpPatch("{experimentId:guid}")]
    public async Task<IActionResult> PatchExperimentAsync(Guid experimentId, [FromBody] JsonElement data,  CancellationToken ct)
    {
        var strData = data.GetRawText();
        var jpatch = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonPatchDocument<Experiment>>(strData)!;
        logger.LogInformation("Patch data: {@Data}, Pid: {}, {}", jpatch, experimentId, Request.ContentType);
        await experimentEngine.PatchExperimentAsync(experimentId, jpatch, ct);
        
        return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(jpatch));
        await Task.Delay(0);
        // return Ok( Newtonsoft.Json.JsonConvert.SerializeObject(data));
    }

   

    // [HttpPost("{experimentId:guid}/processing/result")]
    // public async Task<IActionResult> SubmitResultAsync([FromBody] string html, Guid experimentId, CancellationToken ct)
    // {
    //     await _experimentEngine.SubmitResultAsync(experimentId, html, ct);
    //     return Ok();
    // }
    //
    // [HttpGet("{experimentId:guid}/processing/result")]
    // public async Task<IActionResult> GetResultAsync(Guid experimentId, CancellationToken ct)
    // {
    //     var result = await _experimentEngine.GetResultAsync(experimentId, ct);
    //     
    //     return new JsonResult(result);
    // }
    
    [HttpPost("logs")]
    public async Task<IActionResult> SubmitLogsAsync([FromBody] List<Log> logs, CancellationToken ct)
    {
        // Fix the date time kind of log messages, they are UTC, but were loaded as Local by the stupid model binder!
        foreach (var log in logs) 
        {
            log.Dt = DateTime.SpecifyKind(log.Dt, DateTimeKind.Utc);
        }
        
        logger.LogInformation("Received logs: {@Logs}", logs);

        try
        {
            await experimentEngine.SubmitLogsAsync(logs, ct);
        }
        catch (Exception e) when (e is OperationCanceledException or TaskCanceledException)
        {
            logger.LogWarning("Operation canceled");
        }
        
        return Ok();
    }
    
    [HttpPost("{experimentId:guid}/data")]
    public async Task<IActionResult> UploadResults(Guid experimentId, List<IFormFile> formFiles)
    {
        var experimentDataBase = Path.Combine(appOptions.Value.DataDirectory, "experiment_results", experimentId.ToString());
        Directory.CreateDirectory(experimentDataBase);
        foreach (var formFile in formFiles)
        {
            var fileRelativePath = formFile.FileName.Replace("__", "/");
            var filePath = Path.Combine(experimentDataBase, fileRelativePath);
            await using var stream = System.IO.File.Create(filePath);
            await formFile.CopyToAsync(stream);
        }

        return Ok();
    }
    
}