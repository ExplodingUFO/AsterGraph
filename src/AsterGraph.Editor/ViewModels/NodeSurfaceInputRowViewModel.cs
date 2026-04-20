using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Editor.ViewModels;

/// <summary>
/// Unified node-local input row projection consumed by hosted renderers.
/// </summary>
public sealed class NodeSurfaceInputRowViewModel
{
    public NodeSurfaceInputRowViewModel(
        NodeSurfaceInputRowKind kind,
        string label,
        PortTypeId typeId,
        PortViewModel? port = null,
        NodeParameterEndpointViewModel? parameterEndpoint = null,
        NodeSurfaceInlineContentKind inlineContentKind = NodeSurfaceInlineContentKind.None,
        string? inlineDisplayText = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentNullException.ThrowIfNull(typeId);
        ValidatePayload(kind, port, parameterEndpoint, inlineContentKind, inlineDisplayText);

        Kind = kind;
        Label = label.Trim();
        TypeId = typeId;
        Port = port;
        ParameterEndpoint = parameterEndpoint;
        InlineContentKind = inlineContentKind;
        InlineDisplayText = string.IsNullOrWhiteSpace(inlineDisplayText)
            ? null
            : inlineDisplayText.Trim();
    }

    public NodeSurfaceInputRowKind Kind { get; }

    public string Label { get; }

    public PortTypeId TypeId { get; }

    public PortViewModel? Port { get; }

    public NodeParameterEndpointViewModel? ParameterEndpoint { get; }

    public NodeSurfaceInlineContentKind InlineContentKind { get; }

    public string? InlineDisplayText { get; }

    private static void ValidatePayload(
        NodeSurfaceInputRowKind kind,
        PortViewModel? port,
        NodeParameterEndpointViewModel? parameterEndpoint,
        NodeSurfaceInlineContentKind inlineContentKind,
        string? inlineDisplayText)
    {
        var isPortRow = kind == NodeSurfaceInputRowKind.Port;
        var hasInlineDisplayText = !string.IsNullOrWhiteSpace(inlineDisplayText);

        if (isPortRow && port is null)
        {
            throw new InvalidOperationException("Port rows require a port payload.");
        }

        if (!isPortRow && parameterEndpoint is null)
        {
            throw new InvalidOperationException("Parameter-endpoint rows require an endpoint payload.");
        }

        if (isPortRow && parameterEndpoint is not null)
        {
            throw new InvalidOperationException("Port rows cannot carry a parameter endpoint payload.");
        }

        if (!isPortRow && port is not null)
        {
            throw new InvalidOperationException("Parameter-endpoint rows cannot carry a port payload.");
        }

        var expectsInlineDisplayText = inlineContentKind is NodeSurfaceInlineContentKind.Summary or NodeSurfaceInlineContentKind.Connection;
        if (expectsInlineDisplayText && !hasInlineDisplayText)
        {
            throw new InvalidOperationException("Summary and connection rows require inline display text.");
        }

        if (!expectsInlineDisplayText && hasInlineDisplayText)
        {
            throw new InvalidOperationException("Inline display text is only valid for summary or connection rows.");
        }
    }
}
