using CommunityToolkit.Mvvm.ComponentModel;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Editor.Parameters;
using System.Linq;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class NodeParameterViewModel : ObservableObject
{
    private readonly Action<NodeParameterViewModel, object?> _applyValue;
    private bool _suppressChangeNotifications;
    private bool _hasMixedValues;

    public NodeParameterViewModel(
        NodeParameterDefinition definition,
        IReadOnlyList<object?> currentValues,
        Action<NodeParameterViewModel, object?> applyValue,
        bool isHostReadOnly = false)
    {
        Definition = definition;
        _applyValue = applyValue;

        Key = definition.Key;
        DisplayName = definition.DisplayName;
        Description = definition.Description;
        EditorKind = definition.EditorKind;
        TypeId = definition.ValueType;
        IsRequired = definition.IsRequired;
        IsReadOnly = definition.Constraints.IsReadOnly || isHostReadOnly;
        Options = definition.Constraints.AllowedOptions
            .Select(option => new NodeParameterOptionViewModel(option.Value, option.Label, option.Description))
            .ToList()
            .AsReadOnly();

        InitializeValues(currentValues.Count == 0 ? [definition.DefaultValue] : currentValues);
    }

    public NodeParameterDefinition Definition { get; }

    public string Key { get; }

    public string DisplayName { get; }

    public string? Description { get; }

    public ParameterEditorKind EditorKind { get; }

    public PortTypeId TypeId { get; }

    public bool IsRequired { get; }

    public bool IsReadOnly { get; }

    public bool CanEdit => !IsReadOnly;

    public IReadOnlyList<NodeParameterOptionViewModel> Options { get; }

    public bool IsText => EditorKind is ParameterEditorKind.Text or ParameterEditorKind.Color;

    public bool IsNumber => EditorKind == ParameterEditorKind.Number;

    public bool IsBoolean => EditorKind == ParameterEditorKind.Boolean;

    public bool IsEnum => EditorKind == ParameterEditorKind.Enum;

    public bool IsColor => EditorKind == ParameterEditorKind.Color;

    public bool UsesTextInput => IsText || IsNumber || IsColor;

    public bool HasValidationError => !IsValid;

    public bool HasMixedValues => _hasMixedValues;

    public string MixedValueHint => HasMixedValues ? "Multiple values" : TypeId.Value;

    public string BooleanCaption => HasMixedValues ? "Multiple values" : "Enabled";

    public object? CurrentValue { get; private set; }

    [ObservableProperty]
    private string stringValue = string.Empty;

    [ObservableProperty]
    private bool? boolValue;

    [ObservableProperty]
    private NodeParameterOptionViewModel? selectedOption;

    [ObservableProperty]
    private bool isValid = true;

    [ObservableProperty]
    private string? validationMessage;

    partial void OnStringValueChanged(string value)
    {
        if (_suppressChangeNotifications || IsReadOnly)
        {
            return;
        }

        ValidateAndApply(value, commit: true);
    }

    partial void OnBoolValueChanged(bool? value)
    {
        if (_suppressChangeNotifications || IsReadOnly)
        {
            return;
        }

        if (value is null)
        {
            return;
        }

        ValidateAndApply(value, commit: true);
    }

    partial void OnSelectedOptionChanged(NodeParameterOptionViewModel? value)
    {
        if (_suppressChangeNotifications || IsReadOnly)
        {
            return;
        }

        ValidateAndApply(value?.Value, commit: true);
    }

    private void InitializeValues(IReadOnlyList<object?> currentValues)
    {
        var normalizedValues = currentValues
            .Select(NodeParameterValueAdapter.NormalizeIncomingValue)
            .ToList();
        var firstValue = normalizedValues[0];
        _hasMixedValues = normalizedValues.Skip(1).Any(value => !Equals(value, firstValue));

        if (_hasMixedValues)
        {
            _suppressChangeNotifications = true;
            CurrentValue = null;
            StringValue = string.Empty;
            BoolValue = null;
            SelectedOption = null;
            IsValid = true;
            ValidationMessage = null;
            _suppressChangeNotifications = false;
            OnPropertyChanged(nameof(HasMixedValues));
            OnPropertyChanged(nameof(MixedValueHint));
            OnPropertyChanged(nameof(BooleanCaption));
            return;
        }

        SetFromCurrentValue(firstValue);
        ValidateAndApply(CurrentValue, commit: false);
        OnPropertyChanged(nameof(HasMixedValues));
        OnPropertyChanged(nameof(MixedValueHint));
        OnPropertyChanged(nameof(BooleanCaption));
    }

    private void SetFromCurrentValue(object? value)
    {
        _suppressChangeNotifications = true;

        CurrentValue = value;
        StringValue = NodeParameterValueAdapter.FormatValueForEditor(value);
        BoolValue = NodeParameterValueAdapter.TryNormalizeBoolean(value, out var boolean) ? boolean : null;
        SelectedOption = Options.FirstOrDefault(option => option.Value.Equals(NodeParameterValueAdapter.FormatValueForEditor(value), StringComparison.Ordinal));

        _suppressChangeNotifications = false;
    }

    private void ValidateAndApply(object? candidateValue, bool commit)
    {
        var result = NodeParameterValueAdapter.NormalizeValue(Definition, candidateValue);

        IsValid = result.IsValid;
        ValidationMessage = result.ValidationError;
        OnPropertyChanged(nameof(HasValidationError));

        if (!IsValid)
        {
            return;
        }

        CurrentValue = result.Value;

        if (commit)
        {
            _applyValue(this, result.Value);
        }
    }

}
