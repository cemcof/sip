namespace sip.Documents;

public class DocSelfHttp(HttpClient httpClient)
{
    public async Task<string> GetDocHtml(Guid documentId)
    {

        var result = await httpClient.GetStringAsync($"/documents/{documentId}/component");

        return result;
    }
}