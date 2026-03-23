using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Services;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// AsterGraph 的 Avalonia 宿主视图，负责样式资源接入和全局快捷键路由。
/// </summary>
public partial class GraphEditorView : UserControl
{
    /// <summary>
    /// 编辑器视图模型依赖属性。
    /// </summary>
    public static readonly StyledProperty<GraphEditorViewModel?> EditorProperty =
        AvaloniaProperty.Register<GraphEditorView, GraphEditorViewModel?>(nameof(Editor));

    private NodeCanvas? _nodeCanvas;

    /// <summary>
    /// 初始化图编辑器宿主视图。
    /// </summary>
    public GraphEditorView()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, HandleKeyDown, RoutingStrategies.Bubble);
    }

    /// <summary>
    /// 当前绑定的编辑器视图模型。
    /// </summary>
    public GraphEditorViewModel? Editor
    {
        get => GetValue(EditorProperty);
        set => SetValue(EditorProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EditorProperty)
        {
            change.GetOldValue<GraphEditorViewModel?>()?.SetTextClipboardBridge(null);
            var editor = change.GetNewValue<GraphEditorViewModel?>();
            ApplyStyleOptions(editor);
            ApplyClipboardBridge(editor);
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ApplyClipboardBridge(Editor);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Editor?.SetTextClipboardBridge(null);
        base.OnDetachedFromVisualTree(e);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _nodeCanvas = this.FindControl<NodeCanvas>("PART_NodeCanvas");
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
