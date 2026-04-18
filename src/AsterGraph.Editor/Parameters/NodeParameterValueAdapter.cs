using System.Globalization;
using System.Text.RegularExpressions;
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
        var allowedOptions = definition.Constraints.AllowedOptions;

        if (isRequired && IsMissingRequiredValue(normalizedValue))
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
            if (allowedOptions is not null)
            {
                foreach (var option in allowedOptions)
                {
                    if (option.Value.Equals(optionValue, StringComparison.Ordinal))
                    {
                        return Valid(optionValue);
                    }
                }
            }

            return Invalid($"{displayName} must be one of the declared options.");
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

        if (editorKind == ParameterEditorKind.List)
        {
            var items = NormalizeListValue(normalizedValue);
            if (definition.Constraints.MinimumItemCount is int minItems && items.Count < minItems)
            {
                return Invalid($"{displayName} must contain at least {minItems} item{(minItems == 1 ? string.Empty : "s")}.");
            }

            if (definition.Constraints.MaximumItemCount is int maxItems && items.Count > maxItems)
            {
                return Invalid($"{displayName} must contain at most {maxItems} item{(maxItems == 1 ? string.Empty : "s")}.");
            }

            return Valid(items);
        }

        var textValue = normalizedValue?.ToString() ?? string.Empty;
        if (definition.Constraints.MinimumLength is int minLength && textValue.Length < minLength)
        {
            return Invalid($"{displayName} must be at least {minLength} characters.");
        }

        if (definition.Constraints.MaximumLength is int maxLength && textValue.Length > maxLength)
        {
            return Invalid($"{displayName} must be at most {maxLength} characters.");
        }

        if (!string.IsNullOrWhiteSpace(definition.Constraints.ValidationPattern)
            && !Regex.IsMatch(textValue, definition.Constraints.ValidationPattern, RegexOptions.CultureInvariant))
        {
            var description = string.IsNullOrWhiteSpace(definition.Constraints.ValidationPatternDescription)
                ? definition.Constraints.ValidationPattern
                : definition.Constraints.ValidationPatternDescription;
            return Invalid($"{displayName} must match {description}.");
        }

        return Valid(textValue);
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
            JsonValueKind.Array => json.EnumerateArray()
                .Select(element => element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item!.Trim())
                .ToList(),
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
            IEnumerable<string> items => string.Join(Environment.NewLine, items),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };
    }

    public static bool AreEquivalent(object? left, object? right)
    {
        left = NormalizeIncomingValue(left);
        right = NormalizeIncomingValue(right);

        if (left is IEnumerable<string> leftItems && right is IEnumerable<string> rightItems)
        {
            return leftItems.SequenceEqual(rightItems, StringComparer.Ordinal);
        }

        return Equals(left, right);
    }

    private static IReadOnlyList<string> NormalizeListValue(object? value)
    {
        value = NormalizeIncomingValue(value);
        return value switch
        {
            null => [],
            string text => text
                .Split(["\r\n", "\n"], StringSplitOptions.None)
                .Select(item => item.Trim())
                .Where(item => item.Length > 0)
                .ToList(),
            IEnumerable<string> items => items
                .Select(item => item?.Trim() ?? string.Empty)
                .Where(item => item.Length > 0)
                .ToList(),
            IEnumerable<object?> objects => objects
                .Select(item => item?.ToString()?.Trim() ?? string.Empty)
                .Where(item => item.Length > 0)
                .ToList(),
            _ => [value.ToString() ?? string.Empty],
        };
    }

    private static bool IsMissingRequiredValue(object? value)
    {
        value = NormalizeIncomingValue(value);
        return value switch
        {
            null => true,
            string text => string.IsNullOrWhiteSpace(text),
            IReadOnlyCollection<string> items => items.Count == 0,
            IEnumerable<string> items => !items.Any(),
            _ => string.IsNullOrWhiteSpace(value.ToString()),
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
