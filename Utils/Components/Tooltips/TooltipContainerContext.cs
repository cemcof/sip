using Microsoft.AspNetCore.Components;

namespace sip.Utils.Components.Tooltips;

public class TooltipContainerContext(EventCallback<bool> tooltipShowChanged)
{
    public int ChangeCounter { get; set; }
    public EventCallback<bool> TooltipShowChanged { get; } = tooltipShowChanged;
}