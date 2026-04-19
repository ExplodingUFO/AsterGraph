using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;
using System.Linq;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// 提供与当前 stock `NodeCanvas` 外观等价的默认节点可视展示器。
/// </summary>
public sealed class DefaultGraphNodeVisualPresenter : IGraphNodeVisualPresenter
{
    private const double MinimumNodeWidth = 180d;
    private const double InlineSurfaceMaxHeight = 132d;
    private const double MinimumNodeHeight = 158d;

    /// <inheritdoc />
    public GraphNodeVisual Create(GraphNodeVisualContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var node = context.Node;
        var nodeStyle = GetNodeCardStyle(context);
        var portAnchors = new Dictionary<string, Control>(StringComparer.Ordinal);
        var border = new Border
        {
            Width = node.Width,
            Height = node.Height,
            CornerRadius = new CornerRadius(nodeStyle.CornerRadius),
            BorderThickness = new Thickness(nodeStyle.BorderThickness),
            Focusable = true,
        };
        AutomationProperties.SetName(border, $"{node.Title} node");

        var root = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
        };

        var header = new Border
        {
            Padding = new Thickness(
                nodeStyle.HeaderHorizontalPadding,
                nodeStyle.HeaderTopPadding,
                nodeStyle.HeaderHorizontalPadding,
                nodeStyle.HeaderBottomPadding),
            CornerRadius = new CornerRadius(nodeStyle.CornerRadius, nodeStyle.CornerRadius, 0, 0),
        };

        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 10,
        };

        var headerStack = new StackPanel { Spacing = nodeStyle.HeaderSpacing };
        headerStack.Children.Add(new TextBlock
        {
            Text = node.Category.ToUpperInvariant(),
            FontSize = nodeStyle.CategoryFontSize,
            FontWeight = FontWeight.Bold,
            Foreground = BrushFactory.Solid(nodeStyle.CategoryTextHex, nodeStyle.CategoryTextOpacity),
        });
        headerStack.Children.Add(new TextBlock
        {
            Text = node.Title,
            FontSize = nodeStyle.TitleFontSize,
            FontWeight = FontWeight.SemiBold,
            Foreground = BrushFactory.Solid(nodeStyle.TitleTextHex),
        });
        var subtitle = new TextBlock
        {
            FontSize = nodeStyle.SubtitleFontSize,
            Foreground = BrushFactory.Solid(nodeStyle.SubtitleTextHex, nodeStyle.SubtitleTextOpacity),
        };
        headerStack.Children.Add(subtitle);
        headerGrid.Children.Add(headerStack);

        var badgePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            IsVisible = false,
        };
        Grid.SetColumn(badgePanel, 1);
        headerGrid.Children.Add(badgePanel);
        header.Child = headerGrid;
        root.Children.Add(header);

        var body = new Grid
        {
            Margin = new Thickness(
                nodeStyle.BodyHorizontalPadding,
                nodeStyle.BodyTopPadding,
                nodeStyle.BodyHorizontalPadding,
                nodeStyle.BodyBottomPadding),
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = nodeStyle.BodyColumnSpacing,
            RowSpacing = nodeStyle.BodyRowSpacing,
        };
        Grid.SetRow(body, 1);

        var description = new TextBlock
        {
            FontSize = nodeStyle.DescriptionFontSize,
            TextWrapping = TextWrapping.Wrap,
            Foreground = BrushFactory.Solid(nodeStyle.DescriptionTextHex, nodeStyle.DescriptionTextOpacity),
            MaxHeight = nodeStyle.DescriptionMaxHeight,
        };
        Grid.SetColumnSpan(description, 2);
        body.Children.Add(description);

        var inputs = BuildPortPanel(context, node.Inputs, isInput: true, portAnchors);
        Grid.SetRow(inputs, 1);
        body.Children.Add(inputs);

        var outputs = BuildPortPanel(context, node.Outputs, isInput: false, portAnchors);
        Grid.SetRow(outputs, 1);
        Grid.SetColumn(outputs, 1);
        body.Children.Add(outputs);

        var statusBarText = new TextBlock
        {
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        var statusBar = new Border
        {
            IsVisible = false,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(Math.Max(6, nodeStyle.CornerRadius - 12)),
            Padding = new Thickness(10, 4),
            Child = statusBarText,
        };
        Grid.SetRow(statusBar, 2);
        Grid.SetColumnSpan(statusBar, 2);
        body.Children.Add(statusBar);
        root.Children.Add(body);

        var inlineContent = new StackPanel { Spacing = 10 };
        var inlineSurface = new Border
        {
            IsVisible = false,
            Margin = new Thickness(
                nodeStyle.BodyHorizontalPadding,
                0,
                nodeStyle.BodyHorizontalPadding,
                nodeStyle.BodyBottomPadding),
            Padding = new Thickness(12),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(Math.Max(8, nodeStyle.CornerRadius - 10)),
            Child = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MaxHeight = InlineSurfaceMaxHeight,
                Content = inlineContent,
            },
        };
        Grid.SetRow(inlineSurface, 2);
        root.Children.Add(inlineSurface);

        var resizeThumb = new Thumb
        {
            Width = 16,
            Height = 16,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 10, 10),
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.SizeWestEast),
        };
        AutomationProperties.SetName(resizeThumb, $"{node.Title} resize handle");
        Grid.SetRowSpan(resizeThumb, 3);
        root.Children.Add(resizeThumb);

        border.Child = root;

        border.PointerPressed += (_, args) =>
        {
            if (args.Source is StyledElement { DataContext: PortViewModel or NodeParameterViewModel })
            {
                return;
            }

            border.Focus();
            context.BeginNodeDrag(context.Node, args);
        };
        border.KeyDown += (_, args) =>
        {
            if (args.Key == Key.Apps || (args.Key == Key.F10 && args.KeyModifiers.HasFlag(KeyModifiers.Shift)))
            {
                var menuArgs = new ContextRequestedEventArgs();
                args.Handled = context.OpenNodeContextMenu(border, context.Node, menuArgs);
            }
        };
        border.ContextRequested += (_, args) =>
        {
            args.Handled = context.OpenNodeContextMenu(border, context.Node, args);
        };

        resizeThumb.DragDelta += (_, args) =>
        {
            args.Handled = context.TrySetNodeSize(
                context.Node,
                new GraphSize(
                    Math.Max(MinimumNodeWidth, context.Node.Width + args.Vector.X),
                    Math.Max(MinimumNodeHeight, context.Node.Height + args.Vector.Y)),
                false);
        };

        return new GraphNodeVisual(
            border,
            portAnchors,
            new DefaultNodeVisualState(
                border,
                header,
                subtitle,
                description,
                badgePanel,
                statusBar,
                statusBarText,
                inlineSurface,
                inlineContent));
    }

    /// <inheritdoc />
    public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
    {
        ArgumentNullException.ThrowIfNull(visual);
        ArgumentNullException.ThrowIfNull(context);

        var state = visual.PresenterState as DefaultNodeVisualState
            ?? throw new InvalidOperationException("DefaultGraphNodeVisualPresenter requires its own visual state payload.");
        var node = context.Node;
        var nodeStyle = GetNodeCardStyle(context);

        state.Border.Width = node.Width;
        state.Border.Height = node.Height;
        state.Border.Background = BrushFactory.Solid(node.IsSelected ? nodeStyle.SelectedBackgroundHex : nodeStyle.BackgroundHex);
        state.Border.BorderBrush = BrushFactory.Solid(node.IsSelected ? node.AccentHex : nodeStyle.BorderHex);
        state.Header.Background = BrushFactory.Solid(node.AccentHex, node.IsSelected ? nodeStyle.SelectedHeaderOpacity : nodeStyle.HeaderOpacity);
        state.Border.BorderThickness = new Thickness(node.IsSelected ? nodeStyle.SelectedBorderThickness : nodeStyle.BorderThickness);
        state.Subtitle.Text = node.DisplaySubtitle;
        state.Description.Text = node.DisplayDescription;
        state.Description.IsVisible = node.ActiveSurfaceTier.ShowsSection(AsterGraph.Abstractions.Definitions.NodeSurfaceSectionKeys.Description);
        state.InlineSurface.BorderBrush = BrushFactory.Solid(node.AccentHex, 0.45);
        state.InlineSurface.Background = BrushFactory.Solid(nodeStyle.BackgroundHex, 0.92);

        state.BadgePanel.Children.Clear();
        foreach (var badge in node.Presentation.TopRightBadges)
        {
            var badgeBorder = new Border
            {
                CornerRadius = new CornerRadius(999),
                BorderThickness = new Thickness(1),
                BorderBrush = BrushFactory.SolidSafe(badge.AccentHex, nodeStyle.BorderHex, 0.8),
                Background = BrushFactory.SolidSafe(badge.AccentHex, nodeStyle.BorderHex, 0.2),
                Padding = new Thickness(7, 2),
                Child = new TextBlock
                {
                    Text = badge.Text,
                    FontSize = 10,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = BrushFactory.Solid("#FFFFFF", 0.95),
                    TextWrapping = TextWrapping.NoWrap,
                },
            };

            if (!string.IsNullOrWhiteSpace(badge.ToolTip))
            {
                ToolTip.SetTip(badgeBorder, badge.ToolTip);
            }

            state.BadgePanel.Children.Add(badgeBorder);
        }

        state.BadgePanel.IsVisible = state.BadgePanel.Children.Count > 0;

        if (node.Presentation.StatusBar is { } statusBar)
        {
            state.StatusBar.IsVisible = true;
            state.StatusBar.Background = BrushFactory.SolidSafe(statusBar.AccentHex, nodeStyle.BorderHex, 0.24);
            state.StatusBar.BorderBrush = BrushFactory.SolidSafe(statusBar.AccentHex, nodeStyle.BorderHex, 0.78);
            state.StatusBarText.Text = statusBar.Text;
            ToolTip.SetTip(state.StatusBar, statusBar.ToolTip);
        }
        else
        {
            state.StatusBar.IsVisible = false;
            state.StatusBarText.Text = string.Empty;
            ToolTip.SetTip(state.StatusBar, null);
        }

        RebuildInlineSurface(context, state);
    }

    private static void RebuildInlineSurface(GraphNodeVisualContext context, DefaultNodeVisualState state)
    {
        state.InlineContent.Children.Clear();
        var showInlineInputs = context.Node.ActiveSurfaceTier.ShowsSection(AsterGraph.Abstractions.Definitions.NodeSurfaceSectionKeys.InlineInputs);
        var showParameters = context.Node.ActiveSurfaceTier.ShowsSection(AsterGraph.Abstractions.Definitions.NodeSurfaceSectionKeys.Parameters);
        state.InlineSurface.IsVisible = showInlineInputs || showParameters;
        if (!state.InlineSurface.IsVisible)
        {
            return;
        }

        if (!ReferenceEquals(context.Editor.SelectedNode, context.Node) || context.Editor.HasMultipleSelection)
        {
            state.InlineContent.Children.Add(CreateMutedMessage("Select only this node to edit inline values."));
            return;
        }

        var boundInputKeys = context.Node.Inputs
            .Select(port => port.InlineParameterKey)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);

        var renderedAnySection = false;
        var inlineInputs = context.Node.Inputs
            .Where(port => !string.IsNullOrWhiteSpace(port.InlineParameterKey))
            .Select(port => new
            {
                Port = port,
                Parameter = context.ResolveInlineParameter(context.Node, port),
                IsConnected = context.HasIncomingConnection(context.Node, port),
            })
            .Where(item => item.Parameter is not null)
            .ToList();

        if (showInlineInputs && inlineInputs.Count > 0)
        {
            renderedAnySection = true;
            state.InlineContent.Children.Add(CreateSectionHeader("Input Values"));
            foreach (var item in inlineInputs)
            {
                state.InlineContent.Children.Add(CreateParameterCard(
                    context.NodeParameterEditorRegistry,
                    item.Parameter!,
                    context.Node.ActiveSurfaceTier.InlineEditorTemplateKey,
                    item.Port.Label,
                    item.IsConnected
                        ? "Connected input overrides the local literal."
                        : null));
            }
        }

        var generalParameters = context.Editor.SelectedNodeParameters
            .Where(parameter => !boundInputKeys.Contains(parameter.Key))
            .ToList();
        if (showParameters && generalParameters.Count > 0)
        {
            renderedAnySection = true;
            state.InlineContent.Children.Add(CreateSectionHeader("Parameters"));
            foreach (var parameter in generalParameters)
            {
                state.InlineContent.Children.Add(CreateParameterCard(
                    context.NodeParameterEditorRegistry,
                    parameter,
                    context.Node.ActiveSurfaceTier.InlineEditorTemplateKey,
                    null,
                    null));
            }
        }

        if (!renderedAnySection)
        {
            state.InlineContent.Children.Add(CreateMutedMessage("This node has no inline-editable values."));
        }
    }

    private static Control CreateParameterCard(
        INodeParameterEditorRegistry? nodeParameterEditorRegistry,
        NodeParameterViewModel parameter,
        string? templateKey,
        string? portLabel,
        string? upstreamReason)
    {
        var card = new Border
        {
            Background = BrushFactory.Solid("#0B1420", 0.24),
            BorderBrush = BrushFactory.Solid("#FFFFFF", 0.08),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10),
        };

        var stack = new StackPanel { Spacing = 6 };
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 8,
        };

        var titleStack = new StackPanel { Spacing = 2 };
        titleStack.Children.Add(new TextBlock
        {
            Text = parameter.DisplayName,
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = BrushFactory.Solid("#F7FBFF", 0.96),
        });
        if (!string.IsNullOrWhiteSpace(portLabel))
        {
            titleStack.Children.Add(new TextBlock
            {
                Text = $"Port: {portLabel}",
                FontSize = 11,
                Foreground = BrushFactory.Solid("#9FB5C8", 0.92),
            });
        }

        if (!string.IsNullOrWhiteSpace(parameter.Description))
        {
            titleStack.Children.Add(new TextBlock
            {
                Text = parameter.Description,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Foreground = BrushFactory.Solid("#BFD0DE", 0.9),
            });
        }

        header.Children.Add(titleStack);

        var resetButton = new Button
        {
            Content = "Reset",
            Padding = new Thickness(10, 4),
            MinHeight = 28,
            IsVisible = parameter.CanResetToDefault,
            IsEnabled = parameter.CanResetToDefault,
        };
        resetButton.Bind(Visual.IsVisibleProperty, new Binding(nameof(NodeParameterViewModel.CanResetToDefault)));
        resetButton.Bind(InputElement.IsEnabledProperty, new Binding(nameof(NodeParameterViewModel.CanResetToDefault)));
        resetButton.Click += (_, _) => parameter.ResetToDefault();
        Grid.SetColumn(resetButton, 1);
        header.Children.Add(resetButton);
        stack.Children.Add(header);

        if (!string.IsNullOrWhiteSpace(upstreamReason))
        {
            stack.Children.Add(new Border
            {
                Background = BrushFactory.Solid("#26445D", 0.42),
                BorderBrush = BrushFactory.Solid("#89BFF1", 0.42),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(8, 4),
                Child = new TextBlock
                {
                    Text = upstreamReason,
                    FontSize = 11,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = BrushFactory.Solid("#DCEEFF", 0.96),
                },
            });
        }
        else
        {
            stack.Children.Add(new NodeParameterEditorHost
            {
                Parameter = parameter,
                TemplateKey = templateKey,
                NodeParameterEditorRegistry = nodeParameterEditorRegistry,
                Usage = NodeParameterEditorUsage.NodeInline,
            });
        }

        var help = new TextBlock
        {
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            Foreground = BrushFactory.Solid("#A8BFCE", 0.92),
        };
        help.Bind(TextBlock.TextProperty, new Binding(nameof(NodeParameterViewModel.HelpText)));
        help.Bind(Visual.IsVisibleProperty, new Binding(nameof(NodeParameterViewModel.HasHelpText)));
        stack.Children.Add(help);

        var readOnlyReason = new TextBlock
        {
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            Foreground = BrushFactory.Solid("#F2D08E", 0.96),
        };
        readOnlyReason.Bind(TextBlock.TextProperty, new Binding(nameof(NodeParameterViewModel.ReadOnlyReason)));
        readOnlyReason.Bind(Visual.IsVisibleProperty, new Binding(nameof(NodeParameterViewModel.HasReadOnlyReason)));
        stack.Children.Add(readOnlyReason);

        var validation = new TextBlock
        {
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            Foreground = BrushFactory.Solid("#FF8F8F", 0.98),
        };
        validation.Bind(TextBlock.TextProperty, new Binding(nameof(NodeParameterViewModel.ValidationMessage)));
        validation.Bind(Visual.IsVisibleProperty, new Binding(nameof(NodeParameterViewModel.HasValidationError)));
        stack.Children.Add(validation);

        card.Child = stack;
        card.DataContext = parameter;
        return card;
    }

    private static TextBlock CreateSectionHeader(string text)
        => new()
        {
            Text = text,
            FontSize = 11,
            FontWeight = FontWeight.Bold,
            Foreground = BrushFactory.Solid("#8EA7BA", 0.95),
        };

    private static TextBlock CreateMutedMessage(string text)
        => new()
        {
            Text = text,
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            Foreground = BrushFactory.Solid("#9FB5C8", 0.92),
        };

    private static StackPanel BuildPortPanel(
        GraphNodeVisualContext context,
        IEnumerable<PortViewModel> ports,
        bool isInput,
        Dictionary<string, Control> portAnchors)
    {
        var portStyle = GetPortStyle(context, ports.FirstOrDefault());
        var panel = new StackPanel
        {
            Spacing = portStyle.RowSpacing,
            HorizontalAlignment = isInput ? HorizontalAlignment.Left : HorizontalAlignment.Right,
        };

        foreach (var port in ports)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = portStyle.RowSpacing,
                HorizontalAlignment = isInput ? HorizontalAlignment.Left : HorizontalAlignment.Right,
            };

            var dot = new Border
            {
                Width = portStyle.DotSize,
                Height = portStyle.DotSize,
                CornerRadius = new CornerRadius(999),
                Background = BrushFactory.Solid(port.AccentHex),
                VerticalAlignment = VerticalAlignment.Center,
            };

            var text = new StackPanel
            {
                Spacing = portStyle.TextSpacing,
                HorizontalAlignment = isInput ? HorizontalAlignment.Left : HorizontalAlignment.Right,
            };
            text.Children.Add(new TextBlock
            {
                Text = port.Label,
                FontSize = portStyle.LabelFontSize,
                FontWeight = FontWeight.Medium,
                Foreground = BrushFactory.Solid(portStyle.LabelHex),
                TextAlignment = isInput ? TextAlignment.Left : TextAlignment.Right,
            });
            text.Children.Add(new TextBlock
            {
                Text = port.DataType,
                FontSize = portStyle.TypeFontSize,
                Foreground = BrushFactory.Solid(portStyle.TypeHex, portStyle.TypeOpacity),
                TextAlignment = isInput ? TextAlignment.Left : TextAlignment.Right,
            });

            if (isInput)
            {
                row.Children.Add(dot);
                row.Children.Add(text);
            }
            else
            {
                row.Children.Add(text);
                row.Children.Add(dot);
            }

            var button = new Button
            {
                DataContext = port,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                HorizontalAlignment = isInput ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                HorizontalContentAlignment = isInput ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                Content = row,
            };
            AutomationProperties.SetName(
                button,
                $"{context.Node.Title} {(isInput ? "input" : "output")} {port.Label}");

            button.PointerPressed += (_, args) =>
            {
                if (args.GetCurrentPoint(button).Properties.IsLeftButtonPressed)
                {
                    args.Handled = true;
                }
            };
            button.Click += (_, _) =>
            {
                context.FocusCanvas();
                context.ActivatePort(context.Node, port);
            };
            button.ContextRequested += (_, args) =>
            {
                args.Handled = context.OpenPortContextMenu(button, context.Node, port, args);
            };

            portAnchors[port.Id] = dot;
            panel.Children.Add(button);
        }

        return panel;
    }

    private static NodeCardStyleOptions GetNodeCardStyle(GraphNodeVisualContext context)
        => context.StyleOptions.NodeOverrides.FirstOrDefault(overrideStyle => overrideStyle.DefinitionId == context.Node.DefinitionId)?.Style
           ?? context.StyleOptions.NodeCard;

    private static PortStyleOptions GetPortStyle(GraphNodeVisualContext context, PortViewModel? port)
        => port is null
            ? context.StyleOptions.Port
            : context.StyleOptions.PortOverrides.FirstOrDefault(overrideStyle => overrideStyle.TypeId == port.TypeId)?.Style
              ?? context.StyleOptions.Port;

    private sealed record DefaultNodeVisualState(
        Border Border,
        Border Header,
        TextBlock Subtitle,
        TextBlock Description,
        StackPanel BadgePanel,
        Border StatusBar,
        TextBlock StatusBarText,
        Border InlineSurface,
        StackPanel InlineContent);
}
