using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Avalonia.Styling;
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
public partial class GraphEditorView : UserControl
{
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
    private double _defaultShellRowSpacing;
    private double _defaultShellColumnSpacing;

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
            GraphEditorPlatformSeamBinder.Replace(
                change.GetOldValue<GraphEditorViewModel?>(),
                change.GetNewValue<GraphEditorViewModel?>(),
                this);
            var editor = change.GetNewValue<GraphEditorViewModel?>();
            ApplyStyleOptions(editor);
            ApplyPresentationOptions(Presentation);
        }
        else if (change.Property == ChromeModeProperty
            || change.Property == IsHeaderChromeVisibleProperty
            || change.Property == IsLibraryChromeVisibleProperty
            || change.Property == IsInspectorChromeVisibleProperty
            || change.Property == IsStatusChromeVisibleProperty)
        {
            ApplyChromeMode(ChromeMode);
        }
        else if (change.Property == EnableDefaultContextMenuProperty
            || change.Property == EnableDefaultCommandShortcutsProperty
            || change.Property == EnableDefaultWheelViewportGesturesProperty
            || change.Property == EnableAltLeftDragPanningProperty)
        {
            ApplyCanvasBehaviorOptions();
        }
        else if (change.Property == PresentationProperty)
        {
            ApplyPresentationOptions(change.GetNewValue<AsterGraphPresentationOptions?>());
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        GraphEditorPlatformSeamBinder.Apply(Editor, this);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
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
        _nodeCanvas = this.FindControl<NodeCanvas>("PART_NodeCanvas");
        _inspectorSurface = this.FindControl<GraphInspectorView>("PART_InspectorSurface");
        _miniMapSurface = this.FindControl<GraphMiniMap>("PART_MiniMapSurface");
        if (_nodeCanvas is not null)
        {
            _nodeCanvas.AttachPlatformSeams = false;
        }
        _defaultShellRowSpacing = _shellGrid?.RowSpacing ?? 0;
        _defaultShellColumnSpacing = _shellGrid?.ColumnSpacing ?? 0;
        ApplyChromeMode(ChromeMode);
        ApplyCanvasBehaviorOptions();
        ApplyPresentationOptions(Presentation);
    }

    private void ApplyStyleOptions(GraphEditorViewModel? editor)
    {
        if (editor?.StyleOptions is null)
        {
            return;
        }

        var adapter = new GraphEditorStyleAdapter(editor.StyleOptions);
        adapter.ApplyResources(Resources);
    }

    private void ApplyChromeMode(GraphEditorViewChromeMode chromeMode)
    {
        var showChrome = chromeMode == GraphEditorViewChromeMode.Default;
        var showHeader = showChrome && IsHeaderChromeVisible;
        var showLibrary = showChrome && IsLibraryChromeVisible;
        var showInspector = showChrome && IsInspectorChromeVisible;
        var showStatus = showChrome && IsStatusChromeVisible;

        if (_headerChrome is not null)
        {
            _headerChrome.IsVisible = showHeader;
        }

        if (_libraryChrome is not null)
        {
            _libraryChrome.IsVisible = showLibrary;
        }

        if (_inspectorChrome is not null)
        {
            _inspectorChrome.IsVisible = showInspector;
        }

        if (_statusChrome is not null)
        {
            _statusChrome.IsVisible = showStatus;
        }

        if (_shellGrid is not null)
        {
            _shellGrid.RowSpacing = showHeader || showStatus ? _defaultShellRowSpacing : 0;
            _shellGrid.ColumnSpacing = showLibrary || showInspector ? _defaultShellColumnSpacing : 0;
        }
    }

    private void ApplyCanvasBehaviorOptions()
    {
        if (_nodeCanvas is null)
        {
            return;
        }

        _nodeCanvas.EnableDefaultContextMenu = EnableDefaultContextMenu;
        _nodeCanvas.EnableDefaultCommandShortcuts = EnableDefaultCommandShortcuts;
        _nodeCanvas.EnableDefaultWheelViewportGestures = EnableDefaultWheelViewportGestures;
        _nodeCanvas.EnableAltLeftDragPanning = EnableAltLeftDragPanning;
    }

    private void ApplyPresentationOptions(AsterGraphPresentationOptions? presentation)
    {
        if (_nodeCanvas is not null)
        {
            _nodeCanvas.NodeVisualPresenter = presentation?.NodeVisualPresenter;
            _nodeCanvas.ContextMenuPresenter = presentation?.ContextMenuPresenter;
        }

        if (_inspectorSurface is not null)
        {
            _inspectorSurface.InspectorPresenter = presentation?.InspectorPresenter;
        }

        if (_miniMapSurface is not null)
        {
            _miniMapSurface.MiniMapPresenter = presentation?.MiniMapPresenter;
        }
    }

    private void HandleKeyDown(object? sender, KeyEventArgs args)
    {
        if (!EnableDefaultCommandShortcuts)
        {
            return;
        }

        if (GraphEditorDefaultCommandShortcutRouter.TryHandle(
            Editor,
            args.Source,
            args,
            includePendingConnectionCancel: false))
        {
            args.Handled = true;
        }
    }

}
