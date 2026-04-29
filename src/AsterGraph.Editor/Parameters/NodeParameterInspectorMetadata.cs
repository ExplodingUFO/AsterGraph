using AsterGraph.Abstractions.Definitions;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Parameters;

internal static class NodeParameterInspectorMetadata
{
    public static IReadOnlyList<NodeParameterDefinition> OrderDefinitions(IReadOnlyList<NodeParameterDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        return definitions
            .Select((definition, index) => new OrderedDefinition(definition, index))
            .OrderBy(item => item.Definition.SortOrder)
            .ThenBy(item => item.Index)
            .Select(item => item.Definition)
            .ToList();
    }

    public static string? BuildReadOnlyReason(NodeParameterDefinition definition, bool isHostReadOnly)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (definition.Constraints.IsReadOnly && isHostReadOnly)
        {
            return "参数定义和宿主策略都将此字段标记为只读。";
        }

        if (definition.Constraints.IsReadOnly)
        {
            return "参数定义将此字段标记为只读。";
        }

        if (isHostReadOnly)
        {
            return "宿主策略当前禁止修改该参数。";
        }

        return null;
    }

    public static string? BuildHelpText(NodeParameterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var guidance = new List<string>();
        if (!string.IsNullOrWhiteSpace(definition.HelpText))
        {
            guidance.Add(definition.HelpText);
        }

        var formattedDefault = NodeParameterValueAdapter.FormatValueForEditor(definition.DefaultValue);
        if (!string.IsNullOrWhiteSpace(definition.PlaceholderText))
        {
            guidance.Add($"提示: {definition.PlaceholderText}");
        }

        if (!string.IsNullOrWhiteSpace(formattedDefault))
        {
            guidance.Add($"默认值: {formattedDefault}");
            guidance.Add($"恢复默认将还原为: {formattedDefault}");
        }

        if (!string.IsNullOrWhiteSpace(definition.Constraints.ValidationPatternDescription))
        {
            guidance.Add($"格式: {definition.Constraints.ValidationPatternDescription}");
        }

        if (!string.IsNullOrWhiteSpace(definition.PlaceholderText))
        {
            guidance.Add($"示例: {definition.PlaceholderText}");
        }
        else
        {
            var describedOption = definition.Constraints.AllowedOptions
                .FirstOrDefault(option => !string.IsNullOrWhiteSpace(option.Description));
            if (describedOption is not null)
            {
                guidance.Add($"示例选项: {describedOption.Label} - {describedOption.Description}");
            }
        }

        if (definition.Constraints.Minimum is double min && definition.Constraints.Maximum is double max)
        {
            guidance.Add($"范围: {min} - {max}");
        }
        else if (definition.Constraints.Minimum is double minimum)
        {
            guidance.Add($"最小值: {minimum}");
        }
        else if (definition.Constraints.Maximum is double maximum)
        {
            guidance.Add($"最大值: {maximum}");
        }

        if (definition.Constraints.MinimumLength is int minLength && definition.Constraints.MaximumLength is int maxLength)
        {
            guidance.Add($"长度: {minLength} - {maxLength}");
        }
        else if (definition.Constraints.MinimumLength is int minimumLength)
        {
            guidance.Add($"最短长度: {minimumLength}");
        }
        else if (definition.Constraints.MaximumLength is int maximumLength)
        {
            guidance.Add($"最长长度: {maximumLength}");
        }

        if (definition.Constraints.MinimumItemCount is int minItems && definition.Constraints.MaximumItemCount is int maxItems)
        {
            guidance.Add($"项目数: {minItems} - {maxItems}");
        }
        else if (definition.Constraints.MinimumItemCount is int minimumItems)
        {
            guidance.Add($"最少项目: {minimumItems}");
        }
        else if (definition.Constraints.MaximumItemCount is int maximumItems)
        {
            guidance.Add($"最多项目: {maximumItems}");
        }

        return guidance.Count == 0
            ? null
            : string.Join("  ·  ", guidance);
    }

    public static bool UsesMultilineTextInput(NodeParameterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return definition.EditorKind == ParameterEditorKind.List
            || IsCodeLikeText(definition)
            || ContainsLineBreak(definition.DefaultValue)
            || definition.Constraints.MaximumLength > 160;
    }

    public static bool IsCodeLikeText(NodeParameterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return definition.EditorKind == ParameterEditorKind.Text
            && (ContainsToken(definition.TemplateKey, "code")
                || ContainsToken(definition.ValueType.Value, "code")
                || ContainsToken(definition.ValueType.Value, "json")
                || ContainsToken(definition.ValueType.Value, "yaml")
                || ContainsToken(definition.ValueType.Value, "script"));
    }

    public static bool SupportsEnumSearch(NodeParameterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return definition.EditorKind == ParameterEditorKind.Enum
            && definition.Constraints.AllowedOptions.Count > 0;
    }

    public static string? BuildNumberSliderHint(NodeParameterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return definition.EditorKind == ParameterEditorKind.Number
            && definition.Constraints.Minimum is double min
            && definition.Constraints.Maximum is double max
            ? $"Slider range: {min} - {max}"
            : null;
    }

    public static bool CanApplyValidationFix(
        NodeParameterDefinition definition,
        bool isValid,
        bool canEdit)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return !isValid
            && canEdit
            && NodeParameterValueAdapter.NormalizeValue(definition, definition.DefaultValue).IsValid;
    }

    public static string? BuildValidationFixActionLabel(
        NodeParameterDefinition definition,
        bool isValid,
        bool canEdit)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return CanApplyValidationFix(definition, isValid, canEdit)
            ? "Restore default"
            : null;
    }

    public static bool IsUsingDefaultValue(
        NodeParameterDefinition definition,
        object? currentValue,
        bool hasMixedValues)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return !hasMixedValues
            && NodeParameterValueAdapter.AreEquivalent(currentValue, definition.DefaultValue);
    }

    public static bool CanResetToDefault(
        NodeParameterDefinition definition,
        object? currentValue,
        bool hasMixedValues,
        bool canEdit)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return canEdit
            && (hasMixedValues || !IsUsingDefaultValue(definition, currentValue, hasMixedValues));
    }

    public static string ResolveGroupDisplayName(NodeParameterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return string.IsNullOrWhiteSpace(definition.GroupName)
            ? "General"
            : definition.GroupName;
    }

    public static GraphEditorNodeParameterValueState ResolveValueState(
        NodeParameterDefinition definition,
        object? currentValue,
        bool hasMixedValues)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (hasMixedValues)
        {
            return GraphEditorNodeParameterValueState.Mixed;
        }

        return IsUsingDefaultValue(definition, currentValue, hasMixedValues)
            ? GraphEditorNodeParameterValueState.Default
            : GraphEditorNodeParameterValueState.Overridden;
    }

    public static string DescribeValue(
        NodeParameterDefinition definition,
        object? currentValue,
        bool hasMixedValues)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (hasMixedValues)
        {
            return "Multiple values";
        }

        if (definition.EditorKind == ParameterEditorKind.Boolean)
        {
            return currentValue is bool boolean && boolean
                ? "Enabled"
                : "Disabled";
        }

        var formattedValue = NodeParameterValueAdapter.FormatValueForEditor(currentValue);
        if (definition.EditorKind == ParameterEditorKind.Enum)
        {
            var option = definition.Constraints.AllowedOptions.FirstOrDefault(candidate =>
                string.Equals(candidate.Value, formattedValue, StringComparison.Ordinal));
            return option?.Label ?? formattedValue;
        }

        return string.IsNullOrWhiteSpace(formattedValue)
            ? definition.ValueType.Value
            : formattedValue;
    }

    private readonly record struct OrderedDefinition(NodeParameterDefinition Definition, int Index);

    private static bool ContainsToken(string? value, string token)
        => !string.IsNullOrWhiteSpace(value)
        && value.Contains(token, StringComparison.OrdinalIgnoreCase);

    private static bool ContainsLineBreak(object? value)
        => value is string text
        && (text.Contains('\n', StringComparison.Ordinal) || text.Contains('\r', StringComparison.Ordinal));
}
