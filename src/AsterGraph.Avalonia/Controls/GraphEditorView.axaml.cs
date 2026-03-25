using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Services;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// AsterGraph 的 Avalonia 宿主视图，负责样式资源接入和全局快捷键路由。
/// </summary>
/// <remarks>
/// Phase 1 会继续支持直接构造 <see cref="GraphEditorView"/> 并通过 <see cref="Editor"/>
/// 绑定编辑器实例，作为现有宿主的兼容立面。新的宿主组合代码应优先考虑
/// <see cref="AsterGraphAvaloniaViewFactory"/> 与 <see cref="AsterGraphAvaloniaViewOptions"/>，
/// 但当前迁移窗口不会强制要求宿主立即放弃 <c>new GraphEditorView { Editor = ... }</c>。
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

    private NodeCanvas? _nodeCanvas;
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
    /// 直接构造视图并设置 <see cref="Editor"/> 在 Phase 1 中仍然受支持；
    /// 工厂入口只是新增的规范组合路径，而不是替代现有宿主的强制重写。
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
    /// 新宿主若希望集中组合逻辑，应优先使用
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

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EditorProperty)
        {
            change.GetOldValue<GraphEditorViewModel?>()?.SetTextClipboardBridge(null);
            change.GetOldValue<GraphEditorViewModel?>()?.SetHostContext(null);
            var editor = change.GetNewValue<GraphEditorViewModel?>();
            ApplyStyleOptions(editor);
            ApplyClipboardBridge(editor);
            ApplyHostContext(editor);
        }
        else if (change.Property == ChromeModeProperty)
        {
            ApplyChromeMode(change.GetNewValue<GraphEditorViewChromeMode>());
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ApplyClipboardBridge(Editor);
        ApplyHostContext(Editor);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Editor?.SetTextClipboardBridge(null);
        Editor?.SetHostContext(null);
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
        _defaultShellRowSpacing = _shellGrid?.RowSpacing ?? 0;
        _defaultShellColumnSpacing = _shellGrid?.ColumnSpacing ?? 0;
        ApplyChromeMode(ChromeMode);
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

    private void ApplyClipboardBridge(GraphEditorViewModel? editor)
    {
        if (editor is null)
        {
            return;
        }

        // 编辑器只看到纯文本桥接口，真正的平台剪贴板访问仍由 Avalonia 层负责。
        editor.SetTextClipboardBridge(new AvaloniaTextClipboardBridge(() => TopLevel.GetTopLevel(this)?.Clipboard));
    }

    private void ApplyHostContext(GraphEditorViewModel? editor)
    {
        if (editor is null)
        {
            return;
        }

        editor.SetHostContext(new AvaloniaGraphHostContext(this, TopLevel.GetTopLevel(this)));
    }

    private void ApplyChromeMode(GraphEditorViewChromeMode chromeMode)
    {
        var showChrome = chromeMode == GraphEditorViewChromeMode.Default;

        if (_headerChrome is not null)
        {
            _headerChrome.IsVisible = showChrome;
        }

        if (_libraryChrome is not null)
        {
            _libraryChrome.IsVisible = showChrome;
        }

        if (_inspectorChrome is not null)
        {
            _inspectorChrome.IsVisible = showChrome;
        }

        if (_statusChrome is not null)
        {
            _statusChrome.IsVisible = showChrome;
        }

        if (_shellGrid is not null)
        {
            _shellGrid.RowSpacing = showChrome ? _defaultShellRowSpacing : 0;
            _shellGrid.ColumnSpacing = showChrome ? _defaultShellColumnSpacing : 0;
        }
    }

    private void HandleKeyDown(object? sender, KeyEventArgs args)
    {
        if (Editor is null || ShortcutBelongsToInputControl(args.Source))
        {
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.S)
        {
            if (Editor.SaveCommand.CanExecute(null))
            {
                Editor.SaveCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.O)
        {
            if (Editor.LoadCommand.CanExecute(null))
            {
                Editor.LoadCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control)
            && (args.Key == Key.Y || (args.Key == Key.Z && args.KeyModifiers.HasFlag(KeyModifiers.Shift))))
        {
            if (Editor.RedoCommand.CanExecute(null))
            {
                Editor.RedoCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.Z)
        {
            if (Editor.UndoCommand.CanExecute(null))
            {
                Editor.UndoCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.C)
        {
            if (Editor.CopySelectionCommand.CanExecute(null))
            {
                Editor.CopySelectionCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.V)
        {
            if (Editor.PasteCommand.CanExecute(null))
            {
                Editor.PasteCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

        if (args.Key == Key.Delete)
        {
            if (Editor.DeleteSelectionCommand.CanExecute(null))
            {
                Editor.DeleteSelectionCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

    }

    private static bool ShortcutBelongsToInputControl(object? source)
    {
        for (var current = source as Visual; current is not null; current = current.GetVisualParent())
        {
            if (current is TextBox or ComboBox)
            {
                return true;
            }
        }

        return false;
    }

}
