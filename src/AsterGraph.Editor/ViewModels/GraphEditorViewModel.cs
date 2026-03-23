using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.Viewport;

namespace AsterGraph.Editor.ViewModels;

/// <summary>
/// 图编辑器的主视图模型，承载选择、布局、连线、剪贴板和持久化状态。
/// </summary>
public sealed partial class GraphEditorViewModel : ObservableObject, IGraphContextMenuHost
{
    private const double DefaultZoom = 0.88;
    private const double DefaultPanX = 110;
    private const double DefaultPanY = 96;

    private readonly INodeCatalog _nodeCatalog;
    private readonly IPortCompatibilityService _compatibilityService;
    private readonly GraphWorkspaceService _workspaceService;
    private readonly GraphSelectionClipboard _selectionClipboard;
    private readonly GraphFragmentWorkspaceService _fragmentWorkspaceService;
    private readonly GraphFragmentLibraryService _fragmentLibraryService;
    private readonly GraphEditorHistoryService _historyService;
    private IGraphTextClipboardBridge? _textClipboardBridge;
    private readonly GraphContextMenuBuilder _contextMenuBuilder;
    private bool _suspendDirtyTracking;
    private bool _suspendHistoryTracking;
    private string? _lastSavedDocumentSignature;
    private GraphEditorHistoryState? _pendingInteractionState;
    private double _viewportWidth;
    private double _viewportHeight;

    public GraphEditorViewModel(
        GraphDocument document,
        INodeCatalog nodeCatalog,
        IPortCompatibilityService compatibilityService,
        GraphWorkspaceService? workspaceService = null,
        GraphFragmentWorkspaceService? fragmentWorkspaceService = null,
        GraphEditorStyleOptions? styleOptions = null,
        GraphEditorBehaviorOptions? behaviorOptions = null,
        GraphFragmentLibraryService? fragmentLibraryService = null)
    {
        _nodeCatalog = nodeCatalog ?? throw new ArgumentNullException(nameof(nodeCatalog));
        _compatibilityService = compatibilityService ?? throw new ArgumentNullException(nameof(compatibilityService));
        _workspaceService = workspaceService ?? new GraphWorkspaceService();
        _selectionClipboard = new GraphSelectionClipboard();
        _fragmentWorkspaceService = fragmentWorkspaceService ?? new GraphFragmentWorkspaceService();
        _fragmentLibraryService = fragmentLibraryService ?? new GraphFragmentLibraryService();
        _historyService = new GraphEditorHistoryService();
        StyleOptions = styleOptions ?? GraphEditorStyleOptions.Default;
        BehaviorOptions = ResolveBehaviorOptions(behaviorOptions, StyleOptions);

        SaveCommand = new RelayCommand(SaveWorkspace);
        LoadCommand = new RelayCommand(() => LoadWorkspace());
        FitViewCommand = new RelayCommand(() => FitToViewport(_viewportWidth, _viewportHeight), CanFitView);
        ResetViewCommand = new RelayCommand(() => ResetView());
        UndoCommand = new RelayCommand(Undo, () => CanUndo);
        RedoCommand = new RelayCommand(Redo, () => CanRedo);
        DeleteSelectionCommand = new RelayCommand(DeleteSelection, () => CanDeleteSelection);
        CopySelectionCommand = new AsyncRelayCommand(CopySelectionAsync, () => CanCopySelection);
        PasteCommand = new AsyncRelayCommand(PasteSelectionAsync, () => CanPaste);
        ExportSelectionFragmentCommand = new RelayCommand(ExportSelectionFragment, () => CanExportSelectionFragment);
        ImportFragmentCommand = new RelayCommand(ImportFragment, () => CanImportFragment);
        ClearFragmentCommand = new RelayCommand(ClearFragment, () => CanImportFragment);
        RefreshFragmentTemplatesCommand = new RelayCommand(RefreshFragmentTemplates);
        ExportSelectionAsTemplateCommand = new RelayCommand(ExportSelectionAsTemplate, () => CanExportSelectionFragment && BehaviorOptions.Fragments.EnableFragmentLibrary);
        ImportSelectedTemplateCommand = new RelayCommand(ImportSelectedTemplate, () => CanImportSelectedTemplate);
        DeleteSelectedTemplateCommand = new RelayCommand(DeleteSelectedTemplate, () => CanDeleteSelectedTemplate);
        AlignLeftCommand = new RelayCommand(AlignSelectionLeft, () => CanAlignSelection);
        AlignCenterCommand = new RelayCommand(AlignSelectionCenter, () => CanAlignSelection);
        AlignRightCommand = new RelayCommand(AlignSelectionRight, () => CanAlignSelection);
        AlignTopCommand = new RelayCommand(AlignSelectionTop, () => CanAlignSelection);
        AlignMiddleCommand = new RelayCommand(AlignSelectionMiddle, () => CanAlignSelection);
        AlignBottomCommand = new RelayCommand(AlignSelectionBottom, () => CanAlignSelection);
        DistributeHorizontallyCommand = new RelayCommand(DistributeSelectionHorizontally, () => CanDistributeSelection);
        DistributeVerticallyCommand = new RelayCommand(DistributeSelectionVertically, () => CanDistributeSelection);
        CancelPendingConnectionCommand = new RelayCommand(
            () => CancelPendingConnection("Connection preview cancelled."),
            () => HasPendingConnection);
        AddNodeCommand = new RelayCommand<NodeTemplateViewModel>(template =>
        {
            if (template is not null)
            {
                AddNode(template);
            }
        });

        _contextMenuBuilder = new GraphContextMenuBuilder(this);

        Nodes = new ObservableCollection<NodeViewModel>();
        Connections = new ObservableCollection<ConnectionViewModel>();
        SelectedNodes = new ObservableCollection<NodeViewModel>();
        SelectedNodeParameters = new ObservableCollection<NodeParameterViewModel>();
        NodeTemplates = new ObservableCollection<NodeTemplateViewModel>(
            _nodeCatalog.Definitions.Select(definition => new NodeTemplateViewModel(definition)));
        FragmentTemplates = new ObservableCollection<FragmentTemplateViewModel>();

        Nodes.CollectionChanged += HandleNodesCollectionChanged;
        Connections.CollectionChanged += HandleConnectionsCollectionChanged;

        WorkspacePath = _workspaceService.WorkspacePath;
        Title = document.Title;
        Description = document.Description;

        RefreshFragmentTemplates();
        LoadDocument(document, "Ready to edit.", markClean: true);
        ResetView(updateStatus: false);
    }

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    public ObservableCollection<NodeViewModel> Nodes { get; }

    public ObservableCollection<ConnectionViewModel> Connections { get; }

    public ObservableCollection<NodeViewModel> SelectedNodes { get; }

    public ObservableCollection<NodeParameterViewModel> SelectedNodeParameters { get; }

    public ObservableCollection<NodeTemplateViewModel> NodeTemplates { get; }

    public ObservableCollection<FragmentTemplateViewModel> FragmentTemplates { get; }

    public GraphEditorStyleOptions StyleOptions { get; }

    public GraphEditorBehaviorOptions BehaviorOptions { get; }

    public string WorkspacePath { get; }

    public string FragmentPath => _fragmentWorkspaceService.FragmentPath;

    public string FragmentLibraryPath => _fragmentLibraryService.LibraryPath;

    public double ViewportWidth => _viewportWidth;

    public double ViewportHeight => _viewportHeight;

    public IRelayCommand SaveCommand { get; }

    public IRelayCommand LoadCommand { get; }

    public IRelayCommand FitViewCommand { get; }

    public IRelayCommand ResetViewCommand { get; }

    public IRelayCommand UndoCommand { get; }

    public IRelayCommand RedoCommand { get; }

    public IRelayCommand DeleteSelectionCommand { get; }

    public IAsyncRelayCommand CopySelectionCommand { get; }

    public IAsyncRelayCommand PasteCommand { get; }

    public IRelayCommand ExportSelectionFragmentCommand { get; }

    public IRelayCommand ImportFragmentCommand { get; }

    public IRelayCommand ClearFragmentCommand { get; }

    public IRelayCommand RefreshFragmentTemplatesCommand { get; }

    public IRelayCommand ExportSelectionAsTemplateCommand { get; }

    public IRelayCommand ImportSelectedTemplateCommand { get; }

    public IRelayCommand DeleteSelectedTemplateCommand { get; }

    public IRelayCommand AlignLeftCommand { get; }

    public IRelayCommand AlignCenterCommand { get; }

    public IRelayCommand AlignRightCommand { get; }

    public IRelayCommand AlignTopCommand { get; }

    public IRelayCommand AlignMiddleCommand { get; }

    public IRelayCommand AlignBottomCommand { get; }

    public IRelayCommand DistributeHorizontallyCommand { get; }

    public IRelayCommand DistributeVerticallyCommand { get; }

    public IRelayCommand CancelPendingConnectionCommand { get; }

    public IRelayCommand<NodeTemplateViewModel> AddNodeCommand { get; }

    IEnumerable<NodeTemplateViewModel> IGraphContextMenuHost.NodeTemplates => NodeTemplates;

    IEnumerable<NodeViewModel> IGraphContextMenuHost.Nodes => Nodes;

    int IGraphContextMenuHost.SelectedNodeCount => SelectedNodes.Count;

    ICommand IGraphContextMenuHost.DeleteSelectionCommand => DeleteSelectionCommand;

    ICommand IGraphContextMenuHost.CopySelectionCommand => CopySelectionCommand;

    ICommand IGraphContextMenuHost.ExportSelectionFragmentCommand => ExportSelectionFragmentCommand;

    ICommand IGraphContextMenuHost.ImportFragmentCommand => ImportFragmentCommand;

    ICommand IGraphContextMenuHost.FitViewCommand => FitViewCommand;

    ICommand IGraphContextMenuHost.ResetViewCommand => ResetViewCommand;

    ICommand IGraphContextMenuHost.SaveCommand => SaveCommand;

    ICommand IGraphContextMenuHost.LoadCommand => LoadCommand;

    ICommand IGraphContextMenuHost.PasteCommand => PasteCommand;

    ICommand IGraphContextMenuHost.AlignLeftCommand => AlignLeftCommand;

    ICommand IGraphContextMenuHost.AlignCenterCommand => AlignCenterCommand;

    ICommand IGraphContextMenuHost.AlignRightCommand => AlignRightCommand;

    ICommand IGraphContextMenuHost.AlignTopCommand => AlignTopCommand;

    ICommand IGraphContextMenuHost.AlignMiddleCommand => AlignMiddleCommand;

    ICommand IGraphContextMenuHost.AlignBottomCommand => AlignBottomCommand;

    ICommand IGraphContextMenuHost.DistributeHorizontallyCommand => DistributeHorizontallyCommand;

    ICommand IGraphContextMenuHost.DistributeVerticallyCommand => DistributeVerticallyCommand;

    ICommand IGraphContextMenuHost.CancelPendingConnectionCommand => CancelPendingConnectionCommand;

    [ObservableProperty]
    private double zoom = DefaultZoom;

    [ObservableProperty]
    private double panX = DefaultPanX;

    [ObservableProperty]
    private double panY = DefaultPanY;

    [ObservableProperty]
    private NodeViewModel? selectedNode;

    [ObservableProperty]
    private NodeViewModel? pendingSourceNode;

    [ObservableProperty]
    private PortViewModel? pendingSourcePort;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private bool isDirty;

    [ObservableProperty]
    private FragmentTemplateViewModel? selectedFragmentTemplate;

    /// <summary>
    /// 当图文档发生结构或内容变化时触发。
    /// </summary>
    public event EventHandler<GraphEditorDocumentChangedEventArgs>? DocumentChanged;

    /// <summary>
    /// 当当前选择集合发生变化时触发。
    /// </summary>
    public event EventHandler<GraphEditorSelectionChangedEventArgs>? SelectionChanged;

    /// <summary>
    /// 当视口缩放、平移或尺寸发生变化时触发。
    /// </summary>
    public event EventHandler<GraphEditorViewportChangedEventArgs>? ViewportChanged;

    /// <summary>
    /// 当片段导出成功后触发。
    /// </summary>
    public event EventHandler<GraphEditorFragmentEventArgs>? FragmentExported;

    /// <summary>
    /// 当片段导入并粘贴成功后触发。
    /// </summary>
    public event EventHandler<GraphEditorFragmentEventArgs>? FragmentImported;

    public bool HasPendingConnection => PendingSourceNode is not null && PendingSourcePort is not null;

    public bool CanUndo => _historyService.CanUndo;

    public bool CanRedo => _historyService.CanRedo;

    public bool HasSelection => SelectedNodes.Count > 0;

    public bool HasMultipleSelection => SelectedNodes.Count > 1;

    public bool CanDeleteSelection => HasSelection;

    public bool CanCopySelection => HasSelection;

    public bool CanPaste => _selectionClipboard.HasContent || _textClipboardBridge is not null;

    public bool CanExportSelectionFragment => HasSelection;

    public bool CanImportFragment => _fragmentWorkspaceService.Exists();

    public bool HasFragmentTemplates => FragmentTemplates.Count > 0;

    public bool CanImportSelectedTemplate => SelectedFragmentTemplate is not null;

    public bool CanDeleteSelectedTemplate => SelectedFragmentTemplate is not null;

    public bool CanAlignSelection => SelectedNodes.Count >= 2;

    public bool CanDistributeSelection => SelectedNodes.Count >= 3;

    public bool HasEditableParameters => SelectedNodes.Count == 1 && SelectedNodeParameters.Count > 0;

    public bool HasBatchEditableParameters => SelectedNodes.Count > 1 && SelectedNodeParameters.Count > 0;

    public bool HasAnyEditableParameters => BehaviorOptions.Selection.EnableBatchParameterEditing && SelectedNodeParameters.Count > 0;

    public string StatsCaption => $"{Nodes.Count} nodes  ·  {Connections.Count} links  ·  {Zoom * 100:0}% zoom";

    public string WorkspaceCaption => $"{(IsDirty ? "Unsaved changes" : "Snapshot synced")}  ·  {WorkspacePath}";

    public string FragmentCaption => $"{(_fragmentWorkspaceService.Exists() ? "Fragment available" : "No fragment file")}  ·  {_fragmentWorkspaceService.FragmentPath}";

    public string FragmentStatusCaption
        => !_fragmentWorkspaceService.Exists()
            ? "No saved fragment file."
            : $"Last updated {File.GetLastWriteTime(_fragmentWorkspaceService.FragmentPath):yyyy-MM-dd HH:mm:ss}";

    public string FragmentLibraryCaption => $"{(HasFragmentTemplates ? $"{FragmentTemplates.Count} templates" : "No templates")}  ·  {FragmentLibraryPath}";

    public string ModeCaption => HasPendingConnection
        ? $"Connecting {PendingSourceNode!.Title} / {PendingSourcePort!.Label}  ->  click an input port"
        : HasMultipleSelection
            ? $"Selection mode  ·  {SelectedNodes.Count} nodes selected"
            : "Selection mode  ·  click a template to add a node";

    public string InspectorTitle => SelectedNodes.Count switch
    {
        0 => "Select A Node",
        1 => SelectedNode?.Title ?? "Select A Node",
        _ => $"{SelectedNodes.Count} Nodes Selected",
    };

    public string InspectorCategory => SelectedNodes.Count switch
    {
        0 => "Editor",
        1 => SelectedNode?.Category ?? "Editor",
        _ => "Multi Selection",
    };

    public string InspectorDescription => SelectedNodes.Count switch
    {
        0 => "Build the graph from the left library, connect outputs to inputs, and save snapshots from the toolbar.",
        1 => SelectedNode?.Description ?? "Build the graph from the left library, connect outputs to inputs, and save snapshots from the toolbar.",
        _ => HasBatchEditableParameters
            ? $"Editing shared parameters across {SelectedNodes.Count} nodes of the same definition."
            : "Delete removes the full selection. Copy and paste preserve internal links between the selected nodes.",
    };

    public string InspectorInputs => SelectedNode is null
        ? "Select a node to inspect its input ports."
        : FormatPorts(SelectedNode.Inputs);

    public string InspectorOutputs => SelectedNode is null
        ? "Select a node to inspect its output ports."
        : FormatPorts(SelectedNode.Outputs);

    public string InspectorConnections => SelectedNode is null
        ? "Select a node to inspect its connection summary."
        : $"{GetIncomingConnections(SelectedNode).Count} incoming  ·  {GetOutgoingConnections(SelectedNode).Count} outgoing";

    public string InspectorUpstream => SelectedNode is null
        ? "Select a node to see upstream dependencies."
        : FormatRelatedNodes(GetIncomingConnections(SelectedNode), useSource: true);

    public string InspectorDownstream => SelectedNode is null
        ? "Select a node to see downstream consumers."
        : FormatRelatedNodes(GetOutgoingConnections(SelectedNode), useSource: false);

    public string SelectionCaption => SelectedNode is null
        ? "No selection"
        : HasMultipleSelection
            ? $"{SelectedNodes.Count} nodes selected  ·  primary {SelectedNode.Title}"
            : $"{SelectedNode.InputCount} inputs  ·  {SelectedNode.OutputCount} outputs";

    public IReadOnlyList<MenuItemDescriptor> BuildContextMenu(ContextMenuContext context)
        => _contextMenuBuilder.Build(context);

    public void UpdateViewportSize(double width, double height)
    {
        _viewportWidth = width;
        _viewportHeight = height;
        FitViewCommand.NotifyCanExecuteChanged();
        NotifyViewportChanged();
        RaiseComputedPropertyChanges();
    }

    /// <summary>
    /// 配置宿主提供的纯文本剪贴板桥。
    /// </summary>
    /// <param name="bridge">宿主桥实现；为 <see langword="null"/> 时仅保留进程内剪贴板回退。</param>
    public void SetTextClipboardBridge(IGraphTextClipboardBridge? bridge)
    {
        _textClipboardBridge = bridge;
        RaiseComputedPropertyChanges();
    }

    public void RefreshFragmentTemplates()
    {
        FragmentTemplates.Clear();
        foreach (var template in _fragmentLibraryService.EnumerateTemplates())
        {
            FragmentTemplates.Add(new FragmentTemplateViewModel(template));
        }

        SelectedFragmentTemplate = FragmentTemplates.FirstOrDefault();
        RaiseComputedPropertyChanges();
    }

    /// <summary>
    /// 开始记录一次连续交互的历史基线，通常用于节点拖动这类高频操作。
    /// </summary>
    public void BeginHistoryInteraction()
    {
        _pendingInteractionState ??= CaptureHistoryState();
    }

    /// <summary>
    /// 结束一次连续交互，并在状态发生变化时写入撤销栈。
    /// </summary>
    /// <param name="status">交互完成后显示的状态文本。</param>
    public void CompleteHistoryInteraction(string status)
    {
        if (_pendingInteractionState is null)
        {
            return;
        }

        var previousState = _pendingInteractionState;
        _pendingInteractionState = null;
        if (string.Equals(previousState.Signature, CreateDocumentSignature(), StringComparison.Ordinal))
        {
            return;
        }

        StatusMessage = status;
        UpdateDirtyState();
        PushCurrentHistoryState();
        RaiseComputedPropertyChanges();
    }

    /// <summary>
    /// 撤销到上一个已记录的图编辑状态。
    /// </summary>
    public void Undo()
    {
        if (!_historyService.TryUndo(out var state) || state is null)
        {
            StatusMessage = "No more undo steps.";
            return;
        }

        RestoreHistoryState(state, "Undo applied.");
    }

    /// <summary>
    /// 重做到下一个已记录的图编辑状态。
    /// </summary>
    public void Redo()
    {
        if (!_historyService.TryRedo(out var state) || state is null)
        {
            StatusMessage = "No more redo steps.";
            return;
        }

        RestoreHistoryState(state, "Redo applied.");
    }

    public void SelectNode(NodeViewModel? node)
    {
        SelectSingleNode(node);
    }

    public void ClearSelection(bool updateStatus = false)
        => SetSelection([], null, updateStatus ? "Selection cleared." : null);

    public void SelectSingleNode(NodeViewModel? node, bool updateStatus = true)
    {
        if (node is null)
        {
            SetSelection([], null, updateStatus ? "Selection cleared." : null);
            return;
        }

        SetSelection([node], node, updateStatus && !HasPendingConnection ? $"Selected {node.Title}." : null);
    }

    public void AddNodeToSelection(NodeViewModel node, bool updateStatus = true)
    {
        if (SelectedNodes.Contains(node))
        {
            return;
        }

        var nextSelection = SelectedNodes.ToList();
        nextSelection.Add(node);
        SetSelection(nextSelection, node, updateStatus ? $"Added {node.Title} to the selection." : null);
    }

    public void ToggleNodeSelection(NodeViewModel node, bool updateStatus = true)
    {
        var nextSelection = SelectedNodes.ToList();
        if (nextSelection.Remove(node))
        {
            SetSelection(nextSelection, nextSelection.LastOrDefault(), updateStatus ? $"Removed {node.Title} from the selection." : null);
            return;
        }

        nextSelection.Add(node);
        SetSelection(nextSelection, node, updateStatus ? $"Added {node.Title} to the selection." : null);
    }

    public void SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode = null, string? status = null)
    {
        var uniqueNodes = nodes
            .Where(node => Nodes.Contains(node))
            .Distinct()
            .ToList();

        var nextPrimary = primaryNode is not null && uniqueNodes.Contains(primaryNode)
            ? primaryNode
            : uniqueNodes.LastOrDefault();

        foreach (var item in Nodes)
        {
            item.IsSelected = uniqueNodes.Contains(item);
        }

        SelectedNodes.Clear();
        foreach (var node in uniqueNodes)
        {
            SelectedNodes.Add(node);
        }

        SelectedNode = nextPrimary;

        if (!string.IsNullOrWhiteSpace(status))
        {
            StatusMessage = status;
        }

        RebuildSelectedNodeParameters();
        NotifySelectionChanged();
        RaiseComputedPropertyChanges();
    }

    public IReadOnlyList<NodeViewModel> GetNodesInRectangle(GraphPoint firstCorner, GraphPoint secondCorner)
    {
        var left = Math.Min(firstCorner.X, secondCorner.X);
        var top = Math.Min(firstCorner.Y, secondCorner.Y);
        var right = Math.Max(firstCorner.X, secondCorner.X);
        var bottom = Math.Max(firstCorner.Y, secondCorner.Y);

        return Nodes
            .Where(node => Intersects(node.Bounds, left, top, right, bottom))
            .ToList();
    }

    /// <summary>
    /// 获取当前所有节点位置的不可变快照，供宿主持久化使用。
    /// </summary>
    public IReadOnlyList<NodePositionSnapshot> GetNodePositions()
        => Nodes
            .Select(node => new NodePositionSnapshot(node.Id, new GraphPoint(node.X, node.Y)))
            .ToList();

    /// <summary>
    /// 按节点实例标识读取当前位置。
    /// </summary>
    public bool TryGetNodePosition(string nodeId, out NodePositionSnapshot? snapshot)
    {
        var node = FindNode(nodeId);
        if (node is null)
        {
            snapshot = null;
            return false;
        }

        snapshot = new NodePositionSnapshot(node.Id, new GraphPoint(node.X, node.Y));
        return true;
    }

    /// <summary>
    /// 按节点实例标识更新单个节点的位置。
    /// </summary>
    public bool TrySetNodePosition(string nodeId, GraphPoint position, bool updateStatus = true)
    {
        var node = FindNode(nodeId);
        if (node is null)
        {
            if (updateStatus)
            {
                StatusMessage = $"Node '{nodeId}' was not found.";
            }

            return false;
        }

        _suspendDirtyTracking = true;
        var changed = ApplyNodePosition(node, position);
        _suspendDirtyTracking = false;

        if (!changed)
        {
            return true;
        }

        if (updateStatus)
        {
            MarkDirty($"Updated {node.Title} position.");
        }
        else
        {
            UpdateDirtyState();
            PushCurrentHistoryState();
            RaiseComputedPropertyChanges();
        }

        return true;
    }

    /// <summary>
    /// 批量应用节点位置并返回实际更新的节点数量。
    /// </summary>
    public int SetNodePositions(IEnumerable<NodePositionSnapshot> positions, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(positions);

        var requestedPositions = positions
            .GroupBy(snapshot => snapshot.NodeId, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToList();

        if (requestedPositions.Count == 0)
        {
            if (updateStatus)
            {
                StatusMessage = "No node positions were provided.";
            }

            return 0;
        }

        _suspendDirtyTracking = true;
        var appliedCount = 0;

        foreach (var snapshot in requestedPositions)
        {
            var node = FindNode(snapshot.NodeId);
            if (node is null)
            {
                continue;
            }

            if (ApplyNodePosition(node, snapshot.Position))
            {
                appliedCount++;
            }
        }

        _suspendDirtyTracking = false;

        if (appliedCount == 0)
        {
            if (updateStatus)
            {
                StatusMessage = "No matching nodes were found for the provided positions.";
            }

            return 0;
        }

        if (updateStatus)
        {
            MarkDirty(appliedCount == 1
                ? "Updated 1 node position."
                : $"Updated {appliedCount} node positions.");
        }
        else
        {
            UpdateDirtyState();
            PushCurrentHistoryState();
            RaiseComputedPropertyChanges();
        }

        return appliedCount;
    }

    public GraphPoint ScreenToWorld(GraphPoint screen)
        => ViewportMath.ScreenToWorld(new ViewportState(Zoom, PanX, PanY), screen);

    public void AddNode(NodeTemplateViewModel template, GraphPoint? preferredWorldPosition = null)
    {
        var position = preferredWorldPosition ?? GetViewportCenter();
        var offset = 26 * (Nodes.Count % 4);

        var node = template.CreateNode(
            CreateNodeId(template.Key),
            new GraphPoint(
                position.X - (template.Size.Width / 2) + offset,
                position.Y - (template.Size.Height / 2) + offset));

        Nodes.Add(node);
        SelectSingleNode(node);
        MarkDirty($"Added {template.Title}.");
        NotifyDocumentChanged(GraphEditorDocumentChangeKind.NodesAdded, nodeIds: [node.Id]);
    }

    public void MoveNode(NodeViewModel node, double deltaX, double deltaY)
    {
        if (node.IsSelected && SelectedNodes.Count > 1)
        {
            foreach (var selectedNode in SelectedNodes)
            {
                selectedNode.MoveBy(deltaX, deltaY);
            }

            return;
        }

        node.MoveBy(deltaX, deltaY);
    }

    public void PanBy(double deltaX, double deltaY)
    {
        PanX += deltaX;
        PanY += deltaY;
        NotifyViewportChanged();
    }

    public void ZoomAt(double factor, GraphPoint screenAnchor)
    {
        var updated = ViewportMath.ZoomAround(
            new ViewportState(Zoom, PanX, PanY),
            factor,
            screenAnchor,
            minimumZoom: 0.35,
            maximumZoom: 1.9);

        Zoom = updated.Zoom;
        PanX = updated.PanX;
        PanY = updated.PanY;
        NotifyViewportChanged();
    }

    public void ResetView(bool updateStatus = true)
    {
        Zoom = DefaultZoom;
        PanX = DefaultPanX;
        PanY = DefaultPanY;

        if (updateStatus)
        {
            StatusMessage = "Viewport reset.";
        }

        NotifyViewportChanged();
    }

    public void FitToViewport(double viewportWidth, double viewportHeight, bool updateStatus = true)
    {
        if (Nodes.Count == 0 || viewportWidth <= 0 || viewportHeight <= 0)
        {
            if (updateStatus)
            {
                StatusMessage = "Nothing to fit yet.";
            }

            return;
        }

        var minX = Nodes.Min(node => node.X);
        var minY = Nodes.Min(node => node.Y);
        var maxX = Nodes.Max(node => node.X + node.Width);
        var maxY = Nodes.Max(node => node.Y + node.Height);

        var graphWidth = Math.Max(maxX - minX, 1);
        var graphHeight = Math.Max(maxY - minY, 1);
        const double padding = 120;

        var zoomX = viewportWidth / (graphWidth + (padding * 2));
        var zoomY = viewportHeight / (graphHeight + (padding * 2));
        var nextZoom = Math.Clamp(Math.Min(zoomX, zoomY), 0.32, 1.4);

        Zoom = nextZoom;
        PanX = ((viewportWidth - (graphWidth * nextZoom)) / 2) - (minX * nextZoom);
        PanY = ((viewportHeight - (graphHeight * nextZoom)) / 2) - (minY * nextZoom);

        if (updateStatus)
        {
            StatusMessage = "Viewport fit to scene.";
        }

        NotifyViewportChanged();
    }

    public void ActivatePort(NodeViewModel node, PortViewModel port)
    {
        SelectSingleNode(node);

        if (port.Direction == PortDirection.Output)
        {
            StartConnection(node.Id, port.Id);
            return;
        }

        if (!HasPendingConnection)
        {
            StatusMessage = "Select an output port first.";
            return;
        }

        ConnectPorts(PendingSourceNode!.Id, PendingSourcePort!.Id, node.Id, port.Id);
    }

    public void StartConnection(string sourceNodeId, string sourcePortId)
    {
        var sourceNode = FindNode(sourceNodeId);
        var sourcePort = sourceNode?.GetPort(sourcePortId);
        if (sourceNode is null || sourcePort is null)
        {
            return;
        }

        if (sourcePort.Direction != PortDirection.Output)
        {
            StatusMessage = "Only output ports can start a connection.";
            return;
        }

        if (HasPendingConnection
            && PendingSourceNode?.Id == sourceNode.Id
            && PendingSourcePort?.Id == sourcePort.Id)
        {
            CancelPendingConnection("Connection preview cancelled.");
            return;
        }

        PendingSourceNode = sourceNode;
        PendingSourcePort = sourcePort;
        StatusMessage = $"Connecting from {sourceNode.Title}.{sourcePort.Label}.";
    }

    public void ConnectPorts(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId)
    {
        var sourceNode = FindNode(sourceNodeId);
        var sourcePort = sourceNode?.GetPort(sourcePortId);
        var targetNode = FindNode(targetNodeId);
        var targetPort = targetNode?.GetPort(targetPortId);

        if (sourceNode is null || sourcePort is null || targetNode is null || targetPort is null)
        {
            return;
        }

        if (sourcePort.Direction != PortDirection.Output || targetPort.Direction != PortDirection.Input)
        {
            StatusMessage = "Connections must go from an output port to an input port.";
            return;
        }

        var compatibility = _compatibilityService.Evaluate(sourcePort.TypeId, targetPort.TypeId);
        if (!compatibility.IsCompatible)
        {
            StatusMessage = $"Incompatible connection: {sourcePort.TypeId} -> {targetPort.TypeId}.";
            return;
        }

        if (Connections.Any(connection =>
                connection.SourceNodeId == sourceNode.Id
                && connection.SourcePortId == sourcePort.Id
                && connection.TargetNodeId == targetNode.Id
                && connection.TargetPortId == targetPort.Id))
        {
            CancelPendingConnection("That connection already exists.");
            return;
        }

        var replaced = Connections
            .Where(connection => connection.TargetNodeId == targetNode.Id && connection.TargetPortId == targetPort.Id)
            .ToList();

        foreach (var connection in replaced)
        {
            Connections.Remove(connection);
        }

        Connections.Add(new ConnectionViewModel(
            CreateConnectionId(),
            sourceNode.Id,
            sourcePort.Id,
            targetNode.Id,
            targetPort.Id,
            $"{sourcePort.Label} to {targetPort.Label}",
            sourcePort.AccentHex,
            compatibility.ConversionId));

        PendingSourceNode = null;
        PendingSourcePort = null;
        MarkDirty(
            compatibility.Kind == PortCompatibilityKind.ImplicitConversion
                ? $"Connected {sourceNode.Title} to {targetNode.Title} with implicit conversion."
                : $"Connected {sourceNode.Title} to {targetNode.Title}.");
        NotifyDocumentChanged(
            GraphEditorDocumentChangeKind.ConnectionsChanged,
            nodeIds: [sourceNode.Id, targetNode.Id],
            connectionIds: [Connections[^1].Id]);
    }

    public void CancelPendingConnection(string? status = null)
    {
        PendingSourceNode = null;
        PendingSourcePort = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            StatusMessage = status;
        }
    }

    public void DeleteSelectedNode()
        => DeleteSelection();

    public void DeleteSelection()
    {
        if (SelectedNodes.Count == 0)
        {
            StatusMessage = "Select a node before deleting.";
            return;
        }

        var removedNodes = SelectedNodes.ToList();
        var removedNodeIds = removedNodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
        var removedConnections = Connections
            .Where(connection =>
                removedNodeIds.Contains(connection.SourceNodeId)
                || removedNodeIds.Contains(connection.TargetNodeId))
            .ToList();

        foreach (var connection in removedConnections)
        {
            Connections.Remove(connection);
        }

        foreach (var node in removedNodes)
        {
            Nodes.Remove(node);
        }

        CancelPendingConnection();
        SetSelection([], null);
        MarkDirty(removedNodes.Count == 1
            ? $"Deleted {removedNodes[0].Title}."
            : $"Deleted {removedNodes.Count} nodes.");
        NotifyDocumentChanged(
            GraphEditorDocumentChangeKind.NodesRemoved,
            nodeIds: removedNodes.Select(node => node.Id).ToList(),
            connectionIds: removedConnections.Select(connection => connection.Id).ToList());
    }

    /// <summary>
    /// 将当前选择按左边缘对齐。
    /// </summary>
    public void AlignSelectionLeft()
        => ApplySelectionLayout(NodeSelectionLayoutService.AlignLeft, minimumCount: 2, "Aligned selection left.");

    /// <summary>
    /// 将当前选择按水平中心对齐。
    /// </summary>
    public void AlignSelectionCenter()
        => ApplySelectionLayout(NodeSelectionLayoutService.AlignCenter, minimumCount: 2, "Aligned selection center.");

    /// <summary>
    /// 将当前选择按右边缘对齐。
    /// </summary>
    public void AlignSelectionRight()
        => ApplySelectionLayout(NodeSelectionLayoutService.AlignRight, minimumCount: 2, "Aligned selection right.");

    /// <summary>
    /// 将当前选择按上边缘对齐。
    /// </summary>
    public void AlignSelectionTop()
        => ApplySelectionLayout(NodeSelectionLayoutService.AlignTop, minimumCount: 2, "Aligned selection top.");

    /// <summary>
    /// 将当前选择按垂直中心对齐。
    /// </summary>
    public void AlignSelectionMiddle()
        => ApplySelectionLayout(NodeSelectionLayoutService.AlignMiddle, minimumCount: 2, "Aligned selection middle.");

    /// <summary>
    /// 将当前选择按下边缘对齐。
    /// </summary>
    public void AlignSelectionBottom()
        => ApplySelectionLayout(NodeSelectionLayoutService.AlignBottom, minimumCount: 2, "Aligned selection bottom.");

    /// <summary>
    /// 将当前选择按水平方向均匀分布。
    /// </summary>
    public void DistributeSelectionHorizontally()
        => ApplySelectionLayout(NodeSelectionLayoutService.DistributeHorizontally, minimumCount: 3, "Distributed selection horizontally.");

    /// <summary>
    /// 将当前选择按垂直方向均匀分布。
    /// </summary>
    public void DistributeSelectionVertically()
        => ApplySelectionLayout(NodeSelectionLayoutService.DistributeVertically, minimumCount: 3, "Distributed selection vertically.");

    public void DeleteNodeById(string nodeId)
    {
        var node = FindNode(nodeId);
        if (node is null)
        {
            return;
        }

        var remainingSelection = SelectedNodes.Where(selected => !ReferenceEquals(selected, node)).ToList();
        var removedConnections = Connections
            .Where(connection => connection.SourceNodeId == node.Id || connection.TargetNodeId == node.Id)
            .ToList();

        foreach (var connection in removedConnections)
        {
            Connections.Remove(connection);
        }

        Nodes.Remove(node);
        if (PendingSourceNode?.Id == node.Id)
        {
            CancelPendingConnection();
        }

        SetSelection(remainingSelection, remainingSelection.LastOrDefault());
        MarkDirty($"Deleted {node.Title}.");
        NotifyDocumentChanged(
            GraphEditorDocumentChangeKind.NodesRemoved,
            nodeIds: [node.Id],
            connectionIds: removedConnections.Select(connection => connection.Id).ToList());
    }

    public void DuplicateNode(string nodeId)
    {
        var node = FindNode(nodeId);
        if (node is null)
        {
            return;
        }

        var duplicate = new NodeViewModel(node.ToModel() with
        {
            Id = CreateNodeId(node.DefinitionId, node.Id),
            Position = new GraphPoint(node.X + 48, node.Y + 48),
        });

        Nodes.Add(duplicate);
        SelectSingleNode(duplicate);
        MarkDirty($"Duplicated {node.Title}.");
        NotifyDocumentChanged(GraphEditorDocumentChangeKind.NodesAdded, nodeIds: [duplicate.Id]);
    }

    public void DisconnectIncoming(string nodeId) => RemoveConnections(connection => connection.TargetNodeId == nodeId, "Disconnected incoming links.");

    public void DisconnectOutgoing(string nodeId) => RemoveConnections(connection => connection.SourceNodeId == nodeId, "Disconnected outgoing links.");

    public void DisconnectAll(string nodeId) => RemoveConnections(
        connection => connection.SourceNodeId == nodeId || connection.TargetNodeId == nodeId,
        "Disconnected all links.");

    public void BreakConnectionsForPort(string nodeId, string portId)
        => RemoveConnections(
            connection =>
                (connection.SourceNodeId == nodeId && connection.SourcePortId == portId)
                || (connection.TargetNodeId == nodeId && connection.TargetPortId == portId),
            "Disconnected port links.");

    public void DeleteConnection(string connectionId)
    {
        var connection = FindConnection(connectionId);
        if (connection is null)
        {
            return;
        }

        Connections.Remove(connection);
        MarkDirty($"Deleted connection {connection.Label}.");
        NotifyDocumentChanged(GraphEditorDocumentChangeKind.ConnectionsChanged, connectionIds: [connection.Id]);
    }

    public ConnectionViewModel? FindConnection(string connectionId)
        => Connections.FirstOrDefault(connection => connection.Id == connectionId);

    public void CenterViewOnNode(string nodeId)
    {
        var node = FindNode(nodeId);
        if (node is null || _viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return;
        }

        PanX = (_viewportWidth / 2) - ((node.X + (node.Width / 2)) * Zoom);
        PanY = (_viewportHeight / 2) - ((node.Y + (node.Height / 2)) * Zoom);
        StatusMessage = $"Centered on {node.Title}.";
    }

    public void CenterViewAt(GraphPoint worldPoint, bool updateStatus = true)
    {
        if (_viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return;
        }

        PanX = (_viewportWidth / 2) - (worldPoint.X * Zoom);
        PanY = (_viewportHeight / 2) - (worldPoint.Y * Zoom);

        if (updateStatus)
        {
            StatusMessage = "Viewport centered from mini map.";
        }

        NotifyViewportChanged();
    }

    public IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId)
    {
        var sourceNode = FindNode(sourceNodeId);
        var sourcePort = sourceNode?.GetPort(sourcePortId);
        if (sourceNode is null || sourcePort is null || sourcePort.Direction != PortDirection.Output)
        {
            return [];
        }

        return Nodes
            .SelectMany(node => node.Inputs.Select(port => (node, port)))
            .Where(target => !(target.node.Id == sourceNode.Id && target.port.Id == sourcePort.Id))
            .Select(target => new CompatiblePortTarget(
                target.node,
                target.port,
                _compatibilityService.Evaluate(sourcePort.TypeId, target.port.TypeId)))
            .Where(target => target.Compatibility.IsCompatible)
            .ToList();
    }

    private void ApplySelectionLayout(Action<IReadOnlyList<NodeViewModel>> applyLayout, int minimumCount, string status)
    {
        var selectedNodes = SelectedNodes.ToList();
        if (selectedNodes.Count < minimumCount)
        {
            StatusMessage = minimumCount switch
            {
                2 => "Select at least two nodes for alignment.",
                3 => "Select at least three nodes for distribution.",
                _ => "Selection is too small for that operation.",
            };
            return;
        }

        applyLayout(selectedNodes);
        MarkDirty(status);
        NotifyDocumentChanged(GraphEditorDocumentChangeKind.LayoutChanged, selectedNodes.Select(node => node.Id).ToList());
    }

    private GraphSelectionFragment? CreateSelectionFragment()
    {
        if (SelectedNodes.Count == 0)
        {
            return null;
        }

        // 复制时只保留当前选择诱导出的子图，避免粘贴结果依赖外部未复制节点。
        var selectedIds = SelectedNodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
        var copiedNodes = SelectedNodes
            .Select(node => node.ToModel())
            .ToList();

        var copiedConnections = Connections
            .Where(connection =>
                selectedIds.Contains(connection.SourceNodeId)
                && selectedIds.Contains(connection.TargetNodeId))
            .Select(connection => connection.ToModel())
            .ToList();

        var origin = new GraphPoint(
            copiedNodes.Min(node => node.Position.X),
            copiedNodes.Min(node => node.Position.Y));

        return new GraphSelectionFragment(
            copiedNodes,
            copiedConnections,
            origin,
            SelectedNode?.Id);
    }

    private bool PasteFragment(GraphSelectionFragment fragment, string actionPrefix)
    {
        _selectionClipboard.Store(fragment);
        var targetOrigin = _selectionClipboard.GetNextPasteOrigin(GetViewportCenter());
        var nodeIdMap = new Dictionary<string, string>(StringComparer.Ordinal);
        var pastedNodes = new List<NodeViewModel>(fragment.Nodes.Count);

        foreach (var copiedNode in fragment.Nodes)
        {
            var newId = CreateNodeId(copiedNode.DefinitionId, copiedNode.Id);
            nodeIdMap[copiedNode.Id] = newId;

            var relativePosition = copiedNode.Position - fragment.Origin;
            var pastedNode = new NodeViewModel(copiedNode with
            {
                Id = newId,
                Position = targetOrigin + relativePosition,
            });

            Nodes.Add(pastedNode);
            pastedNodes.Add(pastedNode);
        }

        foreach (var copiedConnection in fragment.Connections)
        {
            if (!nodeIdMap.TryGetValue(copiedConnection.SourceNodeId, out var sourceNodeId)
                || !nodeIdMap.TryGetValue(copiedConnection.TargetNodeId, out var targetNodeId))
            {
                continue;
            }

            Connections.Add(new ConnectionViewModel(
                CreateConnectionId(),
                sourceNodeId,
                copiedConnection.SourcePortId,
                targetNodeId,
                copiedConnection.TargetPortId,
                copiedConnection.Label,
                copiedConnection.AccentHex,
                copiedConnection.ConversionId));
        }

        if (pastedNodes.Count == 0)
        {
            return false;
        }

        NodeViewModel? primaryNode = null;
        if (!string.IsNullOrWhiteSpace(fragment.PrimaryNodeId)
            && nodeIdMap.TryGetValue(fragment.PrimaryNodeId, out var remappedPrimaryNodeId))
        {
            primaryNode = pastedNodes.FirstOrDefault(node => node.Id == remappedPrimaryNodeId);
        }

        SetSelection(pastedNodes, primaryNode ?? pastedNodes[^1]);
        MarkDirty(pastedNodes.Count == 1
            ? $"{actionPrefix} {pastedNodes[0].Title}."
            : $"{actionPrefix} {pastedNodes.Count} nodes.");
        return true;
    }

    private async Task<GraphSelectionFragment?> GetBestAvailableClipboardFragmentAsync()
    {
        if (_textClipboardBridge is not null)
        {
            var clipboardText = await _textClipboardBridge.ReadTextAsync(CancellationToken.None);
            // 优先读取系统剪贴板 JSON，但仍保留进程内剪贴板作为可靠回退。
            if (GraphClipboardPayloadSerializer.TryDeserialize(clipboardText, out var systemFragment))
            {
                return systemFragment;
            }
        }

        return _selectionClipboard.Peek();
    }

    /// <summary>
    /// 复制当前选择，并尽可能同步到系统剪贴板 JSON 文本。
    /// </summary>
    public async Task CopySelectionAsync()
    {
        var fragment = CreateSelectionFragment();
        if (fragment is null)
        {
            StatusMessage = "Select at least one node before copying.";
            return;
        }

        _selectionClipboard.Store(fragment);
        var clipboardJson = GraphClipboardPayloadSerializer.Serialize(fragment);
        if (_textClipboardBridge is not null)
        {
            await _textClipboardBridge.WriteTextAsync(clipboardJson, CancellationToken.None);
        }

        RaiseComputedPropertyChanges();
        StatusMessage = fragment.Nodes.Count == 1
            ? $"Copied {fragment.Nodes[0].Title}."
            : $"Copied {fragment.Nodes.Count} nodes.";
    }

    public void ExportSelectionFragment()
    {
        var fragment = CreateSelectionFragment();
        if (fragment is null)
        {
            StatusMessage = "Select at least one node before exporting a fragment.";
            return;
        }

        _fragmentWorkspaceService.Save(fragment);
        RaiseComputedPropertyChanges();
        StatusMessage = $"Exported fragment to {_fragmentWorkspaceService.FragmentPath}.";
        FragmentExported?.Invoke(
            this,
            new GraphEditorFragmentEventArgs(
                _fragmentWorkspaceService.FragmentPath,
                fragment.Nodes.Count,
                fragment.Connections.Count));
    }

    public void ExportSelectionAsTemplate()
    {
        if (!BehaviorOptions.Fragments.EnableFragmentLibrary)
        {
            StatusMessage = "Fragment template library is disabled.";
            return;
        }

        var fragment = CreateSelectionFragment();
        if (fragment is null)
        {
            StatusMessage = "Select at least one node before exporting a fragment template.";
            return;
        }

        var templateName = SelectedNode?.Title ?? $"selection-{fragment.Nodes.Count}";
        var path = _fragmentLibraryService.SaveTemplate(fragment, templateName);
        RefreshFragmentTemplates();
        StatusMessage = $"Exported fragment template to {path}.";
        FragmentExported?.Invoke(
            this,
            new GraphEditorFragmentEventArgs(
                path,
                fragment.Nodes.Count,
                fragment.Connections.Count));
    }

    /// <summary>
    /// 将当前选择导出到指定片段文件路径。
    /// </summary>
    /// <param name="path">目标文件路径。</param>
    /// <returns>导出成功时返回 <see langword="true"/>。</returns>
    public bool ExportSelectionFragmentTo(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fragment = CreateSelectionFragment();
        if (fragment is null)
        {
            StatusMessage = "Select at least one node before exporting a fragment.";
            return false;
        }

        _fragmentWorkspaceService.Save(fragment, path);
        StatusMessage = $"Exported fragment to {path}.";
        FragmentExported?.Invoke(
            this,
            new GraphEditorFragmentEventArgs(
                path,
                fragment.Nodes.Count,
                fragment.Connections.Count));
        return true;
    }

    /// <summary>
    /// 从系统剪贴板或进程内剪贴板恢复选择片段并粘贴到当前视口附近。
    /// </summary>
    public async Task PasteSelectionAsync()
    {
        var fragment = await GetBestAvailableClipboardFragmentAsync();
        if (fragment is null || fragment.Nodes.Count == 0)
        {
            StatusMessage = "Nothing copied yet.";
            return;
        }

        if (!PasteFragment(fragment, "Pasted"))
        {
            return;
        }
    }

    public void ImportFragment()
    {
        if (!_fragmentWorkspaceService.Exists())
        {
            StatusMessage = "No exported fragment file is available yet.";
            return;
        }

        var fragment = _fragmentWorkspaceService.Load();
        _selectionClipboard.Store(fragment);
        RaiseComputedPropertyChanges();

        if (!PasteFragment(fragment, "Imported"))
        {
            StatusMessage = "Fragment file did not contain any nodes.";
        }
        else
        {
            FragmentImported?.Invoke(
                this,
                new GraphEditorFragmentEventArgs(
                    _fragmentWorkspaceService.FragmentPath,
                    fragment.Nodes.Count,
                    fragment.Connections.Count));
        }
    }

    public void ClearFragment()
    {
        if (!_fragmentWorkspaceService.Exists())
        {
            StatusMessage = "No exported fragment file is available yet.";
            return;
        }

        _fragmentWorkspaceService.Delete();
        RaiseComputedPropertyChanges();
        StatusMessage = "Cleared the saved fragment file.";
    }

    public void ImportSelectedTemplate()
    {
        if (SelectedFragmentTemplate is null)
        {
            StatusMessage = "Select a fragment template first.";
            return;
        }

        ImportFragmentFrom(SelectedFragmentTemplate.Path);
    }

    public void DeleteSelectedTemplate()
    {
        if (SelectedFragmentTemplate is null)
        {
            StatusMessage = "Select a fragment template first.";
            return;
        }

        var deletedPath = SelectedFragmentTemplate.Path;
        _fragmentLibraryService.DeleteTemplate(deletedPath);
        RefreshFragmentTemplates();
        StatusMessage = $"Deleted fragment template {System.IO.Path.GetFileNameWithoutExtension(deletedPath)}.";
    }

    /// <summary>
    /// 从指定片段文件路径导入节点片段。
    /// </summary>
    /// <param name="path">源文件路径。</param>
    /// <returns>导入并粘贴成功时返回 <see langword="true"/>。</returns>
    public bool ImportFragmentFrom(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!_fragmentWorkspaceService.Exists(path))
        {
            StatusMessage = $"Fragment file '{path}' was not found.";
            return false;
        }

        var fragment = _fragmentWorkspaceService.Load(path);
        _selectionClipboard.Store(fragment);
        RaiseComputedPropertyChanges();
        var imported = PasteFragment(fragment, "Imported");
        if (imported)
        {
            FragmentImported?.Invoke(
                this,
                new GraphEditorFragmentEventArgs(
                    path,
                    fragment.Nodes.Count,
                    fragment.Connections.Count));
        }

        return imported;
    }

    public void SaveWorkspace()
    {
        try
        {
            _workspaceService.Save(CreateDocumentSnapshot());
            _lastSavedDocumentSignature = CreateDocumentSignature();
            UpdateDirtyState();
            StatusMessage = $"Saved snapshot to {WorkspacePath}.";
            NotifyDocumentChanged(GraphEditorDocumentChangeKind.WorkspaceSaved, statusMessage: StatusMessage);
            RaiseComputedPropertyChanges();
        }
        catch (Exception exception)
        {
            StatusMessage = $"Save failed: {exception.Message}";
        }
    }

    public bool LoadWorkspace()
    {
        try
        {
            if (!_workspaceService.Exists())
            {
                StatusMessage = "No saved snapshot yet. Save once to create one.";
                return false;
            }

            var document = _workspaceService.Load();
            LoadDocument(document, "Workspace loaded from disk.", markClean: true);
            CancelPendingConnection();
            ClearSelection();
            ResetView(updateStatus: false);
            NotifyDocumentChanged(GraphEditorDocumentChangeKind.WorkspaceLoaded, statusMessage: StatusMessage);
            return true;
        }
        catch (Exception exception)
        {
            StatusMessage = $"Load failed: {exception.Message}";
            return false;
        }
    }

    public GraphDocument CreateDocumentSnapshot()
        => new(
            Title,
            Description,
            Nodes.Select(node => node.ToModel()).ToList(),
            Connections.Select(connection => connection.ToModel()).ToList());

    public NodeViewModel? FindNode(string nodeId)
        => Nodes.FirstOrDefault(node => node.Id == nodeId);

    private void LoadDocument(GraphDocument document, string status, bool markClean)
    {
        _suspendDirtyTracking = true;
        _suspendHistoryTracking = true;

        foreach (var node in Nodes.ToList())
        {
            node.PropertyChanged -= HandleNodePropertyChanged;
        }

        Nodes.Clear();
        Connections.Clear();

        Title = document.Title;
        Description = document.Description;

        foreach (var node in document.Nodes)
        {
            Nodes.Add(new NodeViewModel(node));
        }

        foreach (var connection in document.Connections)
        {
            Connections.Add(new ConnectionViewModel(
                connection.Id,
                connection.SourceNodeId,
                connection.SourcePortId,
                connection.TargetNodeId,
                connection.TargetPortId,
                connection.Label,
                connection.AccentHex,
                connection.ConversionId));
        }

        SelectedNodes.Clear();
        SelectedNode = null;
        SelectedNodeParameters.Clear();
        _pendingInteractionState = null;
        IsDirty = !markClean;
        StatusMessage = status;
        _suspendHistoryTracking = false;
        _suspendDirtyTracking = false;

        var historyState = CaptureHistoryState();
        if (!_suspendHistoryTracking)
        {
            _historyService.Reset(historyState);
        }

        _lastSavedDocumentSignature = markClean
            ? historyState.Signature
            : _lastSavedDocumentSignature;
        RaiseComputedPropertyChanges();
    }

    private void MarkDirty(string status)
    {
        StatusMessage = status;
        UpdateDirtyState();
        PushCurrentHistoryState();
        RaiseComputedPropertyChanges();
    }

    private GraphPoint GetViewportCenter()
    {
        if (_viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return ScreenToWorld(new GraphPoint(820, 440));
        }

        return ScreenToWorld(new GraphPoint(_viewportWidth / 2, _viewportHeight / 2));
    }

    private string CreateNodeId(string templateKey)
        => CreateUniqueId(Nodes.Select(node => node.Id), $"{templateKey}-");

    private string CreateNodeId(NodeDefinitionId? definitionId, string fallbackKey)
        => CreateNodeId(
            (definitionId?.Value ?? fallbackKey)
            .Replace(".", "-", StringComparison.Ordinal));

    private string CreateConnectionId()
        => CreateUniqueId(Connections.Select(connection => connection.Id), "connection-");

    private bool CanFitView()
        => Nodes.Count > 0 && _viewportWidth > 0 && _viewportHeight > 0;

    private static bool Intersects(NodeBounds bounds, double left, double top, double right, double bottom)
        => bounds.X < right
           && (bounds.X + bounds.Width) > left
           && bounds.Y < bottom
           && (bounds.Y + bounds.Height) > top;

    private static bool ApplyNodePosition(NodeViewModel node, GraphPoint position)
    {
        if (Math.Abs(node.X - position.X) < double.Epsilon
            && Math.Abs(node.Y - position.Y) < double.Epsilon)
        {
            return false;
        }

        node.X = position.X;
        node.Y = position.Y;
        return true;
    }

    private GraphEditorHistoryState CaptureHistoryState()
    {
        var document = CreateDocumentSnapshot();
        return new GraphEditorHistoryState(
            document,
            SelectedNodes.Select(node => node.Id).ToList(),
            SelectedNode?.Id,
            CreateDocumentSignature(document));
    }

    private void RestoreHistoryState(GraphEditorHistoryState state, string status)
    {
        _suspendHistoryTracking = true;
        LoadDocument(state.Document, status, markClean: false);

        var restoredSelection = state.SelectedNodeIds
            .Select(FindNode)
            .Where(node => node is not null)
            .Cast<NodeViewModel>()
            .ToList();
        var primaryNode = !string.IsNullOrWhiteSpace(state.PrimarySelectedNodeId)
            ? restoredSelection.FirstOrDefault(node => node.Id == state.PrimarySelectedNodeId)
            : restoredSelection.LastOrDefault();

        SetSelection(restoredSelection, primaryNode, status);
        _suspendHistoryTracking = false;
        UpdateDirtyState();
        RaiseComputedPropertyChanges();
    }

    private void PushCurrentHistoryState()
    {
        if (_suspendHistoryTracking)
        {
            return;
        }

        _historyService.Push(CaptureHistoryState());
    }

    private string CreateDocumentSignature()
        => CreateDocumentSignature(CreateDocumentSnapshot());

    private static string CreateDocumentSignature(GraphDocument document)
        => GraphDocumentSerializer.Serialize(document);

    private static GraphEditorBehaviorOptions ResolveBehaviorOptions(
        GraphEditorBehaviorOptions? behaviorOptions,
        GraphEditorStyleOptions styleOptions)
    {
        if (behaviorOptions is not null)
        {
            return behaviorOptions;
        }

        return GraphEditorBehaviorOptions.Default with
        {
            DragAssist = GraphEditorBehaviorOptions.Default.DragAssist with
            {
                EnableGridSnapping = styleOptions.Canvas.EnableGridSnapping,
                EnableAlignmentGuides = styleOptions.Canvas.EnableAlignmentGuides,
                SnapTolerance = styleOptions.Canvas.SnapTolerance,
            },
        };
    }

    private void UpdateDirtyState()
    {
        if (_suspendDirtyTracking)
        {
            return;
        }

        IsDirty = !string.Equals(CreateDocumentSignature(), _lastSavedDocumentSignature, StringComparison.Ordinal);
    }

    partial void OnZoomChanged(double value) => RaiseComputedPropertyChanges();

    partial void OnSelectedNodeChanged(NodeViewModel? value) => RaiseComputedPropertyChanges();

    partial void OnPendingSourceNodeChanged(NodeViewModel? value) => RaiseComputedPropertyChanges();

    partial void OnPendingSourcePortChanged(PortViewModel? value) => RaiseComputedPropertyChanges();

    partial void OnIsDirtyChanged(bool value) => RaiseComputedPropertyChanges();

    private void HandleNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems is not null)
        {
            foreach (NodeViewModel node in args.OldItems)
            {
                node.PropertyChanged -= HandleNodePropertyChanged;
            }
        }

        if (args.NewItems is not null)
        {
            foreach (NodeViewModel node in args.NewItems)
            {
                node.PropertyChanged += HandleNodePropertyChanged;
            }
        }

        CoerceSelectionToExistingNodes();
        FitViewCommand.NotifyCanExecuteChanged();
        RaiseComputedPropertyChanges();
    }

    private void HandleConnectionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        => RaiseComputedPropertyChanges();

    private void HandleNodePropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (_suspendDirtyTracking || sender is not NodeViewModel)
        {
            return;
        }

        if (args.PropertyName is nameof(NodeViewModel.X) or nameof(NodeViewModel.Y))
        {
            if (!IsDirty)
            {
                IsDirty = true;
                RaiseComputedPropertyChanges();
            }
        }
    }

    private void RaiseComputedPropertyChanges()
    {
        DeleteSelectionCommand.NotifyCanExecuteChanged();
        CopySelectionCommand.NotifyCanExecuteChanged();
        PasteCommand.NotifyCanExecuteChanged();
        ExportSelectionFragmentCommand.NotifyCanExecuteChanged();
        ImportFragmentCommand.NotifyCanExecuteChanged();
        ClearFragmentCommand.NotifyCanExecuteChanged();
        AlignLeftCommand.NotifyCanExecuteChanged();
        AlignCenterCommand.NotifyCanExecuteChanged();
        AlignRightCommand.NotifyCanExecuteChanged();
        AlignTopCommand.NotifyCanExecuteChanged();
        AlignMiddleCommand.NotifyCanExecuteChanged();
        AlignBottomCommand.NotifyCanExecuteChanged();
        DistributeHorizontallyCommand.NotifyCanExecuteChanged();
        DistributeVerticallyCommand.NotifyCanExecuteChanged();
        CancelPendingConnectionCommand.NotifyCanExecuteChanged();
        FitViewCommand.NotifyCanExecuteChanged();

        OnPropertyChanged(nameof(HasPendingConnection));
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(HasMultipleSelection));
        OnPropertyChanged(nameof(CanDeleteSelection));
        OnPropertyChanged(nameof(CanCopySelection));
        OnPropertyChanged(nameof(CanPaste));
        OnPropertyChanged(nameof(CanExportSelectionFragment));
        OnPropertyChanged(nameof(CanImportFragment));
        OnPropertyChanged(nameof(CanAlignSelection));
        OnPropertyChanged(nameof(CanDistributeSelection));
        OnPropertyChanged(nameof(HasEditableParameters));
        OnPropertyChanged(nameof(HasBatchEditableParameters));
        OnPropertyChanged(nameof(HasAnyEditableParameters));
        OnPropertyChanged(nameof(ViewportWidth));
        OnPropertyChanged(nameof(ViewportHeight));
        OnPropertyChanged(nameof(StatsCaption));
        OnPropertyChanged(nameof(WorkspaceCaption));
        OnPropertyChanged(nameof(FragmentPath));
        OnPropertyChanged(nameof(FragmentCaption));
        OnPropertyChanged(nameof(FragmentStatusCaption));
        OnPropertyChanged(nameof(ModeCaption));
        OnPropertyChanged(nameof(InspectorTitle));
        OnPropertyChanged(nameof(InspectorCategory));
        OnPropertyChanged(nameof(InspectorDescription));
        OnPropertyChanged(nameof(InspectorInputs));
        OnPropertyChanged(nameof(InspectorOutputs));
        OnPropertyChanged(nameof(InspectorConnections));
        OnPropertyChanged(nameof(InspectorUpstream));
        OnPropertyChanged(nameof(InspectorDownstream));
        OnPropertyChanged(nameof(SelectionCaption));
    }

    private void NotifyDocumentChanged(
        GraphEditorDocumentChangeKind changeKind,
        IReadOnlyList<string>? nodeIds = null,
        IReadOnlyList<string>? connectionIds = null,
        string? statusMessage = null)
    {
        DocumentChanged?.Invoke(
            this,
            new GraphEditorDocumentChangedEventArgs(
                changeKind,
                nodeIds,
                connectionIds,
                statusMessage ?? StatusMessage));
    }

    private void NotifySelectionChanged()
    {
        SelectionChanged?.Invoke(
            this,
            new GraphEditorSelectionChangedEventArgs(
                SelectedNodes.Select(node => node.Id).ToList(),
                SelectedNode?.Id));
    }

    private void NotifyViewportChanged()
    {
        ViewportChanged?.Invoke(
            this,
            new GraphEditorViewportChangedEventArgs(
                Zoom,
                PanX,
                PanY,
                ViewportWidth,
                ViewportHeight));
    }

    private void CoerceSelectionToExistingNodes()
    {
        if (SelectedNodes.Count == 0 && SelectedNode is null)
        {
            return;
        }

        var nextSelection = SelectedNodes
            .Where(Nodes.Contains)
            .Distinct()
            .ToList();

        var nextPrimary = SelectedNode is not null && nextSelection.Contains(SelectedNode)
            ? SelectedNode
            : nextSelection.LastOrDefault();

        if (nextSelection.SequenceEqual(SelectedNodes) && ReferenceEquals(nextPrimary, SelectedNode))
        {
            return;
        }

        SetSelection(nextSelection, nextPrimary);
    }

    private void RemoveConnections(Func<ConnectionViewModel, bool> predicate, string status)
    {
        var removed = Connections.Where(predicate).ToList();
        if (removed.Count == 0)
        {
            StatusMessage = "No matching connections to remove.";
            return;
        }

        foreach (var connection in removed)
        {
            Connections.Remove(connection);
        }

        MarkDirty(status);
    }

    private List<ConnectionViewModel> GetIncomingConnections(NodeViewModel node)
        => Connections.Where(connection => connection.TargetNodeId == node.Id).ToList();

    private List<ConnectionViewModel> GetOutgoingConnections(NodeViewModel node)
        => Connections.Where(connection => connection.SourceNodeId == node.Id).ToList();

    private string FormatRelatedNodes(IEnumerable<ConnectionViewModel> connections, bool useSource)
    {
        var lines = connections
            .Select(connection =>
            {
                var relatedId = useSource ? connection.SourceNodeId : connection.TargetNodeId;
                var relatedPortId = useSource ? connection.SourcePortId : connection.TargetPortId;
                var relatedNode = FindNode(relatedId);
                var relatedPort = relatedNode?.GetPort(relatedPortId);
                if (relatedNode is null)
                {
                    return null;
                }

                return $"{relatedNode.Title}  ·  {relatedPort?.Label ?? relatedPortId}";
            })
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (lines.Count == 0)
        {
            return "None";
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatPorts(IEnumerable<PortViewModel> ports)
    {
        var items = ports.ToList();
        if (items.Count == 0)
        {
            return "None";
        }

        return string.Join(Environment.NewLine, items.Select(port => $"{port.Label}  ·  {port.DataType}"));
    }

    private static string CreateUniqueId(IEnumerable<string> existingIds, string prefix)
    {
        var ids = existingIds.ToHashSet(StringComparer.Ordinal);
        var next = 1;

        foreach (var id in ids)
        {
            if (!id.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            var suffix = id[prefix.Length..];
            if (int.TryParse(suffix, out var value))
            {
                next = Math.Max(next, value + 1);
            }
        }

        string candidate;
        do
        {
            candidate = $"{prefix}{next:000}";
            next++;
        }
        while (ids.Contains(candidate));

        return candidate;
    }

    private void RebuildSelectedNodeParameters()
    {
        SelectedNodeParameters.Clear();

        if (SelectedNodes.Count == 0)
        {
            OnPropertyChanged(nameof(HasEditableParameters));
            OnPropertyChanged(nameof(HasBatchEditableParameters));
            return;
        }

        var sharedDefinitionId = SelectedNodes[0].DefinitionId;
        if (sharedDefinitionId is null
            || SelectedNodes.Any(node => node.DefinitionId != sharedDefinitionId))
        {
            OnPropertyChanged(nameof(HasEditableParameters));
            OnPropertyChanged(nameof(HasBatchEditableParameters));
            return;
        }

        if (!_nodeCatalog.TryGetDefinition(sharedDefinitionId, out var definition) || definition is null)
        {
            OnPropertyChanged(nameof(HasEditableParameters));
            OnPropertyChanged(nameof(HasBatchEditableParameters));
            return;
        }

        foreach (var parameter in definition.Parameters)
        {
            var currentValues = SelectedNodes
                .Select(node => node.GetParameterValue(parameter.Key) ?? parameter.DefaultValue)
                .ToList();
            SelectedNodeParameters.Add(new NodeParameterViewModel(parameter, currentValues, ApplyParameterValue));
        }

        OnPropertyChanged(nameof(HasEditableParameters));
        OnPropertyChanged(nameof(HasBatchEditableParameters));
    }

    private void ApplyParameterValue(NodeParameterViewModel parameter, object? value)
    {
        if (SelectedNodes.Count == 0)
        {
            return;
        }

        foreach (var node in SelectedNodes)
        {
            node.SetParameterValue(parameter.Key, parameter.TypeId, value);
        }

        MarkDirty(SelectedNodes.Count == 1
            ? $"Updated {SelectedNode!.Title} / {parameter.DisplayName}."
            : $"Updated {SelectedNodes.Count} nodes / {parameter.DisplayName}.");
    }
}
