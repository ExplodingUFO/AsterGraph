using System.Linq;
using System.ComponentModel;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// AsterGraph 的 Avalonia 宿主视图，负责样式资源接入和全局快捷键路由。
/// </summary>
/// <remarks>
/// 直接构造 <see cref="GraphEditorView"/> 并通过 <see cref="Editor"/>
/// 绑定编辑器实例，仍然是当前迁移窗口内受支持的兼容路径。
/// 新的默认 Avalonia 宿主组合代码应优先考虑
/// <see cref="AsterGraphAvaloniaViewFactory"/> 与 <see cref="AsterGraphAvaloniaViewOptions"/>；
/// 该保留视图路径不再是新的首选组合方式。
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public partial class GraphEditorView : UserControl
{
    private const string CommandPaletteActionId = "shell.command-palette";
    private const int CommandPaletteRecentActionLimit = 5;
    private const string HeaderCommandSurfaceId = "workbench.header";
    private const string CommandPaletteSurfaceId = "workbench.command-palette";
    private const string ShortcutHelpSurfaceId = "workbench.shortcut-help";
    private const string CompositeWorkflowSurfaceId = "workbench.composite-workflow";

    /// <summary>
    /// 编辑器视图模型依赖属性。
    /// </summary>
    public static readonly StyledProperty<GraphEditorViewModel?> EditorProperty =
        AvaloniaProperty.Register<GraphEditorView, GraphEditorViewModel?>(nameof(Editor));

    /// <summary>
    /// 外壳显示模式依赖属性。
    /// </summary>
    public static readonly StyledProperty<GraphEditorViewChromeMode> ChromeModeProperty =
        AvaloniaProperty.Register<GraphEditorView, GraphEditorViewChromeMode>(
            nameof(ChromeMode),
            GraphEditorViewChromeMode.Default);

    /// <summary>
    /// 顶部头部区是否可见。
    /// </summary>
    public static readonly StyledProperty<bool> IsHeaderChromeVisibleProperty =
        AvaloniaProperty.Register<GraphEditorView, bool>(nameof(IsHeaderChromeVisible), true);

    /// <summary>
    /// 节点库区是否可见。
    /// </summary>
    public static readonly StyledProperty<bool> IsLibraryChromeVisibleProperty =
        AvaloniaProperty.Register<GraphEditorView, bool>(nameof(IsLibraryChromeVisible), true);

    /// <summary>
    /// 检查器区是否可见。
    /// </summary>
    public static readonly StyledProperty<bool> IsInspectorChromeVisibleProperty =
        AvaloniaProperty.Register<GraphEditorView, bool>(nameof(IsInspectorChromeVisible), true);

    /// <summary>
    /// 底部状态区是否可见。
    /// </summary>
    public static readonly StyledProperty<bool> IsStatusChromeVisibleProperty =
        AvaloniaProperty.Register<GraphEditorView, bool>(nameof(IsStatusChromeVisible), true);

    /// <summary>
    /// 是否启用完整外壳的默认内置上下文菜单。
    /// </summary>
    public static readonly StyledProperty<bool> EnableDefaultContextMenuProperty =
        AvaloniaProperty.Register<GraphEditorView, bool>(nameof(EnableDefaultContextMenu), true);

    /// <summary>
    /// 控制完整外壳的默认内置命令快捷键路由。
    /// </summary>
    public static readonly StyledProperty<AsterGraphCommandShortcutPolicy> CommandShortcutPolicyProperty =
        AvaloniaProperty.Register<GraphEditorView, AsterGraphCommandShortcutPolicy>(
            nameof(CommandShortcutPolicy),
            AsterGraphCommandShortcutPolicy.Default);

    /// <summary>
    /// 是否启用完整外壳的默认滚轮缩放/平移手势。
    /// </summary>
    public static readonly StyledProperty<bool> EnableDefaultWheelViewportGesturesProperty =
        AvaloniaProperty.Register<GraphEditorView, bool>(nameof(EnableDefaultWheelViewportGestures), true);

    /// <summary>
    /// 是否启用完整外壳的 Alt+左键拖拽平移。
    /// </summary>
    public static readonly StyledProperty<bool> EnableAltLeftDragPanningProperty =
        AvaloniaProperty.Register<GraphEditorView, bool>(nameof(EnableAltLeftDragPanning), true);

    /// <summary>
    /// Hosted workbench projection mode for stock Avalonia chrome.
    /// </summary>
    public static readonly StyledProperty<AsterGraphWorkbenchPerformanceMode> WorkbenchPerformanceModeProperty =
        AvaloniaProperty.Register<GraphEditorView, AsterGraphWorkbenchPerformanceMode>(
            nameof(WorkbenchPerformanceMode),
            AsterGraphWorkbenchPerformanceMode.Balanced);

    /// <summary>
    /// 可选的 Avalonia 展示器替换配置依赖属性。
    /// </summary>
    public static readonly StyledProperty<AsterGraphPresentationOptions?> PresentationProperty =
        AvaloniaProperty.Register<GraphEditorView, AsterGraphPresentationOptions?>(nameof(Presentation));

    private NodeCanvas? _nodeCanvas;
    private GraphInspectorView? _inspectorSurface;
    private GraphMiniMap? _miniMapSurface;
    private Grid? _shellGrid;
    private Border? _headerChrome;
    private Border? _libraryChrome;
    private Border? _inspectorChrome;
    private Border? _statusChrome;
    private Border? _validationFeedbackChrome;
    private TextBox? _stencilSearchBox;
    private ComboBox? _stencilSourceFilter;
    private TextBlock? _stencilEmptyStateText;
    private StackPanel? _stencilCardList;
    private WrapPanel? _headerToolbar;
    private WrapPanel? _compositeWorkflowToolbar;
    private WrapPanel? _scopeBreadcrumbs;
    private StackPanel? _shortcutHelpList;
    private TextBlock? _validationStatusText;
    private TextBlock? _statusValidationText;
    private TextBlock? _problemsRepairDiscoveryStatusText;
    private StackPanel? _validationFeedbackList;
    private TextBlock? _fragmentCaptionText;
    private TextBlock? _fragmentStatusText;
    private WrapPanel? _fragmentActionToolbar;
    private TextBlock? _fragmentLibraryCaptionText;
    private ComboBox? _fragmentTemplatePicker;
    private WrapPanel? _fragmentTemplateActionToolbar;
    private ComboBox? _exportFormatPicker;
    private ComboBox? _exportScopePicker;
    private ProgressBar? _exportProgressBar;
    private TextBlock? _exportPreviewText;
    private TextBlock? _exportProgressText;
    private TextBlock? _exportStatusText;
    private Button? _exportRunButton;
    private Button? _exportCancelButton;
    private Button? _openCommandPaletteButton;
    private Border? _commandPaletteChrome;
    private TextBox? _commandPaletteSearchBox;
    private StackPanel? _commandPaletteItems;
    private Control? _commandPaletteReturnFocusTarget;
    private double _defaultShellRowSpacing;
    private double _defaultShellColumnSpacing;
    private readonly GraphEditorViewCompositionCoordinator _compositionCoordinator;
    private readonly HashSet<string> _collapsedStencilCategories = [];
    private readonly HashSet<string> _favoriteStencilTemplateKeys = [];
    private readonly List<string> _recentCommandPaletteActionIds = [];
    private readonly List<string> _recentStencilTemplateKeys = [];
    private string _stencilFilter = string.Empty;
    private string _stencilSourceFilterValue = StencilSourceFilterAll;
    private string _commandPaletteFilter = string.Empty;
    private string? _selectedFragmentTemplatePath;
    private CancellationTokenSource? _exportCancellation;
    private bool _exportCancelRequested;
    private AsterGraphHostedActionDescriptor? _commandPaletteAction;
    private AsterGraphHostedActionProjection? _commandSurfaceProjection;
    private IReadOnlyList<GraphEditorNodeTemplateSnapshot> _stencilTemplateSnapshots = [];
    private IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> _commandDescriptorsById =
        new Dictionary<string, GraphEditorCommandDescriptorSnapshot>(StringComparer.Ordinal);
    private IReadOnlyDictionary<string, GraphEditorCommandRegistryEntrySnapshot> _commandRegistryById =
        new Dictionary<string, GraphEditorCommandRegistryEntrySnapshot>(StringComparer.Ordinal);
    private GraphEditorCommandDescriptorSnapshot? _stencilAddNodeDescriptor;
    private const int StencilRecentTemplateLimit = 5;
    private const string StencilSourceFilterAll = "All";
    private const string StencilSourceFilterBuiltIn = "Built-in";
    private const string StencilSourceFilterPlugin = "Plugin";
    private const string ExportFormatSvg = "SVG";
    private const string ExportFormatPng = "PNG";
    private const string ExportFormatJpeg = "JPEG";
    private const string ExportScopeFullScene = "Full scene";
    private const string ExportScopeSelectedNodes = "Selected nodes";

    /// <summary>
    /// 初始化图编辑器宿主视图。
    /// </summary>
    /// <remarks>
    /// 直接构造视图并设置 <see cref="Editor"/> 仍然受支持；
    /// 但新的默认 Avalonia 宿主代码应优先通过
    /// <see cref="AsterGraphAvaloniaViewFactory.Create(AsterGraphAvaloniaViewOptions)"/> 进行组合。
    /// </remarks>
    public GraphEditorView()
    {
        _compositionCoordinator = new GraphEditorViewCompositionCoordinator(new GraphEditorViewCompositionHost(this));
        InitializeComponent();
        AddHandler(KeyDownEvent, HandleKeyDown, RoutingStrategies.Bubble);
    }

    /// <summary>
    /// 当前绑定的编辑器视图模型。
    /// </summary>
    /// <remarks>
    /// 现有宿主可以继续通过对象初始化器为 <see cref="Editor"/> 赋值，
    /// 例如 <c>new GraphEditorView { Editor = editor }</c>。
    /// 新宿主若希望采用规范 hosted-UI 组合路径，应优先使用
    /// <see cref="AsterGraphAvaloniaViewFactory.Create(AsterGraphAvaloniaViewOptions)"/>。
    /// </remarks>
    public GraphEditorViewModel? Editor
    {
        get => GetValue(EditorProperty);
        set => SetValue(EditorProperty, value);
    }

    /// <summary>
    /// 当前宿主视图的外壳显示模式。
    /// </summary>
    /// <remarks>
    /// 该能力只影响 <see cref="GraphEditorView"/> 的壳层布局，
    /// 不会重建或替换当前 <see cref="Editor"/>。
    /// </remarks>
    public GraphEditorViewChromeMode ChromeMode
    {
        get => GetValue(ChromeModeProperty);
        set => SetValue(ChromeModeProperty, value);
    }

    /// <summary>
    /// 顶部头部区是否可见。
    /// </summary>
    public bool IsHeaderChromeVisible
    {
        get => GetValue(IsHeaderChromeVisibleProperty);
        set => SetValue(IsHeaderChromeVisibleProperty, value);
    }

    /// <summary>
    /// 节点库区是否可见。
    /// </summary>
    public bool IsLibraryChromeVisible
    {
        get => GetValue(IsLibraryChromeVisibleProperty);
        set => SetValue(IsLibraryChromeVisibleProperty, value);
    }

    /// <summary>
    /// 检查器区是否可见。
    /// </summary>
    public bool IsInspectorChromeVisible
    {
        get => GetValue(IsInspectorChromeVisibleProperty);
        set => SetValue(IsInspectorChromeVisibleProperty, value);
    }

    /// <summary>
    /// 底部状态区是否可见。
    /// </summary>
    public bool IsStatusChromeVisible
    {
        get => GetValue(IsStatusChromeVisibleProperty);
        set => SetValue(IsStatusChromeVisibleProperty, value);
    }

    /// <summary>
    /// 是否启用完整外壳的默认内置上下文菜单。
    /// </summary>
    public bool EnableDefaultContextMenu
    {
        get => GetValue(EnableDefaultContextMenuProperty);
        set => SetValue(EnableDefaultContextMenuProperty, value);
    }

    /// <summary>
    /// 控制完整外壳的默认内置命令快捷键路由。
    /// </summary>
    public AsterGraphCommandShortcutPolicy CommandShortcutPolicy
    {
        get => GetValue(CommandShortcutPolicyProperty);
        set => SetValue(CommandShortcutPolicyProperty, value);
    }

    /// <summary>
    /// 是否启用完整外壳的默认滚轮缩放/平移手势。
    /// </summary>
    public bool EnableDefaultWheelViewportGestures
    {
        get => GetValue(EnableDefaultWheelViewportGesturesProperty);
        set => SetValue(EnableDefaultWheelViewportGesturesProperty, value);
    }

    /// <summary>
    /// 是否启用完整外壳的 Alt+左键拖拽平移。
    /// </summary>
    public bool EnableAltLeftDragPanning
    {
        get => GetValue(EnableAltLeftDragPanningProperty);
        set => SetValue(EnableAltLeftDragPanningProperty, value);
    }

    /// <summary>
    /// Hosted workbench projection mode for stock Avalonia chrome.
    /// </summary>
    public AsterGraphWorkbenchPerformanceMode WorkbenchPerformanceMode
    {
        get => GetValue(WorkbenchPerformanceModeProperty);
        set => SetValue(WorkbenchPerformanceModeProperty, value);
    }

    /// <summary>
    /// Gets the hosted projection policy derived from <see cref="WorkbenchPerformanceMode" />.
    /// </summary>
    public AsterGraphWorkbenchPerformancePolicy CurrentWorkbenchPerformancePolicy
        => AsterGraphWorkbenchPerformancePolicy.FromMode(WorkbenchPerformanceMode);

    /// <summary>
    /// 当前宿主提供的 Avalonia 展示器替换配置。
    /// </summary>
    public AsterGraphPresentationOptions? Presentation
    {
        get => GetValue(PresentationProperty);
        set => SetValue(PresentationProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EditorProperty)
        {
            var oldEditor = change.GetOldValue<GraphEditorViewModel?>();
            var editor = change.GetNewValue<GraphEditorViewModel?>();
            DetachCommandSurfaceSubscriptions(oldEditor);
            GraphEditorPlatformSeamBinder.Replace(
                oldEditor,
                editor,
                this);
            AttachCommandSurfaceSubscriptions(editor);
            _compositionCoordinator.ApplyStyleOptions(editor);
            _compositionCoordinator.ApplyPresentationOptions(Presentation);
            RefreshCommandSurface();
        }
        else if (change.Property == ChromeModeProperty
            || change.Property == IsHeaderChromeVisibleProperty
            || change.Property == IsLibraryChromeVisibleProperty
            || change.Property == IsInspectorChromeVisibleProperty
            || change.Property == IsStatusChromeVisibleProperty)
        {
            _compositionCoordinator.ApplyChromeMode();
        }
        else if (change.Property == EnableDefaultContextMenuProperty
            || change.Property == CommandShortcutPolicyProperty
            || change.Property == EnableDefaultWheelViewportGesturesProperty
            || change.Property == EnableAltLeftDragPanningProperty)
        {
            _compositionCoordinator.ApplyCanvasBehaviorOptions();
        }
        else if (change.Property == WorkbenchPerformanceModeProperty)
        {
            ApplyWorkbenchProjectionPolicy();
            BuildStencilLibrary(_stencilTemplateSnapshots, _stencilAddNodeDescriptor);
        }
        else if (change.Property == PresentationProperty)
        {
            _compositionCoordinator.ApplyPresentationOptions(change.GetNewValue<AsterGraphPresentationOptions?>());
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        GraphEditorPlatformSeamBinder.Apply(Editor, this);
        AttachCommandSurfaceSubscriptions(Editor);
        RefreshCommandSurface();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        DetachCommandSurfaceSubscriptions(Editor);
        GraphEditorPlatformSeamBinder.Clear(Editor);
        base.OnDetachedFromVisualTree(e);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        AutomationProperties.SetName(this, "Graph editor host");
        _shellGrid = this.FindControl<Grid>("PART_ShellGrid");
        _headerChrome = this.FindControl<Border>("PART_HeaderChrome");
        _libraryChrome = this.FindControl<Border>("PART_LibraryChrome");
        _inspectorChrome = this.FindControl<Border>("PART_InspectorChrome");
        _statusChrome = this.FindControl<Border>("PART_StatusChrome");
        _validationFeedbackChrome = this.FindControl<Border>("PART_ValidationFeedbackChrome");
        _stencilSearchBox = this.FindControl<TextBox>("PART_StencilSearchBox");
        _stencilSourceFilter = this.FindControl<ComboBox>("PART_StencilSourceFilter");
        _stencilEmptyStateText = this.FindControl<TextBlock>("PART_StencilEmptyStateText");
        _stencilCardList = this.FindControl<StackPanel>("PART_StencilCardList");
        _headerToolbar = this.FindControl<WrapPanel>("PART_HeaderToolbar");
        _compositeWorkflowToolbar = this.FindControl<WrapPanel>("PART_CompositeWorkflowToolbar");
        _scopeBreadcrumbs = this.FindControl<WrapPanel>("PART_ScopeBreadcrumbs");
        _shortcutHelpList = this.FindControl<StackPanel>("PART_ShortcutHelpList");
        _validationStatusText = this.FindControl<TextBlock>("PART_ValidationStatusText");
        _statusValidationText = this.FindControl<TextBlock>("PART_StatusValidationText");
        _problemsRepairDiscoveryStatusText =
            this.FindControl<TextBlock>("PART_ProblemsRepairDiscoveryStatusText");
        _validationFeedbackList = this.FindControl<StackPanel>("PART_ValidationFeedbackList");
        _fragmentCaptionText = this.FindControl<TextBlock>("PART_FragmentCaptionText");
        _fragmentStatusText = this.FindControl<TextBlock>("PART_FragmentStatusText");
        _fragmentActionToolbar = this.FindControl<WrapPanel>("PART_FragmentActionToolbar");
        _fragmentLibraryCaptionText = this.FindControl<TextBlock>("PART_FragmentLibraryCaptionText");
        _fragmentTemplatePicker = this.FindControl<ComboBox>("PART_FragmentTemplatePicker");
        _fragmentTemplateActionToolbar = this.FindControl<WrapPanel>("PART_FragmentTemplateActionToolbar");
        _exportFormatPicker = this.FindControl<ComboBox>("PART_ExportFormatPicker");
        _exportScopePicker = this.FindControl<ComboBox>("PART_ExportScopePicker");
        _exportProgressBar = this.FindControl<ProgressBar>("PART_ExportProgressBar");
        _exportPreviewText = this.FindControl<TextBlock>("PART_ExportPreviewText");
        _exportProgressText = this.FindControl<TextBlock>("PART_ExportProgressText");
        _exportStatusText = this.FindControl<TextBlock>("PART_ExportStatusText");
        _exportRunButton = this.FindControl<Button>("PART_ExportRunButton");
        _exportCancelButton = this.FindControl<Button>("PART_ExportCancelButton");
        _openCommandPaletteButton = this.FindControl<Button>("PART_OpenCommandPaletteButton");
        _commandPaletteChrome = this.FindControl<Border>("PART_CommandPaletteChrome");
        _commandPaletteSearchBox = this.FindControl<TextBox>("PART_CommandPaletteSearchBox");
        _commandPaletteItems = this.FindControl<StackPanel>("PART_CommandPaletteItems");
        _nodeCanvas = this.FindControl<NodeCanvas>("PART_NodeCanvas");
        _inspectorSurface = this.FindControl<GraphInspectorView>("PART_InspectorSurface");
        _miniMapSurface = this.FindControl<GraphMiniMap>("PART_MiniMapSurface");
        ApplyWorkbenchProjectionPolicy();
        if (_stencilSearchBox is not null)
        {
            AutomationProperties.SetName(_stencilSearchBox, "Stencil search");
        }

        if (_stencilSourceFilter is not null)
        {
            AutomationProperties.SetName(_stencilSourceFilter, "Stencil source filter");
            _stencilSourceFilter.ItemsSource = new[] { StencilSourceFilterAll, StencilSourceFilterBuiltIn, StencilSourceFilterPlugin };
            _stencilSourceFilter.SelectedItem = StencilSourceFilterAll;
        }

        if (_openCommandPaletteButton is not null)
        {
            AutomationProperties.SetName(_openCommandPaletteButton, "Open command palette");
        }

        if (_commandPaletteSearchBox is not null)
        {
            AutomationProperties.SetName(_commandPaletteSearchBox, "Command palette search");
        }

        InitializeExportPanelControls();

        if (_inspectorSurface is not null)
        {
            AutomationProperties.SetName(_inspectorSurface, "Graph inspector");
        }

        InitializeAuthoringToolControls();
        if (_stencilSearchBox is not null)
        {
            _stencilSearchBox.TextChanged += HandleStencilSearchChanged;
        }

        if (_stencilSourceFilter is not null)
        {
            _stencilSourceFilter.SelectionChanged += HandleStencilSourceFilterChanged;
        }

        if (_openCommandPaletteButton is not null)
        {
            _openCommandPaletteButton.Click += HandleOpenCommandPaletteClick;
        }

        if (_commandPaletteSearchBox is not null)
        {
            _commandPaletteSearchBox.TextChanged += HandleCommandPaletteSearchChanged;
        }

        if (_fragmentTemplatePicker is not null)
        {
            _fragmentTemplatePicker.SelectionChanged += HandleFragmentTemplateSelectionChanged;
        }

        if (_exportFormatPicker is not null)
        {
            _exportFormatPicker.SelectionChanged += HandleExportOptionChanged;
        }

        if (_exportScopePicker is not null)
        {
            _exportScopePicker.SelectionChanged += HandleExportOptionChanged;
        }

        if (_exportRunButton is not null)
        {
            _exportRunButton.Click += HandleExportRunClick;
        }

        if (_exportCancelButton is not null)
        {
            _exportCancelButton.Click += HandleExportCancelClick;
        }

        if (_nodeCanvas is not null)
        {
            _nodeCanvas.AttachPlatformSeams = false;
        }
        _defaultShellRowSpacing = _shellGrid?.RowSpacing ?? 0;
        _defaultShellColumnSpacing = _shellGrid?.ColumnSpacing ?? 0;
        _compositionCoordinator.ApplyChromeMode();
        _compositionCoordinator.ApplyCanvasBehaviorOptions();
        _compositionCoordinator.ApplyPresentationOptions(Presentation);
        RefreshCommandSurface();
    }

    private void HandleKeyDown(object? sender, KeyEventArgs args)
    {
        if (_commandPaletteChrome?.IsVisible == true && args.Key == Key.Escape)
        {
            CloseCommandPalette();
            args.Handled = true;
            return;
        }

        var projection = GetCommandSurfaceProjection();
        var commandPaletteReturnFocusTarget = args.Source as Control
            ?? TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
        if (projection is not null
            && projection.TryGet(CommandPaletteActionId, out var commandPaletteAction)
            && GraphEditorDefaultCommandShortcutRouter.TryHandle(
                [commandPaletteAction],
                args.Source,
                args,
                allowInputControlFocus: true))
        {
            CaptureCommandPaletteReturnFocusTarget(commandPaletteReturnFocusTarget);
            args.Handled = true;
            return;
        }

        if (projection is not null
            && GraphEditorDefaultCommandShortcutRouter.TryHandle(
                projection.Actions,
                args.Source,
                args,
                allowInputControlFocus: false,
                excludedActionIds:
                [
                    CommandPaletteActionId,
                    "connections.cancel",
                ]))
        {
            args.Handled = true;
        }
    }

    private void HandleOpenCommandPaletteClick(object? sender, RoutedEventArgs args)
    {
        CaptureCommandPaletteReturnFocusTarget(args.Source as Control ?? sender as Control);
        _commandPaletteAction?.TryExecute();
        args.Handled = true;
    }

    private void HandleCommandPaletteSearchChanged(object? sender, TextChangedEventArgs args)
    {
        _commandPaletteFilter = _commandPaletteSearchBox?.Text?.Trim() ?? string.Empty;
        BuildCommandPaletteItems(GetCommandSurfaceProjection());
    }

    private void HandleStencilSearchChanged(object? sender, TextChangedEventArgs args)
    {
        _stencilFilter = _stencilSearchBox?.Text?.Trim() ?? string.Empty;
        BuildStencilLibrary(_stencilTemplateSnapshots, _stencilAddNodeDescriptor);
    }

    private void HandleStencilSourceFilterChanged(object? sender, SelectionChangedEventArgs args)
    {
        _stencilSourceFilterValue = _stencilSourceFilter?.SelectedItem?.ToString() ?? StencilSourceFilterAll;
        BuildStencilLibrary(_stencilTemplateSnapshots, _stencilAddNodeDescriptor);
    }

    private void HandleFragmentTemplateSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        _selectedFragmentTemplatePath = (_fragmentTemplatePicker?.SelectedItem as GraphEditorFragmentTemplateSnapshot)?.Path;
        BuildFragmentTemplateActionToolbar();
    }

    private void HandleExportOptionChanged(object? sender, SelectionChangedEventArgs args)
        => BuildExportPanel();

    private void HandleExportRunClick(object? sender, RoutedEventArgs args)
    {
        RunExportFromPanel();
        args.Handled = true;
    }

    private void HandleExportCancelClick(object? sender, RoutedEventArgs args)
    {
        _exportCancelRequested = true;
        _exportCancellation?.Cancel();
        if (_exportStatusText is not null)
        {
            _exportStatusText.Text = "Export cancel requested.";
        }

        if (_exportProgressText is not null)
        {
            _exportProgressText.Text = "Cancel requested";
        }

        BuildExportPanel();
        args.Handled = true;
    }

    private void AttachCommandSurfaceSubscriptions(GraphEditorViewModel? editor)
    {
        if (editor is null)
        {
            return;
        }

        editor.Session.Events.DocumentChanged -= HandleCommandSurfaceChanged;
        editor.Session.Events.SelectionChanged -= HandleCommandSurfaceChanged;
        editor.Session.Events.ViewportChanged -= HandleCommandSurfaceChanged;
        editor.Session.Events.CommandExecuted -= HandleCommandSurfaceChanged;
        editor.Session.Events.PendingConnectionChanged -= HandleCommandSurfaceChanged;

        editor.Session.Events.DocumentChanged += HandleCommandSurfaceChanged;
        editor.Session.Events.SelectionChanged += HandleCommandSurfaceChanged;
        editor.Session.Events.ViewportChanged += HandleCommandSurfaceChanged;
        editor.Session.Events.CommandExecuted += HandleCommandSurfaceChanged;
        editor.Session.Events.PendingConnectionChanged += HandleCommandSurfaceChanged;
    }

    private void DetachCommandSurfaceSubscriptions(GraphEditorViewModel? editor)
    {
        if (editor is null)
        {
            return;
        }

        editor.Session.Events.DocumentChanged -= HandleCommandSurfaceChanged;
        editor.Session.Events.SelectionChanged -= HandleCommandSurfaceChanged;
        editor.Session.Events.ViewportChanged -= HandleCommandSurfaceChanged;
        editor.Session.Events.CommandExecuted -= HandleCommandSurfaceChanged;
        editor.Session.Events.PendingConnectionChanged -= HandleCommandSurfaceChanged;
    }

    private void HandleCommandSurfaceChanged(object? sender, EventArgs args)
        => RefreshCommandSurface();

    private void RefreshCommandSurface()
    {
        RefreshCommandSurfaceState();
        var projection = _commandSurfaceProjection;
        BuildStencilLibrary(_stencilTemplateSnapshots, _stencilAddNodeDescriptor);
        BuildValidationFeedback();
        BuildFragmentLibrary(projection);
        BuildExportPanel();
        RefreshCommandPaletteButton(projection);
        BuildHeaderToolbar(projection);
        BuildCompositeWorkflowToolbar(projection);
        BuildScopeBreadcrumbs();
        BuildShortcutHelp(projection);
        BuildCommandPaletteItems(projection);
        RefreshAuthoringToolSurface(_commandDescriptorsById);
        if (Editor is null)
        {
            CloseCommandPalette();
        }
    }

    private void RefreshCommandSurfaceState()
    {
        if (Editor is null)
        {
            _commandSurfaceProjection = null;
            _stencilTemplateSnapshots = [];
            _commandDescriptorsById = new Dictionary<string, GraphEditorCommandDescriptorSnapshot>(StringComparer.Ordinal);
            _commandRegistryById = new Dictionary<string, GraphEditorCommandRegistryEntrySnapshot>(StringComparer.Ordinal);
            _stencilAddNodeDescriptor = null;
            _authoringToolSurfaceState = null;
            return;
        }

        var commandDescriptors = Editor.Session.Queries.GetCommandDescriptors();
        var commandRegistry = Editor.Session.Queries.GetCommandRegistry();
        _commandRegistryById = commandRegistry
            .GroupBy(entry => entry.CommandId, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToDictionary(entry => entry.CommandId, StringComparer.Ordinal);
        _commandDescriptorsById = commandDescriptors
            .GroupBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        _stencilAddNodeDescriptor = _commandDescriptorsById.TryGetValue("nodes.add", out var addNodeDescriptor)
            ? addNodeDescriptor
            : null;
        _stencilTemplateSnapshots = Editor.Session.Queries.GetNodeTemplateSnapshots();
        var selection = Editor.Session.Queries.GetSelectionSnapshot();
        _authoringToolSurfaceState = CreateAuthoringToolSurfaceState(selection);
        var composites = Editor.Session.Queries.GetCompositeNodeSnapshots()
            .ToDictionary(snapshot => snapshot.NodeId, StringComparer.Ordinal);
        _commandSurfaceProjection = CreateCommandSurfaceProjection(
            commandRegistry,
            _commandDescriptorsById,
            selection,
            composites,
            _authoringToolSurfaceState);
    }

    private AsterGraphHostedActionProjection? GetCommandSurfaceProjection()
    {
        if (Editor is not null && _commandSurfaceProjection is null)
        {
            RefreshCommandSurfaceState();
        }

        return _commandSurfaceProjection;
    }

    private void ApplyWorkbenchProjectionPolicy()
    {
        if (_miniMapSurface is not null)
        {
            _miniMapSurface.UsesLightweightProjection = !CurrentWorkbenchPerformancePolicy.ProjectMiniMapContinuously;
        }
    }

    private void BuildStencilLibrary(
        IReadOnlyList<GraphEditorNodeTemplateSnapshot> stencilTemplates,
        GraphEditorCommandDescriptorSnapshot? addNodeDescriptor)
    {
        if (_stencilCardList is null)
        {
            return;
        }

        _stencilCardList.Children.Clear();
        if (Editor is null)
        {
            return;
        }

        var filteredTemplates = stencilTemplates
            .Where(MatchesStencilFilter)
            .ToList();
        AddStencilSpecialSection(
            "Favorites",
            filteredTemplates,
            _favoriteStencilTemplateKeys,
            addNodeDescriptor);
        AddStencilSpecialSection(
            "Recent",
            filteredTemplates,
            _recentStencilTemplateKeys,
            addNodeDescriptor);

        var stencilLimit = CurrentWorkbenchPerformancePolicy.StencilCardsPerSectionLimit;
        var stencilSections = filteredTemplates
            .GroupBy(stencilItem => stencilItem.Category, StringComparer.Ordinal)
            .OrderBy(section => section.Key, StringComparer.Ordinal);

        foreach (var stencilSection in stencilSections)
        {
            _stencilCardList.Children.Add(CreateStencilSection(
                stencilSection.Key,
                stencilSection.Take(stencilLimit).ToList(),
                addNodeDescriptor));
        }

        if (_stencilEmptyStateText is not null)
        {
            _stencilEmptyStateText.IsVisible = _stencilCardList.Children.Count == 0;
        }
    }

    private void BuildHeaderToolbar(AsterGraphHostedActionProjection? projection)
    {
        if (_headerToolbar is null)
        {
            return;
        }

        _headerToolbar.Children.Clear();
        if (projection is null)
        {
            return;
        }

        foreach (var action in projection.Select(GetWorkbenchCommandIds(HeaderCommandSurfaceId)))
        {
            _headerToolbar.Children.Add(CreateActionButton(action, $"PART_HeaderCommand_{action.Id}"));
        }
    }

    private void BuildValidationFeedback()
    {
        if (_validationFeedbackChrome is null
            && _validationStatusText is null
            && _statusValidationText is null
            && _validationFeedbackList is null)
        {
            return;
        }

        if (Editor is null)
        {
            ClearValidationFeedback();
            return;
        }

        var snapshot = Editor.Session.Queries.GetValidationSnapshot();
        var caption = CreateValidationStatusCaption(snapshot);
        if (_validationFeedbackChrome is not null)
        {
            _validationFeedbackChrome.IsVisible = true;
        }

        if (_validationStatusText is not null)
        {
            _validationStatusText.Text = caption;
        }

        if (_statusValidationText is not null)
        {
            _statusValidationText.Text = caption;
        }

        if (_problemsRepairDiscoveryStatusText is not null)
        {
            _problemsRepairDiscoveryStatusText.Text = snapshot.Issues.Count == 0
                ? string.Empty
                : "Preview available from each problem menu; apply uses editor history.";
            _problemsRepairDiscoveryStatusText.IsVisible = snapshot.Issues.Count > 0;
        }

        if (_validationFeedbackList is null)
        {
            return;
        }

        _validationFeedbackList.Children.Clear();
        if (snapshot.Issues.Count == 0)
        {
            _validationFeedbackList.Children.Add(new TextBlock
            {
                Text = "No validation issues.",
                FontSize = 12,
                Foreground = GetResourceBrush("AsterGraph.EyebrowBrush"),
            });
            return;
        }

        foreach (var issue in snapshot.Issues)
        {
            _validationFeedbackList.Children.Add(CreateValidationIssueRow(issue));
        }
    }

    private void ClearValidationFeedback()
    {
        if (_validationFeedbackChrome is not null)
        {
            _validationFeedbackChrome.IsVisible = false;
        }

        if (_validationStatusText is not null)
        {
            _validationStatusText.Text = string.Empty;
        }

        if (_statusValidationText is not null)
        {
            _statusValidationText.Text = string.Empty;
        }

        if (_problemsRepairDiscoveryStatusText is not null)
        {
            _problemsRepairDiscoveryStatusText.Text = string.Empty;
            _problemsRepairDiscoveryStatusText.IsVisible = false;
        }

        _validationFeedbackList?.Children.Clear();
    }

    private Control CreateValidationIssueRow(GraphEditorValidationIssueSnapshot issue)
    {
        var focusButton = new Button
        {
            Name = ResolveValidationFocusButtonName(issue),
            Content = "Focus",
        };
        focusButton.Classes.Add("astergraph-toolbar-action");
        AutomationProperties.SetName(focusButton, $"Focus {issue.Code}");
        ToolTip.SetTip(focusButton, CreateValidationIssueHelpCaption(issue));
        focusButton.Click += (_, _) =>
        {
            FocusValidationIssueFromProblemsPanel(issue, openInspector: false);
        };

        var row = new Button
        {
            Name = ResolveValidationProblemRowName(issue),
            Content = new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    new TextBlock
                    {
                        Text = $"{CreateValidationSeverityLabel(issue)}  ·  {issue.Code}",
                        FontSize = 11,
                        FontWeight = global::Avalonia.Media.FontWeight.Bold,
                        Foreground = GetResourceBrush("AsterGraph.HighlightBrush"),
                        TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                    },
                    new TextBlock
                    {
                        Text = issue.Message,
                        FontSize = 12,
                        Foreground = GetResourceBrush("AsterGraph.BodyBrush"),
                        TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                    },
                    new TextBlock
                    {
                        Text = CreateValidationTargetCaption(issue),
                        FontSize = 11,
                        Foreground = GetResourceBrush("AsterGraph.EyebrowBrush"),
                        TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                    },
                    new TextBlock
                    {
                        Text = CreateValidationIssueHelpCaption(issue),
                        FontSize = 11,
                        Foreground = GetResourceBrush("AsterGraph.HighlightBrush"),
                        TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                        IsVisible = issue.HelpTarget is not null,
                    },
                    focusButton,
                },
            },
            ContextMenu = CreateValidationIssueRepairDiscoveryMenu(issue),
        };
        row.Classes.Add("astergraph-problem-row");
        AutomationProperties.SetName(row, $"Problem {issue.Code}");
        if (issue.HelpTarget is not null)
        {
            AutomationProperties.SetHelpText(row, issue.HelpTarget.DisplayText);
        }

        ToolTip.SetTip(row, CreateValidationIssueHelpCaption(issue));
        row.Click += (_, _) => FocusValidationIssueFromProblemsPanel(issue, openInspector: false);
        row.DoubleTapped += (_, args) =>
        {
            FocusValidationIssueFromProblemsPanel(issue, openInspector: true);
            args.Handled = true;
        };

        return row;
    }

    private void FocusValidationIssueFromProblemsPanel(GraphEditorValidationIssueSnapshot issue, bool openInspector)
    {
        if (openInspector)
        {
            IsInspectorChromeVisible = true;
        }

        Editor?.Session.Commands.TryFocusValidationIssue(issue);
        RefreshCommandSurface();
    }

    private ContextMenu CreateValidationIssueRepairDiscoveryMenu(GraphEditorValidationIssueSnapshot issue)
    {
        var repairs = Editor?.Session.Queries.GetValidationIssueRepairActions(issue) ?? [];
        if (repairs.Count == 0)
        {
            var unavailableItem = new MenuItem
            {
                Name = $"{ResolveValidationProblemRowName(issue)}_RepairUnavailable",
                Header = "No proven repair",
                IsEnabled = false,
            };
            ToolTip.SetTip(unavailableItem, "No quick fix can be proven against the current graph state.");
            return new ContextMenu
            {
                ItemsSource = new[]
                {
                    unavailableItem,
                },
            };
        }

        var repairItems = repairs.Select(repair =>
        {
            var item = new MenuItem
            {
                Name = $"{ResolveValidationProblemRowName(issue)}_Repair_{repair.ActionId}",
                Header = repair.Label,
                IsEnabled = true,
            };
            ToolTip.SetTip(item, repair.PreviewText);
            item.Click += (_, _) =>
            {
                if (Editor?.Session.Commands.TryApplyValidationRepair(repair) == true)
                {
                    BuildValidationFeedback();
                    RefreshCommandSurface();
                }
            };
            return item;
        }).ToArray();

        return new ContextMenu
        {
            ItemsSource = repairItems,
        };
    }

    private static string CreateValidationStatusCaption(GraphEditorValidationSnapshot snapshot)
    {
        var errorText = snapshot.ErrorCount == 1
            ? "1 error"
            : $"{snapshot.ErrorCount} errors";
        var warningText = snapshot.WarningCount == 1
            ? "1 warning"
            : $"{snapshot.WarningCount} warnings";
        var readiness = snapshot.IsReady ? "Ready" : "Not ready";
        return $"{readiness}  ·  {errorText}  ·  {warningText}";
    }

    private static string CreateValidationSeverityLabel(GraphEditorValidationIssueSnapshot issue)
        => issue.Severity == GraphEditorValidationIssueSeverity.Error ? "Error" : "Warning";

    private static string CreateValidationTargetCaption(GraphEditorValidationIssueSnapshot issue)
    {
        if (!string.IsNullOrWhiteSpace(issue.ConnectionId))
        {
            return $"Connection {issue.ConnectionId}  ·  scope {issue.ScopeId}";
        }

        if (!string.IsNullOrWhiteSpace(issue.NodeId))
        {
            if (!string.IsNullOrWhiteSpace(issue.ParameterKey))
            {
                return $"Node {issue.NodeId}  ·  parameter {issue.ParameterKey}  ·  scope {issue.ScopeId}";
            }

            return $"Node {issue.NodeId}  ·  scope {issue.ScopeId}";
        }

        return $"Scope {issue.ScopeId}";
    }

    private static string CreateValidationIssueHelpCaption(GraphEditorValidationIssueSnapshot issue)
        => issue.HelpTarget is null
            ? "Click to focus. Double-click to focus and show the inspector."
            : $"Help target  ·  {issue.HelpTarget.DisplayText}";

    private static string ResolveValidationFocusButtonName(GraphEditorValidationIssueSnapshot issue)
        => $"PART_ValidationFocus_{CreateValidationIssueControlNameSuffix(issue)}";

    private static string ResolveValidationProblemRowName(GraphEditorValidationIssueSnapshot issue)
        => $"PART_ProblemsIssue_{CreateValidationIssueControlNameSuffix(issue)}";

    private static string CreateValidationIssueControlNameSuffix(GraphEditorValidationIssueSnapshot issue)
    {
        var target = issue.ConnectionId ?? issue.NodeId ?? issue.ScopeId;
        var segments = new List<string>();
        AddUniqueSegment(segments, target);
        AddUniqueSegment(segments, issue.Code);
        AddUniqueSegment(segments, issue.ParameterKey);
        AddUniqueSegment(segments, issue.EndpointId);
        return NormalizeControlNameSegment(string.Join("_", segments));
    }

    private static void AddUniqueSegment(List<string> segments, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || segments.Contains(value, StringComparer.Ordinal))
        {
            return;
        }

        segments.Add(value);
    }

    private static string NormalizeControlNameSegment(string value)
        => new(value
            .Select(character => char.IsLetterOrDigit(character) || character is '.' or '_' or '-'
                ? character
                : '_')
            .ToArray());

    private void BuildFragmentLibrary(AsterGraphHostedActionProjection? projection)
    {
        if (_fragmentActionToolbar is null
            && _fragmentCaptionText is null
            && _fragmentStatusText is null
            && _fragmentLibraryCaptionText is null
            && _fragmentTemplatePicker is null
            && _fragmentTemplateActionToolbar is null)
        {
            return;
        }

        if (Editor is null)
        {
            ClearFragmentLibrary();
            return;
        }

        var storage = Editor.Session.Queries.GetFragmentStorageSnapshot();
        var templates = Editor.Session.Queries.GetFragmentTemplateSnapshots();

        if (_fragmentCaptionText is not null)
        {
            _fragmentCaptionText.Text = CreateFragmentCaption(storage);
        }

        if (_fragmentStatusText is not null)
        {
            _fragmentStatusText.Text = CreateFragmentStatusCaption(storage);
        }

        if (_fragmentLibraryCaptionText is not null)
        {
            _fragmentLibraryCaptionText.Text = CreateFragmentLibraryCaption(storage, templates);
        }

        if (_fragmentActionToolbar is not null)
        {
            _fragmentActionToolbar.Children.Clear();
            if (projection is not null)
            {
                foreach (var action in projection.Select(
                    [
                        "fragments.export-selection",
                        "fragments.import",
                        "fragments.clear-workspace",
                        "fragments.export-template",
                    ]))
                {
                    _fragmentActionToolbar.Children.Add(CreateActionButton(
                        action,
                        $"PART_FragmentAction_{action.Id}"));
                }
            }
        }

        if (_fragmentTemplatePicker is not null)
        {
            _selectedFragmentTemplatePath = ResolveSelectedTemplatePath(templates);
            _fragmentTemplatePicker.ItemsSource = templates;
            _fragmentTemplatePicker.SelectedItem = templates.FirstOrDefault(
                template => string.Equals(template.Path, _selectedFragmentTemplatePath, StringComparison.Ordinal));
            _fragmentTemplatePicker.IsEnabled = storage.IsTemplateLibraryEnabled && templates.Count > 0;
        }

        BuildFragmentTemplateActionToolbar();
    }

    private void InitializeExportPanelControls()
    {
        if (_exportFormatPicker is not null)
        {
            AutomationProperties.SetName(_exportFormatPicker, "Export format");
            _exportFormatPicker.ItemsSource = new[] { ExportFormatSvg, ExportFormatPng, ExportFormatJpeg };
            _exportFormatPicker.SelectedItem = ExportFormatSvg;
        }

        if (_exportScopePicker is not null)
        {
            AutomationProperties.SetName(_exportScopePicker, "Export scope");
            _exportScopePicker.ItemsSource = new[] { ExportScopeFullScene, ExportScopeSelectedNodes };
            _exportScopePicker.SelectedItem = ExportScopeFullScene;
        }

        if (_exportRunButton is not null)
        {
            AutomationProperties.SetName(_exportRunButton, "Run scene export");
        }

        if (_exportCancelButton is not null)
        {
            AutomationProperties.SetName(_exportCancelButton, "Cancel scene export");
        }

        if (_exportProgressText is not null)
        {
            _exportProgressText.Text = "Idle";
        }

        if (_exportStatusText is not null)
        {
            _exportStatusText.Text = "Choose a format and scope.";
        }
    }

    private void BuildExportPanel()
    {
        if (_exportPreviewText is null
            && _exportRunButton is null
            && _exportCancelButton is null)
        {
            return;
        }

        var snapshot = Editor?.Session.Queries.CreateDocumentSnapshot();
        var selection = Editor?.Session.Queries.GetSelectionSnapshot();
        var selectedNodeCount = selection?.SelectedNodeIds.Count ?? 0;
        var nodeCount = snapshot?.Nodes.Count ?? 0;
        var connectionCount = snapshot?.Connections.Count ?? 0;
        var format = GetSelectedExportFormat();
        var selectedScope = string.Equals(
            _exportScopePicker?.SelectedItem?.ToString(),
            ExportScopeSelectedNodes,
            StringComparison.Ordinal);
        var isSvg = string.Equals(format, ExportFormatSvg, StringComparison.Ordinal);
        var hasEditor = Editor is not null;
        var canExportScope = hasEditor && (!selectedScope || selectedNodeCount > 0);

        if (_exportPreviewText is not null)
        {
            _exportPreviewText.Text = selectedScope
                ? $"Selection scope  ·  {selectedNodeCount} selected nodes"
                : $"Full scene  ·  {nodeCount} nodes  ·  {connectionCount} connections";
        }

        if (_exportScopePicker is not null)
        {
            _exportScopePicker.IsEnabled = hasEditor && !isSvg;
            ToolTip.SetTip(
                _exportScopePicker,
                isSvg ? "SVG export uses the full scene route." : "Raster export supports full scene and selected nodes.");
        }

        if (_exportRunButton is not null)
        {
            _exportRunButton.IsEnabled = canExportScope;
            ToolTip.SetTip(
                _exportRunButton,
                canExportScope ? null : "Select at least one node before exporting selected nodes.");
        }

        if (_exportCancelButton is not null)
        {
            _exportCancelButton.IsEnabled = hasEditor && !isSvg;
        }
    }

    private void RunExportFromPanel()
    {
        if (Editor is null)
        {
            return;
        }

        var format = GetSelectedExportFormat();
        if (_exportProgressBar is not null)
        {
            _exportProgressBar.Value = 0;
        }

        if (_exportProgressText is not null)
        {
            _exportProgressText.Text = "Starting export";
        }

        if (string.Equals(format, ExportFormatSvg, StringComparison.Ordinal))
        {
            var svgExported = Editor.Session.Commands.TryExportSceneAsSvg();
            SetExportCompletedStatus(svgExported, "SVG");
            return;
        }

        using var cancellation = new CancellationTokenSource();
        _exportCancellation = cancellation;
        if (_exportCancelRequested)
        {
            cancellation.Cancel();
        }

        var imageFormat = string.Equals(format, ExportFormatJpeg, StringComparison.Ordinal)
            ? GraphEditorSceneImageExportFormat.Jpeg
            : GraphEditorSceneImageExportFormat.Png;
        var options = new GraphEditorSceneImageExportOptions
        {
            Scope = string.Equals(_exportScopePicker?.SelectedItem?.ToString(), ExportScopeSelectedNodes, StringComparison.Ordinal)
                ? GraphEditorSceneImageExportScope.SelectedNodes
                : GraphEditorSceneImageExportScope.FullScene,
            Progress = new Progress<GraphEditorSceneImageExportProgressSnapshot>(ApplyExportProgress),
            CancellationToken = cancellation.Token,
        };

        var imageExported = Editor.Session.Commands.TryExportSceneAsImage(imageFormat, options: options);
        _exportCancellation = null;
        _exportCancelRequested = false;
        SetExportCompletedStatus(imageExported, format);
        BuildExportPanel();
    }

    private void ApplyExportProgress(GraphEditorSceneImageExportProgressSnapshot snapshot)
    {
        var percent = Math.Clamp(snapshot.Fraction, 0d, 1d) * 100d;
        if (_exportProgressBar is not null)
        {
            _exportProgressBar.Value = percent;
        }

        if (_exportProgressText is not null)
        {
            _exportProgressText.Text = $"{percent:0}%  ·  {snapshot.Stage}";
        }

        if (_exportStatusText is not null)
        {
            _exportStatusText.Text = snapshot.Message;
        }
    }

    private void SetExportCompletedStatus(bool exported, string format)
    {
        if (_exportProgressBar is not null)
        {
            _exportProgressBar.Value = exported ? 100 : 0;
        }

        if (_exportProgressText is not null)
        {
            _exportProgressText.Text = exported ? "100%  ·  written" : "Export did not complete";
        }

        if (_exportStatusText is not null)
        {
            _exportStatusText.Text = exported
                ? $"{format} export completed."
                : $"{format} export failed or was canceled.";
        }
    }

    private string GetSelectedExportFormat()
        => _exportFormatPicker?.SelectedItem?.ToString() switch
        {
            ExportFormatPng => ExportFormatPng,
            ExportFormatJpeg => ExportFormatJpeg,
            _ => ExportFormatSvg,
        };

    private void BuildCompositeWorkflowToolbar(AsterGraphHostedActionProjection? projection)
    {
        if (_compositeWorkflowToolbar is null)
        {
            return;
        }

        _compositeWorkflowToolbar.Children.Clear();
        if (projection is null)
        {
            return;
        }

        foreach (var action in projection.Select(GetWorkbenchCommandIds(CompositeWorkflowSurfaceId)))
        {
            _compositeWorkflowToolbar.Children.Add(CreateActionButton(action, $"PART_CompositeWorkflowAction_{action.Id}"));
        }
    }

    private void BuildScopeBreadcrumbs()
    {
        if (_scopeBreadcrumbs is null)
        {
            return;
        }

        _scopeBreadcrumbs.Children.Clear();
        if (Editor is null)
        {
            return;
        }

        foreach (var action in AsterGraphCompositeWorkflowActionFactory.CreateBreadcrumbActions(Editor.Session))
        {
            _scopeBreadcrumbs.Children.Add(CreateActionButton(action, $"PART_ScopeBreadcrumb_{action.Id["breadcrumb.".Length..]}"));
        }
    }

    private void BuildShortcutHelp(AsterGraphHostedActionProjection? projection)
    {
        if (_shortcutHelpList is null)
        {
            return;
        }

        _shortcutHelpList.Children.Clear();
        if (projection is null)
        {
            return;
        }

        var shortcutActionIds = GetWorkbenchCommandIds(ShortcutHelpSurfaceId)
            .Append(CommandPaletteActionId)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var action in projection.WithShortcuts().Where(action => shortcutActionIds.Contains(action.Id)))
        {
            _shortcutHelpList.Children.Add(CreateShortcutHelpItem($"{action.DefaultShortcut}：{action.Title}"));
        }
    }

    private void BuildCommandPaletteItems(AsterGraphHostedActionProjection? projection)
    {
        if (_commandPaletteItems is null)
        {
            return;
        }

        _commandPaletteItems.Children.Clear();
        if (projection is null)
        {
            return;
        }

        var paletteActionIds = GetWorkbenchCommandIds(CommandPaletteSurfaceId).ToHashSet(StringComparer.Ordinal);
        var actions = projection.Actions
            .Where(action => !string.Equals(action.Id, CommandPaletteActionId, StringComparison.Ordinal))
            .Where(action => paletteActionIds.Contains(action.Id) || !_commandRegistryById.ContainsKey(action.Id))
            .Where(MatchesCommandPaletteFilter)
            .ToList();
        if (actions.Count == 0)
        {
            _commandPaletteItems.Children.Add(new TextBlock
            {
                Text = "No matching commands. Try a different search term.",
            });
            return;
        }

        AddRecentCommandPaletteItems(actions);

        foreach (var group in actions.GroupBy(action => action.Group, StringComparer.Ordinal))
        {
            _commandPaletteItems.Children.Add(CreateCommandPaletteGroupHeader(group.Key));
            foreach (var action in group)
            {
                _commandPaletteItems.Children.Add(CreateCommandPaletteActionButton(
                    action,
                    $"PART_CommandPaletteAction_{action.Id}"));
            }
        }
    }

    private void AddRecentCommandPaletteItems(IReadOnlyList<AsterGraphHostedActionDescriptor> actions)
    {
        if (_commandPaletteItems is null || _recentCommandPaletteActionIds.Count == 0)
        {
            return;
        }

        var recentActions = _recentCommandPaletteActionIds
            .Select(actionId => actions.FirstOrDefault(action => string.Equals(action.Id, actionId, StringComparison.Ordinal)))
            .Where(action => action is not null)
            .Cast<AsterGraphHostedActionDescriptor>()
            .ToList();
        if (recentActions.Count == 0)
        {
            return;
        }

        _commandPaletteItems.Children.Add(new TextBlock
        {
            Name = "PART_CommandPaletteRecentActionsHeading",
            Text = "Recent Actions",
            Classes = { "astergraph-section-heading" },
        });

        foreach (var action in recentActions)
        {
            _commandPaletteItems.Children.Add(CreateCommandPaletteActionButton(
                action,
                $"PART_CommandPaletteRecentAction_{action.Id}"));
        }
    }

    private static TextBlock CreateCommandPaletteGroupHeader(string group)
        => new()
        {
            Name = $"PART_CommandPaletteGroup_{CreateControlNameToken(group)}",
            Text = group,
            Classes = { "astergraph-section-heading" },
        };

    private Button CreateCommandPaletteActionButton(
        AsterGraphHostedActionDescriptor action,
        string name)
        => CreateActionButton(
            action,
            name,
            includeShortcut: true,
            closePaletteOnExecute: true,
            afterExecute: executed =>
            {
                if (executed)
                {
                    RecordCommandPaletteAction(action.Id);
                }
            });

    private AsterGraphHostedActionProjection CreateCommandSurfaceProjection(
        IReadOnlyList<GraphEditorCommandRegistryEntrySnapshot> commandRegistry,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commandDescriptorMap,
        GraphEditorSelectionSnapshot selection,
        IReadOnlyDictionary<string, GraphEditorCompositeNodeSnapshot> composites,
        GraphEditorAuthoringToolSurfaceState? authoringToolSurfaceState)
    {
        ArgumentNullException.ThrowIfNull(Editor);
        ArgumentNullException.ThrowIfNull(commandRegistry);
        ArgumentNullException.ThrowIfNull(commandDescriptorMap);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(composites);

        var commandPaletteDescriptors = commandRegistry
            .Where(entry => entry.Placements.Any(placement =>
                placement.SurfaceKind == GraphEditorCommandSurfaceKind.Workbench
                && string.Equals(placement.SurfaceId, CommandPaletteSurfaceId, StringComparison.Ordinal)))
            .OrderBy(entry => entry.Group, StringComparer.Ordinal)
            .ThenBy(entry => entry.Title, StringComparer.Ordinal)
            .Select(entry => entry.Descriptor)
            .ToList();

        var actions = AsterGraphHostedActionFactory.ApplyCommandShortcutPolicy(
            [
                CreateCommandPaletteAction(),
                .. AsterGraphHostedActionFactory.CreateCommandActions(commandPaletteDescriptors, Editor.Session),
                .. AsterGraphCompositeWorkflowActionFactory.CreateWorkflowActions(Editor.Session, commandDescriptorMap, selection, composites),
                .. authoringToolSurfaceState?.CommandSurfaceActions ?? [],
            ],
            CommandShortcutPolicy);
        return AsterGraphHostedActionFactory.CreateProjection(actions);
    }

    private IReadOnlyList<string> GetWorkbenchCommandIds(string surfaceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(surfaceId);

        return _commandRegistryById.Values
            .SelectMany(entry => entry.Placements
                .Where(placement =>
                    placement.SurfaceKind == GraphEditorCommandSurfaceKind.Workbench
                    && string.Equals(placement.SurfaceId, surfaceId, StringComparison.Ordinal))
                .Select(placement => new
                {
                    entry.CommandId,
                    placement.Order,
                    placement.PlacementId,
                }))
            .OrderBy(item => item.Order)
            .ThenBy(item => item.PlacementId, StringComparer.Ordinal)
            .Select(item => item.CommandId)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private AsterGraphHostedActionDescriptor CreateCommandPaletteAction()
        => AsterGraphHostedActionFactory.CreateHostAction(
            new GraphEditorCommandDescriptorSnapshot(
                CommandPaletteActionId,
                "Command Palette",
                "shell",
                "palette",
                "Ctrl+Shift+P",
                GraphEditorCommandSourceKind.Host,
                isEnabled: true),
            () =>
            {
                ToggleCommandPalette();
                return true;
            });

    private void RefreshCommandPaletteButton(AsterGraphHostedActionProjection? projection)
    {
        _commandPaletteAction = null;
        if (_openCommandPaletteButton is null)
        {
            return;
        }

        if (projection is null || !projection.TryGet(CommandPaletteActionId, out var commandPaletteAction))
        {
            _openCommandPaletteButton.Content = "Command Palette";
            _openCommandPaletteButton.IsEnabled = false;
            ToolTip.SetTip(_openCommandPaletteButton, null);
            return;
        }

        _commandPaletteAction = commandPaletteAction;
        _openCommandPaletteButton.Content = commandPaletteAction.Title;
        _openCommandPaletteButton.IsEnabled = commandPaletteAction.CanExecute;
        ToolTip.SetTip(
            _openCommandPaletteButton,
            commandPaletteAction.DisabledReason ?? commandPaletteAction.DefaultShortcut);
    }

    private bool MatchesCommandPaletteFilter(AsterGraphHostedActionDescriptor action)
    {
        if (string.IsNullOrWhiteSpace(_commandPaletteFilter))
        {
            return true;
        }

        return action.Title.Contains(_commandPaletteFilter, StringComparison.OrdinalIgnoreCase)
            || action.Group.Contains(_commandPaletteFilter, StringComparison.OrdinalIgnoreCase)
            || action.Id.Contains(_commandPaletteFilter, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(action.DefaultShortcut)
                && action.DefaultShortcut.Contains(_commandPaletteFilter, StringComparison.OrdinalIgnoreCase));
    }

    private Button CreateActionButton(
        AsterGraphHostedActionDescriptor action,
        string name,
        bool includeShortcut = false,
        bool closePaletteOnExecute = false,
        Action<bool>? afterExecute = null)
    {
        var button = new Button
        {
            Name = name,
            Content = includeShortcut && !string.IsNullOrWhiteSpace(action.DefaultShortcut)
                ? $"{action.Title} ({action.DefaultShortcut})"
                : action.Title,
            IsEnabled = action.CanExecute,
        };
        button.Classes.Add("astergraph-toolbar-action");
        AutomationProperties.SetName(button, action.Title);
        var actionHint = action.DisabledReason ?? action.DefaultShortcut;
        if (!string.IsNullOrWhiteSpace(action.RecoveryHint))
        {
            actionHint = string.IsNullOrWhiteSpace(actionHint)
                ? $"→ {action.RecoveryHint}"
                : $"{actionHint}\n→ {action.RecoveryHint}";
        }

        if (!string.IsNullOrWhiteSpace(actionHint))
        {
            ToolTip.SetTip(button, actionHint);
        }

        button.Click += (_, _) =>
        {
            var executed = action.TryExecute();
            afterExecute?.Invoke(executed || action.CanExecute);
            if (closePaletteOnExecute)
            {
                CloseCommandPalette();
            }

            RefreshCommandSurface();
        };
        return button;
    }

    private void RecordCommandPaletteAction(string actionId)
    {
        if (string.IsNullOrWhiteSpace(actionId)
            || string.Equals(actionId, CommandPaletteActionId, StringComparison.Ordinal))
        {
            return;
        }

        _recentCommandPaletteActionIds.RemoveAll(id => string.Equals(id, actionId, StringComparison.Ordinal));
        _recentCommandPaletteActionIds.Insert(0, actionId);
        if (_recentCommandPaletteActionIds.Count > CommandPaletteRecentActionLimit)
        {
            _recentCommandPaletteActionIds.RemoveRange(
                CommandPaletteRecentActionLimit,
                _recentCommandPaletteActionIds.Count - CommandPaletteRecentActionLimit);
        }
    }

    private static string CreateControlNameToken(string value)
    {
        var chars = value
            .Select(character => char.IsLetterOrDigit(character) ? character : '_')
            .ToArray();
        return new string(chars);
    }

    private Border CreateShortcutHelpItem(string text)
        => new()
        {
            Classes = { "astergraph-inspector-section" },
            Child = new TextBlock
            {
                Text = text,
                FontSize = 14,
            },
        };

    private Control CreateStencilCard(
        GraphEditorNodeTemplateSnapshot stencilItem,
        GraphEditorCommandDescriptorSnapshot? addNodeDescriptor)
    {
        var favoriteButton = new Button
        {
            Name = $"PART_StencilFavorite_{stencilItem.Key}",
            Content = _favoriteStencilTemplateKeys.Contains(stencilItem.Key) ? "Unfavorite" : "Favorite",
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left,
        };
        favoriteButton.Classes.Add("astergraph-toolbar-action");
        favoriteButton.Click += (_, args) =>
        {
            ToggleFavoriteStencilTemplate(stencilItem.Key);
            args.Handled = true;
        };

        var insertButton = new Button
        {
            Name = $"PART_StencilCard_{stencilItem.Key}",
            Margin = new Thickness(0, 6, 0, 12),
            IsEnabled = addNodeDescriptor?.IsEnabled ?? false,
            Content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new StackPanel
                    {
                        Spacing = 2,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = stencilItem.Category,
                                FontSize = 10,
                                FontWeight = global::Avalonia.Media.FontWeight.Bold,
                                Foreground = GetResourceBrush("AsterGraph.EyebrowBrush"),
                            },
                            new TextBlock
                            {
                                Text = stencilItem.Title,
                                FontSize = 16,
                                FontWeight = global::Avalonia.Media.FontWeight.SemiBold,
                                Foreground = GetResourceBrush("AsterGraph.HeadlineBrush"),
                            },
                            new TextBlock
                            {
                                Text = stencilItem.Subtitle,
                                FontSize = 11,
                                Foreground = GetResourceBrush("AsterGraph.HighlightBrush"),
                            },
                        },
                    },
                    new TextBlock
                    {
                        Text = stencilItem.Description,
                        FontSize = 12,
                        TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                        Foreground = GetResourceBrush("AsterGraph.BodyBrush"),
                    },
                    new Border
                    {
                        Classes = { "astergraph-pill" },
                        HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left,
                        Child = new TextBlock
                        {
                            Text = stencilItem.PortSummary,
                            FontSize = 10,
                            FontWeight = global::Avalonia.Media.FontWeight.SemiBold,
                            Foreground = GetResourceBrush("AsterGraph.HeadlineBrush"),
                        },
                    },
                },
            },
        };
        insertButton.Classes.Add("astergraph-template-card");
        if (!string.IsNullOrWhiteSpace(addNodeDescriptor?.DisabledReason))
        {
            ToolTip.SetTip(insertButton, addNodeDescriptor.DisabledReason);
        }
        else
        {
            ToolTip.SetTip(insertButton, stencilItem.ActionDescription);
        }

        insertButton.Click += (_, _) =>
        {
            Editor?.Session.Commands.AddNode(stencilItem.DefinitionId);
            RecordRecentStencilTemplate(stencilItem.Key);
            RefreshCommandSurface();
        };
        return new StackPanel
        {
            Spacing = 0,
            Children =
            {
                favoriteButton,
                insertButton,
            },
        };
    }

    private Expander CreateStencilSection(
        string category,
        IReadOnlyList<GraphEditorNodeTemplateSnapshot> stencilItems,
        GraphEditorCommandDescriptorSnapshot? addNodeDescriptor)
    {
        var sectionCards = new StackPanel
        {
            Spacing = 0,
        };
        foreach (var stencilItem in stencilItems)
        {
            sectionCards.Children.Add(CreateStencilCard(stencilItem, addNodeDescriptor));
        }

        var expander = new Expander
        {
            Name = $"PART_StencilSection_{CreateStencilSectionKey(category)}",
            Tag = category,
            Margin = new Thickness(0, 0, 0, 12),
            IsExpanded = !_collapsedStencilCategories.Contains(category),
            Header = new StackPanel
            {
                Orientation = global::Avalonia.Layout.Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new TextBlock
                    {
                        Text = category,
                        FontSize = 13,
                        FontWeight = global::Avalonia.Media.FontWeight.SemiBold,
                        Foreground = GetResourceBrush("AsterGraph.HeadlineBrush"),
                    },
                    new Border
                    {
                        Classes = { "astergraph-pill" },
                        VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
                        Child = new TextBlock
                        {
                            Text = stencilItems.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            FontSize = 10,
                            FontWeight = global::Avalonia.Media.FontWeight.SemiBold,
                            Foreground = GetResourceBrush("AsterGraph.HeadlineBrush"),
                        },
                    },
                },
            },
            Content = sectionCards,
        };
        expander.Expanded += HandleStencilSectionExpanded;
        expander.Collapsed += HandleStencilSectionCollapsed;
        return expander;
    }

    private bool MatchesStencilFilter(GraphEditorNodeTemplateSnapshot stencilItem)
    {
        if (!MatchesStencilSourceFilter(stencilItem))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(_stencilFilter))
        {
            return true;
        }

        return stencilItem.Category.Contains(_stencilFilter, StringComparison.OrdinalIgnoreCase)
            || stencilItem.Title.Contains(_stencilFilter, StringComparison.OrdinalIgnoreCase)
            || stencilItem.Subtitle.Contains(_stencilFilter, StringComparison.OrdinalIgnoreCase)
            || stencilItem.Description.Contains(_stencilFilter, StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchesStencilSourceFilter(GraphEditorNodeTemplateSnapshot stencilItem)
    {
        var isPluginTemplate = IsPluginStencilTemplate(stencilItem);
        return _stencilSourceFilterValue switch
        {
            StencilSourceFilterBuiltIn => !isPluginTemplate,
            StencilSourceFilterPlugin => isPluginTemplate,
            _ => true,
        };
    }

    private static bool IsPluginStencilTemplate(GraphEditorNodeTemplateSnapshot stencilItem)
        => stencilItem.Category.Contains("plugin", StringComparison.OrdinalIgnoreCase);

    private void AddStencilSpecialSection(
        string sectionName,
        IReadOnlyList<GraphEditorNodeTemplateSnapshot> filteredTemplates,
        IEnumerable<string> orderedTemplateKeys,
        GraphEditorCommandDescriptorSnapshot? addNodeDescriptor)
    {
        var templatesByKey = filteredTemplates.ToDictionary(template => template.Key, StringComparer.Ordinal);
        var stencilItems = orderedTemplateKeys
            .Where(templatesByKey.ContainsKey)
            .Select(key => templatesByKey[key])
            .ToList();
        if (stencilItems.Count == 0)
        {
            return;
        }

        _stencilCardList?.Children.Add(CreateStencilSection(sectionName, stencilItems, addNodeDescriptor));
    }

    private void HandleStencilSectionExpanded(object? sender, RoutedEventArgs args)
    {
        if (sender is Expander expander && expander.Tag is string category)
        {
            _collapsedStencilCategories.Remove(category);
        }
    }

    private void HandleStencilSectionCollapsed(object? sender, RoutedEventArgs args)
    {
        if (sender is Expander expander && expander.Tag is string category)
        {
            _collapsedStencilCategories.Add(category);
        }
    }

    private void ToggleFavoriteStencilTemplate(string templateKey)
    {
        if (string.IsNullOrWhiteSpace(templateKey))
        {
            return;
        }

        if (!_favoriteStencilTemplateKeys.Add(templateKey))
        {
            _favoriteStencilTemplateKeys.Remove(templateKey);
        }

        BuildStencilLibrary(_stencilTemplateSnapshots, _stencilAddNodeDescriptor);
    }

    private void RecordRecentStencilTemplate(string templateKey)
    {
        if (string.IsNullOrWhiteSpace(templateKey))
        {
            return;
        }

        _recentStencilTemplateKeys.RemoveAll(key => string.Equals(key, templateKey, StringComparison.Ordinal));
        _recentStencilTemplateKeys.Insert(0, templateKey);
        if (_recentStencilTemplateKeys.Count > StencilRecentTemplateLimit)
        {
            _recentStencilTemplateKeys.RemoveRange(
                StencilRecentTemplateLimit,
                _recentStencilTemplateKeys.Count - StencilRecentTemplateLimit);
        }
    }

    private static string CreateStencilSectionKey(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return "uncategorized";
        }

        var sanitized = new string(category
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray())
            .Trim('-');

        return string.IsNullOrWhiteSpace(sanitized)
            ? "uncategorized"
            : sanitized;
    }

    private global::Avalonia.Media.IBrush? GetResourceBrush(string key)
        => this.TryGetResource(key, ActualThemeVariant, out var resource)
            ? resource as global::Avalonia.Media.IBrush
            : null;

    private void ClearFragmentLibrary()
    {
        _selectedFragmentTemplatePath = null;
        if (_fragmentCaptionText is not null)
        {
            _fragmentCaptionText.Text = string.Empty;
        }

        if (_fragmentStatusText is not null)
        {
            _fragmentStatusText.Text = string.Empty;
        }

        if (_fragmentLibraryCaptionText is not null)
        {
            _fragmentLibraryCaptionText.Text = string.Empty;
        }

        if (_fragmentActionToolbar is not null)
        {
            _fragmentActionToolbar.Children.Clear();
        }

        if (_fragmentTemplateActionToolbar is not null)
        {
            _fragmentTemplateActionToolbar.Children.Clear();
        }

        if (_fragmentTemplatePicker is not null)
        {
            _fragmentTemplatePicker.ItemsSource = null;
            _fragmentTemplatePicker.SelectedItem = null;
            _fragmentTemplatePicker.IsEnabled = false;
        }
    }

    private void BuildFragmentTemplateActionToolbar()
    {
        if (_fragmentTemplateActionToolbar is null)
        {
            return;
        }

        _fragmentTemplateActionToolbar.Children.Clear();
        if (Editor is null)
        {
            return;
        }

        var storage = Editor.Session.Queries.GetFragmentStorageSnapshot();
        var templates = Editor.Session.Queries.GetFragmentTemplateSnapshots();
        var selectedTemplate = templates.FirstOrDefault(
            template => string.Equals(template.Path, _selectedFragmentTemplatePath, StringComparison.Ordinal));
        var importAction = CreateFragmentTemplateAction(
            "fragments.import-template",
            "Import Template",
            storage.CanImportFragmentTemplate,
            storage.IsTemplateLibraryEnabled,
            selectedTemplate,
            path => Editor.Session.Commands.TryImportFragmentTemplate(path));
        var deleteAction = CreateFragmentTemplateAction(
            "fragments.delete-template",
            "Delete Template",
            storage.CanDeleteFragmentTemplate,
            storage.IsTemplateLibraryEnabled,
            selectedTemplate,
            path => Editor.Session.Commands.TryDeleteFragmentTemplate(path));

        _fragmentTemplateActionToolbar.Children.Add(CreateActionButton(importAction, "PART_FragmentTemplateImportButton"));
        _fragmentTemplateActionToolbar.Children.Add(CreateActionButton(deleteAction, "PART_FragmentTemplateDeleteButton"));
    }

    private AsterGraphHostedActionDescriptor CreateFragmentTemplateAction(
        string id,
        string title,
        bool canExecute,
        bool isLibraryEnabled,
        GraphEditorFragmentTemplateSnapshot? selectedTemplate,
        Func<string, bool> execute)
    {
        var disabledReason = GetFragmentTemplateDisabledReason(canExecute, isLibraryEnabled, selectedTemplate);
        return AsterGraphHostedActionFactory.CreateHostAction(
            new GraphEditorCommandDescriptorSnapshot(
                id,
                title,
                "fragments",
                null,
                null,
                GraphEditorCommandSourceKind.Host,
                disabledReason is null,
                disabledReason),
            () => selectedTemplate is not null && execute(selectedTemplate.Path));
    }

    private static string? GetFragmentTemplateDisabledReason(
        bool canExecute,
        bool isLibraryEnabled,
        GraphEditorFragmentTemplateSnapshot? selectedTemplate)
    {
        if (!isLibraryEnabled)
        {
            return "Fragment template library is disabled.";
        }

        if (!canExecute)
        {
            return "Fragment template actions are disabled by host permissions.";
        }

        if (selectedTemplate is null)
        {
            return "Choose a fragment template first.";
        }

        return null;
    }

    private string? ResolveSelectedTemplatePath(IReadOnlyList<GraphEditorFragmentTemplateSnapshot> templates)
    {
        if (templates.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(_selectedFragmentTemplatePath)
            && templates.Any(template => string.Equals(template.Path, _selectedFragmentTemplatePath, StringComparison.Ordinal)))
        {
            return _selectedFragmentTemplatePath;
        }

        return templates[0].Path;
    }

    private static string CreateFragmentCaption(GraphEditorFragmentStorageSnapshot storage)
    {
        var availability = storage.HasWorkspaceFragment
            ? "Fragment available"
            : "No fragment file";
        return $"{availability}  ·  {storage.WorkspaceFragmentPath}";
    }

    private static string CreateFragmentStatusCaption(GraphEditorFragmentStorageSnapshot storage)
        => !storage.HasWorkspaceFragment || storage.WorkspaceFragmentLastModified is null
            ? "No saved fragment file."
            : $"Last updated {storage.WorkspaceFragmentLastModified.Value:yyyy-MM-dd HH:mm:ss}";

    private static string CreateFragmentLibraryCaption(
        GraphEditorFragmentStorageSnapshot storage,
        IReadOnlyList<GraphEditorFragmentTemplateSnapshot> templates)
    {
        var templateState = templates.Count > 0
            ? $"{templates.Count} templates"
            : "No templates yet — export a selection as a template to get started";
        return $"{templateState}  ·  {storage.TemplateLibraryPath}";
    }

    private void ToggleCommandPalette()
    {
        if (_commandPaletteChrome?.IsVisible == true)
        {
            CloseCommandPalette();
            return;
        }

        OpenCommandPalette();
    }

    private void OpenCommandPalette()
    {
        if (_commandPaletteChrome is null)
        {
            return;
        }

        if (_commandPaletteReturnFocusTarget is null)
        {
            CaptureCommandPaletteReturnFocusTarget();
        }

        _commandPaletteFilter = string.Empty;
        if (_commandPaletteSearchBox is not null)
        {
            _commandPaletteSearchBox.Text = string.Empty;
        }

        _commandPaletteChrome.IsVisible = true;
        BuildCommandPaletteItems(GetCommandSurfaceProjection());
        _commandPaletteSearchBox?.Focus();
    }

    private void CloseCommandPalette()
    {
        if (_commandPaletteChrome is null)
        {
            return;
        }

        _commandPaletteChrome.IsVisible = false;
        RestoreCommandPaletteFocus();
    }

    private void CaptureCommandPaletteReturnFocusTarget(Control? focusSource = null)
    {
        var candidate = focusSource ?? TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
        _commandPaletteReturnFocusTarget = IsCommandPaletteControl(candidate)
            ? null
            : candidate;
    }

    private void RestoreCommandPaletteFocus()
    {
        var focusTarget = _commandPaletteReturnFocusTarget;
        _commandPaletteReturnFocusTarget = null;

        if (focusTarget is not null
            && !IsCommandPaletteControl(focusTarget)
            && TopLevel.GetTopLevel(focusTarget) is not null
            && focusTarget.IsVisible
            && focusTarget.Focusable)
        {
            focusTarget.Focus();
            return;
        }

        if (_nodeCanvas is not null && _nodeCanvas.Focusable)
        {
            _nodeCanvas.Focus();
            return;
        }

        Focus();
    }

    private bool IsCommandPaletteControl(Control? control)
    {
        if (control is null || _commandPaletteChrome is null)
        {
            return false;
        }

        for (Visual? current = control; current is not null; current = current.GetVisualParent())
        {
            if (ReferenceEquals(current, _commandPaletteChrome))
            {
                return true;
            }
        }

        return false;
    }
}
