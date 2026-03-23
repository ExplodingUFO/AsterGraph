using System.Globalization;
using System.Linq;
using System.Text.Json;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Editor.Parameters;

internal readonly record struct NodeParameterValueNormalizationResult(
    bool IsValid,
    object? Value,
    string? ValidationError);

internal static class NodeParameterValueAdapter
{
    public static NodeParameterValueNormalizationResult NormalizeValue(
        NodeParameterDefinition definition,
        object? candidateValue)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var normalizedValue = NormalizeIncomingValue(candidateValue);
        var displayName = definition.DisplayName;
        var editorKind = definition.EditorKind;
        var typeId = definition.ValueType;
        var isRequired = definition.IsRequired;
        var allowedOptionValues = definition.Constraints.AllowedOptions is null
            ? []
            : definition.Constraints.AllowedOptions.Select(option => option.Value).ToList();

        if (isRequired && (normalizedValue is null || string.IsNullOrWhiteSpace(normalizedValue.ToString())))
        {
            return Invalid($"{displayName} is required.");
        }

        if (editorKind == ParameterEditorKind.Boolean)
        {
            if (!TryNormalizeBoolean(normalizedValue, out var parsedBool))
            {
                return Invalid($"{displayName} must be true or false.");
            }

            return Valid(parsedBool);
        }

        if (editorKind == ParameterEditorKind.Enum)
        {
            var optionValue = normalizedValue?.ToString() ?? string.Empty;
            if (!allowedOptionValues.Any(option => option.Equals(optionValue, StringComparison.Ordinal)))
            {
                return Invalid($"{displayName} must be one of the declared options.");
            }

            return Valid(optionValue);
        }

        if (editorKind == ParameterEditorKind.Number)
        {
            var rawText = normalizedValue?.ToString() ?? string.Empty;
            if (!TryParseDoubleFlexible(rawText, out var parsedNumber))
            {
                return Invalid($"{displayName} must be numeric.");
            }

            if (definition.Constraints.Minimum is double min && parsedNumber < min)
            {
                return Invalid($"{displayName} must be >= {min}.");
            }

            if (definition.Constraints.Maximum is double max && parsedNumber > max)
            {
                return Invalid($"{displayName} must be <= {max}.");
            }

            if (typeId.Value.Equals("int", StringComparison.Ordinal))
            {
                if (Math.Abs(parsedNumber % 1) > double.Epsilon)
                {
                    return Invalid($"{displayName} must be a whole number.");
                }

                return Valid(Convert.ToInt32(parsedNumber, CultureInfo.InvariantCulture));
            }

            return Valid(parsedNumber);
        }

        return Valid(normalizedValue?.ToString());
    }

    public static object? NormalizeIncomingValue(object? value)
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

    public static bool TryNormalizeBoolean(object? value, out bool result)
    {
        value = NormalizeIncomingValue(value);
        if (value is bool boolean)
        {
            result = boolean;
            return true;
        }

        return bool.TryParse(value?.ToString(), out result);
    }

    public static string FormatValueForEditor(object? value)
    {
        value = NormalizeIncomingValue(value);

        return value switch
        {
            null => string.Empty,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };
    }

    private static bool TryParseDoubleFlexible(string rawText, out double result)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        var currentDecimal = currentCulture.NumberFormat.NumberDecimalSeparator;
        if (currentDecimal == "," && rawText.Contains('.') && !rawText.Contains(','))
        {
            var lastDot = rawText.LastIndexOf('.');
            var digitsAfter = rawText.Length - lastDot - 1;
            if (digitsAfter != 3
                && double.TryParse(rawText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result))
            {
                return true;
            }
        }

        if (double.TryParse(rawText, NumberStyles.Float | NumberStyles.AllowThousands, currentCulture, out result))
        {
            return true;
        }

        return double.TryParse(rawText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);
    }

    private static NodeParameterValueNormalizationResult Valid(object? value)
        => new(true, value, null);

    private static NodeParameterValueNormalizationResult Invalid(string validationError)
        => new(false, null, validationError);
}
