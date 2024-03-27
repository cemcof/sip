using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;

namespace sip.Utils;

public class Each<TItem> : ComponentBase
{
    [Parameter] public RenderFragment? Empty { get; set; }

    [Parameter] public string ContainerTag { get; set; } = "div";
    
    [Parameter] public RenderFragment<TItem> Item { get; set; } = null!;
    
    [Parameter] public IReadOnlyCollection<TItem>? Items { get; set; }
    
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Items is null || Items.Count == 0)
        {
            builder.AddContent(0, Empty);
        }
        else
        {
            builder.OpenElement(1, ContainerTag);
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            foreach (var item in Items)
            {
                builder.AddContent(3, Item(item));
            }
            
            builder.CloseElement();
        }
    }
    
    protected override void OnParametersSet()
    {
        ArgumentNullException.ThrowIfNull(Item, nameof(Item));
    }
    
}