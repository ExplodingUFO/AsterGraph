using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Hosting;

public static class AsterGraphAuthoringToolActionFactory
{
    public static IReadOnlyList<AsterGraphHostedActionDescriptor> CreateSelectionActions(IGraphEditorSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var selection = session.Queries.GetSelectionSnapshot();
        if (selection.SelectedNodeIds.Count == 0)
        {
            return [];
        }

        return AsterGraphHostedActionFactory.CreateToolActions(
            session,
            session.Queries.GetToolDescriptors(
                GraphEditorToolContextSnapshot.ForSelection(selection.SelectedNodeIds, selection.PrimarySelectedNodeId)));
    }

    public static IReadOnlyList<AsterGraphHostedActionDescriptor> CreateNodeActions(
        IGraphEditorSession session,
        string nodeId,
        IReadOnlyList<string>? selectedNodeIds = null,
        string? primarySelectedNodeId = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        return AsterGraphHostedActionFactory.CreateToolActions(
            session,
            session.Queries.GetToolDescriptors(
                GraphEditorToolContextSnapshot.ForNode(nodeId, selectedNodeIds, primarySelectedNodeId)));
    }

    public static IReadOnlyList<AsterGraphHostedActionDescriptor> CreateConnectionActions(
        IGraphEditorSession session,
        string connectionId,
        IReadOnlyList<string>? selectedNodeIds = null,
        string? primarySelectedNodeId = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        return AsterGraphHostedActionFactory.CreateToolActions(
            session,
            session.Queries.GetToolDescriptors(
                GraphEditorToolContextSnapshot.ForConnection(connectionId, selectedNodeIds, primarySelectedNodeId)));
    }

    public static IReadOnlyList<AsterGraphHostedActionDescriptor> CreateCommandSurfaceActions(IGraphEditorSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var selection = session.Queries.GetSelectionSnapshot();
        var actions = new List<AsterGraphHostedActionDescriptor>();

        actions.AddRange(
            CreateSelectionActions(session)
                .Where(action => !IsDuplicateGlobalAction(action.Id)));

        var primaryNodeId = ResolvePrimarySelectedNodeId(selection);
        if (primaryNodeId is null)
        {
            return actions;
        }

        actions.AddRange(
            CreateNodeActions(session, primaryNodeId, selection.SelectedNodeIds, selection.PrimarySelectedNodeId)
                .Where(action => !IsDuplicateGlobalAction(action.Id)));

        var singleConnectionId = ResolveSingleIncidentConnectionId(session, primaryNodeId);
        if (singleConnectionId is not null)
        {
            actions.AddRange(CreateConnectionActions(session, singleConnectionId, selection.SelectedNodeIds, selection.PrimarySelectedNodeId));
        }

        return actions;
    }

    private static string? ResolvePrimarySelectedNodeId(GraphEditorSelectionSnapshot selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        if (!string.IsNullOrWhiteSpace(selection.PrimarySelectedNodeId))
        {
            return selection.PrimarySelectedNodeId;
        }

        return selection.SelectedNodeIds.Count == 1
            ? selection.SelectedNodeIds[0]
            : null;
    }

    private static string? ResolveSingleIncidentConnectionId(IGraphEditorSession session, string nodeId)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var connectionIds = session.Queries.CreateDocumentSnapshot()
            .Connections
            .Where(connection =>
                string.Equals(connection.SourceNodeId, nodeId, StringComparison.Ordinal)
                || string.Equals(connection.TargetNodeId, nodeId, StringComparison.Ordinal))
            .Select(connection => connection.Id)
            .Take(2)
            .ToArray();

        return connectionIds.Length == 1
            ? connectionIds[0]
            : null;
    }

    private static bool IsDuplicateGlobalAction(string actionId)
        => actionId is "selection-wrap-composite" or "node-enter-composite-scope";
}
