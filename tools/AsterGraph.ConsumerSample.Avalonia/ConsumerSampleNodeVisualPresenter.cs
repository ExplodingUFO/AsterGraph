using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.ConsumerSample;

internal sealed class ConsumerSampleNodeVisualPresenter : IGraphNodeVisualPresenter
{
    private readonly DefaultGraphNodeVisualPresenter _fallback = new();

    public GraphNodeVisual Create(GraphNodeVisualContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!ConsumerSampleAuthoringSurfaceRecipe.UsesCustomNodePresentation(context.Node))
        {
            return _fallback.Create(context);
        }

        var titleText = new TextBlock
        {
            FontSize = 15,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brush.Parse("#F8FBFD"),
        };
        var subtitleText = new TextBlock
        {
            FontSize = 11,
            Foreground = Brush.Parse("#9EB7C8"),
        };
        var titleStack = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                titleText,
                subtitleText,
            },
        };

        var widenButton = CreateToolbarButton($"PART_RecipeToolbarWiden_{context.Node.Id}", "Widen");
        widenButton.Click += (_, _) =>
        {
            var nextWidth = Math.Max(context.Node.Width + 72d, 392d);
            var nextHeight = Math.Max(context.Node.Height, 236d);
            context.TrySetNodeSize(context.Node, new GraphSize(nextWidth, nextHeight), true);
        };

        var resetButton = CreateToolbarButton($"PART_RecipeToolbarReset_{context.Node.Id}", "Reset");
        resetButton.Click += (_, _) =>
            context.TrySetNodeSize(context.Node, ConsumerSampleHost.ReviewNodeDefaultSize, true);

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                widenButton,
                resetButton,
            },
        };

        var header = new Border
        {
            Name = $"PART_RecipeNodeHeader_{context.Node.Id}",
            Background = Brush.Parse("#102031"),
            Padding = new Thickness(14, 12),
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                ColumnSpacing = 12,
                Children =
                {
                    titleStack,
                    toolbar,
                },
            },
        };
        Grid.SetColumn(toolbar, 1);
        header.PointerPressed += (_, args) =>
        {
            if (args.Source is Button)
            {
                return;
            }

            context.BeginNodeDrag(context.Node, args);
        };

        var inputPanel = CreatePortPanel("Inputs");
        var bodyPanel = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 8),
        };
        var outputPanel = CreatePortPanel("Outputs");

        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            ColumnSpacing = 14,
            Margin = new Thickness(14, 12, 14, 14),
            Children =
            {
                inputPanel,
                bodyPanel,
                outputPanel,
            },
        };
        Grid.SetColumn(bodyPanel, 1);
        Grid.SetColumn(outputPanel, 2);

        var root = new Border
        {
            Background = Brush.Parse("#0C1622"),
            BorderBrush = Brush.Parse("#406379"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*"),
                Children =
                {
                    header,
                    content,
                },
            },
        };
        Grid.SetRow(content, 1);
        GraphPresentationSemantics.ApplyStockNodeSurfaceSemantics(root, $"AUTHORING RECIPE NODE:{context.Node.Title}");

        var state = new ConsumerSampleNodeVisualState(
            TitleText: titleText,
            SubtitleText: subtitleText,
            InputPanel: inputPanel,
            BodyPanel: bodyPanel,
            OutputPanel: outputPanel,
            PortAnchors: new Dictionary<string, Control>(StringComparer.Ordinal),
            ConnectionTargetAnchors: new Dictionary<GraphConnectionTargetRef, Control>());

        var visual = new GraphNodeVisual(root, state.PortAnchors, state.ConnectionTargetAnchors, state);
        Update(visual, context);
        return visual;
    }

    public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
    {
        ArgumentNullException.ThrowIfNull(visual);
        ArgumentNullException.ThrowIfNull(context);

        if (!ConsumerSampleAuthoringSurfaceRecipe.UsesCustomNodePresentation(context.Node)
            || visual.PresenterState is not ConsumerSampleNodeVisualState state)
        {
            _fallback.Update(visual, context);
            return;
        }

        var size = context.SurfacePreviewSize ?? new GraphSize(context.Node.Width, context.Node.Height);
        visual.Root.Width = size.Width;
        visual.Root.Height = size.Height;

        state.TitleText.Text = context.Node.Title;
        state.SubtitleText.Text = $"Custom review lane • {context.Node.Inputs.Count} in / {context.Node.Outputs.Count} out";

        state.InputPanel.Children.Clear();
        state.BodyPanel.Children.Clear();
        state.OutputPanel.Children.Clear();
        state.PortAnchors.Clear();
        state.ConnectionTargetAnchors.Clear();

        foreach (var port in context.Node.Inputs)
        {
            state.InputPanel.Children.Add(CreatePortButton(context, port, state.PortAnchors));
        }

        foreach (var port in context.Node.Outputs)
        {
            state.OutputPanel.Children.Add(CreatePortButton(context, port, state.PortAnchors));
        }

        state.BodyPanel.Children.Add(new TextBlock
        {
            Text = context.Node.DisplayDescription,
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brush.Parse("#C9D7E3"),
        });

        foreach (var row in context.Node.InputRows.Where(row => row.Kind != NodeSurfaceInputRowKind.Port))
        {
            state.BodyPanel.Children.Add(CreateParameterRow(context, row, state.ConnectionTargetAnchors));
        }
    }

    private static StackPanel CreatePortPanel(string heading)
        => new()
        {
            Spacing = 8,
            Width = 148,
            Children =
            {
                new TextBlock
                {
                    Text = heading,
                    FontSize = 11,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = Brush.Parse("#88A9BD"),
                },
            },
        };

    private static Button CreateToolbarButton(string name, string text)
        => new()
        {
            Name = name,
            Content = text,
            MinWidth = 72,
            Padding = new Thickness(10, 4),
            Classes = { "astergraph-toolbar-action" },
        };

    private static Control CreatePortButton(
        GraphNodeVisualContext context,
        PortViewModel port,
        Dictionary<string, Control> portAnchors)
    {
        var anchor = new Border
        {
            Width = 12,
            Height = 12,
            CornerRadius = new CornerRadius(999),
            Background = Brush.Parse(port.AccentHex),
            VerticalAlignment = VerticalAlignment.Center,
        };
        portAnchors[port.Id] = anchor;

        var text = new TextBlock
        {
            Text = port.Label,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brush.Parse("#F4F8FB"),
        };

        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            ColumnSpacing = 8,
            Children =
            {
                anchor,
                text,
            },
        };
        Grid.SetColumn(text, 1);

        var button = new Button
        {
            Name = $"PART_RecipePort_{context.Node.Id}_{port.Id}",
            Background = Brush.Parse("#111F2D"),
            BorderBrush = Brush.Parse("#2C495C"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10, 8),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Content = content,
        };
        AutomationProperties.SetName(button, $"{context.Node.Title} port {port.Label}");
        button.Click += (_, _) =>
        {
            context.FocusCanvas();
            context.ActivatePort(context.Node, port);
        };
        return button;
    }

    private static Control CreateParameterRow(
        GraphNodeVisualContext context,
        NodeSurfaceInputRowViewModel row,
        Dictionary<GraphConnectionTargetRef, Control> connectionTargetAnchors)
    {
        var endpoint = row.ParameterEndpoint!;
        var target = new Border
        {
            Width = 12,
            Height = 12,
            CornerRadius = new CornerRadius(999),
            Background = Brush.Parse("#F3B36B"),
            VerticalAlignment = VerticalAlignment.Center,
        };
        connectionTargetAnchors[endpoint.Target] = target;

        var targetButton = new Button
        {
            Name = $"PART_RecipeTarget_{context.Node.Id}_{endpoint.Parameter.Key}",
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Content = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                ColumnSpacing = 8,
                Children =
                {
                    target,
                    new TextBlock
                    {
                        Text = row.Label,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = Brush.Parse("#F4F8FB"),
                    },
                },
            },
        };
        targetButton.Click += (_, _) =>
        {
            context.FocusCanvas();
            context.ActivateConnectionTarget(context.Node, endpoint.Target);
        };

        var rowContent = new StackPanel
        {
            Spacing = 6,
            Children =
            {
                targetButton,
            },
        };

        if (row.InlineContentKind == NodeSurfaceInlineContentKind.Editor)
        {
            rowContent.Children.Add(new NodeParameterEditorHost
            {
                Parameter = endpoint.Parameter,
                NodeParameterEditorRegistry = context.NodeParameterEditorRegistry,
                TemplateKey = endpoint.Parameter.Definition.TemplateKey,
                Usage = NodeParameterEditorUsage.NodeSurface,
            });
        }
        else if (!string.IsNullOrWhiteSpace(row.InlineDisplayText))
        {
            rowContent.Children.Add(new Border
            {
                Background = Brush.Parse("#152434"),
                BorderBrush = Brush.Parse("#2C495C"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 6),
                Child = new TextBlock
                {
                    Text = row.InlineDisplayText,
                    Foreground = Brush.Parse("#EAF2F7"),
                    TextWrapping = TextWrapping.Wrap,
                },
            });
        }

        return new Border
        {
            Background = Brush.Parse("#111A27"),
            BorderBrush = Brush.Parse("#23384A"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(10),
            Child = rowContent,
        };
    }

    private sealed record ConsumerSampleNodeVisualState(
        TextBlock TitleText,
        TextBlock SubtitleText,
        StackPanel InputPanel,
        StackPanel BodyPanel,
        StackPanel OutputPanel,
        Dictionary<string, Control> PortAnchors,
        Dictionary<GraphConnectionTargetRef, Control> ConnectionTargetAnchors);
}
