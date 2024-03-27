using Microsoft.AspNetCore.Components.Forms;

namespace sip.Forms;

public class BsFieldClassProvider : FieldCssClassProvider
{
    public override string GetFieldCssClass(EditContext editContext, 
        in FieldIdentifier fieldIdentifier)
    {
        var isValid = !editContext.GetValidationMessages(fieldIdentifier).Any();
        if (!editContext.IsModified(fieldIdentifier) && isValid)
            return "";
            
        return isValid ? "" : "is-invalid";
    }
}