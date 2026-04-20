namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private GraphEditorInteractionFocusState _interactionFocus = GraphEditorInteractionFocusState.Empty;
    private string? _inspectorEditingParameterKey;

    /// <summary>
    /// Host-facing snapshot that differentiates inspected and actively edited graph surfaces.
    /// </summary>
    public GraphEditorInteractionFocusState InteractionFocus => _interactionFocus;

    /// <summary>
    /// Updates the currently focused inspector parameter. Invalid or stale keys are coerced away.
    /// </summary>
    public void SetInspectorEditingParameter(string? parameterKey)
    {
        var normalizedKey = NormalizeInspectorEditingParameterKey(parameterKey);
        if (string.Equals(_inspectorEditingParameterKey, normalizedKey, StringComparison.Ordinal))
        {
            return;
        }

        _inspectorEditingParameterKey = normalizedKey;
        SynchronizeInteractionFocus();
    }

    private void SynchronizeInteractionFocus()
    {
        var nextFocus = BuildInteractionFocus();
        if (EqualityComparer<GraphEditorInteractionFocusState>.Default.Equals(_interactionFocus, nextFocus))
        {
            return;
        }

        _interactionFocus = nextFocus;
        OnPropertyChanged(nameof(InteractionFocus));
    }

    private GraphEditorInteractionFocusState BuildInteractionFocus()
    {
        var inspectedNodeId = SelectedNode?.Id;
        var editingParameterKey = NormalizeInspectorEditingParameterKey(_inspectorEditingParameterKey);
        if (!string.Equals(_inspectorEditingParameterKey, editingParameterKey, StringComparison.Ordinal))
        {
            _inspectorEditingParameterKey = editingParameterKey;
        }

        return new GraphEditorInteractionFocusState(
            inspectedNodeId,
            editingParameterKey is null ? null : inspectedNodeId,
            editingParameterKey);
    }

    private string? NormalizeInspectorEditingParameterKey(string? parameterKey)
    {
        var normalizedKey = string.IsNullOrWhiteSpace(parameterKey)
            ? null
            : parameterKey.Trim();
        if (normalizedKey is null
            || SelectedNode is null
            || SelectedNodes.Count != 1)
        {
            return null;
        }

        return SelectedNodeParameters.Any(parameter => string.Equals(parameter.Key, normalizedKey, StringComparison.Ordinal))
            ? normalizedKey
            : null;
    }
}
