using System.Collections.ObjectModel;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorSelectionProjectionApplierHost
{
    NodeViewModel? PrimarySelectedNode { get; }

    IReadOnlyList<NodeViewModel> SelectionNodes { get; }

    ObservableCollection<NodeParameterViewModel> SelectedParameters { get; }

    INodeCatalog NodeCatalog { get; }

    bool EnableBatchParameterEditing { get; }

    bool CanEditNodeParameters { get; }

    void ApplyParameterValue(NodeParameterViewModel parameter, object? value);

    IReadOnlyList<ConnectionViewModel> GetIncomingConnections(NodeViewModel node);

    IReadOnlyList<ConnectionViewModel> GetOutgoingConnections(NodeViewModel node);

    NodeViewModel? FindNode(string nodeId);

    void ApplyProjectionText(
        string inspectorConnectionsText,
        string inspectorUpstreamText,
        string inspectorDownstreamText,
        string selectionCaptionText);

    void SynchronizeInteractionFocus();

    void RaiseParameterProjectionPropertyChanges();
}

internal sealed class GraphEditorSelectionProjectionApplier
{
    private readonly IGraphEditorSelectionProjectionApplierHost _host;
    private readonly GraphEditorSelectionProjection _projection;

    public GraphEditorSelectionProjectionApplier(
        IGraphEditorSelectionProjectionApplierHost host,
        GraphEditorSelectionProjection projection)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _projection = projection ?? throw new ArgumentNullException(nameof(projection));
    }

    public void RefreshSelectionProjection()
    {
        var projection = _projection.Project(
            _host.PrimarySelectedNode,
            _host.SelectionNodes,
            _host.NodeCatalog,
            _host.EnableBatchParameterEditing,
            _host.CanEditNodeParameters,
            _host.ApplyParameterValue,
            _host.GetIncomingConnections,
            _host.GetOutgoingConnections,
            _host.FindNode);

        _host.ApplyProjectionText(
            projection.InspectorConnectionsText,
            projection.InspectorUpstreamText,
            projection.InspectorDownstreamText,
            projection.SelectionCaptionText);

        _host.SelectedParameters.Clear();
        foreach (var parameter in projection.Parameters)
        {
            _host.SelectedParameters.Add(parameter);
        }

        _host.SynchronizeInteractionFocus();
        _host.RaiseParameterProjectionPropertyChanges();
    }
}
