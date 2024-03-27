using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace sip.Forms;

public class InputBasedValidator : ComponentBase
{
    [CascadingParameter] private EditContext EditContext { get; set; } = null!;
    private ValidationMessageStore _validationMessageStore = null!;
    
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        // Keep a reference to the original values so we can check if they have changed
        EditContext previousEditContext = EditContext;
        await base.SetParametersAsync(parameters);
        if (EditContext == null)
            throw new NullReferenceException($"{nameof(InputBasedValidator)} must be placed within an {nameof(EditForm)}");
        // If the EditForm.Model changes then we get a new EditContext
        // and need to hook it up
        if (EditContext != previousEditContext)
            EditContextChanged();
    }
    
    void EditContextChanged()
    {
        _validationMessageStore = new ValidationMessageStore(EditContext);
        HookUpEditContextEvents();
    }
    
    private void HookUpEditContextEvents()
    {
        EditContext.OnValidationRequested += ValidationRequested;
        EditContext.OnFieldChanged += FieldChanged;
    }

    private void FieldChanged(object? sender, FieldChangedEventArgs e)
    {
        var fields = EditContext.EnsureFieldDictionary();
        _validationMessageStore.Clear(e.FieldIdentifier);

        var success = fields.TryGetValue(e.FieldIdentifier, out var component);
        if (!success || component is null) return;
        
        var validationContext = new ValidationContext(fields)
            {DisplayName = e.FieldIdentifier.FieldName, MemberName = e.FieldIdentifier.FieldName};
        var messages = component.SelfValidate(validationContext);
        foreach (var message in messages)
        {
            _validationMessageStore.Add(e.FieldIdentifier, message);
        }
        
        InvokeAsync(EditContext.NotifyValidationStateChanged);
    }

    void ValidationRequested(object? sender, ValidationRequestedEventArgs args)
    {
        var fields = EditContext.EnsureFieldDictionary();
        _validationMessageStore.Clear();
        
        foreach (var f in fields)
        {
            var validationContext = new ValidationContext(fields)
                {DisplayName = f.Key.FieldName, MemberName = f.Key.FieldName};
            foreach (var message in f.Value.SelfValidate(validationContext))
            {
                _validationMessageStore.Add(f.Key, message);
            }
        }
        
        InvokeAsync(EditContext.NotifyValidationStateChanged);
    }
}