using Avalonia.Controls;
using AsterGraph.Core.Models;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// Thin body-only presenter seam used by the stock node shell composition.
/// </summary>
public interface IGraphNodeBodyPresenter
{
    /// <summary>
    /// Creates a body visual for the current node state.
    /// </summary>
    GraphNodeBodyVisual Create(GraphNodeVisualContext context);

    /// <summary>
    /// Updates an existing body visual with the latest node state.
    /// </summary>
    void Update(GraphNodeBodyVisual visual, GraphNodeVisualContext context);
}

/// <summary>
/// Result object for body-only stock presenter composition.
/// </summary>
public sealed class GraphNodeBodyVisual
{
    public GraphNodeBodyVisual(
        Control root,
        IReadOnlyDictionary<GraphConnectionTargetRef, Control>? connectionTargetAnchors = null,
        object? presenterState = null)
    {
        ArgumentNullException.ThrowIfNull(root);

        Root = root;
        ConnectionTargetAnchors = connectionTargetAnchors is Dictionary<GraphConnectionTargetRef, Control> mutableAnchors
            ? mutableAnchors
            : connectionTargetAnchors is null
                ? new Dictionary<GraphConnectionTargetRef, Control>()
                : new Dictionary<GraphConnectionTargetRef, Control>(connectionTargetAnchors);
        PresenterState = presenterState;
    }

    public Control Root { get; }

    public IReadOnlyDictionary<GraphConnectionTargetRef, Control> ConnectionTargetAnchors { get; }

    public object? PresenterState { get; }
}
