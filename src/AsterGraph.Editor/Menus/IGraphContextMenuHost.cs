using System.Windows.Input;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// 编辑器内部的菜单宿主接口，用于避免菜单生成器直接依赖完整的视图模型实现。
/// </summary>
internal interface IGraphContextMenuHost
{
    IEnumerable<NodeTemplateViewModel> NodeTemplates { get; }

    IEnumerable<NodeViewModel> Nodes { get; }

    IEnumerable<NodeViewModel> SelectedNodes { get; }

    GraphEditorCommandPermissions CommandPermissions { get; }

    int SelectedNodeCount { get; }

    ICommand DeleteSelectionCommand { get; }

    ICommand CopySelectionCommand { get; }

    ICommand ExportSelectionFragmentCommand { get; }

    ICommand ImportFragmentCommand { get; }

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

    void DisconnectConnection(string connectionId);

    void ConnectPorts(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId);

    NodeViewModel? FindNode(string nodeId);

    ConnectionViewModel? FindConnection(string connectionId);

#pragma warning disable CS0618
    IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId);
#pragma warning restore CS0618
}
