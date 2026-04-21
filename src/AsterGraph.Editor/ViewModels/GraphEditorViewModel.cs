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
/// 本类型是当前迁移窗口内保留的 compatibility facade，
/// 用于支持现有宿主基于 <c>new GraphEditorViewModel(...)</c> 的直接集成路径。
/// 新的默认 hosted-UI 组合代码应优先考虑 <see cref="AsterGraphEditorFactory.Create(AsterGraphEditorOptions)"/>，
/// 而自定义 UI 宿主应优先考虑 <see cref="AsterGraphEditorFactory.CreateSession(AsterGraphEditorOptions)"/>。
/// 本类型在当前迁移窗口内仍然受支持，但不应再被视为新的首选组合根。
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Advanced)]
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
        nameof(InspectorChromeTitle),
        nameof(InspectorCategory),
        nameof(InspectorDescription),
        nameof(InspectorConnectionsTitle),
        nameof(InspectorInputsTitle),
        nameof(InspectorOutputsTitle),
        nameof(InspectorInputs),
        nameof(InspectorOutputs),
        nameof(InspectorConnections),
        nameof(InspectorUpstreamTitle),
        nameof(InspectorUpstream),
        nameof(InspectorDownstreamTitle),
        nameof(InspectorDownstream),
        nameof(InspectorParametersTitle),
        nameof(InspectorParametersIntro),
        nameof(InspectorParametersSurfaceHint),
        nameof(InspectorParametersGuidance),
        nameof(InspectorParametersSearchWatermark),
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
    private readonly GraphEditorViewModelParameterEditHost _parameterEditHost;
    private readonly GraphEditorParameterEditCoordinator _parameterEditCoordinator;
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
        var facadeBootstrap = new GraphEditorViewModelFacadeBootstrap(this, _historyService);
        _documentProjectionApplier = facadeBootstrap.DocumentProjectionApplier;
        _presentationLocalizationCoordinatorHost = facadeBootstrap.PresentationLocalizationCoordinatorHost;
        _presentationLocalizationCoordinator = facadeBootstrap.PresentationLocalizationCoordinator;
        _storageProjectionHost = facadeBootstrap.StorageProjectionHost;
        _storageProjectionSupport = facadeBootstrap.StorageProjectionSupport;
        _selectionProjection = facadeBootstrap.SelectionProjection;
        _kernelProjectionHost = facadeBootstrap.KernelProjectionHost;
        _kernelProjectionApplier = facadeBootstrap.KernelProjectionApplier;
        _historyStateHost = facadeBootstrap.HistoryStateHost;
        _historyStateCoordinator = facadeBootstrap.HistoryStateCoordinator;
        _selectionCoordinatorHost = facadeBootstrap.SelectionCoordinatorHost;
        _selectionCoordinator = facadeBootstrap.SelectionCoordinator;
        _selectionStateSynchronizerHost = facadeBootstrap.SelectionStateSynchronizerHost;
        _selectionStateSynchronizer = facadeBootstrap.SelectionStateSynchronizer;
        _selectionProjectionApplierHost = facadeBootstrap.SelectionProjectionApplierHost;
        _selectionProjectionApplier = facadeBootstrap.SelectionProjectionApplier;
        _parameterEditHost = facadeBootstrap.ParameterEditHost;
        _parameterEditCoordinator = facadeBootstrap.ParameterEditCoordinator;
        _documentCollectionSynchronizerHost = facadeBootstrap.DocumentCollectionSynchronizerHost;
        _documentCollectionSynchronizer = facadeBootstrap.DocumentCollectionSynchronizer;
        _persistenceCoordinatorHost = facadeBootstrap.PersistenceCoordinatorHost;
        _documentLoadCoordinator = facadeBootstrap.DocumentLoadCoordinator;
        _nodePositionDirtyTrackerHost = facadeBootstrap.NodePositionDirtyTrackerHost;
        _nodePositionDirtyTracker = facadeBootstrap.NodePositionDirtyTracker;
        _retainedEventPublisherHost = facadeBootstrap.RetainedEventPublisherHost;
        _retainedEventPublisher = facadeBootstrap.RetainedEventPublisher;
        _nodeLayoutCoordinatorHost = facadeBootstrap.NodeLayoutCoordinatorHost;
        _nodeLayoutCoordinator = facadeBootstrap.NodeLayoutCoordinator;
        _workspaceSaveCoordinator = facadeBootstrap.WorkspaceSaveCoordinator;
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
            BehaviorOptions,
            textClipboardBridge: null,
            _clipboardPayloadSerializer);
        _sessionHost = new GraphEditorViewModelKernelAdapter(_kernel, this);
        Session = new GraphEditorSession(_sessionHost, diagnosticsSink, CreateSessionDescriptorSupport());

        Nodes.CollectionChanged += HandleNodesCollectionChanged;
        Connections.CollectionChanged += HandleConnectionsCollectionChanged;
        SelectedNodes.CollectionChanged += HandleSelectedNodesCollectionChanged;

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
    /// 该会话通过 adapter-backed kernel 路径暴露运行时能力，
    /// 不会把 <see cref="GraphEditorViewModel"/> 重新当作 canonical runtime state owner。
    /// </summary>
    public IGraphEditorSession Session { get; }

    internal IGraphEditorSessionHost SessionHost => _sessionHost;

    internal GraphEditorSessionDescriptorSupport CreateSessionDescriptorSupport()
        => new GraphEditorSessionDescriptorSupportBuilder(this).Build();

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

}
