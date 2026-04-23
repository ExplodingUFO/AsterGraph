using System.Diagnostics;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.ScaleSmoke;

public sealed record ScaleSmokeAuthoringProbeResult(
    bool StencilFilterOk,
    bool CommandSurfaceRefreshOk,
    bool QuickToolProjectionOk,
    bool QuickToolExecutionOk,
    int CommandDescriptorCount,
    int SelectionToolCount,
    int NodeToolCount,
    int ConnectionToolCount,
    int FilteredTemplateCount,
    ScaleSmokeAuthoringMetrics Metrics)
{
    public bool IsOk
        => StencilFilterOk
        && CommandSurfaceRefreshOk
        && QuickToolProjectionOk
        && QuickToolExecutionOk;

    public string ToMarker(string tierId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tierId);

        return string.Join(
            ':',
            [
                $"SCALE_AUTHORING_PROBE_OK:{tierId}",
                IsOk.ToString(),
                $"templates={FilteredTemplateCount}",
                $"commands={CommandDescriptorCount}",
                $"selection-tools={SelectionToolCount}",
                $"node-tools={NodeToolCount}",
                $"connection-tools={ConnectionToolCount}",
            ]);
    }
}

public static class ScaleSmokeAuthoringProbe
{
    public static ScaleSmokeAuthoringProbeResult Run(
        IGraphEditorSession session,
        string stencilFilter,
        string nodeId,
        string connectionId)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        session.Commands.SetSelection([nodeId], nodeId, updateStatus: false);

        IReadOnlyList<GraphEditorNodeTemplateSnapshot> filteredTemplates = [];
        var stencilFilterMs = MeasureMilliseconds(() =>
        {
            filteredTemplates = session.Queries.GetNodeTemplateSnapshots()
                .Where(template => MatchesStencilFilter(template, stencilFilter))
                .ToArray();
        });
        var stencilFilterOk = filteredTemplates.Count > 0;

        IReadOnlyList<GraphEditorCommandDescriptorSnapshot> commandDescriptors = [];
        IReadOnlyList<GraphEditorToolDescriptorSnapshot> selectionTools = [];
        var commandSurfaceRefreshMs = MeasureMilliseconds(() =>
        {
            commandDescriptors = session.Queries.GetCommandDescriptors();
            selectionTools = session.Queries.GetToolDescriptors(
                GraphEditorToolContextSnapshot.ForSelection([nodeId], nodeId));
        });
        var commandIds = commandDescriptors.Select(descriptor => descriptor.Id).ToHashSet(StringComparer.Ordinal);
        var commandSurfaceRefreshOk = commandIds.Contains("nodes.add")
            && commandIds.Contains("workspace.save")
            && selectionTools.Any(tool => string.Equals(tool.Id, "selection-wrap-composite", StringComparison.Ordinal));

        IReadOnlyList<GraphEditorToolDescriptorSnapshot> nodeTools = [];
        IReadOnlyList<GraphEditorToolDescriptorSnapshot> connectionTools = [];
        var quickToolProjectionMs = MeasureMilliseconds(() =>
        {
            nodeTools = session.Queries.GetToolDescriptors(
                GraphEditorToolContextSnapshot.ForNode(nodeId, [nodeId], nodeId));
            connectionTools = session.Queries.GetToolDescriptors(
                GraphEditorToolContextSnapshot.ForConnection(connectionId, [nodeId], nodeId));
        });
        var expansionTool = nodeTools.FirstOrDefault(tool => string.Equals(tool.Id, "node-toggle-surface-expansion", StringComparison.Ordinal));
        var disconnectTool = connectionTools.FirstOrDefault(tool => string.Equals(tool.Id, "connection-disconnect", StringComparison.Ordinal));
        var quickToolProjectionOk = expansionTool is not null && disconnectTool is not null;

        var initialExpansionState = session.Queries.GetNodeSurfaceSnapshots()
            .Single(snapshot => string.Equals(snapshot.NodeId, nodeId, StringComparison.Ordinal))
            .ExpansionState;
        var initialConnectionCount = session.Queries.CreateDocumentSnapshot().Connections.Count;
        var quickToolExecutionMs = MeasureMilliseconds(() =>
        {
            if (expansionTool is null || disconnectTool is null)
            {
                return;
            }

            session.Commands.TryExecuteCommand(expansionTool.Invocation);
            session.Commands.TryExecuteCommand(disconnectTool.Invocation);
        });
        var finalExpansionState = session.Queries.GetNodeSurfaceSnapshots()
            .Single(snapshot => string.Equals(snapshot.NodeId, nodeId, StringComparison.Ordinal))
            .ExpansionState;
        var finalConnectionCount = session.Queries.CreateDocumentSnapshot().Connections.Count;
        var quickToolExecutionOk = quickToolProjectionOk
            && finalExpansionState != initialExpansionState
            && finalConnectionCount == initialConnectionCount - 1;

        return new ScaleSmokeAuthoringProbeResult(
            StencilFilterOk: stencilFilterOk,
            CommandSurfaceRefreshOk: commandSurfaceRefreshOk,
            QuickToolProjectionOk: quickToolProjectionOk,
            QuickToolExecutionOk: quickToolExecutionOk,
            CommandDescriptorCount: commandDescriptors.Count,
            SelectionToolCount: selectionTools.Count,
            NodeToolCount: nodeTools.Count,
            ConnectionToolCount: connectionTools.Count,
            FilteredTemplateCount: filteredTemplates.Count,
            Metrics: new ScaleSmokeAuthoringMetrics(
                StencilFilterMs: stencilFilterMs,
                CommandSurfaceRefreshMs: commandSurfaceRefreshMs,
                QuickToolProjectionMs: quickToolProjectionMs,
                QuickToolExecutionMs: quickToolExecutionMs));
    }

    private static long MeasureMilliseconds(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

    private static bool MatchesStencilFilter(GraphEditorNodeTemplateSnapshot template, string filter)
        => string.IsNullOrWhiteSpace(filter)
        || template.Category.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || template.Title.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || template.Subtitle.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || template.Description.Contains(filter, StringComparison.OrdinalIgnoreCase);
}
