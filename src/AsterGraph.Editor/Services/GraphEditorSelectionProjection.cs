using AsterGraph.Abstractions.Catalog;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal sealed class GraphEditorSelectionProjection
{
    private readonly GraphEditorInspectorProjection _inspectorProjection;
    private readonly Func<string, string, string> _localizeText;
    private readonly Func<string, string, object?[], string> _localizeFormat;

    public GraphEditorSelectionProjection(
        GraphEditorInspectorProjection inspectorProjection,
        Func<string, string, string>? localizeText = null,
        Func<string, string, object?[], string>? localizeFormat = null)
    {
        _inspectorProjection = inspectorProjection ?? throw new ArgumentNullException(nameof(inspectorProjection));
        _localizeText = localizeText ?? ((_, fallback) => fallback);
        _localizeFormat = localizeFormat ?? ((_, fallback, arguments) => string.Format(fallback, arguments));
    }

    public GraphEditorSelectionProjectionResult Project(
        NodeViewModel? selectedNode,
        IReadOnlyList<NodeViewModel> selectedNodes,
        INodeCatalog nodeCatalog,
        bool enableBatchParameterEditing,
        bool canEditNodeParameters,
        Action<NodeParameterViewModel, object?> applyParameterValue,
        Func<NodeViewModel, IReadOnlyList<ConnectionViewModel>> getIncomingConnections,
        Func<NodeViewModel, IReadOnlyList<ConnectionViewModel>> getOutgoingConnections,
        Func<string, NodeViewModel?> findNode)
    {
        ArgumentNullException.ThrowIfNull(selectedNodes);
        ArgumentNullException.ThrowIfNull(nodeCatalog);
        ArgumentNullException.ThrowIfNull(applyParameterValue);
        ArgumentNullException.ThrowIfNull(getIncomingConnections);
        ArgumentNullException.ThrowIfNull(getOutgoingConnections);
        ArgumentNullException.ThrowIfNull(findNode);

        string inspectorConnectionsText;
        string inspectorUpstreamText;
        string inspectorDownstreamText;

        if (selectedNode is null)
        {
            inspectorConnectionsText = _localizeText("editor.inspector.connections.none", "Select a node to inspect its connection summary.");
            inspectorUpstreamText = _localizeText("editor.inspector.upstream.none", "Select a node to see upstream dependencies.");
            inspectorDownstreamText = _localizeText("editor.inspector.downstream.none", "Select a node to see downstream consumers.");
        }
        else
        {
            var incomingConnections = getIncomingConnections(selectedNode);
            var outgoingConnections = getOutgoingConnections(selectedNode);

            inspectorConnectionsText = _localizeFormat(
                "editor.inspector.connections.summary",
                "{0} incoming  ·  {1} outgoing",
                [incomingConnections.Count, outgoingConnections.Count]);
            inspectorUpstreamText = _inspectorProjection.FormatRelatedNodes(incomingConnections, useSource: true, findNode);
            inspectorDownstreamText = _inspectorProjection.FormatRelatedNodes(outgoingConnections, useSource: false, findNode);
        }

        return new GraphEditorSelectionProjectionResult(
            inspectorConnectionsText,
            inspectorUpstreamText,
            inspectorDownstreamText,
            _inspectorProjection.FormatSelectionCaption(selectedNode, selectedNodes.Count > 1, selectedNodes.Count),
            _inspectorProjection.BuildSelectedNodeParameters(
                selectedNodes,
                nodeCatalog,
                enableBatchParameterEditing,
                canEditNodeParameters,
                applyParameterValue));
    }
}

internal sealed record GraphEditorSelectionProjectionResult(
    string InspectorConnectionsText,
    string InspectorUpstreamText,
    string InspectorDownstreamText,
    string SelectionCaptionText,
    IReadOnlyList<NodeParameterViewModel> Parameters);
