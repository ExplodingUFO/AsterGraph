using System.Windows;
using System.Windows.Controls;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Wpf.Hosting;

namespace AsterGraph.Wpf.Controls;

public partial class GraphEditorView : UserControl
{
    public static readonly DependencyProperty EditorProperty = DependencyProperty.Register(
        nameof(Editor),
        typeof(GraphEditorViewModel),
        typeof(GraphEditorView),
        new PropertyMetadata(null, OnEditorChanged));

    public static readonly DependencyProperty ApplyHostServicesProperty = DependencyProperty.Register(
        nameof(ApplyHostServices),
        typeof(bool),
        typeof(GraphEditorView),
        new PropertyMetadata(true, OnApplyHostServicesChanged));

    private bool _isLoaded;

    public GraphEditorView()
    {
        InitializeComponent();
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
    }

    public GraphEditorViewModel? Editor
    {
        get => (GraphEditorViewModel?)GetValue(EditorProperty);
        set => SetValue(EditorProperty, value);
    }

    public bool ApplyHostServices
    {
        get => (bool)GetValue(ApplyHostServicesProperty);
        set => SetValue(ApplyHostServicesProperty, value);
    }

    private static void OnEditorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as GraphEditorView)?.HandleEditorChanged(
            (GraphEditorViewModel?)e.OldValue,
            (GraphEditorViewModel?)e.NewValue);

    private static void OnApplyHostServicesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as GraphEditorView)?.HandleApplyHostServicesChanged(
            (bool)e.OldValue,
            (bool)e.NewValue);

    private void HandleEditorChanged(GraphEditorViewModel? oldEditor, GraphEditorViewModel? newEditor)
    {
        if (!_isLoaded || !ApplyHostServices)
        {
            return;
        }

        WpfGraphPlatformSeamBinder.Replace(oldEditor, newEditor, this);
    }

    private void HandleApplyHostServicesChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue || !_isLoaded)
        {
            return;
        }

        if (newValue)
        {
            WpfGraphPlatformSeamBinder.Apply(Editor, this);
            return;
        }

        WpfGraphPlatformSeamBinder.Clear(Editor);
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;

        if (ApplyHostServices)
        {
            WpfGraphPlatformSeamBinder.Apply(Editor, this);
        }
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        if (!_isLoaded)
        {
            return;
        }

        if (ApplyHostServices)
        {
            WpfGraphPlatformSeamBinder.Clear(Editor);
        }

        _isLoaded = false;
    }
}
