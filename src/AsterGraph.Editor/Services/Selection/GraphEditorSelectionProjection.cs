using System.Globalization;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal sealed class GraphEditorSelectionProjection
{
    private readonly Func<string, string, string> _localizeText;
    private readonly Func<string, string, object?[], string> _localizeFormat;
    private readonly GraphEditorInspectorProjection _inspectorProjection;

    public GraphEditorSelectionProjection(
        Func<string, string, string>? localizeText = null,
        Func<string, string, object?[], string>? localizeFormat = null)
    {
        _localizeText = localizeText ?? ((_, fallback) => fallback);
        _localizeFormat = localizeFormat ?? ((_, fallback, arguments) => string.Format(CultureInfo.InvariantCulture, fallback, arguments));
        _inspectorProjection = new GraphEditorInspectorProjection(_localizeText, _localizeFormat);
    }

    public string FormatPorts(IEnumerable<PortViewModel> ports)
        => _inspectorProjection.FormatPorts(ports);

    public string FormatRelatedNodes(
        IEnumerable<ConnectionViewModel> connections,
        bool useSource,
        Func<string, NodeViewModel?> findNode)
        => _inspectorProjection.FormatRelatedNodes(connections, useSource, findNode);

    public IReadOnlyList<NodeParameterViewModel> BuildSelectedNodeParameters(
        IReadOnlyList<NodeViewModel> selectedNodes,
        INodeCatalog nodeCatalog,
        bool enableBatchParameterEditing,
        bool canEditNodeParameters,
        Action<NodeParameterViewModel, object?> applyParameterValue)
        => _inspectorProjection.BuildSelectedNodeParameters(
            selectedNodes,
            nodeCatalog,
            enableBatchParameterEditing,
            canEditNodeParameters,
            applyParameterValue);

    public GraphEditorSelectionProjectionState ProjectInspectorState(
        IReadOnlyList<NodeViewModel> selectedNodes,
        NodeViewModel? selectedNode,
        Func<NodeViewModel, IReadOnlyList<ConnectionViewModel>> getIncomingConnections,
        Func<NodeViewModel, IReadOnlyList<ConnectionViewModel>> getOutgoingConnections,
        Func<string, NodeViewModel?> findNode)
    {
        ArgumentNullException.ThrowIfNull(selectedNodes);
        ArgumentNullException.ThrowIfNull(getIncomingConnections);
        ArgumentNullException.ThrowIfNull(getOutgoingConnections);
        ArgumentNullException.ThrowIfNull(findNode);

        var selectionCaption = _inspectorProjection.FormatSelectionCaption(
            selectedNode,
            selectedNodes.Count > 1,
            selectedNodes.Count);

        if (selectedNode is null)
        {
            return new GraphEditorSelectionProjectionState(
                _localizeText("editor.inspector.connections.none", "Select a node to inspect its connection summary."),
                _localizeText("editor.inspector.upstream.none", "Select a node to see upstream dependencies."),
                _localizeText("editor.inspector.downstream.none", "Select a node to see downstream consumers."),
                selectionCaption);
        }

        var incomingConnections = getIncomingConnections(selectedNode);
        var outgoingConnections = getOutgoingConnections(selectedNode);

        return new GraphEditorSelectionProjectionState(
            _localizeFormat(
                "editor.inspector.connections.summary",
                "{0} incoming  ·  {1} outgoing",
                [incomingConnections.Count, outgoingConnections.Count]),
            _inspectorProjection.FormatRelatedNodes(incomingConnections, useSource: true, findNode),
            _inspectorProjection.FormatRelatedNodes(outgoingConnections, useSource: false, findNode),
            selectionCaption);
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

        var inspectorState = ProjectInspectorState(
            selectedNodes,
            selectedNode,
            getIncomingConnections,
            getOutgoingConnections,
            findNode);

        return new GraphEditorSelectionProjectionResult(
            inspectorState.InspectorConnections,
            inspectorState.InspectorUpstream,
            inspectorState.InspectorDownstream,
            inspectorState.SelectionCaption,
            BuildSelectedNodeParameters(
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

internal readonly record struct GraphEditorSelectionProjectionState(
    string InspectorConnections,
    string InspectorUpstream,
    string InspectorDownstream,
    string SelectionCaption);
