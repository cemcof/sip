using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Forms;

namespace sip.Forms;

public static class EditContextExtensions
{
    public static ConcurrentDictionary<FieldIdentifier, ICInputBase> EnsureFieldDictionary(this EditContext editContext)
    {
        var hasfields = editContext.Properties.TryGetValue("cfields", out var cfields);
        if (hasfields && cfields is ConcurrentDictionary<FieldIdentifier, ICInputBase> cfieldsTyped)
        {
            return cfieldsTyped;
        }

        var newCfields = new ConcurrentDictionary<FieldIdentifier, ICInputBase>();
        editContext.Properties["cfields"] = newCfields;
        return newCfields;
    }
}