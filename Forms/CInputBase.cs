using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace sip.Forms;
#pragma warning disable BL0007
public abstract class CInputBase<TInput> : InputBase<TInput>, ICInputBase
{
    [Parameter] public EventCallback<TInput> OnValueSet { get; set; }
    
    private bool? _hasLabel;
    [Parameter]
    public bool HasLabel { get => _hasLabel ?? !string.IsNullOrEmpty(DisplayName); set => _hasLabel = value; }
    public string InputId { get; } = Guid.NewGuid().ToString();
        
    [Parameter]
    public InputRenderingStrategy RenderingStrategy { get; set; } = InputRenderingStrategy.Horizontal;

    [Parameter] public TInput? Default { get; set; }
        
    private string? _tip;

    [Parameter]
    public string? Tip
    {
        get => _tip ?? ModelPropertyType.GetCustomAttribute<RenderAttribute>()?.Tip; 
        set => _tip = value;
    }

    
    private Sizing _sizing;
    [Parameter]
    public Sizing Sizing
    {
        get => _sizing == default ? ModelPropertyType.GetCustomAttribute<RenderAttribute>()?.Sizing ?? Sizing.Default : _sizing;
        set => _sizing = value;
    }


    private string? _noteIn;

    [Parameter]
    public string? NoteIn
    {
        get => _noteIn ?? ModelPropertyType.GetCustomAttribute<RenderAttribute>()?.NoteIn; 
        set => _noteIn = value;
    }

    public new string DisplayName
    {
        get => 
            base.DisplayName ?? ModelPropertyType.GetCustomAttribute<RenderAttribute>()?.Title
            ?? ModelPropertyType.Name.TitleCaseToText();
        set => base.DisplayName = value;
    }

    private int? _minLength;

    /// <summary>
    /// Minimal value of the input. Default (null) is unlimited.
    /// TODO - it makes sense to make this non-nullable and use default 0 for unlimited?
    /// </summary>
    [Parameter]
    public int? MinLength
    {
        get => _minLength ?? ModelPropertyType.GetCustomAttribute<MinLengthAttribute>()?.Length ?? (IsRequired ? 1 : null); 
        set => _minLength = value;
    }

    private int? _maxLength;

    /// <summary>
    /// Maximal value of the input. Default (null) is unlimited.
    /// </summary>
    [Parameter]
    public int? MaxLength
    {
        get => _maxLength ?? ModelPropertyType.GetCustomAttribute<MaxLengthAttribute>()?.Length; 
        set => _maxLength = value;
    }
        
    private double? _rangeMin;

    /// <summary>
    /// Minimal value of the input. Default (null) is unlimited.
    /// </summary>
    [Parameter]
    public double? RangeMin
    {
        get => _rangeMin ?? (double)(ModelPropertyType.GetCustomAttribute<RangeAttribute>()?.Minimum ?? double.MinValue); 
        set => _rangeMin = value;
    }

    private double? _rangeMax;

    /// <summary>
    /// Maximal value of the input. Default (null) is unlimited.
    /// </summary>
    [Parameter]
    public double? RangeMax
    {
        get => _rangeMax ?? (double)(ModelPropertyType.GetCustomAttribute<RangeAttribute>()?.Maximum ?? double.MaxValue); 
        set => _rangeMax = value;
    }

    private double? _step;
    public double? Step
    {
        get => _step;
        set => _step = value;
    }

    private string? _unit;
        
    [Parameter]
    public string? Unit
    {
        get => _unit ?? ModelPropertyType.GetCustomAttribute<RenderAttribute>()?.Unit; 
        set => _unit = value;
    }
        
    // Flex aligning 
    [CascadingParameter(Name = nameof(FlexCascade))] 
    public string? FlexCascade { get; set; }

    private string? _flex;
    [Parameter]
    public string? Flex { get => FlexCascade ?? _flex; set => _flex = value; }
        
    protected string GetFlexStyleForIndex(int index)
    {
        if (string.IsNullOrWhiteSpace(Flex)) return string.Empty;
        var splitted = Flex.Split(",");
        if (splitted.Length <= index) return string.Empty;
        return $"flex: {splitted[index]}";
    }

    // This already exist on input base, but is private for some reason... 
    protected string FieldCssClass => EditContext.FieldCssClass(FieldIdentifier);

    protected MemberInfo ModelPropertyType =>
        (MemberInfo?) FieldIdentifier.Model.GetType().GetProperty(FieldIdentifier.FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
        FieldIdentifier.Model.GetType().GetField(FieldIdentifier.FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
        throw new InvalidOperationException("ModelPropertyType: cannot lookup property on a model.");

    private bool? _isRequired;
    [CascadingParameter(Name = nameof(IsRequiredCascade))] public bool? IsRequiredCascade { get; set; }

    [Parameter]
    public bool IsRequired
    {
        get => IsRequiredCascade ?? _isRequired ?? ModelPropertyType.GetCustomAttribute<RequiredAttribute>() is not null; 
        set => _isRequired = value;
    }

    private bool _isReadonly;
    [CascadingParameter(Name = nameof(IsReadonlyCascade))] public bool? IsReadonlyCascade { get; set; }

    [Parameter]
    public bool IsReadonly
    {
        get => IsReadonlyCascade ?? _isReadonly;
        set => _isReadonly = value;
    }

    private bool _isDisabled;
    [CascadingParameter(Name = nameof(IsDisabledCascade))] public bool? IsDisabledCascade { get; set; }

    [Parameter]
    public bool IsDisabled
    {
        get => IsDisabledCascade ?? _isDisabled; 
        set => _isDisabled = value;
    }

    protected ElementReference? InputRef { get; set; }

    /// <summary>
    /// Delegates focus request to input element reference, if available
    /// </summary>
    public async Task FocusAsync()
    {
        if (InputRef is not null)
        {
            await InputRef.Value.FocusAsync();
        }
    }


    protected override void OnInitialized()
    {
        base.OnInitialized();
        var fields = EditContext.EnsureFieldDictionary();
        fields[FieldIdentifier] = this;

        if (ObjectExtensions.ShouldSetDefault(Default, CurrentValue))
        {
            CurrentValue = Default;
        }
        

        if (OnValueSet.HasDelegate)
        {
            EditContext.OnFieldChanged += FiledNotifier;
        }
    }

    protected void FiledNotifier(object? sender, FieldChangedEventArgs? args)
    {
        if (OnValueSet.HasDelegate && FieldIdentifier.Equals(args?.FieldIdentifier))
        {
            OnValueSet.InvokeAsync(Value);
        }
    }

    protected override void Dispose(bool disposing)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (EditContext is not null)
        {
            EditContext.OnFieldChanged -= FiledNotifier;
            var fields = EditContext.EnsureFieldDictionary();
            fields.TryRemove(FieldIdentifier, out _);
        }
    }

    public virtual IEnumerable<string> SelfValidate(ValidationContext validationContext)
    {
        if (IsRequired)
        {
            var attr = ModelPropertyType.GetCustomAttribute<RequiredAttribute>() ?? new RequiredAttribute();
            var validationResult = attr.GetValidationResult(CurrentValue, validationContext);
            if (validationResult?.ErrorMessage is not null)
            {
                yield return validationResult.ErrorMessage;
            }
        }
    }
}

#pragma warning restore BL0007