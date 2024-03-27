using Microsoft.AspNetCore.Components;

namespace sip.Utils;

/// <summary>
/// Custom component base class
/// </summary>
public abstract class CComponentBase : ComponentBase
{
    protected abstract Task OnParametersChangedAsync(CancellationToken ct);
    
    protected override async Task OnParametersSetAsync()
    {
        await OnParametersChangedAsync(CancellationToken.None);
    }
}