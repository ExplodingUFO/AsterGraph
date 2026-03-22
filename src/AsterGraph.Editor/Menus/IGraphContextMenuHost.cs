using System.Windows.Input;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// Internal editor seam for menu generation so the builder does not depend on the full editor view-model type.
/// </summary>
internal interface IGraphContextMenuHost
{
    IEnumerable<NodeTemplateViewModel> NodeTemplates { get; }

    IEnumerable<NodeViewModel> Nodes { get; }

    int SelectedNodeCount { get; }

    ICommand DeleteSelectionCommand { get; }

    ICommand CopySelectionCommand { get; }

    bool HasPendingConnection { get; }

    ICommand FitViewCommand { get; }

    ICommand ResetViewCommand { get; }

    ICommand SaveCommand { get; }

    ICommand LoadCommand { get; }

    ICommand PasteCommand { get; }

    ICommand AlignLeftCommand { get; }

    ICommand AlignCenterCommand { get; }

    ICommand AlignRightCommand { get; }

    ICommand AlignTopCommand { get; }

    ICommand AlignMiddleCommand { get; }

    ICommand AlignBottomCommand { get; }

    ICommand DistributeHorizontallyCommand { get; }

    ICommand DistributeVerticallyCommand { get; }

    ICommand CancelPendingConnectionCommand { get; }

    void AddNode(NodeTemplateViewModel template, GraphPoint? preferredWorldPosition = null);

    void SelectNode(NodeViewModel? node);

    void CenterViewOnNode(string nodeId);

    void DeleteNodeById(string nodeId);

    void DuplicateNode(string nodeId);

    void DisconnectIncoming(string nodeId);

    void DisconnectOutgoing(string nodeId);

    void DisconnectAll(string nodeId);

    void StartConnection(string sourceNodeId, string sourcePortId);

    void BreakConnectionsForPort(string nodeId, string portId);

    void DeleteConnection(string connectionId);

    void ConnectPorts(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId);

    NodeViewModel? FindNode(string nodeId);

    ConnectionViewModel? FindConnection(string connectionId);

    IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId);
}
