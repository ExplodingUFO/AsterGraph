using CommunityToolkit.Mvvm.ComponentModel;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using System.Globalization;
using System.Text.Json;

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
            .Select(value => NormalizeIncomingValue(value))
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
        StringValue = FormatValueForEditor(value);
        BoolValue = TryNormalizeBoolean(value, out var boolean) ? boolean : null;
        SelectedOption = Options.FirstOrDefault(option => option.Value.Equals(FormatValueForEditor(value), StringComparison.Ordinal));

        _suppressChangeNotifications = false;
    }

    private void ValidateAndApply(object? candidateValue, bool commit)
    {
        if (!TryNormalizeValue(candidateValue, out var normalizedValue, out var validationError))
        {
            IsValid = false;
            ValidationMessage = validationError;
            OnPropertyChanged(nameof(HasValidationError));
            return;
        }

        IsValid = true;
        ValidationMessage = null;
        OnPropertyChanged(nameof(HasValidationError));
        CurrentValue = normalizedValue;

        if (commit)
        {
            _applyValue(this, normalizedValue);
        }
    }

    private bool TryNormalizeValue(object? candidateValue, out object? normalizedValue, out string? validationError)
    {
        validationError = null;
        normalizedValue = NormalizeIncomingValue(candidateValue);

        if (IsRequired && (normalizedValue is null || string.IsNullOrWhiteSpace(normalizedValue.ToString())))
        {
            validationError = $"{DisplayName} is required.";
            return false;
        }

        if (EditorKind == ParameterEditorKind.Boolean)
        {
            if (!TryNormalizeBoolean(normalizedValue, out var parsedBool))
            {
                validationError = $"{DisplayName} must be true or false.";
                return false;
            }

            normalizedValue = parsedBool;
            return true;
        }

        if (EditorKind == ParameterEditorKind.Enum)
        {
            var optionValue = normalizedValue?.ToString() ?? string.Empty;
            if (!Options.Any(option => option.Value.Equals(optionValue, StringComparison.Ordinal)))
            {
                validationError = $"{DisplayName} must be one of the declared options.";
                return false;
            }

            normalizedValue = optionValue;
            return true;
        }

        if (EditorKind == ParameterEditorKind.Number)
        {
            var rawText = normalizedValue?.ToString() ?? string.Empty;
            if (!TryParseDoubleFlexible(rawText, out var parsedNumber))
            {
                validationError = $"{DisplayName} must be numeric.";
                return false;
            }

            if (Definition.Constraints.Minimum is double min && parsedNumber < min)
            {
                validationError = $"{DisplayName} must be >= {min}.";
                return false;
            }

            if (Definition.Constraints.Maximum is double max && parsedNumber > max)
            {
                validationError = $"{DisplayName} must be <= {max}.";
                return false;
            }

            if (TypeId.Value.Equals("int", StringComparison.Ordinal))
            {
                if (Math.Abs(parsedNumber % 1) > double.Epsilon)
                {
                    validationError = $"{DisplayName} must be a whole number.";
                    return false;
                }

                normalizedValue = Convert.ToInt32(parsedNumber, CultureInfo.InvariantCulture);
            }
            else
            {
                normalizedValue = parsedNumber;
            }

            return true;
        }

        normalizedValue = normalizedValue?.ToString();
        return true;
    }

    private static object? NormalizeIncomingValue(object? value)
    {
        if (value is not JsonElement json)
        {
            return value;
        }

        return json.ValueKind switch
        {
            JsonValueKind.String => json.GetString(),
            JsonValueKind.Number when json.TryGetInt32(out var intValue) => intValue,
            JsonValueKind.Number when json.TryGetDouble(out var doubleValue) => doubleValue,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => json.ToString(),
        };
    }

    private static bool TryNormalizeBoolean(object? value, out bool result)
    {
        value = NormalizeIncomingValue(value);
        if (value is bool boolean)
        {
            result = boolean;
            return true;
        }

        return bool.TryParse(value?.ToString(), out result);
    }

    private static bool TryParseDoubleFlexible(string rawText, out double result)
        => double.TryParse(rawText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out result)
           || double.TryParse(rawText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);

    private static string FormatValueForEditor(object? value)
    {
        value = NormalizeIncomingValue(value);

        return value switch
        {
            null => string.Empty,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };
    }
}
