using System.Linq;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
    /// 是否启用完整外壳的默认内置命令快捷键。
    /// </summary>
    public static readonly StyledProperty<bool> EnableDefaultCommandShortcutsProperty =
        AvaloniaProperty.Register<GraphEditorView, bool>(nameof(EnableDefaultCommandShortcuts), true);

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
    private WrapPanel? _headerToolbar;
    private WrapPanel? _compositeWorkflowToolbar;
    private WrapPanel? _scopeBreadcrumbs;
    private StackPanel? _shortcutHelpList;
    private Button? _openCommandPaletteButton;
    private Border? _commandPaletteChrome;
    private TextBox? _commandPaletteSearchBox;
    private StackPanel? _commandPaletteItems;
    private double _defaultShellRowSpacing;
    private double _defaultShellColumnSpacing;
    private readonly GraphEditorViewCompositionCoordinator _compositionCoordinator;
    private string _commandPaletteFilter = string.Empty;
    private AsterGraphHostedActionDescriptor? _commandPaletteAction;

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
    /// 是否启用完整外壳的默认内置命令快捷键。
    /// </summary>
    public bool EnableDefaultCommandShortcuts
    {
        get => GetValue(EnableDefaultCommandShortcutsProperty);
        set => SetValue(EnableDefaultCommandShortcutsProperty, value);
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
            || change.Property == EnableDefaultCommandShortcutsProperty
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
        _shellGrid = this.FindControl<Grid>("PART_ShellGrid");
        _headerChrome = this.FindControl<Border>("PART_HeaderChrome");
        _libraryChrome = this.FindControl<Border>("PART_LibraryChrome");
        _inspectorChrome = this.FindControl<Border>("PART_InspectorChrome");
        _statusChrome = this.FindControl<Border>("PART_StatusChrome");
        _headerToolbar = this.FindControl<WrapPanel>("PART_HeaderToolbar");
        _compositeWorkflowToolbar = this.FindControl<WrapPanel>("PART_CompositeWorkflowToolbar");
        _scopeBreadcrumbs = this.FindControl<WrapPanel>("PART_ScopeBreadcrumbs");
        _shortcutHelpList = this.FindControl<StackPanel>("PART_ShortcutHelpList");
        _openCommandPaletteButton = this.FindControl<Button>("PART_OpenCommandPaletteButton");
        _commandPaletteChrome = this.FindControl<Border>("PART_CommandPaletteChrome");
        _commandPaletteSearchBox = this.FindControl<TextBox>("PART_CommandPaletteSearchBox");
        _commandPaletteItems = this.FindControl<StackPanel>("PART_CommandPaletteItems");
        _nodeCanvas = this.FindControl<NodeCanvas>("PART_NodeCanvas");
        _inspectorSurface = this.FindControl<GraphInspectorView>("PART_InspectorSurface");
        _miniMapSurface = this.FindControl<GraphMiniMap>("PART_MiniMapSurface");
        if (_openCommandPaletteButton is not null)
        {
            _openCommandPaletteButton.Click += HandleOpenCommandPaletteClick;
        }

        if (_commandPaletteSearchBox is not null)
        {
            _commandPaletteSearchBox.TextChanged += HandleCommandPaletteSearchChanged;
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

        if (_commandPaletteAction is not null
            && GraphEditorDefaultCommandShortcutRouter.TryHandle(
                [_commandPaletteAction],
                args.Source,
                args,
                allowInputControlFocus: true))
        {
            args.Handled = true;
            return;
        }

        if (!EnableDefaultCommandShortcuts)
        {
            return;
        }

        var projection = CreateCommandSurfaceProjection();
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
        _commandPaletteAction?.TryExecute();
        RefreshCommandSurface();
        args.Handled = true;
    }

    private void HandleCommandPaletteSearchChanged(object? sender, TextChangedEventArgs args)
    {
        _commandPaletteFilter = _commandPaletteSearchBox?.Text?.Trim() ?? string.Empty;
        BuildCommandPaletteItems(CreateCommandSurfaceProjection());
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
        var projection = CreateCommandSurfaceProjection();
        RefreshCommandPaletteButton(projection);
        BuildHeaderToolbar(projection);
        BuildCompositeWorkflowToolbar(projection);
        BuildScopeBreadcrumbs();
        BuildShortcutHelp(projection);
        BuildCommandPaletteItems(projection);
        if (Editor is null)
        {
            CloseCommandPalette();
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

    private AsterGraphHostedActionProjection? CreateCommandSurfaceProjection()
    {
        if (Editor is null)
        {
            return null;
        }

        return AsterGraphHostedActionFactory.CreateProjection(
        [
            CreateCommandPaletteAction(),
            .. AsterGraphHostedActionFactory.CreateCommandActions(Editor.Session),
            .. AsterGraphCompositeWorkflowActionFactory.CreateWorkflowActions(Editor.Session),
        ]);
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
        if (!string.IsNullOrWhiteSpace(action.DisabledReason))
        {
            ToolTip.SetTip(button, action.DisabledReason);
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

        _commandPaletteFilter = string.Empty;
        if (_commandPaletteSearchBox is not null)
        {
            _commandPaletteSearchBox.Text = string.Empty;
        }

        _commandPaletteChrome.IsVisible = true;
        BuildCommandPaletteItems(CreateCommandSurfaceProjection());
        _commandPaletteSearchBox?.Focus();
    }

    private void CloseCommandPalette()
    {
        if (_commandPaletteChrome is null)
        {
            return;
        }

        _commandPaletteChrome.IsVisible = false;
    }
}
