using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using sip.Forms.Dynamic;
using sip.Utils;

namespace sip.Experiments.Workflows;

public class WorkflowhubWorkflowProvider(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        ILogger<WorkflowhubWorkflowProvider> logger,
        IOptionsMonitor<WorkflowhubOptions> wfhOptions)
    : IWorkflowProvider
{
    public async IAsyncEnumerable<Workflow> GetWorkflowsAsync(WorkflowFilter filter)
    {
        var wfhOpts = wfhOptions.Get(filter.Organization);
        
        logger.LogDebug("Getting workflows from the workflowhub...");
        if (wfhOpts.CollectionId is null)
        {
            logger.LogDebug("No workflowhub configured, skipping...");
            yield break;
        }

        // Cache - TODO - not used yet
        if (memoryCache.TryGetValue(wfhOpts, out List<Workflow>? cachedWorkflows))
        {
            logger.LogDebug("Returning cached workflows from workflowhub");
            foreach (var cachedWorkflow in cachedWorkflows!)
            {
                yield return cachedWorkflow;
            }

            yield break;
        }

        using var client = _PrepareHttpClient(wfhOpts);

        (string?, string?) GetWorkflowIdAndTitleFromItem(JsonElement item)
        {
            var dataProp = item.GetProperty("relationships")
                .GetProperty("asset")
                .GetProperty("data");

            var metaProp = item.GetProperty("relationships")
                .GetProperty("asset")
                .GetProperty("meta");
            
            if (dataProp.GetProperty("type").GetString() == "workflows")
                return (dataProp.GetProperty("id").GetString()!, metaProp.GetProperty("title").GetString()!);

            return (null, null);
        }

        var url = $"collections/{wfhOpts.CollectionId}/items?format=json";
        var response = await client.GetFromJsonAsync<JsonDocument>(url);

        foreach (var wf in response!.RootElement.GetProperty("data").EnumerateArray())
        {
            Workflow? workflow;

            try
            {
                var (id, title) = GetWorkflowIdAndTitleFromItem(wf);

                if (id is null || title is null)
                    continue;

                if (wfhOpts.Pattern is not null && !Regex.IsMatch(title, wfhOpts.Pattern))
                    continue;

                logger.LogDebug("Getting workflowhub workflow: {}, {}", id, title);

                workflow = await GetWorkflowByIdAsync(id, filter.Organization, client);
                if (workflow is null) throw new InvalidOperationException($"Workflow {id} not found in workflowhub");
                if (!filter.Tags.Match(workflow.Tags))
                    continue;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while getting workflow from workflowhub");
                continue;
            }

            yield return workflow;
        }
    }

    private HttpClient _PrepareHttpClient(WorkflowhubOptions wfhOpts)
    {
        HttpClient client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(wfhOpts.BaseUrl);
        return client;
    }

    public Task<Workflow?> GetWorkflowByIdAsync(string id, IOrganization organization)
    {
        using var httpc = _PrepareHttpClient(wfhOptions.Get(organization));
        return GetWorkflowByIdAsync(id, organization, httpc);
    }

    public async Task<Workflow?> GetWorkflowByIdAsync(string id, IOrganization organization, HttpClient client)
    {
        var wfhOpts = wfhOptions.Get(organization);
        
        // WFH ids are integers
        if (!int.TryParse(id, out _)) return null;
        
        // Try cache first 
        if (memoryCache.TryGetValue($"workflow/{wfhOpts.CollectionId}/{id}", out Workflow? cachedWorkflow))
        {
            logger.LogDebug("Returning cached workflow {}", id);
            return cachedWorkflow!;
        }

        // Get basic info - name, description and tags
        var urlInfo = $"workflows/{id}?format=json";
        JsonDocument wfInfo;
        
        try
        {
            wfInfo = (await client.GetFromJsonAsync<JsonDocument>(urlInfo))!;
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
                return null;
            throw;
        }
        
        var attributes = wfInfo.RootElement.GetProperty("data").GetProperty("attributes");
        
        var wf = new Workflow()
        {
            Title = attributes
                .GetProperty("title")
                .GetString()!,
            Description = attributes
                .GetProperty("description")
                .GetString()!,
            Diagram = new Uri(new Uri(wfhOpts.BaseUrl), $"workflows/{id}/diagram").ToString(),
            Tags = attributes.GetProperty("tags")
                .EnumerateArray()
                .Select(a => a.GetString() ?? string.Empty)
                .ToList(),
            Provider = "WorkflowHub"
        };

        // Get JSON workflow
        var latestVersion = attributes.GetProperty("versions")
            .EnumerateArray()
            .First(v => v.GetProperty("version").GetDecimal() == attributes.GetProperty("latest_version").GetDecimal());
        var gitTreePath = latestVersion.GetProperty("tree").GetString()!.Trim('/');
        var gitRawPath = gitTreePath.Replace("/tree", "/raw");
        var gitTreeResponse = await client.GetFromJsonAsync<JsonDocument>(gitTreePath  + "?format=json");
        var files = gitTreeResponse!.RootElement.GetProperty("tree")
            .EnumerateArray()
            .Where(x => x.GetProperty("type").GetString()! == "blob")
            .Select(x => (x.GetProperty("name").GetString()!, x.GetProperty("path").GetString()!))
            .ToList();

        // var diagramPath = files.FirstOrDefault(x => ContentType.Parse(MimeKit.MimeTypes.GetMimeType(x.Item1)).IsImage());
        var workflowPath = files.FirstOrDefault(x => x.Item1.EndsWith(".json") || x.Item1.EndsWith(".template")).Item2;
        var urlWorkflow = $"{gitRawPath}/{workflowPath}";
        
        // Get JSON workflow, but as a raw text
        var workflowText = await client.GetStringAsync(urlWorkflow);
        // In some cases there can be parasite characters before json starts, strip them until [ is found
        workflowText = workflowText[workflowText.IndexOf('[')..];
        
        // Data to JSON and then to object (dic/list)
        wf.Data = JsonSerializer.Deserialize<JsonDocument>(workflowText)?.RootElement.ToObject();
        
        // Put it to the cache
        memoryCache.Set($"workflow/{wfhOpts.CollectionId}/{id}", wf, wfhOpts.CacheTime);
        return wf;
    }
}