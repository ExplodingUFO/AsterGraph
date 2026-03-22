using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Services;

internal sealed class GraphSelectionClipboard
{
    private const double PasteOffsetStep = 36;
    private GraphSelectionFragment? _fragment;
    private int _pasteCount;

    public bool HasContent => _fragment is not null && _fragment.Nodes.Count > 0;

    public void Store(GraphSelectionFragment fragment)
    {
        ArgumentNullException.ThrowIfNull(fragment);

        _fragment = fragment;
        _pasteCount = 0;
    }

    public GraphSelectionFragment? Peek()
        => _fragment;

    public GraphPoint GetNextPasteOrigin(GraphPoint baseOrigin)
    {
        var offset = PasteOffsetStep * (++_pasteCount);
        return new GraphPoint(baseOrigin.X + offset, baseOrigin.Y + offset);
    }
}

internal sealed record GraphSelectionFragment(
    IReadOnlyList<GraphNode> Nodes,
    IReadOnlyList<GraphConnection> Connections,
    GraphPoint Origin,
    string? PrimaryNodeId = null);
