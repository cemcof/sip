using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace sip.Forms;

public class CEditForm<TModelType> : ComponentBase, IDisposable
    where TModelType : class, new()
{
    private EditContext _editContext = null!;
    private readonly Func<Task> _handleSubmitDelegate; // Cache to avoid per-render allocations
    
    [Parameter] public FieldCssClassProvider FieldCssClassProvider { get; set; } = new BsFieldClassProvider();

    /// <summary>
    /// A callback that will be invoked when the form is submitted and the
    /// <see cref="EditContext"/> is determined to be valid.
    /// </summary>
    [Parameter] public EventCallback<EditContext> OnValidSubmit { get; set; }

    /// <summary>
    /// A callback that will be invoked when the form is submitted and the
    /// <see cref="EditContext"/> is determined to be invalid.
    /// </summary>
    [Parameter] public EventCallback<EditContext> OnInvalidSubmit { get; set; }
    
    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="EditForm"/>.
    /// </summary>
    [Parameter] public RenderFragment<CEditForm<TModelType>>? ChildContent { get; set; }

    [Parameter] public RenderFragment<CEditForm<TModelType>> FormFooter { get; set; } = OkCancelFooter;

    public static RenderFragment OkCancelFooter(CEditForm<TModelType> context)
    {
        return builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "cform-panel");
            
            builder.OpenElement(2, "button");
            builder.AddAttribute(3, "type", "button"); // To prevent double submitting
            builder.AddAttribute(4, "onclick", context.TrySubmit);
            builder.AddContent(5, "OK");
            builder.CloseElement();
            
            builder.OpenElement(6, "button");
            builder.AddAttribute(7, "type", "button");
            builder.AddAttribute(8, "onclick", context.Cancel);
            builder.AddContent(9, "Cancel");
            builder.CloseElement();

            builder.CloseElement();
        };
    } 
    
    public static RenderFragment SaveFooter(CEditForm<TModelType> context)
    {
        return builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "cform-panel");
            
            builder.OpenElement(2, "button");
            builder.AddAttribute(3, "onclick", context.TrySubmit);
            builder.AddContent(4, "Save");
            builder.CloseElement();
            
            builder.CloseElement();
        };
    } 
    
    public static RenderFragment SubmitFooter(CEditForm<TModelType> context)
    {
        return builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "cform-panel");
            
            builder.OpenElement(2, "button");
            builder.AddAttribute(3, "onclick", context.TrySubmit);
            builder.AddContent(4, "Save");
            builder.CloseElement();
            
            builder.CloseElement();
        };
    } 
    
    public static RenderFragment EmptyFooter(CEditForm<TModelType> context) =>
        _ => { };

    public CEditForm()
    {
        _handleSubmitDelegate = TrySubmit;
    }

    public async Task TrySubmit()
    {
        Debug.Assert(_editContext != null);
        
        // Otherwise, the system implicitly runs validation on form submission
        var isValid = _editContext.Validate(); // This will likely become ValidateAsync later
        Logger.LogDebug("Trying to submit, valid={}", isValid);

        if (isValid && OnSubmit.HasDelegate)
        {
            await OnSubmit.InvokeAsync(Model);
        }

        if (!isValid && OnInvalidSubmit.HasDelegate)
        {
            await OnInvalidSubmit.InvokeAsync(_editContext);
        }
    }

    public async Task Cancel()
    {
        await OnCancel.InvokeAsync();
    }

    [Inject] protected ILogger<CEditForm<TModelType>> Logger { get; set; } = default!;
    
    [Parameter]
    public bool RenderAsFormElement { get; set; } = true;

    [Parameter]
    public string CssTheme { get; set; } = "cform";
    
    [Parameter]
    public EventCallback<TModelType> OnSubmit { get; set; }
    
    [Parameter]
    public EventCallback OnCancel { get; set; }

    [Parameter]
    public TModelType Model { get; set; } = null!;
    
    [Parameter]
    public Func<TModelType>? ModelFactory { get; set; } = null!;
    
    [Parameter]
    public EventCallback<FieldIdentifier> OnFieldChanged { get; set; }
    
    /// <summary>
    /// Gets or sets a collection of additional attributes that will be applied to the created <c>form</c> element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    
    protected override void OnParametersSet()
    {
        // Ensure we always have the Model
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        
        // TODO - this is wrong! will overwrite on each component update (Model becomes always null from parameter)
        if (Model is null)
        {
            if (ModelFactory is not null) Model = ModelFactory.Invoke();
            else Model = new TModelType();
        }
        
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_editContext is not null && !_editContext.Model.Equals(Model))
        {
            _editContext.OnFieldChanged -= _onFieldChangedHandler;
            _editContext = null!;
        }
        
        if (_editContext is null)
        {
            _editContext = new EditContext(Model);
            _editContext.OnFieldChanged += _onFieldChangedHandler;
        }

        _editContext.SetFieldCssClassProvider(FieldCssClassProvider);
    }
    
    private void _onFieldChangedHandler(object? sender, FieldChangedEventArgs e)
    {
        OnFieldChanged.InvokeAsync(e.FieldIdentifier);
    }
    
    public void Dispose()
    {
        Logger.LogTrace("Disposing editform");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (_editContext is not null)
        {
            _editContext.OnFieldChanged -= _onFieldChangedHandler;
        }
    }
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        Debug.Assert(_editContext != null);

        // If _editContext changes, tear down and recreate all descendants.
        // This is so we can safely use the IsFixed optimization on CascadingValue,
        // optimizing for the common case where _editContext never changes.
        builder.OpenRegion(_editContext.GetHashCode());

        if (RenderAsFormElement)
        {
            builder.OpenElement(0, "form");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "onsubmit", _handleSubmitDelegate);
        }
        else
        {
            builder.OpenElement(0, "div");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "formdiv", true);
        }

        RenderFragment<CEditForm<TModelType>> ccContent = context => bd =>
        {
            if (ChildContent is not null)
            {
                bd.OpenRegion(7);
                bd.OpenComponent<InputBasedValidator>(1);
                bd.CloseComponent();
                bd.CloseRegion();
                
                bd.OpenRegion(8);
                ChildContent(context)(bd);
                bd.CloseRegion();
                
                bd.OpenRegion(9);
                FormFooter(context)(bd);
                bd.CloseRegion();
            }
        };
        
        builder.OpenComponent<CascadingValue<EditContext>>(3);
        builder.AddAttribute(4, "IsFixed", true);
        builder.AddAttribute(5, "Value", _editContext);
        builder.AddAttribute(6, "ChildContent", ccContent.Invoke(this));

        builder.CloseComponent();
        builder.CloseElement();

        builder.CloseRegion();
    }
}