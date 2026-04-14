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
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Kernel;
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
/// 本类型是当前迁移窗口内保留的兼容立面，
/// 用于支持现有宿主基于 <c>new GraphEditorViewModel(...)</c> 的直接集成路径。
/// 新的默认 hosted-UI 组合代码应优先考虑 <see cref="AsterGraphEditorFactory.Create(AsterGraphEditorOptions)"/>，
/// 而自定义 UI 宿主应优先考虑 <see cref="AsterGraphEditorFactory.CreateSession(AsterGraphEditorOptions)"/>。
/// 本类型在当前迁移窗口内仍然受支持，但不应再被视为新的首选组合根。
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
    private readonly GraphEditorDocumentProjectionApplier _documentProjectionApplier;
    private readonly GraphEditorSelectionProjection _selectionProjection;
    private readonly GraphEditorViewModelKernelProjectionHost _kernelProjectionHost;
    private readonly GraphEditorViewModelSelectionProjectionApplierHost _selectionProjectionApplierHost;
    private readonly GraphEditorKernelProjectionApplier _kernelProjectionApplier;
    private readonly GraphEditorViewModelHistoryStateHost _historyStateHost;
    private readonly GraphEditorHistoryStateCoordinator _historyStateCoordinator;
    private readonly GraphEditorSelectionCoordinator _selectionCoordinator;
    private readonly GraphEditorViewModelSelectionCoordinatorHost _selectionCoordinatorHost;
    private readonly GraphEditorViewModelSelectionStateSynchronizerHost _selectionStateSynchronizerHost;
    private readonly GraphEditorSelectionStateSynchronizer _selectionStateSynchronizer;
    private readonly GraphEditorSelectionProjectionApplier _selectionProjectionApplier;
    private readonly GraphEditorViewModelDocumentCollectionSynchronizerHost _documentCollectionSynchronizerHost;
    private readonly GraphEditorDocumentCollectionSynchronizer _documentCollectionSynchronizer;
    private readonly GraphEditorDocumentLoadCoordinator _documentLoadCoordinator;
    private readonly GraphEditorViewModelNodePositionDirtyTrackerHost _nodePositionDirtyTrackerHost;
    private readonly GraphEditorNodePositionDirtyTracker _nodePositionDirtyTracker;
    private readonly GraphEditorViewModelRetainedEventPublisherHost _retainedEventPublisherHost;
    private readonly GraphEditorRetainedEventPublisher _retainedEventPublisher;
    private readonly GraphEditorViewModelNodeLayoutCoordinatorHost _nodeLayoutCoordinatorHost;
    private readonly GraphEditorNodeLayoutCoordinator _nodeLayoutCoordinator;
    private readonly GraphEditorViewModelPresentationLocalizationCoordinatorHost _presentationLocalizationCoordinatorHost;
    private readonly GraphEditorPresentationLocalizationCoordinator _presentationLocalizationCoordinator;
    private readonly GraphEditorViewModelStorageProjectionHost _storageProjectionHost;
    private readonly GraphEditorStorageProjectionSupport _storageProjectionSupport;
    private readonly GraphEditorViewModelPersistenceCoordinatorHost _persistenceCoordinatorHost;
    private readonly GraphEditorWorkspaceSaveCoordinator _workspaceSaveCoordinator;
    private readonly GraphEditorKernel _kernel;
    private readonly GraphEditorViewModelKernelAdapter _sessionHost;
    private readonly GraphEditorCommandStateNotifier _commandStateNotifier = new();
    private readonly GraphEditorViewModelCompatibilityCommandHost _compatibilityCommandHost;
    private readonly GraphEditorViewModelFragmentCommandHost _fragmentCommandHost;
    private readonly IRelayCommand[] _computedStateCommands;
    private string _inspectorConnectionsText = string.Empty;
    private string _inspectorUpstreamText = string.Empty;
    private string _inspectorDownstreamText = string.Empty;
    private string _selectionCaptionText = string.Empty;
    private IGraphContextMenuAugmentor? _contextMenuAugmentor;
    private GraphEditorBehaviorOptions _behaviorOptions = GraphEditorBehaviorOptions.Default;
    private IGraphTextClipboardBridge? _textClipboardBridge;
    private IGraphHostContext? _hostContext;
    private INodePresentationProvider? _nodePresentationProvider;
    private IGraphLocalizationProvider? _localizationProvider;
    private readonly GraphEditorCompatibilityCommands _compatibilityCommands;
    private readonly GraphEditorFragmentCommands _fragmentCommands;
    private bool _suspendSelectionTracking;
    private bool _suspendDirtyTracking;
    private bool _suspendHistoryTracking;
    private bool _isApplyingKernelProjection;
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
    /// 该构造函数保留为受支持的兼容入口，供现有宿主继续沿用
    /// <c>new GraphEditorViewModel(...)</c> 的组合方式。
    /// 对于新的默认 hosted-UI 组合代码，请优先使用
    /// <see cref="AsterGraphEditorFactory.Create(AsterGraphEditorOptions)"/>；
    /// 对于新的自定义 UI 宿主，请优先使用
    /// <see cref="AsterGraphEditorFactory.CreateSession(AsterGraphEditorOptions)"/>。
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
        _documentProjectionApplier = new GraphEditorDocumentProjectionApplier();
        _presentationLocalizationCoordinatorHost = new GraphEditorViewModelPresentationLocalizationCoordinatorHost(this);
        _presentationLocalizationCoordinator = new GraphEditorPresentationLocalizationCoordinator(_presentationLocalizationCoordinatorHost);
        _storageProjectionHost = new GraphEditorViewModelStorageProjectionHost(this);
        _storageProjectionSupport = new GraphEditorStorageProjectionSupport(_storageProjectionHost);
        _selectionProjection = new GraphEditorSelectionProjection(
            LocalizeText,
            (key, fallback, arguments) => LocalizeFormat(key, fallback, arguments));
        _kernelProjectionHost = new GraphEditorViewModelKernelProjectionHost(this);
        _kernelProjectionApplier = new GraphEditorKernelProjectionApplier(_kernelProjectionHost);
        _historyStateHost = new GraphEditorViewModelHistoryStateHost(this);
        _historyStateCoordinator = new GraphEditorHistoryStateCoordinator(_historyStateHost, _historyService);
        _selectionCoordinatorHost = new GraphEditorViewModelSelectionCoordinatorHost(this);
        _selectionCoordinator = new GraphEditorSelectionCoordinator(_selectionCoordinatorHost);
        _selectionStateSynchronizerHost = new GraphEditorViewModelSelectionStateSynchronizerHost(this);
        _selectionStateSynchronizer = new GraphEditorSelectionStateSynchronizer(_selectionStateSynchronizerHost);
        _selectionProjectionApplierHost = new GraphEditorViewModelSelectionProjectionApplierHost(this);
        _selectionProjectionApplier = new GraphEditorSelectionProjectionApplier(_selectionProjectionApplierHost, _selectionProjection);
        _documentCollectionSynchronizerHost = new GraphEditorViewModelDocumentCollectionSynchronizerHost(this);
        _documentCollectionSynchronizer = new GraphEditorDocumentCollectionSynchronizer(_documentCollectionSynchronizerHost, _documentProjectionApplier);
        _persistenceCoordinatorHost = new GraphEditorViewModelPersistenceCoordinatorHost(this);
        _documentLoadCoordinator = new GraphEditorDocumentLoadCoordinator(_persistenceCoordinatorHost);
        _nodePositionDirtyTrackerHost = new GraphEditorViewModelNodePositionDirtyTrackerHost(this);
        _nodePositionDirtyTracker = new GraphEditorNodePositionDirtyTracker(_nodePositionDirtyTrackerHost);
        _retainedEventPublisherHost = new GraphEditorViewModelRetainedEventPublisherHost(this);
        _retainedEventPublisher = new GraphEditorRetainedEventPublisher(_retainedEventPublisherHost);
        _nodeLayoutCoordinatorHost = new GraphEditorViewModelNodeLayoutCoordinatorHost(this);
        _nodeLayoutCoordinator = new GraphEditorNodeLayoutCoordinator(_nodeLayoutCoordinatorHost);
        _workspaceSaveCoordinator = new GraphEditorWorkspaceSaveCoordinator(_persistenceCoordinatorHost);
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

        _compatibilityCommandHost = new GraphEditorViewModelCompatibilityCommandHost(this);
        _compatibilityCommands = new GraphEditorCompatibilityCommands(_compatibilityCommandHost);
        _fragmentCommandHost = new GraphEditorViewModelFragmentCommandHost(this);
        _fragmentCommands = new GraphEditorFragmentCommands(_fragmentCommandHost);

        Nodes = new ObservableCollection<NodeViewModel>();
        Connections = new ObservableCollection<ConnectionViewModel>();
        SelectedNodes = new ObservableCollection<NodeViewModel>();
        SelectedNodeParameters = new ObservableCollection<NodeParameterViewModel>();
        NodeTemplates = new ObservableCollection<NodeTemplateViewModel>(
            _nodeCatalog.Definitions.Select(definition => new NodeTemplateViewModel(definition)));
        FragmentTemplates = new ObservableCollection<FragmentTemplateViewModel>();
        _kernel = new GraphEditorKernel(
            document,
            _nodeCatalog,
            _compatibilityService,
            _workspaceService,
            StyleOptions,
            BehaviorOptions);
        _sessionHost = new GraphEditorViewModelKernelAdapter(_kernel, this);
        Session = new GraphEditorSession(_sessionHost, diagnosticsSink, CreateSessionDescriptorSupport());

        Nodes.CollectionChanged += HandleNodesCollectionChanged;
        Connections.CollectionChanged += HandleConnectionsCollectionChanged;
        SelectedNodes.CollectionChanged += HandleSelectedNodesCollectionChanged;

        WorkspacePath = _workspaceService.WorkspacePath;

        RefreshFragmentTemplates();
        _sessionHost.Initialize();
        _historyService.Reset(CaptureHistoryState());
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

    internal IGraphEditorSessionHost SessionHost => _sessionHost;

    internal GraphEditorSessionDescriptorSupport CreateSessionDescriptorSupport()
        => new(
            _nodeCatalog,
            _presentationLocalizationCoordinator.LocalizeText,
            this,
            hasFragmentWorkspaceService: _fragmentWorkspaceService is not null,
            hasFragmentLibraryService: _fragmentLibraryService is not null,
            hasClipboardPayloadSerializer: _clipboardPayloadSerializer is not null,
            hasPluginLoader: true,
            hasContextMenuAugmentor: _contextMenuAugmentor is not null,
            hasNodePresentationProvider: () => _presentationLocalizationCoordinator.HasNodePresentationProvider,
            hasLocalizationProvider: () => _presentationLocalizationCoordinator.HasLocalizationProvider);

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

    internal event EventHandler<GraphEditorRecoverableFailureEventArgs>? RecoverableFailureRaised;

    internal event Action<GraphEditorDiagnostic>? DiagnosticPublished;

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
    public bool CanUndo
        => BehaviorOptions.History.EnableUndoRedo
           && CommandPermissions.History.AllowUndo
           && (_kernel.GetCapabilitySnapshot().CanUndo || _historyService.CanUndo);

    /// <summary>
    /// 指示重做命令当前是否可用。
    /// </summary>
    public bool CanRedo
        => BehaviorOptions.History.EnableUndoRedo
           && CommandPermissions.History.AllowRedo
           && (_kernel.GetCapabilitySnapshot().CanRedo || _historyService.CanRedo);

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
        => _storageProjectionSupport.GetWorkspaceCaption();

    /// <summary>
    /// 片段工作区状态摘要文本。
    /// </summary>
    public string FragmentCaption
        => _storageProjectionSupport.GetFragmentCaption();

    /// <summary>
    /// 片段文件更新时间摘要文本。
    /// </summary>
    public string FragmentStatusCaption
        => _storageProjectionSupport.GetFragmentStatusCaption();

    /// <summary>
    /// 片段模板库状态摘要文本。
    /// </summary>
    public string FragmentLibraryCaption
        => _storageProjectionSupport.GetFragmentLibraryCaption();

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
        : _selectionProjection.FormatPorts(SelectedNode.Inputs);

    /// <summary>
    /// 获取当前主选节点的输出端口摘要文本。
    /// </summary>
    public string InspectorOutputs => SelectedNode is null
        ? LocalizeText("editor.inspector.outputs.none", "Select a node to inspect its output ports.")
        : _selectionProjection.FormatPorts(SelectedNode.Outputs);

    /// <summary>
    /// 获取当前主选节点的连线统计摘要文本。
    /// </summary>
    public string InspectorConnections => SelectedNode is null
        ? LocalizeText("editor.inspector.connections.none", "Select a node to inspect its connection summary.")
        : _inspectorConnectionsText;

    /// <summary>
    /// 获取当前主选节点的上游依赖摘要文本。
    /// </summary>
    public string InspectorUpstream => SelectedNode is null
        ? LocalizeText("editor.inspector.upstream.none", "Select a node to see upstream dependencies.")
        : _inspectorUpstreamText;

    /// <summary>
    /// 获取当前主选节点的下游消费者摘要文本。
    /// </summary>
    public string InspectorDownstream => SelectedNode is null
        ? LocalizeText("editor.inspector.downstream.none", "Select a node to see downstream consumers.")
        : _inspectorDownstreamText;

    /// <summary>
    /// 获取面向状态栏和检查器的当前选择摘要文本。
    /// </summary>
    public string SelectionCaption => _selectionCaptionText;

    /// <summary>
    /// 为给定上下文构建右键菜单描述。
    /// </summary>
    /// <param name="context">当前菜单上下文。</param>
    /// <returns>供视图层渲染的菜单项集合。</returns>
    public IReadOnlyList<MenuItemDescriptor> BuildContextMenu(ContextMenuContext context)
        => _compatibilityCommands.BuildContextMenu(context);

    /// <summary>
    /// 更新当前视口尺寸。
    /// </summary>
    /// <param name="width">视口宽度。</param>
    /// <param name="height">视口高度。</param>
    public void UpdateViewportSize(double width, double height)
        => _kernel.UpdateViewportSize(width, height);

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
        => _presentationLocalizationCoordinator.SetLocalizationProvider(provider);

    /// <summary>
    /// 设置节点展示状态提供器。
    /// </summary>
    /// <param name="provider">新的展示状态提供器。</param>
    /// <param name="refreshImmediately">是否立即刷新当前全部节点展示状态。</param>
    public void SetNodePresentationProvider(INodePresentationProvider? provider, bool refreshImmediately = true)
        => _presentationLocalizationCoordinator.SetNodePresentationProvider(provider, refreshImmediately);

    /// <summary>
    /// 刷新单个节点的展示状态。
    /// </summary>
    /// <param name="nodeId">目标节点标识。</param>
    /// <returns>找到节点并完成刷新时返回 <see langword="true"/>。</returns>
    public bool RefreshNodePresentation(string nodeId)
        => _presentationLocalizationCoordinator.RefreshNodePresentation(nodeId);

    /// <summary>
    /// 刷新当前图中全部节点的展示状态。
    /// </summary>
    /// <returns>刷新节点数量。</returns>
    public int RefreshNodePresentations()
        => _presentationLocalizationCoordinator.RefreshNodePresentations();

    /// <summary>
    /// 重新扫描片段模板库并刷新当前模板列表。
    /// </summary>
    public void RefreshFragmentTemplates()
        => _storageProjectionSupport.RefreshFragmentTemplates();

    /// <summary>
    /// 在运行时替换编辑器行为配置。
    /// </summary>
    /// <param name="behaviorOptions">新的行为配置。</param>
    /// <param name="status">可选状态文本。</param>
    public void UpdateBehaviorOptions(GraphEditorBehaviorOptions behaviorOptions, string? status = null)
    {
        ArgumentNullException.ThrowIfNull(behaviorOptions);

        _kernel.UpdateBehaviorOptions(behaviorOptions);
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
        => _nodeLayoutCoordinator.ApplyDragOffset(originPositions, deltaX, deltaY);

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
        var currentState = CaptureHistoryState();
        if (string.Equals(previousState.Signature, currentState.Signature, StringComparison.Ordinal))
        {
            UpdateDirtyState(currentState.Signature);
            RaiseComputedPropertyChanges();
            return;
        }

        StatusMessage = status;
        UpdateDirtyState(currentState.Signature);
        PushHistoryState(currentState);
        RaiseComputedPropertyChanges();
    }

    /// <summary>
    /// 撤销到上一个已记录的图编辑状态。
    /// </summary>
    public void Undo()
    {
        if (!BehaviorOptions.History.EnableUndoRedo || !CommandPermissions.History.AllowUndo)
        {
            SetStatus("editor.status.history.undo.disabledByPermissions", "Undo is disabled by host permissions.");
            return;
        }

        if (_kernel.GetCapabilitySnapshot().CanUndo)
        {
            _kernel.Undo();
            return;
        }

        if (!_historyService.TryUndo(out var state) || state is null)
        {
            SetStatus("editor.status.history.undo.none", "No more undo steps.");
            return;
        }

        RestoreHistoryState(state, "Undo applied.");
    }

    /// <summary>
    /// 重做到下一个已记录的图编辑状态。
    /// </summary>
    public void Redo()
    {
        if (!BehaviorOptions.History.EnableUndoRedo || !CommandPermissions.History.AllowRedo)
        {
            SetStatus("editor.status.history.redo.disabledByPermissions", "Redo is disabled by host permissions.");
            return;
        }

        if (_kernel.GetCapabilitySnapshot().CanRedo)
        {
            _kernel.Redo();
            return;
        }

        if (!_historyService.TryRedo(out var state) || state is null)
        {
            SetStatus("editor.status.history.redo.none", "No more redo steps.");
            return;
        }

        RestoreHistoryState(state, "Redo applied.");
    }

    /// <summary>
    /// 选中指定节点，等价于单选该节点。
    /// </summary>
    /// <param name="node">要选中的节点；为 <see langword="null"/> 时清空选择。</param>
    public void SelectNode(NodeViewModel? node) => _selectionCoordinator.SelectSingleNode(node);

    /// <summary>
    /// 清空当前选择。
    /// </summary>
    /// <param name="updateStatus">是否同步更新状态文本。</param>
    public void ClearSelection(bool updateStatus = false)
        => _selectionCoordinator.ClearSelection(updateStatus);

    /// <summary>
    /// 将选择切换为单个节点。
    /// </summary>
    /// <param name="node">目标节点；为 <see langword="null"/> 时清空选择。</param>
    /// <param name="updateStatus">是否更新状态文本。</param>
    public void SelectSingleNode(NodeViewModel? node, bool updateStatus = true)
        => _selectionCoordinator.SelectSingleNode(node, updateStatus);

    /// <summary>
    /// 将节点追加到当前选择集合。
    /// </summary>
    /// <param name="node">要追加的节点。</param>
    /// <param name="updateStatus">是否更新状态文本。</param>
    public void AddNodeToSelection(NodeViewModel node, bool updateStatus = true)
        => _selectionCoordinator.AddNodeToSelection(node, updateStatus);

    /// <summary>
    /// 切换节点在当前选择集合中的状态。
    /// </summary>
    /// <param name="node">目标节点。</param>
    /// <param name="updateStatus">是否更新状态文本。</param>
    public void ToggleNodeSelection(NodeViewModel node, bool updateStatus = true)
        => _selectionCoordinator.ToggleNodeSelection(node, updateStatus);

    /// <summary>
    /// 直接设置当前选择集合及主选中节点。
    /// </summary>
    /// <param name="nodes">新的选择集合。</param>
    /// <param name="primaryNode">新的主选中节点。</param>
    /// <param name="status">可选状态文本。</param>
    public void SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode = null, string? status = null)
        => _selectionCoordinator.SetSelection(nodes, primaryNode, status);

    private void SetSelectionCore(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode = null, string? status = null)
        => _selectionCoordinator.SetSelectionCore(nodes, primaryNode, status);

    /// <summary>
    /// 获取与指定矩形相交的节点集合。
    /// </summary>
    /// <param name="firstCorner">矩形第一个角点。</param>
    /// <param name="secondCorner">矩形第二个角点。</param>
    /// <returns>命中的节点集合。</returns>
    public IReadOnlyList<NodeViewModel> GetNodesInRectangle(GraphPoint firstCorner, GraphPoint secondCorner)
        => _nodeLayoutCoordinator.GetNodesInRectangle(firstCorner, secondCorner);

    /// <summary>
    /// 获取当前所有节点位置的不可变快照，供宿主持久化使用。
    /// </summary>
    public IReadOnlyList<NodePositionSnapshot> GetNodePositions()
        => _nodeLayoutCoordinator.GetNodePositions();

    /// <summary>
    /// 按节点实例标识读取当前位置。
    /// </summary>
    public bool TryGetNodePosition(string nodeId, out NodePositionSnapshot? snapshot)
        => _nodeLayoutCoordinator.TryGetNodePosition(nodeId, out snapshot);

    /// <summary>
    /// 按节点实例标识更新单个节点的位置。
    /// </summary>
    public bool TrySetNodePosition(string nodeId, GraphPoint position, bool updateStatus = true)
        => _nodeLayoutCoordinator.TrySetNodePosition(nodeId, position, updateStatus);

    /// <summary>
    /// 批量应用节点位置并返回实际更新的节点数量。
    /// </summary>
    public int SetNodePositions(IEnumerable<NodePositionSnapshot> positions, bool updateStatus = true)
        => _nodeLayoutCoordinator.SetNodePositions(positions, updateStatus);

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

        _kernel.AddNode(template.Definition.Id, preferredWorldPosition);
    }

    /// <summary>
    /// 按指定偏移移动节点；如果节点属于当前多选，则移动整个选择集。
    /// </summary>
    /// <param name="node">拖动源节点。</param>
    /// <param name="deltaX">水平偏移。</param>
    /// <param name="deltaY">垂直偏移。</param>
    public void MoveNode(NodeViewModel node, double deltaX, double deltaY)
        => _nodeLayoutCoordinator.MoveNode(node, deltaX, deltaY);

    /// <summary>
    /// 按屏幕偏移平移当前视口。
    /// </summary>
    public void PanBy(double deltaX, double deltaY)
        => _kernel.PanBy(deltaX, deltaY);

    /// <summary>
    /// 围绕指定屏幕锚点缩放当前视口。
    /// </summary>
    /// <param name="factor">缩放系数。</param>
    /// <param name="screenAnchor">屏幕锚点。</param>
    public void ZoomAt(double factor, GraphPoint screenAnchor)
        => _kernel.ZoomAt(factor, screenAnchor);

    /// <summary>
    /// 重置缩放和平移到默认视口。
    /// </summary>
    public void ResetView(bool updateStatus = true)
        => _kernel.ResetView(updateStatus);

    /// <summary>
    /// 将当前图内容适配到指定视口范围。
    /// </summary>
    public void FitToViewport(double viewportWidth, double viewportHeight, bool updateStatus = true)
    {
        _kernel.UpdateViewportSize(viewportWidth, viewportHeight);
        _kernel.FitToViewport(updateStatus);
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
        => _kernel.StartConnection(sourceNodeId, sourcePortId);

    /// <summary>
    /// 连接源输出端口与目标输入端口。
    /// </summary>
    public void ConnectPorts(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId)
    {
        var pendingConnection = _kernel.GetPendingConnectionSnapshot();
        if (pendingConnection.HasPendingConnection
            && string.Equals(pendingConnection.SourceNodeId, sourceNodeId, StringComparison.Ordinal)
            && string.Equals(pendingConnection.SourcePortId, sourcePortId, StringComparison.Ordinal))
        {
            _kernel.CompleteConnection(targetNodeId, targetPortId);
            return;
        }

        _kernel.StartConnection(sourceNodeId, sourcePortId);
        _kernel.CompleteConnection(targetNodeId, targetPortId);
    }

    /// <summary>
    /// 取消当前待完成的连线预览。
    /// </summary>
    public void CancelPendingConnection(string? status = null)
    {
        _kernel.CancelPendingConnection();

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

        _kernel.DeleteSelection();
    }

    /// <summary>
    /// 将当前选择按左边缘对齐。
    /// </summary>
    public void AlignSelectionLeft()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.AlignLeft,
            minimumCount: 2,
            StatusText("editor.status.layout.alignLeft", "Aligned selection left."));

    /// <summary>
    /// 将当前选择按水平中心对齐。
    /// </summary>
    public void AlignSelectionCenter()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.AlignCenter,
            minimumCount: 2,
            StatusText("editor.status.layout.alignCenter", "Aligned selection center."));

    /// <summary>
    /// 将当前选择按右边缘对齐。
    /// </summary>
    public void AlignSelectionRight()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.AlignRight,
            minimumCount: 2,
            StatusText("editor.status.layout.alignRight", "Aligned selection right."));

    /// <summary>
    /// 将当前选择按上边缘对齐。
    /// </summary>
    public void AlignSelectionTop()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.AlignTop,
            minimumCount: 2,
            StatusText("editor.status.layout.alignTop", "Aligned selection top."));

    /// <summary>
    /// 将当前选择按垂直中心对齐。
    /// </summary>
    public void AlignSelectionMiddle()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.AlignMiddle,
            minimumCount: 2,
            StatusText("editor.status.layout.alignMiddle", "Aligned selection middle."));

    /// <summary>
    /// 将当前选择按下边缘对齐。
    /// </summary>
    public void AlignSelectionBottom()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.AlignBottom,
            minimumCount: 2,
            StatusText("editor.status.layout.alignBottom", "Aligned selection bottom."));

    /// <summary>
    /// 将当前选择按水平方向均匀分布。
    /// </summary>
    public void DistributeSelectionHorizontally()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.DistributeHorizontally,
            minimumCount: 3,
            StatusText("editor.status.layout.distributeHorizontally", "Distributed selection horizontally."));

    /// <summary>
    /// 将当前选择按垂直方向均匀分布。
    /// </summary>
    public void DistributeSelectionVertically()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.DistributeVertically,
            minimumCount: 3,
            StatusText("editor.status.layout.distributeVertically", "Distributed selection vertically."));

    /// <summary>
    /// 按实例标识删除单个节点。
    /// </summary>
    public void DeleteNodeById(string nodeId)
        => _compatibilityCommands.DeleteNodeById(nodeId);

    /// <summary>
    /// 复制单个节点并自动偏移生成副本。
    /// </summary>
    public void DuplicateNode(string nodeId)
        => _compatibilityCommands.DuplicateNode(nodeId);

    /// <summary>
    /// 断开指定节点的所有入边。
    /// </summary>
    public void DisconnectIncoming(string nodeId)
        => _compatibilityCommands.DisconnectIncoming(nodeId);

    /// <summary>
    /// 断开指定节点的所有出边。
    /// </summary>
    public void DisconnectOutgoing(string nodeId)
        => _compatibilityCommands.DisconnectOutgoing(nodeId);

    /// <summary>
    /// 断开指定节点的全部连线。
    /// </summary>
    public void DisconnectAll(string nodeId)
        => _compatibilityCommands.DisconnectAll(nodeId);

    /// <summary>
    /// 断开指定端口上的全部连线。
    /// </summary>
    public void BreakConnectionsForPort(string nodeId, string portId)
        => _compatibilityCommands.BreakConnectionsForPort(nodeId, portId);

    /// <summary>
    /// 删除指定连线。
    /// </summary>
    public void DeleteConnection(string connectionId)
        => _compatibilityCommands.DeleteConnection(connectionId);

    /// <summary>
    /// 按实例标识查找连线视图模型。
    /// </summary>
    public ConnectionViewModel? FindConnection(string connectionId)
        => _documentProjectionApplier.FindConnection(connectionId);

    /// <summary>
    /// 将视口中心移动到指定节点。
    /// </summary>
    public void CenterViewOnNode(string nodeId)
        => _kernel.CenterViewOnNode(nodeId);

    /// <summary>
    /// 将视口中心移动到指定世界坐标。
    /// </summary>
    public void CenterViewAt(GraphPoint worldPoint, bool updateStatus = true)
        => _kernel.CenterViewAt(worldPoint, updateStatus);

    /// <summary>
    /// 查询指定输出端口可连接的兼容输入端口。
    /// </summary>
#pragma warning disable CS0618
    public IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId)
        => _kernel.GetCompatiblePortTargets(sourceNodeId, sourcePortId)
            .Select(target =>
            {
                var node = FindNode(target.NodeId);
                var port = node?.GetPort(target.PortId);
                return node is null || port is null
                    ? null
                    : new CompatiblePortTarget(node, port, target.Compatibility);
            })
            .Where(target => target is not null)
            .Select(target => target!)
            .ToList();
#pragma warning restore CS0618

    private GraphSelectionFragment? CreateSelectionFragment()
        => _fragmentCommands.CreateSelectionFragment();

    private string? PasteFragment(GraphSelectionFragment fragment, string actionPrefix)
        => _fragmentCommands.PasteFragment(fragment, actionPrefix);

    private Task<GraphSelectionFragment?> GetBestAvailableClipboardFragmentAsync()
        => _fragmentCommands.GetBestAvailableClipboardFragmentAsync();

    /// <summary>
    /// 复制当前选择，并尽可能同步到系统剪贴板 JSON 文本。
    /// </summary>
    public Task CopySelectionAsync()
        => _fragmentCommands.CopySelectionAsync();

    /// <summary>
    /// 将当前选择导出到默认片段文件。
    /// </summary>
    public void ExportSelectionFragment()
        => _fragmentCommands.ExportSelectionFragment();

    /// <summary>
    /// 将当前选择导出为片段模板。
    /// </summary>
    public void ExportSelectionAsTemplate()
        => _fragmentCommands.ExportSelectionAsTemplate();

    /// <summary>
    /// 将当前选择导出到指定片段文件路径。
    /// </summary>
    /// <param name="path">目标文件路径。</param>
    /// <returns>导出成功时返回 <see langword="true"/>。</returns>
    public bool ExportSelectionFragmentTo(string path)
        => _fragmentCommands.ExportSelectionFragmentTo(path);

    /// <summary>
    /// 从系统剪贴板或进程内剪贴板恢复选择片段并粘贴到当前视口附近。
    /// </summary>
    /// <summary>
    /// 从系统剪贴板或进程内剪贴板粘贴当前片段。
    /// </summary>
    public Task PasteSelectionAsync()
        => _fragmentCommands.PasteSelectionAsync();

    /// <summary>
    /// 从默认片段文件导入片段。
    /// </summary>
    public void ImportFragment()
        => _fragmentCommands.ImportFragment();

    /// <summary>
    /// 清理默认片段文件。
    /// </summary>
    public void ClearFragment()
        => _fragmentCommands.ClearFragment();

    /// <summary>
    /// 导入当前选中的片段模板。
    /// </summary>
    public void ImportSelectedTemplate()
        => _fragmentCommands.ImportSelectedTemplate();

    /// <summary>
    /// 删除当前选中的片段模板。
    /// </summary>
    public void DeleteSelectedTemplate()
        => _fragmentCommands.DeleteSelectedTemplate();

    /// <summary>
    /// 从指定片段文件路径导入节点片段。
    /// </summary>
    /// <param name="path">源文件路径。</param>
    /// <returns>导入并粘贴成功时返回 <see langword="true"/>。</returns>
    public bool ImportFragmentFrom(string path)
        => _fragmentCommands.ImportFragmentFrom(path);

    /// <summary>
    /// 将当前图保存到默认工作区文件。
    /// </summary>
    public void SaveWorkspace()
        => _workspaceSaveCoordinator.SaveWorkspace();

    /// <summary>
    /// 从默认工作区文件加载图。
    /// </summary>
    /// <returns>加载成功时返回 <see langword="true"/>。</returns>
    public bool LoadWorkspace()
        => _kernel.LoadWorkspace();

    /// <summary>
    /// 生成当前图文档的不可变快照。
    /// </summary>
    public GraphDocument CreateDocumentSnapshot()
        => _kernel.CreateDocumentSnapshot();

    internal void ApplyKernelDocument(GraphDocument document, string status, bool markClean)
        => _kernelProjectionApplier.ApplyDocument(document, status, markClean);

    internal void ApplyKernelSelection(GraphEditorSelectionSnapshot snapshot)
        => ApplyKernelSelection(snapshot.SelectedNodeIds, snapshot.PrimarySelectedNodeId);

    internal void ApplyKernelSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId)
        => _kernelProjectionApplier.ApplySelection(nodeIds, primaryNodeId);

    internal void ApplyKernelViewport(GraphEditorViewportSnapshot snapshot)
        => _kernelProjectionApplier.ApplyViewport(snapshot);

    internal void ApplyKernelPendingConnection(GraphEditorPendingConnectionSnapshot snapshot)
        => _kernelProjectionApplier.ApplyPendingConnection(snapshot);

    internal void ApplyKernelStatus(string statusMessage)
    {
        if (string.IsNullOrWhiteSpace(statusMessage))
        {
            return;
        }

        StatusMessage = statusMessage;
    }

    internal void ApplyKernelDirtyState(bool isDirty)
        => IsDirty = isDirty;

    /// <summary>
    /// 按实例标识查找节点视图模型。
    /// </summary>
    public NodeViewModel? FindNode(string nodeId)
        => _documentProjectionApplier.FindNode(nodeId);

    private void LoadDocument(GraphDocument document, string status, bool markClean, bool resetHistory = true)
        => _documentLoadCoordinator.LoadDocument(document, status, markClean, resetHistory);

    private void MarkDirty(string status)
        => _historyStateCoordinator.MarkDirty(status);

    private GraphPoint GetViewportCenter()
    {
        if (_viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return ScreenToWorld(new GraphPoint(820, 440));
        }

        return ScreenToWorld(new GraphPoint(_viewportWidth / 2, _viewportHeight / 2));
    }

    private string LocalizeText(string key, string fallback)
        => _presentationLocalizationCoordinator.LocalizeText(key, fallback);

    private string LocalizeFormat(string key, string fallback, params object?[] arguments)
        => _presentationLocalizationCoordinator.LocalizeFormat(key, fallback, arguments);

    private string StatusText(string key, string fallback)
        => _presentationLocalizationCoordinator.StatusText(key, fallback);

    private string StatusText(string key, string fallback, params object?[] arguments)
        => _presentationLocalizationCoordinator.StatusText(key, fallback, arguments);

    private bool TrySetNodePresentationProvider(INodePresentationProvider? provider)
        => SetProperty(ref _nodePresentationProvider, provider, nameof(NodePresentationProvider));

    private bool TrySetLocalizationProvider(IGraphLocalizationProvider? provider)
        => SetProperty(ref _localizationProvider, provider, nameof(LocalizationProvider));

    private void SetStatus(string key, string fallback)
        => StatusMessage = StatusText(key, fallback);

    private void SetStatus(string key, string fallback, params object?[] arguments)
        => StatusMessage = StatusText(key, fallback, arguments);

    private void SetStatus((string Key, string Fallback) status)
        => SetStatus(status.Key, status.Fallback);

    private void SetStatus((string Key, string Fallback, object?[] Arguments) status)
        => SetStatus(status.Key, status.Fallback, status.Arguments);

    private void PublishRecoverableFailure(string code, string operation, string message, Exception? exception = null)
        => RecoverableFailureRaised?.Invoke(
            this,
            new GraphEditorRecoverableFailureEventArgs(code, operation, message, exception));

    private void PublishRuntimeDiagnostic(
        string code,
        string operation,
        string message,
        GraphEditorDiagnosticSeverity severity,
        Exception? exception = null)
        => DiagnosticPublished?.Invoke(new GraphEditorDiagnostic(code, operation, message, severity, exception));

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

    private bool CanReplaceIncomingConnection()
        => CommandPermissions.Connections.AllowDelete || CommandPermissions.Connections.AllowDisconnect;

    private bool CanRemoveConnectionsAsSideEffect()
        => CommandPermissions.Connections.AllowDelete || CommandPermissions.Connections.AllowDisconnect;

    private GraphEditorHistoryState CaptureHistoryState()
        => _historyStateCoordinator.CaptureHistoryState();

    private void RestoreHistoryState(GraphEditorHistoryState state, string status)
        => _historyStateCoordinator.RestoreHistoryState(state, status);

    private void PushCurrentHistoryState()
        => _historyStateCoordinator.PushCurrentHistoryState();

    private void PushHistoryState(GraphEditorHistoryState state)
        => _historyStateCoordinator.PushHistoryState(state);

    private GraphDocument CreateViewModelDocumentSnapshot()
        => new(
            Title,
            Description,
            Nodes.Select(node => node.ToModel()).ToList(),
            Connections.Select(connection => connection.ToModel()).ToList());

    private string CreateDocumentSignature()
        => CreateDocumentSignature(CreateViewModelDocumentSnapshot());

    private static string CreateDocumentSignature(GraphDocument document)
        => GraphEditorHistoryStateCoordinator.CreateDocumentSignature(document);

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
        => _historyStateCoordinator.UpdateDirtyState();

    private void UpdateDirtyState(string currentSignature)
        => _historyStateCoordinator.UpdateDirtyState(currentSignature);

    partial void OnZoomChanged(double value) => RaiseComputedPropertyChanges();

    partial void OnSelectedFragmentTemplateChanged(FragmentTemplateViewModel? value) => RaiseComputedPropertyChanges();

    partial void OnSelectedNodeChanged(NodeViewModel? value)
    {
        if (_suspendSelectionTracking)
        {
            return;
        }

        RefreshSelectionProjection();
    }

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
        => _documentCollectionSynchronizer.HandleNodesCollectionChanged(args, HandleNodePropertyChanged);

    private void HandleConnectionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        => _documentCollectionSynchronizer.HandleConnectionsCollectionChanged(args);

    private void HandleSelectedNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        => _selectionStateSynchronizer.HandleSelectedNodesCollectionChanged();

    private void HandleNodePropertyChanged(object? sender, PropertyChangedEventArgs args)
        => _nodePositionDirtyTracker.HandleNodePropertyChanged(sender, args);

    private void RaiseComputedPropertyChanges()
    {
        _commandStateNotifier.NotifyCanExecuteChanged(_computedStateCommands);

        _commandStateNotifier.NotifyPropertyChanged(OnPropertyChanged, ComputedPropertyNames);
    }

    private void RefreshSelectionProjection()
        => _selectionProjectionApplier.RefreshSelectionProjection();

    private void NotifyPendingConnectionChanged()
        => _retainedEventPublisher.PublishPendingConnectionChanged();

    private void NotifyDocumentChanged(
        GraphEditorDocumentChangeKind changeKind,
        IReadOnlyList<string>? nodeIds = null,
        IReadOnlyList<string>? connectionIds = null,
        string? statusMessage = null)
        => _retainedEventPublisher.PublishDocumentChanged(changeKind, nodeIds, connectionIds, statusMessage);

    private void NotifySelectionChanged()
        => _retainedEventPublisher.PublishSelectionChanged();

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
        => _selectionStateSynchronizer.CoerceSelectionToExistingNodes();

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
