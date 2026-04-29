namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Contextual documentation target attached to a validation issue when definition metadata can prove it.
/// </summary>
public sealed record GraphEditorValidationHelpTargetSnapshot(
    string Kind,
    string Title,
    string HelpText,
    string? NodeId = null,
    string? ConnectionId = null,
    string? EndpointId = null,
    string? ParameterKey = null)
{
    public string DisplayText => $"{Title}: {HelpText}";
}
