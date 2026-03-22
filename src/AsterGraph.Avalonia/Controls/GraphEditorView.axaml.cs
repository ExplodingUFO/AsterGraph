using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _nodeCanvas = this.FindControl<NodeCanvas>("PART_NodeCanvas");
    }

    private void HandleKeyDown(object? sender, KeyEventArgs args)
    {
        if (Editor is null)
        {
            return;
        }

        if (ShouldBypassGlobalKeyHandling(args.Source))
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

        switch (args.Key)
        {
            case Key.Delete:
                if (Editor.DeleteSelectionCommand.CanExecute(null))
                {
                    Editor.DeleteSelectionCommand.Execute(null);
                }

                args.Handled = true;
                break;
            case Key.Escape:
                if (Editor.CancelPendingConnectionCommand.CanExecute(null))
                {
                    Editor.CancelPendingConnectionCommand.Execute(null);
                }

                args.Handled = true;
                break;
        }
    }

    private bool ShouldBypassGlobalKeyHandling(object? eventSource)
    {
        if (IsWithinEditableControl(eventSource as StyledElement))
        {
            return true;
        }

        return IsWithinEditableControl(TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as StyledElement);
    }

    private void HandleEmbeddedEditorKeyDown(object? sender, KeyEventArgs args)
    {
        if (args.Key is Key.Delete or Key.Escape)
        {
            args.Handled = true;
        }
    }

    private static bool IsWithinEditableControl(StyledElement? element)
    {
        for (StyledElement? current = element; current is not null; current = current.Parent)
        {
            if (current is TextBox or ComboBox)
            {
                return true;
            }
        }

        return false;
    }
}
