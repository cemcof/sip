using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace sip.Utils.Items;

public record BasicFilter(
    int Count = -1,
    int Offset = 0,
    string? FilterQuery = null,
    CancellationToken CancellationToken = default
    ) : IFilter;

public static class FilterExtensions
{
    public static IFilter ToFilter(this ItemsProviderRequest req, string? queryString = null)
    {
        return new BasicFilter(
            req.Count,
            req.StartIndex,
            queryString,
            req.CancellationToken
        );
    }
}