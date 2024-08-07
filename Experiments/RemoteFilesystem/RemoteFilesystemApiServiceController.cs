using Microsoft.AspNetCore.Mvc;

namespace sip.Experiments.RemoteFilesystem;

[Route("/api/fs")]
public class RemoteFilesystemApiServiceController(RemoteFilesystemApiService remoteFilesystemApiService) : Controller
{
    [HttpGet]
    public Task<IActionResult> GetPending()
    {
        var pendings = remoteFilesystemApiService.GetPendingPaths();

        var result = pendings.ToDictionary(k => k.Key, v => new { v.Value.Path, v.Value.Scope });
        var response = Ok(result);

        return Task.FromResult<IActionResult>(response);
    }

    [HttpPost]
    public Task<IActionResult> SubmitResults([FromBody] Dictionary<Guid, List<FileSystemItemInfo>> results)
    {
        remoteFilesystemApiService.SubmitResults(results);
        return Task.FromResult<IActionResult>(Ok());
    }
}