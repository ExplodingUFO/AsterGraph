using System.Globalization;
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
        string displayName,
        PortTypeId typeId,
        ParameterEditorKind editorKind,
        bool isRequired,
        IReadOnlyList<string> allowedOptionValues,
        object? candidateValue)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(typeId);
        ArgumentNullException.ThrowIfNull(allowedOptionValues);

        var normalizedValue = NormalizeIncomingValue(candidateValue);

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
        var canonical = rawText.Replace(',', '.');
        if (double.TryParse(canonical, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out result))
        {
            return true;
        }

        if (double.TryParse(canonical, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result))
        {
            return true;
        }

        result = default;
        return false;
    }

    private static NodeParameterValueNormalizationResult Valid(object? value)
        => new(true, value, null);

    private static NodeParameterValueNormalizationResult Invalid(string validationError)
        => new(false, null, validationError);
}
