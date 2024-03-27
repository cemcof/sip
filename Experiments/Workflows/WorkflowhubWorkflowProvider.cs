using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using sip.Utils;

namespace sip.Experiments.Workflows;

public class WfhConfigurationProvider : ConfigurationProvider
{
    public void ParseWorkflow(JsonDocument workflow)
    {
        Data = JsonConfigParser.Parse("Workflow", workflow.RootElement);

        // Fill necessary properties (strings only)
        var propMap = new Dictionary<string, string>()
        {
            {Workflow.PROTOCOL_TYPE_KEY, "object.className"},
            {Workflow.PROTOCOL_ID_KEY, "object.id"},
            {Workflow.PROTOCOL_NAME_KEY, "object.label"},
            {Workflow.PROTOCOL_DESCRIPTION_KEY, "object.comment"},
        };
        
        foreach (var keyValuePair in propMap)
        {
            foreach (var key in Data.Keys.Where	(k => k.EndsWith(keyValuePair.Value)).ToList	())
            {
                Data[key.Replace(keyValuePair.Value, keyValuePair.Key)] = Data[key];
            }
        }
    }
};

public class WorkflowhubWorkflowProvider(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        ILogger<WorkflowhubWorkflowProvider> logger)
    : IWorkflowProvider
{
    private IConfigurationSection _ParseJsonWfToProtocols(JsonDocument workflow)
    {
        var parsed = new WfhConfigurationProvider();
        parsed.ParseWorkflow(workflow);
        var root = new ConfigurationRoot(new List<IConfigurationProvider>() {parsed});
        return root.GetRequiredSection("Workflow");
    }

    public async IAsyncEnumerable<Workflow> GetWorkflowsAsync(ExperimentOptions forExp)
    {
        logger.LogDebug("Getting workflows from the workflowhub...");
        if (forExp.Workflowhub is null)
        {
            logger.LogDebug("No workflowhub configured, skipping...");
            yield break;
        }

        // Cache
        if (memoryCache.TryGetValue(forExp.Workflowhub, out List<Workflow>? cachedWorkflows))
        {
            logger.LogDebug("Returning cached workflows from workflowhub");
            foreach (var cachedWorkflow in cachedWorkflows!)
            {
                yield return cachedWorkflow;
            }

            yield break;
        }


        WorfklowHubOptions opts = forExp.Workflowhub;

        HttpClient client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(opts.BaseUrl);

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

        async Task<Workflow> GetWorkflowByIdAsync(string id)
        {
            // Try cache first 
            if (memoryCache.TryGetValue($"workflow/{opts.CollectionId}/{id}", out Workflow? cachedWorkflow))
            {
                logger.LogDebug("Returning cached workflow {}", id);
                return cachedWorkflow!;
            }

            // Get basic info - name and description
            var urlInfo = $"workflows/{id}?format=json";
            var wfInfo = (await client.GetFromJsonAsync<JsonDocument>(urlInfo))!;
            var attributes = wfInfo.RootElement.GetProperty("data").GetProperty("attributes");
            
            var wf = new Workflow()
            {
                Title = attributes
                    .GetProperty("title")
                    .GetString()!,
                Description = attributes
                    .GetProperty("description")
                    .GetString()!,
                Diagram = new Uri(new Uri(opts.BaseUrl), $"workflows/{id}/diagram").ToString(),
                Provider = "WorkflowHub"
            };

            // Get JSON workflow
            var version = attributes.GetProperty("versions")
                .EnumerateArray()
                .MaxBy(v => v.GetProperty("version").GetDouble());
            var gitTreePath = version.GetProperty("tree").GetString()!.Trim('/');
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
            var workflowJson = JsonSerializer.Deserialize<JsonDocument>(workflowText);
            wf.Protocols = _ParseJsonWfToProtocols(workflowJson!);

            // Put it to the cache
            memoryCache.Set($"workflow/{opts.CollectionId}/{id}", wf, opts.CacheTime);
            return wf;
        }

        var url = $"collections/{opts.CollectionId}/items?format=json";
        var response = await client.GetFromJsonAsync<JsonDocument>(url);

        foreach (var wf in response!.RootElement.GetProperty("data").EnumerateArray())
        {
            Workflow? workflow;

            try
            {
                var (id, title) = GetWorkflowIdAndTitleFromItem(wf);

                if (id is null || title is null)
                    continue;

                if (opts.Pattern is not null && !Regex.IsMatch(title, opts.Pattern))
                    continue;

                logger.LogDebug("Getting workflowhub workflow: {}, {}", id, title);

                workflow = await GetWorkflowByIdAsync(id);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while getting workflow from workflowhub");
                continue;
            }

            yield return workflow;
        }
    }

    public Task<Workflow?> GetWorkflowByIdAsync(string id)
    {
        if (!id.StartsWith("WFH-")) return null;
        
        throw new NotImplementedException();
    }
}