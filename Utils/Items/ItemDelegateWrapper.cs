using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace sip.Utils.Items;

public delegate ValueTask<ItemsProviderResult<TItem>> ItemProviderRequestWithSearchDelegate<TItem>(ItemsProviderRequest request, string? searchString); 

/// <summary>
/// IDW stands for item delegate wrapper
/// Provides several helper static methods that can wrap simpler delegates into complex delegates that is needed by
/// item renderer components.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IDW<TItem>
{
    // public static  AsSearchFilter<TFilter>()

    public static ItemsProviderDelegate<TItem> WrapList(List<TItem> items)
    {
        // Taken from Virtualize component source code
        return r => ValueTask.FromResult(new ItemsProviderResult<TItem>(
            items.Skip(r.StartIndex).Take(r.Count),
            items.Count));
    }
    
    public static ItemsProviderDelegate<TItem> WrapList(Func<Task<IList<TItem>>> listProvider)
    {
        return async r =>
        {
            var items = await listProvider();
            return new ItemsProviderResult<TItem>(items.Skip(r.StartIndex).Take(r.Count),
                items.Count);
        };
    }

    public static ItemsProviderDelegate<TItem> Wrap<TParam>(
        Func<TParam, ItemsProviderRequest, ValueTask<ItemsProviderResult<TItem>>> func, TParam param)
    {
        return request => func(param, request);
    }

    public static ItemsProviderDelegate<TItem> Wrap<TParam, TParam2>(
        Func<TParam, TParam2, ItemsProviderRequest, ValueTask<ItemsProviderResult<TItem>>> func, TParam param, TParam2 param2)
    {
        return request => func(param, param2, request);
    }


    public static ItemProviderRequestWithSearchDelegate<TItem> WrapListWithSearch(List<TItem> items, string searchString)
    {
        // Taken from Virtualize component source code
        // return r => ValueTask.FromResult(new ItemsProviderResult<TItem>(
        //     items.Skip(r.StartIndex).Take(r.Count),
        //     items.Count));
        throw new NotImplementedException();
    }

    public static ItemProviderRequestWithSearchDelegate<TItem> WrapListWithSearchableModel(
        IStringSearchable filterModel)
    {
        // return (request, searchString) =>
        throw new NotImplementedException();
    }
    
    
    
    // public static ItemProviderRequestWithSearchDelegate<TItem> WrapParamAndSearch<TParam>(TParam param)
    

    public static ItemProviderRequestWithSearchDelegate<TItem> WrapSimpleSearch(Func<string?, CancellationToken, Task<IEnumerable<TItem>>> method)
    {
        async ValueTask<ItemsProviderResult<TItem>> Wrapped(ItemsProviderRequest request, string? search)
        {
            var res = await (method(search, request.CancellationToken));
            var result = res.ToList();
            return new ItemsProviderResult<TItem>(result, result.Count);
        }

        return Wrapped;
    }
    

}