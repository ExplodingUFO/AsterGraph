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

    private static readonly IReadOnlyList<string> HeaderCommandIds =
    [
        "workspace.save",
        "workspace.load",
        "history.undo",
        "history.redo",
        "viewport.fit",
        "viewport.fit-selection",
        "viewport.focus-selection",
        "viewport.reset",
        "selection.delete",
    ];

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
    private TextBox? _stencilSearchBox;
    private StackPanel? _stencilCardList;
    private WrapPanel? _headerToolbar;
    private WrapPanel? _compositeWorkflowToolbar;
    private WrapPanel? _scopeBreadcrumbs;
    private StackPanel? _shortcutHelpList;
    private TextBlock? _fragmentCaptionText;
    private TextBlock? _fragmentStatusText;
    private WrapPanel? _fragmentActionToolbar;
    private TextBlock? _fragmentLibraryCaptionText;
    private ComboBox? _fragmentTemplatePicker;
    private WrapPanel? _fragmentTemplateActionToolbar;
    private Button? _openCommandPaletteButton;
    private Border? _commandPaletteChrome;
    private TextBox? _commandPaletteSearchBox;
    private StackPanel? _commandPaletteItems;
    private Control? _commandPaletteReturnFocusTarget;
    private double _defaultShellRowSpacing;
    private double _defaultShellColumnSpacing;
    private readonly GraphEditorViewCompositionCoordinator _compositionCoordinator;
    private readonly HashSet<string> _collapsedStencilCategories = [];
    private string _stencilFilter = string.Empty;
    private string _commandPaletteFilter = string.Empty;
    private string? _selectedFragmentTemplatePath;
    private AsterGraphHostedActionDescriptor? _commandPaletteAction;
    private AsterGraphHostedActionProjection? _commandSurfaceProjection;
    private IReadOnlyList<GraphEditorNodeTemplateSnapshot> _stencilTemplateSnapshots = [];
    private IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> _commandDescriptorsById =
        new Dictionary<string, GraphEditorCommandDescriptorSnapshot>(StringComparer.Ordinal);
    private GraphEditorCommandDescriptorSnapshot? _stencilAddNodeDescriptor;

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
        _stencilSearchBox = this.FindControl<TextBox>("PART_StencilSearchBox");
        _stencilCardList = this.FindControl<StackPanel>("PART_StencilCardList");
        _headerToolbar = this.FindControl<WrapPanel>("PART_HeaderToolbar");
        _compositeWorkflowToolbar = this.FindControl<WrapPanel>("PART_CompositeWorkflowToolbar");
        _scopeBreadcrumbs = this.FindControl<WrapPanel>("PART_ScopeBreadcrumbs");
        _shortcutHelpList = this.FindControl<StackPanel>("PART_ShortcutHelpList");
        _fragmentCaptionText = this.FindControl<TextBlock>("PART_FragmentCaptionText");
        _fragmentStatusText = this.FindControl<TextBlock>("PART_FragmentStatusText");
        _fragmentActionToolbar = this.FindControl<WrapPanel>("PART_FragmentActionToolbar");
        _fragmentLibraryCaptionText = this.FindControl<TextBlock>("PART_FragmentLibraryCaptionText");
        _fragmentTemplatePicker = this.FindControl<ComboBox>("PART_FragmentTemplatePicker");
        _fragmentTemplateActionToolbar = this.FindControl<WrapPanel>("PART_FragmentTemplateActionToolbar");
        _openCommandPaletteButton = this.FindControl<Button>("PART_OpenCommandPaletteButton");
        _commandPaletteChrome = this.FindControl<Border>("PART_CommandPaletteChrome");
        _commandPaletteSearchBox = this.FindControl<TextBox>("PART_CommandPaletteSearchBox");
        _commandPaletteItems = this.FindControl<StackPanel>("PART_CommandPaletteItems");
        _nodeCanvas = this.FindControl<NodeCanvas>("PART_NodeCanvas");
        _inspectorSurface = this.FindControl<GraphInspectorView>("PART_InspectorSurface");
        _miniMapSurface = this.FindControl<GraphMiniMap>("PART_MiniMapSurface");
        if (_stencilSearchBox is not null)
        {
            AutomationProperties.SetName(_stencilSearchBox, "Stencil search");
        }

        if (_openCommandPaletteButton is not null)
        {
            AutomationProperties.SetName(_openCommandPaletteButton, "Open command palette");
        }

        if (_commandPaletteSearchBox is not null)
        {
            AutomationProperties.SetName(_commandPaletteSearchBox, "Command palette search");
        }

        if (_inspectorSurface is not null)
        {
            AutomationProperties.SetName(_inspectorSurface, "Graph inspector");
        }

        InitializeAuthoringToolControls();
        if (_stencilSearchBox is not null)
        {
            _stencilSearchBox.TextChanged += HandleStencilSearchChanged;
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

    private void HandleFragmentTemplateSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        _selectedFragmentTemplatePath = (_fragmentTemplatePicker?.SelectedItem as GraphEditorFragmentTemplateSnapshot)?.Path;
        BuildFragmentTemplateActionToolbar();
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
        BuildFragmentLibrary(projection);
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
            _stencilAddNodeDescriptor = null;
            _authoringToolSurfaceState = null;
            return;
        }

        var commandDescriptors = Editor.Session.Queries.GetCommandDescriptors();
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
            commandDescriptors,
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

        var stencilSections = stencilTemplates
            .Where(MatchesStencilFilter)
            .GroupBy(stencilItem => stencilItem.Category, StringComparer.Ordinal)
            .OrderBy(section => section.Key, StringComparer.Ordinal);

        foreach (var stencilSection in stencilSections)
        {
            _stencilCardList.Children.Add(CreateStencilSection(
                stencilSection.Key,
                stencilSection.ToList(),
                addNodeDescriptor));
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

        foreach (var action in projection.Select(HeaderCommandIds))
        {
            _headerToolbar.Children.Add(CreateActionButton(action, $"PART_HeaderCommand_{action.Id}"));
        }
    }

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

        foreach (var action in projection.Select(["composites.wrap-selection", "scopes.enter", "scopes.exit"]))
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

        foreach (var action in projection.WithShortcuts())
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

        var actions = projection.Actions
            .Where(action => !string.Equals(action.Id, CommandPaletteActionId, StringComparison.Ordinal))
            .Where(MatchesCommandPaletteFilter)
            .ToList();
        if (actions.Count == 0)
        {
            _commandPaletteItems.Children.Add(new TextBlock
            {
                Text = "No matching commands.",
            });
            return;
        }

        foreach (var action in actions)
        {
            _commandPaletteItems.Children.Add(CreateActionButton(
                action,
                $"PART_CommandPaletteAction_{action.Id}",
                includeShortcut: true,
                closePaletteOnExecute: true));
        }
    }

    private AsterGraphHostedActionProjection CreateCommandSurfaceProjection(
        IReadOnlyList<GraphEditorCommandDescriptorSnapshot> commandDescriptors,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commandDescriptorMap,
        GraphEditorSelectionSnapshot selection,
        IReadOnlyDictionary<string, GraphEditorCompositeNodeSnapshot> composites,
        GraphEditorAuthoringToolSurfaceState? authoringToolSurfaceState)
    {
        ArgumentNullException.ThrowIfNull(Editor);
        ArgumentNullException.ThrowIfNull(commandDescriptors);
        ArgumentNullException.ThrowIfNull(commandDescriptorMap);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(composites);

        var actions = AsterGraphHostedActionFactory.ApplyCommandShortcutPolicy(
            [
                CreateCommandPaletteAction(),
                .. AsterGraphHostedActionFactory.CreateCommandActions(commandDescriptors, Editor.Session),
                .. AsterGraphCompositeWorkflowActionFactory.CreateWorkflowActions(Editor.Session, commandDescriptorMap, selection, composites),
                .. authoringToolSurfaceState?.CommandSurfaceActions ?? [],
            ],
            CommandShortcutPolicy);
        return AsterGraphHostedActionFactory.CreateProjection(actions);
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
        bool closePaletteOnExecute = false)
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
        if (!string.IsNullOrWhiteSpace(actionHint))
        {
            ToolTip.SetTip(button, actionHint);
        }

        button.Click += (_, _) =>
        {
            action.TryExecute();
            if (closePaletteOnExecute)
            {
                CloseCommandPalette();
            }

            RefreshCommandSurface();
        };
        return button;
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

    private Button CreateStencilCard(
        GraphEditorNodeTemplateSnapshot stencilItem,
        GraphEditorCommandDescriptorSnapshot? addNodeDescriptor)
    {
        var button = new Button
        {
            Name = $"PART_StencilCard_{stencilItem.Key}",
            Margin = new Thickness(0, 0, 0, 12),
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
        button.Classes.Add("astergraph-template-card");
        if (!string.IsNullOrWhiteSpace(addNodeDescriptor?.DisabledReason))
        {
            ToolTip.SetTip(button, addNodeDescriptor.DisabledReason);
        }

        button.Click += (_, _) =>
        {
            Editor?.Session.Commands.AddNode(stencilItem.DefinitionId);
            RefreshCommandSurface();
        };
        return button;
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
        if (string.IsNullOrWhiteSpace(_stencilFilter))
        {
            return true;
        }

        return stencilItem.Category.Contains(_stencilFilter, StringComparison.OrdinalIgnoreCase)
            || stencilItem.Title.Contains(_stencilFilter, StringComparison.OrdinalIgnoreCase)
            || stencilItem.Subtitle.Contains(_stencilFilter, StringComparison.OrdinalIgnoreCase)
            || stencilItem.Description.Contains(_stencilFilter, StringComparison.OrdinalIgnoreCase);
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
            : "No templates";
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
