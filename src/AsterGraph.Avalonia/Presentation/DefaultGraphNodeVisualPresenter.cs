using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// 提供与当前 stock `NodeCanvas` 外观等价的默认节点可视展示器。
/// </summary>
public sealed class DefaultGraphNodeVisualPresenter : IGraphNodeVisualPresenter
{
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
            RowDefinitions = new RowDefinitions("Auto,*"),
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
        border.Child = root;

        border.PointerPressed += (_, args) =>
        {
            if (args.Source is StyledElement { DataContext: PortViewModel })
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
                statusBarText));
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
    }

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
        TextBlock StatusBarText);
}
