using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using sip.Core;
using sip.Experiments.Logs;
using sip.Experiments.Model;
using sip.Messaging.Email;

namespace sip.Experiments;

public class OrganizationControllerBase : ControllerBase
{
    public IOrganization Organization { get; set; } = null!;
}


[ApiController]
[ServiceFilter(typeof(OrganizationActionFilter))]
[Route("/api/experiments")]
public class ExperimentsController(
    ExperimentsService            experimentsService,
        ExperimentLoggingService     experimentLoggingService,
        ILogger<ExperimentsController>  logger,
        IOptions<AppOptions>            appOptions)
    : OrganizationControllerBase
{
    [HttpGet("{experimentId:guid}")]
    public async Task<Experiment> GetExperimentAsync(Guid experimentId, CancellationToken cancellationToken)
    {
        var exp = await experimentsService.GetExperimentAsync(experimentId, cancellationToken: cancellationToken);
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
    
    [HttpGet("source_cleanable")]
    public async Task<IActionResult> GetExperimentsWithSourceDirAsync(CancellationToken cancellationToken)
    {
        // Prepare filter
        var filter = new ExperimentsFilter(
            Organization,
            CustomFilter: e => e.DataSource.CleanAfter != null && e.DataSource.DtCleaned == null,
            CancellationToken: cancellationToken
        );
        
        var exps = await experimentsService.GetExperimentsAsync(filter);
        return new JsonResult(exps.Items);
    }
    
    [HttpPost("{experimentId:guid}/email")]
    public async Task<IActionResult> SendEmailNotificationAsync(Guid experimentId, [FromBody] EmailTemplateOptions data, CancellationToken ct)
    {
        var exp = await experimentsService.GetExperimentAsync(experimentId, cancellationToken: ct);
        await experimentsService.SendEmailNotificationAsync(exp, data, ct);
        return Ok();
    }
    
    [HttpPatch("{experimentId:guid}")]
    public async Task<IActionResult> PatchExperimentAsync(Guid experimentId, [FromBody] JsonElement data,  CancellationToken ct)
    {
        var strData = data.GetRawText();
        var jpatch = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonPatchDocument<Experiment>>(strData)!;
        logger.LogInformation("Patch data: {@Data}, Pid: {}, {}", jpatch, experimentId, Request.ContentType);
        await experimentsService.PatchExperimentAsync(experimentId, jpatch, ct);
        
        return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(jpatch));
    }

    [HttpPost("logs")]
    public IActionResult SubmitLogsAsync([FromBody] List<Log> logs)
    {
        // Fix the date time kind of log messages, they are UTC, but were loaded as Local by the stupid model binder!
        foreach (var log in logs) 
        {
            log.Dt = DateTime.SpecifyKind(log.Dt, DateTimeKind.Utc);
            logger.Log(log.Level, "Explog {}: {}", log.ExperimentId, log.Message);
        }
        
        Task.Run(() => experimentLoggingService.SubmitLogsAsync(logs, CancellationToken.None));
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