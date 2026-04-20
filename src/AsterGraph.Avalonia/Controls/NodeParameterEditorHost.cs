using Avalonia;
using Avalonia.Controls;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// Small host control that renders one parameter-editor body through a replaceable Avalonia registry.
/// </summary>
public sealed class NodeParameterEditorHost : ContentControl
{
    public static readonly StyledProperty<NodeParameterViewModel?> ParameterProperty =
        AvaloniaProperty.Register<NodeParameterEditorHost, NodeParameterViewModel?>(nameof(Parameter));

    public static readonly StyledProperty<INodeParameterEditorRegistry?> NodeParameterEditorRegistryProperty =
        AvaloniaProperty.Register<NodeParameterEditorHost, INodeParameterEditorRegistry?>(nameof(NodeParameterEditorRegistry));

    public static readonly StyledProperty<string?> TemplateKeyProperty =
        AvaloniaProperty.Register<NodeParameterEditorHost, string?>(nameof(TemplateKey));

    public static readonly StyledProperty<NodeParameterEditorUsage> UsageProperty =
        AvaloniaProperty.Register<NodeParameterEditorHost, NodeParameterEditorUsage>(nameof(Usage), NodeParameterEditorUsage.Inspector);

    public NodeParameterViewModel? Parameter
    {
        get => GetValue(ParameterProperty);
        set => SetValue(ParameterProperty, value);
    }

    public INodeParameterEditorRegistry? NodeParameterEditorRegistry
    {
        get => GetValue(NodeParameterEditorRegistryProperty);
        set => SetValue(NodeParameterEditorRegistryProperty, value);
    }

    public string? TemplateKey
    {
        get => GetValue(TemplateKeyProperty);
        set => SetValue(TemplateKeyProperty, value);
    }

    public NodeParameterEditorUsage Usage
    {
        get => GetValue(UsageProperty);
        set => SetValue(UsageProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ParameterProperty
            || change.Property == NodeParameterEditorRegistryProperty
            || change.Property == TemplateKeyProperty
            || change.Property == UsageProperty)
        {
            RebuildContent();
        }
    }

    private void RebuildContent()
    {
        if (Parameter is null)
        {
            Content = null;
            return;
        }

        var registry = NodeParameterEditorRegistry ?? DefaultNodeParameterEditorRegistry.Shared;
        Content = registry.CreateEditor(new NodeParameterEditorRequest(
            Parameter,
            TemplateKey ?? Parameter.Definition.TemplateKey,
            Usage));
    }
}
