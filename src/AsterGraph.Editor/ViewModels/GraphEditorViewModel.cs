using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
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
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.Viewport;

namespace AsterGraph.Editor.ViewModels;

/// <summary>
/// 图编辑器的主视图模型，承载选择、布局、连线、剪贴板和持久化状态。
/// </summary>
/// <remarks>
/// Phase 1 会继续支持直接构造 <see cref="GraphEditorViewModel"/> 作为兼容立面，
/// 以满足现有宿主基于 <c>new GraphEditorViewModel(...)</c> 的集成路径。
/// 新宿主应优先考虑 <see cref="AsterGraphEditorFactory"/> 和 <see cref="AsterGraphEditorOptions"/>，
/// 但本类型在当前迁移窗口内不会因为新增工厂入口而被移除或标记为过时。
/// </remarks>
public sealed partial class GraphEditorViewModel : ObservableObject, IGraphContextMenuHost
{
    private const double DefaultZoom = 0.88;
    private const double DefaultPanX = 110;
    private const double DefaultPanY = 96;
    private static readonly string[] ComputedPropertyNames =
    [
        nameof(HasPendingConnection),
        nameof(CanSaveWorkspace),
        nameof(CanLoadWorkspace),
        nameof(HasSelection),
        nameof(HasMultipleSelection),
        nameof(CanCreateNodes),
        nameof(CanDeleteSelection),
        nameof(CanCopySelection),
        nameof(CanInsertFragmentContent),
        nameof(CanPaste),
        nameof(CanExportSelectionFragment),
        nameof(CanImportFragment),
        nameof(CanClearFragment),
        nameof(CanExportSelectionAsTemplate),
        nameof(CanImportSelectedTemplate),
        nameof(CanDeleteSelectedTemplate),
        nameof(CanAlignSelection),
        nameof(CanDistributeSelection),
        nameof(HasEditableParameters),
        nameof(HasBatchEditableParameters),
        nameof(HasAnyEditableParameters),
        nameof(CanEditNodeParameters),
        nameof(ViewportWidth),
        nameof(ViewportHeight),
        nameof(StatsCaption),
        nameof(WorkspaceCaption),
        nameof(FragmentPath),
        nameof(FragmentCaption),
        nameof(FragmentStatusCaption),
        nameof(FragmentLibraryCaption),
        nameof(ModeCaption),
        nameof(InspectorTitle),
        nameof(InspectorCategory),
        nameof(InspectorDescription),
        nameof(InspectorInputs),
        nameof(InspectorOutputs),
        nameof(InspectorConnections),
        nameof(InspectorUpstream),
        nameof(InspectorDownstream),
        nameof(SelectionCaption),
    ];

    private readonly INodeCatalog _nodeCatalog;
    private readonly IPortCompatibilityService _compatibilityService;
    private readonly IGraphWorkspaceService _workspaceService;
    private readonly GraphSelectionClipboard _selectionClipboard;
    private readonly IGraphFragmentWorkspaceService _fragmentWorkspaceService;
    private readonly IGraphFragmentLibraryService _fragmentLibraryService;
    private readonly IGraphClipboardPayloadSerializer _clipboardPayloadSerializer;
    private readonly GraphEditorHistoryService _historyService;
    private readonly GraphEditorInspectorProjection _inspectorProjection;
    private readonly GraphEditorCommandStateNotifier _commandStateNotifier = new();
    private readonly IRelayCommand[] _computedStateCommands;
    private readonly Dictionary<string, NodeViewModel> _nodesById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ConnectionViewModel> _connectionsById = new(StringComparer.Ordinal);
    private IGraphContextMenuAugmentor? _contextMenuAugmentor;
    private GraphEditorBehaviorOptions _behaviorOptions = GraphEditorBehaviorOptions.Default;
    private IGraphTextClipboardBridge? _textClipboardBridge;
    private IGraphHostContext? _hostContext;
    private INodePresentationProvider? _nodePresentationProvider;
    private IGraphLocalizationProvider? _localizationProvider;
    private readonly GraphContextMenuBuilder _contextMenuBuilder;
    private bool _suspendDirtyTracking;
    private bool _suspendHistoryTracking;
    private string? _lastSavedDocumentSignature;
    private GraphEditorHistoryState? _pendingInteractionState;
    private double _viewportWidth;
    private double _viewportHeight;
    private bool _isInitialized;

    /// <summary>
    /// 初始化图编辑器视图模型。
    /// </summary>
    /// <param name="document">初始图文档。</param>
    /// <param name="nodeCatalog">节点目录。</param>
    /// <param name="compatibilityService">端口兼容性服务。</param>
    /// <param name="workspaceService">工作区持久化服务。</param>
    /// <param name="fragmentWorkspaceService">单文件片段工作区服务。</param>
    /// <param name="styleOptions">样式配置。</param>
    /// <param name="behaviorOptions">行为配置。</param>
    /// <param name="fragmentLibraryService">片段模板库服务。</param>
    /// <param name="contextMenuAugmentor">宿主右键菜单增强器。</param>
    /// <param name="nodePresentationProvider">节点展示状态提供器。</param>
    /// <param name="localizationProvider">编辑器内置文案本地化提供器。</param>
    /// <param name="clipboardPayloadSerializer">片段和剪贴板载荷的序列化器。</param>
    /// <param name="diagnosticsSink">可选的宿主诊断发布器。</param>
    /// <remarks>
    /// 该构造函数在 Phase 1 中保留为受支持的兼容入口，供现有宿主继续沿用
    /// <c>new GraphEditorViewModel(...)</c> 的组合方式。对于新的宿主组合代码，
    /// 请优先使用 <see cref="AsterGraphEditorFactory.Create(AsterGraphEditorOptions)"/>，
    /// 以便后续阶段通过统一的选项契约扩展初始化能力。
    /// </remarks>
    public GraphEditorViewModel(
        GraphDocument document,
        INodeCatalog nodeCatalog,
        IPortCompatibilityService compatibilityService,
        IGraphWorkspaceService? workspaceService = null,
        IGraphFragmentWorkspaceService? fragmentWorkspaceService = null,
        GraphEditorStyleOptions? styleOptions = null,
        GraphEditorBehaviorOptions? behaviorOptions = null,
        IGraphFragmentLibraryService? fragmentLibraryService = null,
        IGraphContextMenuAugmentor? contextMenuAugmentor = null,
        INodePresentationProvider? nodePresentationProvider = null,
        IGraphLocalizationProvider? localizationProvider = null,
        IGraphClipboardPayloadSerializer? clipboardPayloadSerializer = null,
        IGraphEditorDiagnosticsSink? diagnosticsSink = null)
    {
        _nodeCatalog = nodeCatalog ?? throw new ArgumentNullException(nameof(nodeCatalog));
        _compatibilityService = compatibilityService ?? throw new ArgumentNullException(nameof(compatibilityService));
        _workspaceService = workspaceService ?? new GraphWorkspaceService();
        _selectionClipboard = new GraphSelectionClipboard();
        _clipboardPayloadSerializer = clipboardPayloadSerializer ?? new GraphClipboardPayloadSerializer();
        _fragmentWorkspaceService = fragmentWorkspaceService ?? new GraphFragmentWorkspaceService(clipboardPayloadSerializer: _clipboardPayloadSerializer);
        _fragmentLibraryService = fragmentLibraryService ?? new GraphFragmentLibraryService(clipboardPayloadSerializer: _clipboardPayloadSerializer);
        _historyService = new GraphEditorHistoryService();
        _contextMenuAugmentor = contextMenuAugmentor;
        _nodePresentationProvider = nodePresentationProvider;
        _localizationProvider = localizationProvider;
        _inspectorProjection = new GraphEditorInspectorProjection(
            LocalizeText,
            (key, fallback, arguments) => LocalizeFormat(key, fallback, arguments));
        StyleOptions = styleOptions ?? GraphEditorStyleOptions.Default;
        BehaviorOptions = ResolveBehaviorOptions(behaviorOptions, StyleOptions);

        SaveCommand = new RelayCommand(SaveWorkspace, () => CanSaveWorkspace);
        LoadCommand = new RelayCommand(() => LoadWorkspace(), () => CanLoadWorkspace);
        FitViewCommand = new RelayCommand(() => FitToViewport(_viewportWidth, _viewportHeight), CanFitView);
        ResetViewCommand = new RelayCommand(() => ResetView());
        UndoCommand = new RelayCommand(Undo, () => CanUndo);
        RedoCommand = new RelayCommand(Redo, () => CanRedo);
        DeleteSelectionCommand = new RelayCommand(DeleteSelection, () => CanDeleteSelection);
        CopySelectionCommand = new AsyncRelayCommand(CopySelectionAsync, () => CanCopySelection);
        PasteCommand = new AsyncRelayCommand(PasteSelectionAsync, () => CanPaste);
        ExportSelectionFragmentCommand = new RelayCommand(ExportSelectionFragment, () => CanExportSelectionFragment);
        ImportFragmentCommand = new RelayCommand(ImportFragment, () => CanImportFragment);
        ClearFragmentCommand = new RelayCommand(ClearFragment, () => CanClearFragment);
        RefreshFragmentTemplatesCommand = new RelayCommand(RefreshFragmentTemplates);
        ExportSelectionAsTemplateCommand = new RelayCommand(ExportSelectionAsTemplate, () => CanExportSelectionAsTemplate);
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
            () => CancelPendingConnection(StatusText("editor.status.connection.previewCancelled", "Connection preview cancelled.")),
            () => HasPendingConnection);
        AddNodeCommand = new RelayCommand<NodeTemplateViewModel>(
            template =>
            {
                if (template is not null)
                {
                    AddNode(template);
                }
            },
            template => CanCreateNodes && template is not null);
        _computedStateCommands =
        [
            SaveCommand,
            LoadCommand,
            UndoCommand,
            RedoCommand,
            DeleteSelectionCommand,
            CopySelectionCommand,
            PasteCommand,
            ExportSelectionFragmentCommand,
            ImportFragmentCommand,
            ClearFragmentCommand,
            ExportSelectionAsTemplateCommand,
            ImportSelectedTemplateCommand,
            DeleteSelectedTemplateCommand,
            AlignLeftCommand,
            AlignCenterCommand,
            AlignRightCommand,
            AlignTopCommand,
            AlignMiddleCommand,
            AlignBottomCommand,
            DistributeHorizontallyCommand,
            DistributeVerticallyCommand,
            CancelPendingConnectionCommand,
            FitViewCommand,
            AddNodeCommand,
        ];

        _contextMenuBuilder = new GraphContextMenuBuilder(this, LocalizeText);

        Nodes = new ObservableCollection<NodeViewModel>();
        Connections = new ObservableCollection<ConnectionViewModel>();
        SelectedNodes = new ObservableCollection<NodeViewModel>();
        SelectedNodeParameters = new ObservableCollection<NodeParameterViewModel>();
        NodeTemplates = new ObservableCollection<NodeTemplateViewModel>(
            _nodeCatalog.Definitions.Select(definition => new NodeTemplateViewModel(definition)));
        FragmentTemplates = new ObservableCollection<FragmentTemplateViewModel>();
        Session = new GraphEditorSession(this, diagnosticsSink);

        Nodes.CollectionChanged += HandleNodesCollectionChanged;
        Connections.CollectionChanged += HandleConnectionsCollectionChanged;

        WorkspacePath = _workspaceService.WorkspacePath;
        Title = document.Title;
        Description = document.Description;

        RefreshFragmentTemplates();
        LoadDocument(document, LocalizeText("editor.status.readyToEdit", "Ready to edit."), markClean: true);
        ResetView(updateStatus: false);
        _isInitialized = true;
    }

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    /// <summary>
    /// 获取当前图中的节点集合。
    /// </summary>
    public ObservableCollection<NodeViewModel> Nodes { get; }

    /// <summary>
    /// 获取当前图中的连线集合。
    /// </summary>
    public ObservableCollection<ConnectionViewModel> Connections { get; }

    /// <summary>
    /// 获取当前选中的节点集合。
    /// </summary>
    public ObservableCollection<NodeViewModel> SelectedNodes { get; }

    /// <summary>
    /// 获取基于当前选择投影出的可编辑参数集合。
    /// </summary>
    public ObservableCollection<NodeParameterViewModel> SelectedNodeParameters { get; }

    /// <summary>
    /// 获取由节点目录生成的可插入节点模板集合。
    /// </summary>
    public ObservableCollection<NodeTemplateViewModel> NodeTemplates { get; }

    /// <summary>
    /// 获取从片段模板库加载的模板集合。
    /// </summary>
    public ObservableCollection<FragmentTemplateViewModel> FragmentTemplates { get; }

    /// <summary>
    /// 获取初始化时绑定到编辑器的样式选项。
    /// </summary>
    public GraphEditorStyleOptions StyleOptions { get; }

    /// <summary>
    /// 获取与当前兼容立面共享的运行时会话。
    /// </summary>
    public IGraphEditorSession Session { get; }

    /// <summary>
    /// 获取当前命令权限配置。
    /// </summary>
    public GraphEditorCommandPermissions CommandPermissions => BehaviorOptions.Commands;

    /// <summary>
    /// 获取当前编辑器行为配置。
    /// </summary>
    public GraphEditorBehaviorOptions BehaviorOptions
    {
        get => _behaviorOptions;
        private set
        {
            if (SetProperty(ref _behaviorOptions, value))
            {
                if (!_isInitialized)
                {
                    return;
                }

                OnPropertyChanged(nameof(CommandPermissions));
                ExportSelectionAsTemplateCommand.NotifyCanExecuteChanged();
                RaiseComputedPropertyChanges();
            }
        }
    }

    /// <summary>
    /// 获取或设置宿主右键菜单增强器。
    /// </summary>
    public IGraphContextMenuAugmentor? ContextMenuAugmentor
    {
        get => _contextMenuAugmentor;
        set => SetProperty(ref _contextMenuAugmentor, value);
    }

    /// <summary>
    /// 获取当前宿主上下文信息。
    /// </summary>
    public IGraphHostContext? HostContext => _hostContext;

    /// <summary>
    /// 获取当前节点展示状态提供器。
    /// </summary>
    public INodePresentationProvider? NodePresentationProvider => _nodePresentationProvider;

    /// <summary>
    /// 获取当前图编辑器内置文案本地化提供器。
    /// </summary>
    public IGraphLocalizationProvider? LocalizationProvider => _localizationProvider;

    /// <summary>
    /// 当前工作区快照文件路径。
    /// </summary>
    public string WorkspacePath { get; }

    /// <summary>
    /// 当前默认片段文件路径。
    /// </summary>
    public string FragmentPath => _fragmentWorkspaceService.FragmentPath;

    /// <summary>
    /// 当前片段模板库目录路径。
    /// </summary>
    public string FragmentLibraryPath => _fragmentLibraryService.LibraryPath;

    /// <summary>
    /// 当前视口宽度。
    /// </summary>
    public double ViewportWidth => _viewportWidth;

    /// <summary>
    /// 当前视口高度。
    /// </summary>
    public double ViewportHeight => _viewportHeight;

    /// <summary>
    /// 获取将当前图保存到默认工作区文件的命令。
    /// </summary>
    public IRelayCommand SaveCommand { get; }

    /// <summary>
    /// 获取从默认工作区文件加载图快照的命令。
    /// </summary>
    public IRelayCommand LoadCommand { get; }

    /// <summary>
    /// 获取根据当前节点边界调整视口的命令。
    /// </summary>
    public IRelayCommand FitViewCommand { get; }

    /// <summary>
    /// 获取将缩放和平移恢复为默认视图状态的命令。
    /// </summary>
    public IRelayCommand ResetViewCommand { get; }

    /// <summary>
    /// 获取回退到上一条历史快照的命令。
    /// </summary>
    public IRelayCommand UndoCommand { get; }

    /// <summary>
    /// 获取重新应用下一条历史快照的命令。
    /// </summary>
    public IRelayCommand RedoCommand { get; }

    /// <summary>
    /// 获取删除当前选择内容的命令。
    /// </summary>
    public IRelayCommand DeleteSelectionCommand { get; }

    /// <summary>
    /// 获取将当前选择复制到编辑器剪贴板及宿主文本剪贴板的命令。
    /// </summary>
    public IAsyncRelayCommand CopySelectionCommand { get; }

    /// <summary>
    /// 获取从编辑器剪贴板或宿主文本剪贴板粘贴内容的命令。
    /// </summary>
    public IAsyncRelayCommand PasteCommand { get; }

    /// <summary>
    /// 获取将当前选择导出到默认片段文件的命令。
    /// </summary>
    public IRelayCommand ExportSelectionFragmentCommand { get; }

    /// <summary>
    /// 获取从默认片段文件导入并粘贴内容的命令。
    /// </summary>
    public IRelayCommand ImportFragmentCommand { get; }

    /// <summary>
    /// 获取删除默认片段文件的命令。
    /// </summary>
    public IRelayCommand ClearFragmentCommand { get; }

    /// <summary>
    /// 获取重新加载片段模板库内容的命令。
    /// </summary>
    public IRelayCommand RefreshFragmentTemplatesCommand { get; }

    /// <summary>
    /// 获取将当前选择保存为片段模板的命令。
    /// </summary>
    public IRelayCommand ExportSelectionAsTemplateCommand { get; }

    /// <summary>
    /// 获取导入当前选中片段模板的命令。
    /// </summary>
    public IRelayCommand ImportSelectedTemplateCommand { get; }

    /// <summary>
    /// 获取删除当前选中片段模板的命令。
    /// </summary>
    public IRelayCommand DeleteSelectedTemplateCommand { get; }

    /// <summary>
    /// 获取按左边缘对齐当前选择的命令。
    /// </summary>
    public IRelayCommand AlignLeftCommand { get; }

    /// <summary>
    /// 获取按水平中心对齐当前选择的命令。
    /// </summary>
    public IRelayCommand AlignCenterCommand { get; }

    /// <summary>
    /// 获取按右边缘对齐当前选择的命令。
    /// </summary>
    public IRelayCommand AlignRightCommand { get; }

    /// <summary>
    /// 获取按上边缘对齐当前选择的命令。
    /// </summary>
    public IRelayCommand AlignTopCommand { get; }

    /// <summary>
    /// 获取按垂直中心对齐当前选择的命令。
    /// </summary>
    public IRelayCommand AlignMiddleCommand { get; }

    /// <summary>
    /// 获取按下边缘对齐当前选择的命令。
    /// </summary>
    public IRelayCommand AlignBottomCommand { get; }

    /// <summary>
    /// 获取按水平方向均匀分布当前选择的命令。
    /// </summary>
    public IRelayCommand DistributeHorizontallyCommand { get; }

    /// <summary>
    /// 获取按垂直方向均匀分布当前选择的命令。
    /// </summary>
    public IRelayCommand DistributeVerticallyCommand { get; }

    /// <summary>
    /// 获取取消当前连线预览的命令。
    /// </summary>
    public IRelayCommand CancelPendingConnectionCommand { get; }

    /// <summary>
    /// 获取基于给定模板创建节点的命令。
    /// </summary>
    public IRelayCommand<NodeTemplateViewModel> AddNodeCommand { get; }

    IEnumerable<NodeTemplateViewModel> IGraphContextMenuHost.NodeTemplates => NodeTemplates;

    IEnumerable<NodeViewModel> IGraphContextMenuHost.Nodes => Nodes;

    IEnumerable<NodeViewModel> IGraphContextMenuHost.SelectedNodes => SelectedNodes;

    GraphEditorCommandPermissions IGraphContextMenuHost.CommandPermissions => CommandPermissions;

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
    private string statusMessage = string.Empty;

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

    /// <summary>
    /// 当待完成连线状态发生变化时触发。
    /// </summary>
    public event EventHandler<GraphEditorPendingConnectionChangedEventArgs>? PendingConnectionChanged;

    /// <summary>
    /// 当前是否存在等待完成的连线预览。
    /// </summary>
    public bool HasPendingConnection => PendingSourceNode is not null && PendingSourcePort is not null;

    /// <summary>
    /// 指示宿主权限当前是否允许保存工作区快照。
    /// </summary>
    public bool CanSaveWorkspace => CommandPermissions.Workspace.AllowSave;

    /// <summary>
    /// 指示宿主权限当前是否允许加载工作区快照。
    /// </summary>
    public bool CanLoadWorkspace => CommandPermissions.Workspace.AllowLoad;

    /// <summary>
    /// 指示撤销命令当前是否可用。
    /// </summary>
    public bool CanUndo => BehaviorOptions.History.EnableUndoRedo && CommandPermissions.History.AllowUndo && _historyService.CanUndo;

    /// <summary>
    /// 指示重做命令当前是否可用。
    /// </summary>
    public bool CanRedo => BehaviorOptions.History.EnableUndoRedo && CommandPermissions.History.AllowRedo && _historyService.CanRedo;

    /// <summary>
    /// 指示当前是否至少选中了一个节点。
    /// </summary>
    public bool HasSelection => SelectedNodes.Count > 0;

    /// <summary>
    /// 指示当前是否同时选中了多个节点。
    /// </summary>
    public bool HasMultipleSelection => SelectedNodes.Count > 1;

    /// <summary>
    /// 指示宿主权限当前是否允许创建节点。
    /// </summary>
    public bool CanCreateNodes => CommandPermissions.Nodes.AllowCreate;

    /// <summary>
    /// 指示当前选择是否满足删除条件且宿主允许删除。
    /// </summary>
    public bool CanDeleteSelection => HasSelection && CommandPermissions.Nodes.AllowDelete;

    /// <summary>
    /// 指示当前选择是否满足复制条件且宿主允许复制。
    /// </summary>
    public bool CanCopySelection => HasSelection && CommandPermissions.Clipboard.AllowCopy;

    /// <summary>
    /// 指示当前是否允许将片段内容插入图中。
    /// </summary>
    public bool CanInsertFragmentContent => CommandPermissions.Nodes.AllowCreate;

    /// <summary>
    /// 指示当前是否允许执行粘贴。
    /// </summary>
    public bool CanPaste => CommandPermissions.Clipboard.AllowPaste && CanInsertFragmentContent && (_selectionClipboard.HasContent || _textClipboardBridge is not null);

    /// <summary>
    /// 指示当前选择是否可导出为默认片段文件。
    /// </summary>
    public bool CanExportSelectionFragment => HasSelection && CommandPermissions.Fragments.AllowExport;

    /// <summary>
    /// 指示默认片段文件当前是否存在且允许被导入。
    /// </summary>
    public bool CanImportFragment => CommandPermissions.Fragments.AllowImport && CanInsertFragmentContent && _fragmentWorkspaceService.Exists();

    /// <summary>
    /// 指示默认片段文件当前是否存在且允许被清理。
    /// </summary>
    public bool CanClearFragment => CommandPermissions.Fragments.AllowClearWorkspaceFragment && _fragmentWorkspaceService.Exists();

    /// <summary>
    /// 指示片段模板库当前是否包含任何模板。
    /// </summary>
    public bool HasFragmentTemplates => FragmentTemplates.Count > 0;

    /// <summary>
    /// 指示当前选择是否可导出为片段模板。
    /// </summary>
    public bool CanExportSelectionAsTemplate => CanExportSelectionFragment && CommandPermissions.Fragments.AllowTemplateManagement && BehaviorOptions.Fragments.EnableFragmentLibrary;

    /// <summary>
    /// 指示当前选中的片段模板是否可导入。
    /// </summary>
    public bool CanImportSelectedTemplate => SelectedFragmentTemplate is not null && CommandPermissions.Fragments.AllowImport && CommandPermissions.Fragments.AllowTemplateManagement && CanInsertFragmentContent && BehaviorOptions.Fragments.EnableFragmentLibrary;

    /// <summary>
    /// 指示当前选中的片段模板是否可删除。
    /// </summary>
    public bool CanDeleteSelectedTemplate => SelectedFragmentTemplate is not null && CommandPermissions.Fragments.AllowTemplateManagement && BehaviorOptions.Fragments.EnableFragmentLibrary;

    /// <summary>
    /// 指示当前选择是否满足对齐操作条件。
    /// </summary>
    public bool CanAlignSelection => SelectedNodes.Count >= 2 && CommandPermissions.Layout.AllowAlign;

    /// <summary>
    /// 指示当前选择是否满足分布操作条件。
    /// </summary>
    public bool CanDistributeSelection => SelectedNodes.Count >= 3 && CommandPermissions.Layout.AllowDistribute;

    /// <summary>
    /// 指示当前单选节点是否暴露了可编辑参数。
    /// </summary>
    public bool HasEditableParameters => SelectedNodes.Count == 1 && SelectedNodeParameters.Count > 0;

    /// <summary>
    /// 指示当前多选节点是否存在可批量编辑的共享参数。
    /// </summary>
    public bool HasBatchEditableParameters => SelectedNodes.Count > 1 && SelectedNodeParameters.Count > 0 && CanEditNodeParameters;

    /// <summary>
    /// 指示当前选择投影是否产生了任何参数项。
    /// </summary>
    public bool HasAnyEditableParameters => SelectedNodeParameters.Count > 0;

    /// <summary>
    /// 指示宿主权限当前是否允许编辑节点参数。
    /// </summary>
    public bool CanEditNodeParameters => CommandPermissions.Nodes.AllowEditParameters;

    /// <summary>
    /// 面向宿主和状态栏的图统计文本。
    /// </summary>
    public string StatsCaption => LocalizeFormat(
        "editor.stats.caption",
        "{0} nodes  ·  {1} links  ·  {2:0}% zoom",
        Nodes.Count,
        Connections.Count,
        Zoom * 100);

    /// <summary>
    /// 工作区状态摘要文本。
    /// </summary>
    public string WorkspaceCaption
    {
        get
        {
            var workspaceState = IsDirty
                ? LocalizeText("editor.workspace.state.unsaved", "Unsaved changes")
                : LocalizeText("editor.workspace.state.synced", "Snapshot synced");
            return LocalizeFormat(
                "editor.workspace.caption",
                "{0}  ·  {1}",
                workspaceState,
                WorkspacePath);
        }
    }

    /// <summary>
    /// 片段工作区状态摘要文本。
    /// </summary>
    public string FragmentCaption
    {
        get
        {
            var availability = _fragmentWorkspaceService.Exists()
                ? LocalizeText("editor.fragment.state.available", "Fragment available")
                : LocalizeText("editor.fragment.state.missing", "No fragment file");
            return LocalizeFormat(
                "editor.fragment.caption",
                "{0}  ·  {1}",
                availability,
                _fragmentWorkspaceService.FragmentPath);
        }
    }

    /// <summary>
    /// 片段文件更新时间摘要文本。
    /// </summary>
    public string FragmentStatusCaption
        => !_fragmentWorkspaceService.Exists()
            ? LocalizeText("editor.fragment.status.missing", "No saved fragment file.")
            : LocalizeFormat(
                "editor.fragment.status.updated",
                "Last updated {0:yyyy-MM-dd HH:mm:ss}",
                File.GetLastWriteTime(_fragmentWorkspaceService.FragmentPath));

    /// <summary>
    /// 片段模板库状态摘要文本。
    /// </summary>
    public string FragmentLibraryCaption
    {
        get
        {
            var templateState = HasFragmentTemplates
                ? LocalizeFormat("editor.fragmentLibrary.state.hasTemplates", "{0} templates", FragmentTemplates.Count)
                : LocalizeText("editor.fragmentLibrary.state.noTemplates", "No templates");
            return LocalizeFormat(
                "editor.fragmentLibrary.caption",
                "{0}  ·  {1}",
                templateState,
                FragmentLibraryPath);
        }
    }

    /// <summary>
    /// 当前编辑模式摘要文本。
    /// </summary>
    public string ModeCaption => HasPendingConnection
        ? LocalizeFormat(
            "editor.mode.connecting",
            "Connecting {0} / {1}  ->  click an input port",
            PendingSourceNode!.Title,
            PendingSourcePort!.Label)
        : HasMultipleSelection
            ? LocalizeFormat(
                "editor.mode.selection.multiple",
                "Selection mode  ·  {0} nodes selected",
                SelectedNodes.Count)
            : LocalizeText(
                "editor.mode.selection.default",
                "Selection mode  ·  click a template to add a node");

    /// <summary>
    /// 获取基于当前选择生成的检查器标题文本。
    /// </summary>
    public string InspectorTitle => SelectedNodes.Count switch
    {
        0 => LocalizeText("editor.inspector.title.none", "Select A Node"),
        1 => SelectedNode?.Title ?? LocalizeText("editor.inspector.title.none", "Select A Node"),
        _ => LocalizeFormat("editor.inspector.title.multiple", "{0} Nodes Selected", SelectedNodes.Count),
    };

    /// <summary>
    /// 获取基于当前选择生成的检查器分类文本。
    /// </summary>
    public string InspectorCategory => SelectedNodes.Count switch
    {
        0 => LocalizeText("editor.inspector.category.none", "Editor"),
        1 => SelectedNode?.Category ?? LocalizeText("editor.inspector.category.none", "Editor"),
        _ => LocalizeText("editor.inspector.category.multiple", "Multi Selection"),
    };

    /// <summary>
    /// 获取基于当前选择生成的检查器描述文本。
    /// </summary>
    public string InspectorDescription => SelectedNodes.Count switch
    {
        0 => LocalizeText(
            "editor.inspector.description.none",
            "Build the graph from the left library, connect outputs to inputs, and save snapshots from the toolbar."),
        1 => SelectedNode?.Description ?? LocalizeText(
            "editor.inspector.description.none",
            "Build the graph from the left library, connect outputs to inputs, and save snapshots from the toolbar."),
        _ => HasBatchEditableParameters
            ? LocalizeFormat(
                "editor.inspector.description.multiple.batch",
                "Editing shared parameters across {0} nodes of the same definition.",
                SelectedNodes.Count)
            : LocalizeText(
                "editor.inspector.description.multiple.default",
                "Delete removes the full selection. Copy and paste preserve internal links between the selected nodes."),
    };

    /// <summary>
    /// 获取当前主选节点的输入端口摘要文本。
    /// </summary>
    public string InspectorInputs => SelectedNode is null
        ? LocalizeText("editor.inspector.inputs.none", "Select a node to inspect its input ports.")
        : _inspectorProjection.FormatPorts(SelectedNode.Inputs);

    /// <summary>
    /// 获取当前主选节点的输出端口摘要文本。
    /// </summary>
    public string InspectorOutputs => SelectedNode is null
        ? LocalizeText("editor.inspector.outputs.none", "Select a node to inspect its output ports.")
        : _inspectorProjection.FormatPorts(SelectedNode.Outputs);

    /// <summary>
    /// 获取当前主选节点的连线统计摘要文本。
    /// </summary>
    public string InspectorConnections => SelectedNode is null
        ? LocalizeText("editor.inspector.connections.none", "Select a node to inspect its connection summary.")
        : LocalizeFormat(
            "editor.inspector.connections.summary",
            "{0} incoming  ·  {1} outgoing",
            GetIncomingConnections(SelectedNode).Count,
            GetOutgoingConnections(SelectedNode).Count);

    /// <summary>
    /// 获取当前主选节点的上游依赖摘要文本。
    /// </summary>
    public string InspectorUpstream => SelectedNode is null
        ? LocalizeText("editor.inspector.upstream.none", "Select a node to see upstream dependencies.")
        : _inspectorProjection.FormatRelatedNodes(
            GetIncomingConnections(SelectedNode),
            useSource: true,
            FindNode);

    /// <summary>
    /// 获取当前主选节点的下游消费者摘要文本。
    /// </summary>
    public string InspectorDownstream => SelectedNode is null
        ? LocalizeText("editor.inspector.downstream.none", "Select a node to see downstream consumers.")
        : _inspectorProjection.FormatRelatedNodes(
            GetOutgoingConnections(SelectedNode),
            useSource: false,
            FindNode);

    /// <summary>
    /// 获取面向状态栏和检查器的当前选择摘要文本。
    /// </summary>
    public string SelectionCaption => _inspectorProjection.FormatSelectionCaption(
        SelectedNode,
        HasMultipleSelection,
        SelectedNodes.Count);

    /// <summary>
    /// 为给定上下文构建右键菜单描述。
    /// </summary>
    /// <param name="context">当前菜单上下文。</param>
    /// <returns>供视图层渲染的菜单项集合。</returns>
    public IReadOnlyList<MenuItemDescriptor> BuildContextMenu(ContextMenuContext context)
    {
        var stockItems = _contextMenuBuilder.Build(context);
        if (ContextMenuAugmentor is null)
        {
            return stockItems;
        }

        try
        {
            return ContextMenuAugmentor.Augment(
                new GraphContextMenuAugmentationContext(
                    Session,
                    context,
                    stockItems,
                    CommandPermissions,
                    this));
        }
        catch (Exception exception)
        {
            SetStatus(
                "editor.status.menu.augmentorFailed",
                "Context menu augmentor failed: {0}. Using stock menu.",
                exception.GetType().Name);
            PublishRecoverableFailure(
                "contextmenu.augment.failed",
                "contextmenu.augment",
                StatusMessage ?? exception.Message,
                exception);
            return stockItems;
        }
    }

    /// <summary>
    /// 更新当前视口尺寸。
    /// </summary>
    /// <param name="width">视口宽度。</param>
    /// <param name="height">视口高度。</param>
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

    /// <summary>
    /// 设置宿主上下文信息。
    /// </summary>
    /// <param name="hostContext">宿主上下文；为 <see langword="null"/> 时清空当前宿主上下文。</param>
    public void SetHostContext(IGraphHostContext? hostContext)
    {
        SetProperty(ref _hostContext, hostContext, nameof(HostContext));
    }

    /// <summary>
    /// 设置图编辑器内置文案本地化提供器。
    /// </summary>
    /// <param name="provider">本地化提供器；为 <see langword="null"/> 时回退到默认文案。</param>
    public void SetLocalizationProvider(IGraphLocalizationProvider? provider)
    {
        if (!SetProperty(ref _localizationProvider, provider, nameof(LocalizationProvider)))
        {
            return;
        }

        RaiseComputedPropertyChanges();
        OnPropertyChanged(nameof(FragmentLibraryCaption));
    }

    /// <summary>
    /// 设置节点展示状态提供器。
    /// </summary>
    /// <param name="provider">新的展示状态提供器。</param>
    /// <param name="refreshImmediately">是否立即刷新当前全部节点展示状态。</param>
    public void SetNodePresentationProvider(INodePresentationProvider? provider, bool refreshImmediately = true)
    {
        if (!SetProperty(ref _nodePresentationProvider, provider, nameof(NodePresentationProvider)))
        {
            return;
        }

        if (refreshImmediately)
        {
            RefreshNodePresentations();
        }
    }

    /// <summary>
    /// 刷新单个节点的展示状态。
    /// </summary>
    /// <param name="nodeId">目标节点标识。</param>
    /// <returns>找到节点并完成刷新时返回 <see langword="true"/>。</returns>
    public bool RefreshNodePresentation(string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var node = FindNode(nodeId);
        if (node is null)
        {
            return false;
        }

        ApplyNodePresentation(node);
        return true;
    }

    /// <summary>
    /// 刷新当前图中全部节点的展示状态。
    /// </summary>
    /// <returns>刷新节点数量。</returns>
    public int RefreshNodePresentations()
    {
        foreach (var node in Nodes)
        {
            ApplyNodePresentation(node);
        }

        return Nodes.Count;
    }

    /// <summary>
    /// 重新扫描片段模板库并刷新当前模板列表。
    /// </summary>
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
    /// 在运行时替换编辑器行为配置。
    /// </summary>
    /// <param name="behaviorOptions">新的行为配置。</param>
    /// <param name="status">可选状态文本。</param>
    public void UpdateBehaviorOptions(GraphEditorBehaviorOptions behaviorOptions, string? status = null)
    {
        ArgumentNullException.ThrowIfNull(behaviorOptions);

        BehaviorOptions = behaviorOptions;
        if (!string.IsNullOrWhiteSpace(status))
        {
            StatusMessage = status;
        }
    }

    /// <summary>
    /// 将一组节点重定位到拖拽起点加偏移后的绝对位置。
    /// </summary>
    /// <param name="originPositions">拖拽开始时记录的节点起始位置。</param>
    /// <param name="deltaX">相对起始位置的水平偏移。</param>
    /// <param name="deltaY">相对起始位置的垂直偏移。</param>
    public void ApplyDragOffset(IReadOnlyDictionary<string, GraphPoint> originPositions, double deltaX, double deltaY)
    {
        ArgumentNullException.ThrowIfNull(originPositions);

        if (!CommandPermissions.Nodes.AllowMove)
        {
            return;
        }

        foreach (var entry in originPositions)
        {
            var node = FindNode(entry.Key);
            if (node is null)
            {
                continue;
            }

            node.X = entry.Value.X + deltaX;
            node.Y = entry.Value.Y + deltaY;
        }
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
        if (!BehaviorOptions.History.EnableUndoRedo || !CommandPermissions.History.AllowUndo)
        {
            SetStatus("editor.status.undo.disabledByPermissions", "Undo is disabled by host permissions.");
            return;
        }

        if (!_historyService.TryUndo(out var state) || state is null)
        {
            SetStatus("editor.status.undo.noMoreSteps", "No more undo steps.");
            return;
        }

        RestoreHistoryState(state, StatusText("editor.status.undo.applied", "Undo applied."));
    }

    /// <summary>
    /// 重做到下一个已记录的图编辑状态。
    /// </summary>
    public void Redo()
    {
        if (!BehaviorOptions.History.EnableUndoRedo || !CommandPermissions.History.AllowRedo)
        {
            SetStatus("editor.status.redo.disabledByPermissions", "Redo is disabled by host permissions.");
            return;
        }

        if (!_historyService.TryRedo(out var state) || state is null)
        {
            SetStatus("editor.status.redo.noMoreSteps", "No more redo steps.");
            return;
        }

        RestoreHistoryState(state, StatusText("editor.status.redo.applied", "Redo applied."));
    }

    /// <summary>
    /// 选中指定节点，等价于单选该节点。
    /// </summary>
    /// <param name="node">要选中的节点；为 <see langword="null"/> 时清空选择。</param>
    public void SelectNode(NodeViewModel? node)
    {
        SelectSingleNode(node);
    }

    /// <summary>
    /// 清空当前选择。
    /// </summary>
    /// <param name="updateStatus">是否同步更新状态文本。</param>
    public void ClearSelection(bool updateStatus = false)
        => SetSelection(
            [],
            null,
            updateStatus ? StatusText("editor.status.selection.cleared", "Selection cleared.") : null);

    /// <summary>
    /// 将选择切换为单个节点。
    /// </summary>
    /// <param name="node">目标节点；为 <see langword="null"/> 时清空选择。</param>
    /// <param name="updateStatus">是否更新状态文本。</param>
    public void SelectSingleNode(NodeViewModel? node, bool updateStatus = true)
    {
        if (node is null)
        {
            SetSelection(
                [],
                null,
                updateStatus ? StatusText("editor.status.selection.cleared", "Selection cleared.") : null);
            return;
        }

        SetSelection(
            [node],
            node,
            updateStatus && !HasPendingConnection
                ? StatusText("editor.status.selection.selectedNode", "Selected {0}.", node.Title)
                : null);
    }

    /// <summary>
    /// 将节点追加到当前选择集合。
    /// </summary>
    /// <param name="node">要追加的节点。</param>
    /// <param name="updateStatus">是否更新状态文本。</param>
    public void AddNodeToSelection(NodeViewModel node, bool updateStatus = true)
    {
        if (SelectedNodes.Contains(node))
        {
            return;
        }

        var nextSelection = SelectedNodes.ToList();
        nextSelection.Add(node);
        SetSelection(
            nextSelection,
            node,
            updateStatus
                ? StatusText("editor.status.selection.addedNode", "Added {0} to the selection.", node.Title)
                : null);
    }

    /// <summary>
    /// 切换节点在当前选择集合中的状态。
    /// </summary>
    /// <param name="node">目标节点。</param>
    /// <param name="updateStatus">是否更新状态文本。</param>
    public void ToggleNodeSelection(NodeViewModel node, bool updateStatus = true)
    {
        var nextSelection = SelectedNodes.ToList();
        if (nextSelection.Remove(node))
        {
            SetSelection(
                nextSelection,
                nextSelection.LastOrDefault(),
                updateStatus
                    ? StatusText("editor.status.selection.removedNode", "Removed {0} from the selection.", node.Title)
                    : null);
            return;
        }

        nextSelection.Add(node);
        SetSelection(
            nextSelection,
            node,
            updateStatus
                ? StatusText("editor.status.selection.addedNode", "Added {0} to the selection.", node.Title)
                : null);
    }

    /// <summary>
    /// 直接设置当前选择集合及主选中节点。
    /// </summary>
    /// <param name="nodes">新的选择集合。</param>
    /// <param name="primaryNode">新的主选中节点。</param>
    /// <param name="status">可选状态文本。</param>
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

    /// <summary>
    /// 获取与指定矩形相交的节点集合。
    /// </summary>
    /// <param name="firstCorner">矩形第一个角点。</param>
    /// <param name="secondCorner">矩形第二个角点。</param>
    /// <returns>命中的节点集合。</returns>
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
        if (!CommandPermissions.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                SetStatus("editor.status.node.move.disabledByPermissions", "Node movement is disabled by host permissions.");
            }

            return false;
        }

        var node = FindNode(nodeId);
        if (node is null)
        {
            if (updateStatus)
            {
                SetStatus("editor.status.node.notFoundById", "Node '{0}' was not found.", nodeId);
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
            MarkDirty(StatusText("editor.status.node.position.updatedSingle", "Updated {0} position.", node.Title));
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

        if (!CommandPermissions.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                SetStatus("editor.status.node.move.disabledByPermissions", "Node movement is disabled by host permissions.");
            }

            return 0;
        }

        var requestedPositions = positions
            .GroupBy(snapshot => snapshot.NodeId, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToList();

        if (requestedPositions.Count == 0)
        {
            if (updateStatus)
            {
                SetStatus("editor.status.node.position.noneProvided", "No node positions were provided.");
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
                SetStatus("editor.status.node.position.noMatches", "No matching nodes were found for the provided positions.");
            }

            return 0;
        }

        if (updateStatus)
        {
            MarkDirty(appliedCount == 1
                ? StatusText("editor.status.node.position.updatedOne", "Updated 1 node position.")
                : StatusText("editor.status.node.position.updatedMany", "Updated {0} node positions.", appliedCount));
        }
        else
        {
            UpdateDirtyState();
            PushCurrentHistoryState();
            RaiseComputedPropertyChanges();
        }

        return appliedCount;
    }

    /// <summary>
    /// 将屏幕坐标转换为当前视口下的世界坐标。
    /// </summary>
    /// <param name="screen">待转换的屏幕坐标。</param>
    /// <returns>应用当前缩放和平移后的世界坐标。</returns>
    public GraphPoint ScreenToWorld(GraphPoint screen)
        => ViewportMath.ScreenToWorld(new ViewportState(Zoom, PanX, PanY), screen);

    /// <summary>
    /// 从节点模板创建一个新节点。
    /// </summary>
    /// <param name="template">节点模板。</param>
    /// <param name="preferredWorldPosition">可选的首选世界坐标。</param>
    public void AddNode(NodeTemplateViewModel template, GraphPoint? preferredWorldPosition = null)
    {
        if (!CommandPermissions.Nodes.AllowCreate)
        {
            SetStatus("editor.status.node.create.disabledByPermissions", "Node creation is disabled by host permissions.");
            return;
        }

        var position = preferredWorldPosition ?? GetViewportCenter();
        var offset = 26 * (Nodes.Count % 4);

        var node = template.CreateNode(
            CreateNodeId(template.Key),
            new GraphPoint(
                position.X - (template.Size.Width / 2) + offset,
                position.Y - (template.Size.Height / 2) + offset));

        ApplyNodePresentation(node);
        Nodes.Add(node);
        SelectSingleNode(node);
        MarkDirty(StatusText("editor.status.node.added", "Added {0}.", template.Title));
        NotifyDocumentChanged(GraphEditorDocumentChangeKind.NodesAdded, nodeIds: [node.Id]);
    }

    /// <summary>
    /// 按指定偏移移动节点；如果节点属于当前多选，则移动整个选择集。
    /// </summary>
    /// <param name="node">拖动源节点。</param>
    /// <param name="deltaX">水平偏移。</param>
    /// <param name="deltaY">垂直偏移。</param>
    public void MoveNode(NodeViewModel node, double deltaX, double deltaY)
    {
        if (!CommandPermissions.Nodes.AllowMove)
        {
            return;
        }

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

    /// <summary>
    /// 按屏幕偏移平移当前视口。
    /// </summary>
    public void PanBy(double deltaX, double deltaY)
    {
        PanX += deltaX;
        PanY += deltaY;
        NotifyViewportChanged();
    }

    /// <summary>
    /// 围绕指定屏幕锚点缩放当前视口。
    /// </summary>
    /// <param name="factor">缩放系数。</param>
    /// <param name="screenAnchor">屏幕锚点。</param>
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

    /// <summary>
    /// 重置缩放和平移到默认视口。
    /// </summary>
    public void ResetView(bool updateStatus = true)
    {
        Zoom = DefaultZoom;
        PanX = DefaultPanX;
        PanY = DefaultPanY;

        if (updateStatus)
        {
            SetStatus("editor.status.viewport.reset", "Viewport reset.");
        }

        NotifyViewportChanged();
    }

    /// <summary>
    /// 将当前图内容适配到指定视口范围。
    /// </summary>
    public void FitToViewport(double viewportWidth, double viewportHeight, bool updateStatus = true)
    {
        if (Nodes.Count == 0 || viewportWidth <= 0 || viewportHeight <= 0)
        {
            if (updateStatus)
            {
                SetStatus("editor.status.viewport.fit.nothingToFit", "Nothing to fit yet.");
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
            SetStatus("editor.status.viewport.fit.applied", "Viewport fit to scene.");
        }

        NotifyViewportChanged();
    }

    /// <summary>
    /// 激活指定端口，按方向决定是开始连线还是完成连线。
    /// </summary>
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
            SetStatus("editor.status.connection.selectOutputPortFirst", "Select an output port first.");
            return;
        }

        ConnectPorts(PendingSourceNode!.Id, PendingSourcePort!.Id, node.Id, port.Id);
    }

    /// <summary>
    /// 以指定输出端口作为连线起点。
    /// </summary>
    public void StartConnection(string sourceNodeId, string sourcePortId)
    {
        if (!CommandPermissions.Connections.AllowCreate)
        {
            SetStatus("editor.status.connection.create.disabledByPermissions", "Connection creation is disabled by host permissions.");
            return;
        }

        var sourceNode = FindNode(sourceNodeId);
        var sourcePort = sourceNode?.GetPort(sourcePortId);
        if (sourceNode is null || sourcePort is null)
        {
            return;
        }

        if (sourcePort.Direction != PortDirection.Output)
        {
            SetStatus("editor.status.connection.onlyOutputCanStart", "Only output ports can start a connection.");
            return;
        }

        if (HasPendingConnection
            && PendingSourceNode?.Id == sourceNode.Id
            && PendingSourcePort?.Id == sourcePort.Id)
        {
            CancelPendingConnection(StatusText("editor.status.connection.previewCancelled", "Connection preview cancelled."));
            return;
        }

        PendingSourceNode = sourceNode;
        PendingSourcePort = sourcePort;
        SetStatus("editor.status.connection.connectingFrom", "Connecting from {0}.{1}.", sourceNode.Title, sourcePort.Label);
    }

    /// <summary>
    /// 连接源输出端口与目标输入端口。
    /// </summary>
    public void ConnectPorts(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId)
    {
        if (!CommandPermissions.Connections.AllowCreate)
        {
            SetStatus("editor.status.connection.create.disabledByPermissions", "Connection creation is disabled by host permissions.");
            return;
        }

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
            SetStatus("editor.status.connection.directionMismatch", "Connections must go from an output port to an input port.");
            return;
        }

        var compatibility = _compatibilityService.Evaluate(sourcePort.TypeId, targetPort.TypeId);
        if (!compatibility.IsCompatible)
        {
            SetStatus("editor.status.connection.incompatible", "Incompatible connection: {0} -> {1}.", sourcePort.TypeId, targetPort.TypeId);
            return;
        }

        if (Connections.Any(connection =>
                connection.SourceNodeId == sourceNode.Id
                && connection.SourcePortId == sourcePort.Id
                && connection.TargetNodeId == targetNode.Id
                && connection.TargetPortId == targetPort.Id))
        {
            CancelPendingConnection(StatusText("editor.status.connection.alreadyExists", "That connection already exists."));
            return;
        }

        var replaced = Connections
            .Where(connection => connection.TargetNodeId == targetNode.Id && connection.TargetPortId == targetPort.Id)
            .ToList();

        if (replaced.Count > 0 && !CanReplaceIncomingConnection())
        {
            SetStatus("editor.status.connection.replaceIncomingRequiresPermission", "Replacing an incoming connection requires delete or disconnect permission.");
            return;
        }

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
                ? StatusText("editor.status.connection.connectedWithImplicitConversion", "Connected {0} to {1} with implicit conversion.", sourceNode.Title, targetNode.Title)
                : StatusText("editor.status.connection.connected", "Connected {0} to {1}.", sourceNode.Title, targetNode.Title));
        NotifyDocumentChanged(
            GraphEditorDocumentChangeKind.ConnectionsChanged,
            nodeIds: [sourceNode.Id, targetNode.Id],
            connectionIds: [Connections[^1].Id]);
    }

    /// <summary>
    /// 取消当前待完成的连线预览。
    /// </summary>
    public void CancelPendingConnection(string? status = null)
    {
        PendingSourceNode = null;
        PendingSourcePort = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            StatusMessage = status;
        }
    }

    /// <summary>
    /// 删除当前选择，保留旧的单节点删除入口以兼容宿主调用。
    /// </summary>
    public void DeleteSelectedNode()
        => DeleteSelection();

    /// <summary>
    /// 删除当前选择集及其相关连线。
    /// </summary>
    public void DeleteSelection()
    {
        if (!CommandPermissions.Nodes.AllowDelete)
        {
            SetStatus("editor.status.node.delete.disabledByPermissions", "Node deletion is disabled by host permissions.");
            return;
        }

        if (SelectedNodes.Count == 0)
        {
            SetStatus("editor.status.node.delete.selectNodeFirst", "Select a node before deleting.");
            return;
        }

        var removedNodes = SelectedNodes.ToList();
        var removedNodeIds = removedNodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
        var removedConnections = Connections
            .Where(connection =>
                removedNodeIds.Contains(connection.SourceNodeId)
                || removedNodeIds.Contains(connection.TargetNodeId))
            .ToList();

        if (removedConnections.Count > 0 && !CanRemoveConnectionsAsSideEffect())
        {
            SetStatus("editor.status.node.delete.connectedRequiresPermission", "Deleting connected nodes requires delete or disconnect permission for the affected links.");
            return;
        }

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
            ? StatusText("editor.status.node.deletedSingle", "Deleted {0}.", removedNodes[0].Title)
            : StatusText("editor.status.node.deletedMultiple", "Deleted {0} nodes.", removedNodes.Count));
        NotifyDocumentChanged(
            GraphEditorDocumentChangeKind.NodesRemoved,
            nodeIds: removedNodes.Select(node => node.Id).ToList(),
            connectionIds: removedConnections.Select(connection => connection.Id).ToList());
    }

    /// <summary>
    /// 将当前选择按左边缘对齐。
    /// </summary>
    public void AlignSelectionLeft()
        => ApplySelectionLayout(
            NodeSelectionLayoutService.AlignLeft,
            minimumCount: 2,
            StatusText("editor.status.layout.alignLeft", "Aligned selection left."));

    /// <summary>
    /// 将当前选择按水平中心对齐。
    /// </summary>
    public void AlignSelectionCenter()
        => ApplySelectionLayout(
            NodeSelectionLayoutService.AlignCenter,
            minimumCount: 2,
            StatusText("editor.status.layout.alignCenter", "Aligned selection center."));

    /// <summary>
    /// 将当前选择按右边缘对齐。
    /// </summary>
    public void AlignSelectionRight()
        => ApplySelectionLayout(
            NodeSelectionLayoutService.AlignRight,
            minimumCount: 2,
            StatusText("editor.status.layout.alignRight", "Aligned selection right."));

    /// <summary>
    /// 将当前选择按上边缘对齐。
    /// </summary>
    public void AlignSelectionTop()
        => ApplySelectionLayout(
            NodeSelectionLayoutService.AlignTop,
            minimumCount: 2,
            StatusText("editor.status.layout.alignTop", "Aligned selection top."));

    /// <summary>
    /// 将当前选择按垂直中心对齐。
    /// </summary>
    public void AlignSelectionMiddle()
        => ApplySelectionLayout(
            NodeSelectionLayoutService.AlignMiddle,
            minimumCount: 2,
            StatusText("editor.status.layout.alignMiddle", "Aligned selection middle."));

    /// <summary>
    /// 将当前选择按下边缘对齐。
    /// </summary>
    public void AlignSelectionBottom()
        => ApplySelectionLayout(
            NodeSelectionLayoutService.AlignBottom,
            minimumCount: 2,
            StatusText("editor.status.layout.alignBottom", "Aligned selection bottom."));

    /// <summary>
    /// 将当前选择按水平方向均匀分布。
    /// </summary>
    public void DistributeSelectionHorizontally()
        => ApplySelectionLayout(
            NodeSelectionLayoutService.DistributeHorizontally,
            minimumCount: 3,
            StatusText("editor.status.layout.distributeHorizontally", "Distributed selection horizontally."));

    /// <summary>
    /// 将当前选择按垂直方向均匀分布。
    /// </summary>
    public void DistributeSelectionVertically()
        => ApplySelectionLayout(
            NodeSelectionLayoutService.DistributeVertically,
            minimumCount: 3,
            StatusText("editor.status.layout.distributeVertically", "Distributed selection vertically."));

    /// <summary>
    /// 按实例标识删除单个节点。
    /// </summary>
    public void DeleteNodeById(string nodeId)
    {
        if (!CommandPermissions.Nodes.AllowDelete)
        {
            SetStatus("editor.status.node.delete.disabledByPermissions", "Node deletion is disabled by host permissions.");
            return;
        }

        var node = FindNode(nodeId);
        if (node is null)
        {
            return;
        }

        var remainingSelection = SelectedNodes.Where(selected => !ReferenceEquals(selected, node)).ToList();
        var removedConnections = Connections
            .Where(connection => connection.SourceNodeId == node.Id || connection.TargetNodeId == node.Id)
            .ToList();

        if (removedConnections.Count > 0 && !CanRemoveConnectionsAsSideEffect())
        {
            SetStatus("editor.status.node.delete.singleConnectedRequiresPermission", "Deleting a connected node requires delete or disconnect permission for the affected links.");
            return;
        }

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
        MarkDirty(StatusText("editor.status.node.deletedSingle", "Deleted {0}.", node.Title));
        NotifyDocumentChanged(
            GraphEditorDocumentChangeKind.NodesRemoved,
            nodeIds: [node.Id],
            connectionIds: removedConnections.Select(connection => connection.Id).ToList());
    }

    /// <summary>
    /// 复制单个节点并自动偏移生成副本。
    /// </summary>
    public void DuplicateNode(string nodeId)
    {
        if (!CommandPermissions.Nodes.AllowDuplicate)
        {
            SetStatus("editor.status.node.duplicate.disabledByPermissions", "Node duplication is disabled by host permissions.");
            return;
        }

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

        ApplyNodePresentation(duplicate);
        Nodes.Add(duplicate);
        SelectSingleNode(duplicate);
        MarkDirty(StatusText("editor.status.node.duplicated", "Duplicated {0}.", node.Title));
        NotifyDocumentChanged(GraphEditorDocumentChangeKind.NodesAdded, nodeIds: [duplicate.Id]);
    }

    /// <summary>
    /// 断开指定节点的所有入边。
    /// </summary>
    public void DisconnectIncoming(string nodeId)
    {
        if (!CommandPermissions.Connections.AllowDisconnect)
        {
            SetStatus("editor.status.connection.disconnect.disabledByPermissions", "Disconnect is disabled by host permissions.");
            return;
        }

        RemoveConnections(
            connection => connection.TargetNodeId == nodeId,
            StatusText("editor.status.connection.disconnect.incoming", "Disconnected incoming links."));
    }

    /// <summary>
    /// 断开指定节点的所有出边。
    /// </summary>
    public void DisconnectOutgoing(string nodeId)
    {
        if (!CommandPermissions.Connections.AllowDisconnect)
        {
            SetStatus("editor.status.connection.disconnect.disabledByPermissions", "Disconnect is disabled by host permissions.");
            return;
        }

        RemoveConnections(
            connection => connection.SourceNodeId == nodeId,
            StatusText("editor.status.connection.disconnect.outgoing", "Disconnected outgoing links."));
    }

    /// <summary>
    /// 断开指定节点的全部连线。
    /// </summary>
    public void DisconnectAll(string nodeId)
    {
        if (!CommandPermissions.Connections.AllowDisconnect)
        {
            SetStatus("editor.status.connection.disconnect.disabledByPermissions", "Disconnect is disabled by host permissions.");
            return;
        }

        RemoveConnections(
            connection => connection.SourceNodeId == nodeId || connection.TargetNodeId == nodeId,
            StatusText("editor.status.connection.disconnect.all", "Disconnected all links."));
    }

    /// <summary>
    /// 断开指定端口上的全部连线。
    /// </summary>
    public void BreakConnectionsForPort(string nodeId, string portId)
    {
        if (!CommandPermissions.Connections.AllowDisconnect)
        {
            SetStatus("editor.status.connection.disconnect.disabledByPermissions", "Disconnect is disabled by host permissions.");
            return;
        }

        RemoveConnections(
            connection =>
                (connection.SourceNodeId == nodeId && connection.SourcePortId == portId)
                || (connection.TargetNodeId == nodeId && connection.TargetPortId == portId),
            StatusText("editor.status.connection.disconnect.port", "Disconnected port links."));
    }

    /// <summary>
    /// 删除指定连线。
    /// </summary>
    public void DeleteConnection(string connectionId)
    {
        if (!CommandPermissions.Connections.AllowDelete)
        {
            SetStatus("editor.status.connection.delete.disabledByPermissions", "Connection deletion is disabled by host permissions.");
            return;
        }

        var connection = FindConnection(connectionId);
        if (connection is null)
        {
            return;
        }

        Connections.Remove(connection);
        MarkDirty(StatusText("editor.status.connection.deleted", "Deleted connection {0}.", connection.Label));
        NotifyDocumentChanged(GraphEditorDocumentChangeKind.ConnectionsChanged, connectionIds: [connection.Id]);
    }

    /// <summary>
    /// 按实例标识查找连线视图模型。
    /// </summary>
    public ConnectionViewModel? FindConnection(string connectionId)
        => _connectionsById.GetValueOrDefault(connectionId);

    /// <summary>
    /// 将视口中心移动到指定节点。
    /// </summary>
    public void CenterViewOnNode(string nodeId)
    {
        var node = FindNode(nodeId);
        if (node is null || _viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return;
        }

        PanX = (_viewportWidth / 2) - ((node.X + (node.Width / 2)) * Zoom);
        PanY = (_viewportHeight / 2) - ((node.Y + (node.Height / 2)) * Zoom);
        SetStatus("editor.status.viewport.centeredOnNode", "Centered on {0}.", node.Title);
        NotifyViewportChanged();
    }

    /// <summary>
    /// 将视口中心移动到指定世界坐标。
    /// </summary>
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
            SetStatus("editor.status.viewport.centeredFromMiniMap", "Viewport centered from mini map.");
        }

        NotifyViewportChanged();
    }

    /// <summary>
    /// 查询指定输出端口可连接的兼容输入端口。
    /// </summary>
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
        if ((minimumCount >= 3 && !CommandPermissions.Layout.AllowDistribute)
            || (minimumCount < 3 && !CommandPermissions.Layout.AllowAlign))
        {
            SetStatus("editor.status.layout.disabledByPermissions", "Layout tools are disabled by host permissions.");
            return;
        }

        var selectedNodes = SelectedNodes.ToList();
        if (selectedNodes.Count < minimumCount)
        {
            SetStatus(minimumCount switch
            {
                2 => ("editor.status.layout.selectAtLeastTwo", "Select at least two nodes for alignment."),
                3 => ("editor.status.layout.selectAtLeastThree", "Select at least three nodes for distribution."),
                _ => ("editor.status.layout.selectionTooSmall", "Selection is too small for that operation."),
            });
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
        if (!CommandPermissions.Nodes.AllowCreate)
        {
            SetStatus("editor.status.fragment.insert.disabledByPermissions", "Fragment insertion is disabled by host permissions.");
            return false;
        }

        if (fragment.Connections.Count > 0 && !CommandPermissions.Connections.AllowCreate)
        {
            SetStatus("editor.status.fragment.insert.connectionCreateDisabled", "This fragment contains connections, but connection creation is disabled by host permissions.");
            return false;
        }

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

            ApplyNodePresentation(pastedNode);
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
            ? StatusText("editor.status.fragment.action.single", "{0} {1}.", actionPrefix, pastedNodes[0].Title)
            : StatusText("editor.status.fragment.action.multiple", "{0} {1} nodes.", actionPrefix, pastedNodes.Count));
        return true;
    }

    private async Task<GraphSelectionFragment?> GetBestAvailableClipboardFragmentAsync()
    {
        if (_textClipboardBridge is not null)
        {
            var clipboardText = await _textClipboardBridge.ReadTextAsync(CancellationToken.None);
            // 优先读取系统剪贴板 JSON，但仍保留进程内剪贴板作为可靠回退。
            if (_clipboardPayloadSerializer.TryDeserialize(clipboardText, out var systemFragment))
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
        if (!CommandPermissions.Clipboard.AllowCopy)
        {
            SetStatus("editor.status.clipboard.copy.disabledByPermissions", "Copy is disabled by host permissions.");
            return;
        }

        var fragment = CreateSelectionFragment();
        if (fragment is null)
        {
            SetStatus("editor.status.clipboard.copy.selectNodeFirst", "Select at least one node before copying.");
            return;
        }

        _selectionClipboard.Store(fragment);
        var clipboardJson = _clipboardPayloadSerializer.Serialize(fragment);
        if (_textClipboardBridge is not null)
        {
            await _textClipboardBridge.WriteTextAsync(clipboardJson, CancellationToken.None);
        }

        RaiseComputedPropertyChanges();
        SetStatus(
            fragment.Nodes.Count == 1
                ? ("editor.status.clipboard.copy.single", "Copied {0}.", new object?[] { fragment.Nodes[0].Title })
                : ("editor.status.clipboard.copy.multiple", "Copied {0} nodes.", new object?[] { fragment.Nodes.Count }));
    }

    /// <summary>
    /// 将当前选择导出到默认片段文件。
    /// </summary>
    public void ExportSelectionFragment()
    {
        if (!CommandPermissions.Fragments.AllowExport)
        {
            SetStatus("editor.status.fragment.export.disabledByPermissions", "Fragment export is disabled by host permissions.");
            return;
        }

        var fragment = CreateSelectionFragment();
        if (fragment is null)
        {
            SetStatus("editor.status.fragment.export.selectNodeFirst", "Select at least one node before exporting a fragment.");
            return;
        }

        _fragmentWorkspaceService.Save(fragment);
        RaiseComputedPropertyChanges();
        SetStatus("editor.status.fragment.export.savedToPath", "Exported fragment to {0}.", _fragmentWorkspaceService.FragmentPath);
        PublishRuntimeDiagnostic(
            "fragment.export.succeeded",
            "fragment.export",
            StatusMessage ?? _fragmentWorkspaceService.FragmentPath,
            GraphEditorDiagnosticSeverity.Info);
        FragmentExported?.Invoke(
            this,
            new GraphEditorFragmentEventArgs(
                _fragmentWorkspaceService.FragmentPath,
                fragment.Nodes.Count,
                fragment.Connections.Count));
    }

    /// <summary>
    /// 将当前选择导出为片段模板。
    /// </summary>
    public void ExportSelectionAsTemplate()
    {
        if (!CommandPermissions.Fragments.AllowTemplateManagement || !CommandPermissions.Fragments.AllowExport)
        {
            SetStatus("editor.status.fragmentTemplate.export.disabledByPermissions", "Template export is disabled by host permissions.");
            return;
        }

        if (!BehaviorOptions.Fragments.EnableFragmentLibrary)
        {
            SetStatus("editor.status.fragmentTemplate.library.disabled", "Fragment template library is disabled.");
            return;
        }

        var fragment = CreateSelectionFragment();
        if (fragment is null)
        {
            SetStatus("editor.status.fragmentTemplate.export.selectNodeFirst", "Select at least one node before exporting a fragment template.");
            return;
        }

        var templateName = SelectedNode?.Title ?? $"selection-{fragment.Nodes.Count}";
        var path = _fragmentLibraryService.SaveTemplate(fragment, templateName);
        RefreshFragmentTemplates();
        SetStatus("editor.status.fragmentTemplate.export.savedToPath", "Exported fragment template to {0}.", path);
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

        if (!CommandPermissions.Fragments.AllowExport)
        {
            SetStatus("editor.status.fragment.export.disabledByPermissions", "Fragment export is disabled by host permissions.");
            return false;
        }

        var fragment = CreateSelectionFragment();
        if (fragment is null)
        {
            SetStatus("editor.status.fragment.export.selectNodeFirst", "Select at least one node before exporting a fragment.");
            return false;
        }

        _fragmentWorkspaceService.Save(fragment, path);
        SetStatus("editor.status.fragment.export.savedToPath", "Exported fragment to {0}.", path);
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
    /// <summary>
    /// 从系统剪贴板或进程内剪贴板粘贴当前片段。
    /// </summary>
    public async Task PasteSelectionAsync()
    {
        if (!CommandPermissions.Clipboard.AllowPaste)
        {
            SetStatus("editor.status.clipboard.paste.disabledByPermissions", "Paste is disabled by host permissions.");
            return;
        }

        var fragment = await GetBestAvailableClipboardFragmentAsync();
        if (fragment is null || fragment.Nodes.Count == 0)
        {
            SetStatus("editor.status.clipboard.paste.nothingCopied", "Nothing copied yet.");
            return;
        }

        if (!PasteFragment(fragment, "Pasted"))
        {
            return;
        }
    }

    /// <summary>
    /// 从默认片段文件导入片段。
    /// </summary>
    public void ImportFragment()
    {
        if (!CommandPermissions.Fragments.AllowImport)
        {
            SetStatus("editor.status.fragment.import.disabledByPermissions", "Fragment import is disabled by host permissions.");
            return;
        }

        if (!_fragmentWorkspaceService.Exists())
        {
            SetStatus("editor.status.fragment.import.noExportedFile", "No exported fragment file is available yet.");
            PublishRuntimeDiagnostic(
                "fragment.import.missing",
                "fragment.import",
                StatusMessage ?? "No exported fragment file is available yet.",
                GraphEditorDiagnosticSeverity.Warning);
            return;
        }

        var fragment = _fragmentWorkspaceService.Load();
        _selectionClipboard.Store(fragment);
        RaiseComputedPropertyChanges();

        if (!PasteFragment(fragment, "Imported"))
        {
            SetStatus("editor.status.fragment.import.noNodesInFile", "Fragment file did not contain any nodes.");
            PublishRuntimeDiagnostic(
                "fragment.import.empty",
                "fragment.import",
                StatusMessage ?? "Fragment file did not contain any nodes.",
                GraphEditorDiagnosticSeverity.Warning);
        }
        else
        {
            PublishRuntimeDiagnostic(
                "fragment.import.succeeded",
                "fragment.import",
                StatusMessage ?? _fragmentWorkspaceService.FragmentPath,
                GraphEditorDiagnosticSeverity.Info);
            FragmentImported?.Invoke(
                this,
                new GraphEditorFragmentEventArgs(
                    _fragmentWorkspaceService.FragmentPath,
                    fragment.Nodes.Count,
                    fragment.Connections.Count));
        }
    }

    /// <summary>
    /// 清理默认片段文件。
    /// </summary>
    public void ClearFragment()
    {
        if (!CommandPermissions.Fragments.AllowClearWorkspaceFragment)
        {
            SetStatus("editor.status.fragment.clear.disabledByPermissions", "Fragment clearing is disabled by host permissions.");
            return;
        }

        if (!_fragmentWorkspaceService.Exists())
        {
            SetStatus("editor.status.fragment.import.noExportedFile", "No exported fragment file is available yet.");
            return;
        }

        _fragmentWorkspaceService.Delete();
        RaiseComputedPropertyChanges();
        SetStatus("editor.status.fragment.clear.cleared", "Cleared the saved fragment file.");
    }

    /// <summary>
    /// 导入当前选中的片段模板。
    /// </summary>
    public void ImportSelectedTemplate()
    {
        if (!CommandPermissions.Fragments.AllowTemplateManagement || !CommandPermissions.Fragments.AllowImport)
        {
            SetStatus("editor.status.fragmentTemplate.import.disabledByPermissions", "Template import is disabled by host permissions.");
            return;
        }

        if (SelectedFragmentTemplate is null)
        {
            SetStatus("editor.status.fragmentTemplate.selectTemplateFirst", "Select a fragment template first.");
            return;
        }

        ImportFragmentFrom(SelectedFragmentTemplate.Path);
    }

    /// <summary>
    /// 删除当前选中的片段模板。
    /// </summary>
    public void DeleteSelectedTemplate()
    {
        if (!CommandPermissions.Fragments.AllowTemplateManagement)
        {
            SetStatus("editor.status.fragmentTemplate.delete.disabledByPermissions", "Template deletion is disabled by host permissions.");
            return;
        }

        if (SelectedFragmentTemplate is null)
        {
            SetStatus("editor.status.fragmentTemplate.selectTemplateFirst", "Select a fragment template first.");
            return;
        }

        var deletedPath = SelectedFragmentTemplate.Path;
        _fragmentLibraryService.DeleteTemplate(deletedPath);
        RefreshFragmentTemplates();
        SetStatus(
            "editor.status.fragmentTemplate.deleted",
            "Deleted fragment template {0}.",
            Path.GetFileNameWithoutExtension(deletedPath));
    }

    /// <summary>
    /// 从指定片段文件路径导入节点片段。
    /// </summary>
    /// <param name="path">源文件路径。</param>
    /// <returns>导入并粘贴成功时返回 <see langword="true"/>。</returns>
    public bool ImportFragmentFrom(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!CommandPermissions.Fragments.AllowImport)
        {
            SetStatus("editor.status.fragment.import.disabledByPermissions", "Fragment import is disabled by host permissions.");
            return false;
        }

        if (!_fragmentWorkspaceService.Exists(path))
        {
            SetStatus("editor.status.fragment.import.fileNotFound", "Fragment file '{0}' was not found.", path);
            PublishRuntimeDiagnostic(
                "fragment.import.fileMissing",
                "fragment.import",
                StatusMessage ?? path,
                GraphEditorDiagnosticSeverity.Warning);
            return false;
        }

        var fragment = _fragmentWorkspaceService.Load(path);
        _selectionClipboard.Store(fragment);
        RaiseComputedPropertyChanges();
        var imported = PasteFragment(fragment, "Imported");
        if (imported)
        {
            PublishRuntimeDiagnostic(
                "fragment.import.succeeded",
                "fragment.import",
                StatusMessage ?? path,
                GraphEditorDiagnosticSeverity.Info);
            FragmentImported?.Invoke(
                this,
                new GraphEditorFragmentEventArgs(
                    path,
                    fragment.Nodes.Count,
                    fragment.Connections.Count));
        }

        return imported;
    }

    /// <summary>
    /// 将当前图保存到默认工作区文件。
    /// </summary>
    public void SaveWorkspace()
    {
        if (!CommandPermissions.Workspace.AllowSave)
        {
            SetStatus("editor.status.workspace.save.disabledByPermissions", "Saving is disabled by host permissions.");
            return;
        }

        try
        {
            _workspaceService.Save(CreateDocumentSnapshot());
            _lastSavedDocumentSignature = CreateDocumentSignature();
            UpdateDirtyState();
            SetStatus("editor.status.workspace.save.savedToPath", "Saved snapshot to {0}.", WorkspacePath);
            PublishRuntimeDiagnostic(
                "workspace.save.succeeded",
                "workspace.save",
                StatusMessage ?? WorkspacePath,
                GraphEditorDiagnosticSeverity.Info);
            NotifyDocumentChanged(GraphEditorDocumentChangeKind.WorkspaceSaved, statusMessage: StatusMessage);
            RaiseComputedPropertyChanges();
        }
        catch (Exception exception)
        {
            SetStatus("editor.status.workspace.save.failed", "Save failed: {0}", exception.Message);
            PublishRecoverableFailure(
                "workspace.save.failed",
                "workspace.save",
                StatusMessage ?? exception.Message,
                exception);
        }
    }

    /// <summary>
    /// 从默认工作区文件加载图。
    /// </summary>
    /// <returns>加载成功时返回 <see langword="true"/>。</returns>
    public bool LoadWorkspace()
    {
        if (!CommandPermissions.Workspace.AllowLoad)
        {
            SetStatus("editor.status.workspace.load.disabledByPermissions", "Loading is disabled by host permissions.");
            return false;
        }

        try
        {
            if (!_workspaceService.Exists())
            {
                SetStatus("editor.status.workspace.load.noSnapshot", "No saved snapshot yet. Save once to create one.");
                PublishRuntimeDiagnostic(
                    "workspace.load.missing",
                    "workspace.load",
                    StatusMessage ?? "No saved snapshot yet.",
                    GraphEditorDiagnosticSeverity.Warning);
                return false;
            }

            var document = _workspaceService.Load();
            LoadDocument(document, StatusText("editor.status.workspace.load.loadedFromDisk", "Workspace loaded from disk."), markClean: true);
            CancelPendingConnection();
            ClearSelection();
            ResetView(updateStatus: false);
            PublishRuntimeDiagnostic(
                "workspace.load.succeeded",
                "workspace.load",
                StatusMessage ?? WorkspacePath,
                GraphEditorDiagnosticSeverity.Info);
            NotifyDocumentChanged(GraphEditorDocumentChangeKind.WorkspaceLoaded, statusMessage: StatusMessage);
            return true;
        }
        catch (Exception exception)
        {
            SetStatus("editor.status.workspace.load.failed", "Load failed: {0}", exception.Message);
            PublishRecoverableFailure(
                "workspace.load.failed",
                "workspace.load",
                StatusMessage ?? exception.Message,
                exception);
            return false;
        }
    }

    /// <summary>
    /// 生成当前图文档的不可变快照。
    /// </summary>
    public GraphDocument CreateDocumentSnapshot()
        => new(
            Title,
            Description,
            Nodes.Select(node => node.ToModel()).ToList(),
            Connections.Select(connection => connection.ToModel()).ToList());

    /// <summary>
    /// 按实例标识查找节点视图模型。
    /// </summary>
    public NodeViewModel? FindNode(string nodeId)
        => _nodesById.GetValueOrDefault(nodeId);

    private void ApplyNodePresentation(NodeViewModel node)
    {
        if (_nodePresentationProvider is null)
        {
            node.UpdatePresentation(NodePresentationState.Empty);
            return;
        }

        var state = _nodePresentationProvider.GetNodePresentation(
            new NodePresentationContext(
                Session,
                node.Id,
                node.DefinitionId,
                node.Title,
                node.Category,
                node.Subtitle,
                node.Description,
                node.AccentHex,
                node.IsSelected,
                node.InputCount,
                node.OutputCount,
                node.ParameterValues,
                node));
        ArgumentNullException.ThrowIfNull(state);
        node.UpdatePresentation(state);
    }

    private void LoadDocument(GraphDocument document, string status, bool markClean)
    {
        _suspendDirtyTracking = true;
        _suspendHistoryTracking = true;

        foreach (var node in Nodes.ToList())
        {
            node.PropertyChanged -= HandleNodePropertyChanged;
        }

        _nodesById.Clear();
        _connectionsById.Clear();
        Nodes.Clear();
        Connections.Clear();

        Title = document.Title;
        Description = document.Description;

        foreach (var node in document.Nodes)
        {
            var viewModel = new NodeViewModel(node);
            ApplyNodePresentation(viewModel);
            Nodes.Add(viewModel);
            _nodesById[viewModel.Id] = viewModel;
        }

        foreach (var connection in document.Connections)
        {
            var viewModel = new ConnectionViewModel(
                connection.Id,
                connection.SourceNodeId,
                connection.SourcePortId,
                connection.TargetNodeId,
                connection.TargetPortId,
                connection.Label,
                connection.AccentHex,
                connection.ConversionId);
            Connections.Add(viewModel);
            _connectionsById[viewModel.Id] = viewModel;
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

    private string LocalizeText(string key, string fallback)
    {
        if (_localizationProvider is null)
        {
            return fallback;
        }

        var localized = _localizationProvider.GetString(key, fallback);
        return string.IsNullOrWhiteSpace(localized) ? fallback : localized;
    }

    private string LocalizeFormat(string key, string fallback, params object?[] arguments)
        => string.Format(CultureInfo.InvariantCulture, LocalizeText(key, fallback), arguments);

    private string StatusText(string key, string fallback)
        => LocalizeText(key, fallback);

    private string StatusText(string key, string fallback, params object?[] arguments)
        => LocalizeFormat(key, fallback, arguments);

    private void SetStatus(string key, string fallback)
        => StatusMessage = StatusText(key, fallback);

    private void SetStatus(string key, string fallback, params object?[] arguments)
        => StatusMessage = StatusText(key, fallback, arguments);

    private void SetStatus((string Key, string Fallback) status)
        => SetStatus(status.Key, status.Fallback);

    private void SetStatus((string Key, string Fallback, object?[] Arguments) status)
        => SetStatus(status.Key, status.Fallback, status.Arguments);

    private void PublishRecoverableFailure(string code, string operation, string message, Exception? exception = null)
    {
        if (Session is GraphEditorSession runtimeSession)
        {
            runtimeSession.PublishRecoverableFailure(
                new GraphEditorRecoverableFailureEventArgs(code, operation, message, exception));
        }
    }

    private void PublishRuntimeDiagnostic(
        string code,
        string operation,
        string message,
        GraphEditorDiagnosticSeverity severity,
        Exception? exception = null)
    {
        if (Session is GraphEditorSession runtimeSession)
        {
            runtimeSession.PublishDiagnostic(new GraphEditorDiagnostic(code, operation, message, severity, exception));
        }
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

    private bool CanReplaceIncomingConnection()
        => CommandPermissions.Connections.AllowDelete || CommandPermissions.Connections.AllowDisconnect;

    private bool CanRemoveConnectionsAsSideEffect()
        => CommandPermissions.Connections.AllowDelete || CommandPermissions.Connections.AllowDisconnect;

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

    partial void OnSelectedFragmentTemplateChanged(FragmentTemplateViewModel? value) => RaiseComputedPropertyChanges();

    partial void OnSelectedNodeChanged(NodeViewModel? value) => RaiseComputedPropertyChanges();

    partial void OnPendingSourceNodeChanged(NodeViewModel? value)
    {
        RaiseComputedPropertyChanges();
        NotifyPendingConnectionChanged();
    }

    partial void OnPendingSourcePortChanged(PortViewModel? value)
    {
        RaiseComputedPropertyChanges();
        NotifyPendingConnectionChanged();
    }

    partial void OnIsDirtyChanged(bool value) => RaiseComputedPropertyChanges();

    private void HandleNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems is not null)
        {
            foreach (NodeViewModel node in args.OldItems)
            {
                node.PropertyChanged -= HandleNodePropertyChanged;
                _nodesById.Remove(node.Id);
            }
        }

        if (args.NewItems is not null)
        {
            foreach (NodeViewModel node in args.NewItems)
            {
                node.PropertyChanged += HandleNodePropertyChanged;
                _nodesById[node.Id] = node;
            }
        }

        CoerceSelectionToExistingNodes();
        FitViewCommand.NotifyCanExecuteChanged();
        RaiseComputedPropertyChanges();
    }

    private void HandleConnectionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems is not null)
        {
            foreach (ConnectionViewModel connection in args.OldItems)
            {
                _connectionsById.Remove(connection.Id);
            }
        }

        if (args.NewItems is not null)
        {
            foreach (ConnectionViewModel connection in args.NewItems)
            {
                _connectionsById[connection.Id] = connection;
            }
        }

        RaiseComputedPropertyChanges();
    }

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
        _commandStateNotifier.NotifyCanExecuteChanged(_computedStateCommands);

        _commandStateNotifier.NotifyPropertyChanged(OnPropertyChanged, ComputedPropertyNames);
    }

    private void NotifyPendingConnectionChanged()
        => PendingConnectionChanged?.Invoke(
            this,
            new GraphEditorPendingConnectionChangedEventArgs(
                GraphEditorPendingConnectionSnapshot.Create(
                    HasPendingConnection,
                    PendingSourceNode?.Id,
                    PendingSourcePort?.Id)));

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
            SetStatus("editor.status.connection.remove.noMatches", "No matching connections to remove.");
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

        var projectedParameters = _inspectorProjection.BuildSelectedNodeParameters(
            SelectedNodes,
            _nodeCatalog,
            BehaviorOptions.Selection.EnableBatchParameterEditing,
            CanEditNodeParameters,
            ApplyParameterValue);

        foreach (var parameter in projectedParameters)
        {
            SelectedNodeParameters.Add(parameter);
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

        if (!CanEditNodeParameters)
        {
            SetStatus("editor.status.parameter.edit.disabledByPermissions", "Parameter editing is disabled by host permissions.");
            return;
        }

        foreach (var node in SelectedNodes)
        {
            node.SetParameterValue(parameter.Key, parameter.TypeId, value);
        }

        MarkDirty(SelectedNodes.Count == 1
            ? StatusText("editor.status.parameter.updatedSingle", "Updated {0} / {1}.", SelectedNode!.Title, parameter.DisplayName)
            : StatusText("editor.status.parameter.updatedMultiple", "Updated {0} nodes / {1}.", SelectedNodes.Count, parameter.DisplayName));
    }
}
