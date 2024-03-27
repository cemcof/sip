using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using sip.Utils.NtlmRequest;

namespace sip.Utils.Crm;

public class CrmGetReturn<T>
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<T> value { get; set; } = new();
}

public class CrmService(HttpNtlmPyWrapper httpClient, IOptions<CrmOptions> options)
{
    public Task<List<T>> GetEntitiesAsync<T>(string entityName, string? filter = null, string? select = null)
    {
        // var query = HttpUtility.ParseQueryString(string.Empty);
        // if (filter != null) query["$filter"] = filter;
        // if (@select != null) query["$select"] = @select;
        //
        // var result = await _httpClient.GetFromJsonAsync<CrmGetReturn<T>>(entityName + "?" + query);
        // return result!.value;
        throw new NotImplementedException();
        // return Task.FromResult<List<T>>(null);
    }


    public Task<string> PostEntityAsync(string entityName, Dictionary<string, object> postData)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> FetchByXmlAsync<T>(string entityName, string xmlFetch)
    {
        // var result =
        //     await _httpClient.GetFromJsonAsync<CrmGetReturn<T>>(entityName + "?" + "fetchXml=" +
        //                                                         HttpUtility.UrlEncode(xmlFetch));

        // return result!.value;
        throw new NotImplementedException();
        // return Task.FromResult<List<T>>(null!);
    }

    public async Task<JsonNode> FetchRawByXmlRaw(string entityName, string xmlFetch)
    {
        
        var result = await httpClient.GetStringAsync(options.Value.BaseUrl + entityName + "?" + "fetchXml=" +
                                                        HttpUtility.UrlEncode(xmlFetch), options.Value.Username, options.Value.Password);
        return JsonSerializer.Deserialize<JsonNode>(result)!;
    }

    public Task<string> PingBase()
    {
        return httpClient.GetStringAsync(options.Value.BaseUrl.ToString(), options.Value.Username, options.Value.Password);
    }
    
    
}