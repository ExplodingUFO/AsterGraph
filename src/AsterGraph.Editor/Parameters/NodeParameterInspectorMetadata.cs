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
        }

        if (!string.IsNullOrWhiteSpace(definition.Constraints.ValidationPatternDescription))
        {
            guidance.Add($"格式: {definition.Constraints.ValidationPatternDescription}");
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
}
