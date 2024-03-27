using System.Diagnostics;
using Microsoft.AspNetCore.Components.Forms;

namespace sip.Forms;

public class BsEditForm : EditForm
{
        
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        Debug.Assert(EditContext is not null);
        EditContext.SetFieldCssClassProvider(new BsFieldClassProvider());
    }
}