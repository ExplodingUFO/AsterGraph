using AsterGraph.Abstractions.Definitions;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Parameters;

internal static class GraphEditorNodeParameterSnapshotProjector
{
public static IReadOnlyList<GraphEditorNodeParameterSnapshot> Project(
        IReadOnlyList<NodeParameterDefinition> definitions,
        Func<NodeParameterDefinition, IReadOnlyList<object?>> resolveValues,
        Func<NodeParameterDefinition, bool> resolveIsHostReadOnly,
        Func<NodeParameterDefinition, string?>? resolveReadOnlyReason = null)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(resolveValues);
        ArgumentNullException.ThrowIfNull(resolveIsHostReadOnly);

        if (definitions.Count == 0)
        {
            return [];
        }

        var orderedDefinitions = NodeParameterInspectorMetadata.OrderDefinitions(definitions);
        var orderedGroups = orderedDefinitions
            .Select(definition => definition.GroupName ?? string.Empty)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var shouldShowGroupHeaders = orderedGroups.Count > 1 || orderedGroups.Any(group => !string.IsNullOrWhiteSpace(group));
        var snapshots = new List<GraphEditorNodeParameterSnapshot>(orderedDefinitions.Count);

        foreach (var group in orderedGroups)
        {
            var groupDefinitions = orderedDefinitions
                .Where(definition => string.Equals(definition.GroupName ?? string.Empty, group, StringComparison.Ordinal))
                .ToList();
            for (var index = 0; index < groupDefinitions.Count; index++)
            {
                var definition = groupDefinitions[index];
                var values = resolveValues(definition);
                var normalizedValues = (values.Count == 0 ? [definition.DefaultValue] : values)
                    .Select(NodeParameterValueAdapter.NormalizeIncomingValue)
                    .ToList();
                var firstValue = normalizedValues[0];
                var hasMixedValues = normalizedValues.Skip(1)
                    .Any(candidate => !NodeParameterValueAdapter.AreEquivalent(candidate, firstValue));
                var currentValue = hasMixedValues ? null : firstValue;
                var validation = NodeParameterValueAdapter.NormalizeValue(definition, firstValue);
                var isHostReadOnly = resolveIsHostReadOnly(definition);
                var canEdit = !definition.Constraints.IsReadOnly && !isHostReadOnly;

                snapshots.Add(new GraphEditorNodeParameterSnapshot(
                    definition,
                    currentValue,
                    hasMixedValues,
                    canEdit,
                    validation.IsValid,
                    validation.ValidationError,
                    NodeParameterInspectorMetadata.CanResetToDefault(definition, currentValue, hasMixedValues, canEdit),
                    NodeParameterInspectorMetadata.IsUsingDefaultValue(definition, currentValue, hasMixedValues),
                    resolveReadOnlyReason?.Invoke(definition)
                        ?? NodeParameterInspectorMetadata.BuildReadOnlyReason(definition, isHostReadOnly),
                    NodeParameterInspectorMetadata.BuildHelpText(definition),
                    NodeParameterInspectorMetadata.ResolveGroupDisplayName(definition),
                    shouldShowGroupHeaders && index == 0,
                    NodeParameterInspectorMetadata.ResolveValueState(definition, currentValue, hasMixedValues),
                    NodeParameterInspectorMetadata.DescribeValue(definition, currentValue, hasMixedValues)));
            }
        }

        return snapshots;
    }
}
