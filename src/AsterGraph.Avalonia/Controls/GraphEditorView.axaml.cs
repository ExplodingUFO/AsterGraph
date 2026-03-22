using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

public partial class GraphEditorView : UserControl
{
    public static readonly StyledProperty<GraphEditorViewModel?> EditorProperty =
        AvaloniaProperty.Register<GraphEditorView, GraphEditorViewModel?>(nameof(Editor));

    private NodeCanvas? _nodeCanvas;

    public GraphEditorView()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, HandleKeyDown, RoutingStrategies.Bubble);
    }

    public GraphEditorViewModel? Editor
    {
        get => GetValue(EditorProperty);
        set => SetValue(EditorProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EditorProperty)
        {
            ApplyStyleOptions(change.GetNewValue<GraphEditorViewModel?>());
        }
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
