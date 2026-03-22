namespace AsterGraph.Abstractions.Styling;

public sealed record GraphEditorStyleOptions
{
    public static GraphEditorStyleOptions Default { get; } = new();

    public ShellStyleOptions Shell { get; init; } = new();

    public CanvasStyleOptions Canvas { get; init; } = new();

    public NodeCardStyleOptions NodeCard { get; init; } = new();

    public PortStyleOptions Port { get; init; } = new();

    public ConnectionStyleOptions Connection { get; init; } = new();

    public InspectorStyleOptions Inspector { get; init; } = new();

    public ContextMenuStyleOptions ContextMenu { get; init; } = new();

    public IReadOnlyList<NodeStyleOverride> NodeOverrides { get; init; } = [];

    public IReadOnlyList<PortStyleOverride> PortOverrides { get; init; } = [];

    public IReadOnlyList<ConnectionStyleOverride> ConnectionOverrides { get; init; } = [];
}
