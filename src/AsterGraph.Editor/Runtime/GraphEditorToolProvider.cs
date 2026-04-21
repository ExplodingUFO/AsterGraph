namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes one tool-provider query.
/// </summary>
public sealed record GraphEditorToolProviderContext
{
    public GraphEditorToolProviderContext(
        IGraphEditorSession session,
        GraphEditorToolContextSnapshot context)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IGraphEditorSession Session { get; }

    public GraphEditorToolContextSnapshot Context { get; }
}

/// <summary>
/// Provides contextual authoring tools on top of the shared command route.
/// </summary>
public interface IGraphEditorToolProvider
{
    IReadOnlyList<GraphEditorToolDescriptorSnapshot> GetToolDescriptors(GraphEditorToolProviderContext context);
}
