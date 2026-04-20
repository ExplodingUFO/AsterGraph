using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Controls.Internal;
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
    private const double MinimumNodeHeight = 160d;
    private const double MinimumNodeChromeHeight = 180d;
    private const double AdditionalPortRowHeight = 34d;
    private const double StatusBarReserveHeight = 34d;
    private const double MinimumParameterRailWidth = 144d;
    private const double EditorParameterRailWidth = 192d;
    private const double MaximumParameterRailWidth = 228d;

    public GraphNodeVisual Create(GraphNodeVisualContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var node = context.Node;
        var renderedSize = ResolveRenderedNodeSize(context);
        var nodeStyle = GetNodeCardStyle(context);
        var portAnchors = new Dictionary<string, Control>(StringComparer.Ordinal);
        var connectionTargetAnchors = new Dictionary<GraphConnectionTargetRef, Control>();
        var border = new Border
        {
            Width = renderedSize.Width,
            Height = ResolveRenderedNodeHeight(node, renderedSize.Height, hasStatusBar: false),
            CornerRadius = new CornerRadius(nodeStyle.CornerRadius),
            BorderThickness = new Thickness(nodeStyle.BorderThickness),
            Focusable = true,
            ClipToBounds = true,
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

        var contentGrid = new Grid
        {
            Margin = new Thickness(
                nodeStyle.BodyHorizontalPadding,
                nodeStyle.BodyTopPadding,
                nodeStyle.BodyHorizontalPadding,
                nodeStyle.BodyBottomPadding),
            RowDefinitions = new RowDefinitions("*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            ColumnSpacing = nodeStyle.BodyColumnSpacing,
            RowSpacing = nodeStyle.BodyRowSpacing,
        };
        Grid.SetRow(contentGrid, 1);

        var mainBody = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = nodeStyle.BodyColumnSpacing,
            RowSpacing = nodeStyle.BodyRowSpacing,
        };
        contentGrid.Children.Add(mainBody);

        var description = new TextBlock
        {
            FontSize = nodeStyle.DescriptionFontSize,
            TextWrapping = TextWrapping.Wrap,
            Foreground = BrushFactory.Solid(nodeStyle.DescriptionTextHex, nodeStyle.DescriptionTextOpacity),
            MaxHeight = nodeStyle.DescriptionMaxHeight,
        };
        Grid.SetColumnSpan(description, 2);
        mainBody.Children.Add(description);

        var inputs = BuildPortPanel(context, node.Inputs, isInput: true, portAnchors);
        var parameterEndpointStack = new StackPanel
        {
            Spacing = 8,
        };
        var inputColumn = new StackPanel
        {
            Spacing = nodeStyle.BodyRowSpacing,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        inputColumn.Children.Add(inputs);
        inputColumn.Children.Add(parameterEndpointStack);
        Grid.SetRow(inputColumn, 1);
        mainBody.Children.Add(inputColumn);

        var outputs = BuildPortPanel(context, node.Outputs, isInput: false, portAnchors);
        Grid.SetRow(outputs, 1);
        Grid.SetColumn(outputs, 1);
        mainBody.Children.Add(outputs);

        var parameterRailStack = new StackPanel
        {
            Spacing = 10,
        };
        var parameterRail = new Border
        {
            IsVisible = false,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(Math.Max(10, nodeStyle.CornerRadius - 10)),
            Padding = new Thickness(10),
            Child = parameterRailStack,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        parameterRail.PointerPressed += (_, args) => args.Handled = true;
        AutomationProperties.SetName(parameterRail, $"{node.Title} parameter rail");
        Grid.SetColumn(parameterRail, 1);
        contentGrid.Children.Add(parameterRail);

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
        Grid.SetRow(statusBar, 1);
        contentGrid.Children.Add(statusBar);

        var widthResizeThumb = CreateResizeThumb(
            $"{node.Title} width resize handle",
            HorizontalAlignment.Right,
            VerticalAlignment.Stretch,
            width: 10,
            height: double.NaN);
        widthResizeThumb.AddHandler(
            InputElement.PointerPressedEvent,
            (_, args) => context.BeginNodeResize(context.Node, GraphNodeResizeHandleKind.Right, args),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
            handledEventsToo: true);
        Grid.SetRowSpan(widthResizeThumb, 2);
        root.Children.Add(widthResizeThumb);

        var heightResizeThumb = CreateResizeThumb(
            $"{node.Title} height resize handle",
            HorizontalAlignment.Stretch,
            VerticalAlignment.Bottom,
            width: double.NaN,
            height: 10);
        heightResizeThumb.AddHandler(
            InputElement.PointerPressedEvent,
            (_, args) => context.BeginNodeResize(context.Node, GraphNodeResizeHandleKind.Bottom, args),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
            handledEventsToo: true);
        Grid.SetRowSpan(heightResizeThumb, 2);
        root.Children.Add(heightResizeThumb);

        var resizeThumb = CreateResizeThumb(
            $"{node.Title} resize handle",
            HorizontalAlignment.Right,
            VerticalAlignment.Bottom,
            width: 16,
            height: 16,
            margin: new Thickness(0, 0, 2, 2));
        resizeThumb.AddHandler(
            InputElement.PointerPressedEvent,
            (_, args) => context.BeginNodeResize(context.Node, GraphNodeResizeHandleKind.BottomRight, args),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
            handledEventsToo: true);
        Grid.SetRowSpan(resizeThumb, 2);
        root.Children.Add(resizeThumb);

        root.Children.Add(contentGrid);
        border.Child = root;
        border.PointerPressed += (_, args) =>
        {
            if (args.Source is Thumb
                || args.Source is StyledElement { DataContext: PortViewModel }
                || args.Source is StyledElement { DataContext: NodeParameterEndpointViewModel })
            {
                return;
            }

            if (NodeCanvasResizeFeedbackResolver.TryResolveNode(border, args.GetPosition(border), out var resizeHit)
                && resizeHit.NodeHandle is GraphNodeResizeHandleKind handleKind)
            {
                context.BeginNodeResize(context.Node, handleKind, args);
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

        var state = new DefaultNodeVisualState(
            border,
            header,
            subtitle,
            description,
            badgePanel,
            contentGrid,
            parameterEndpointStack,
            parameterRail,
            parameterRailStack,
            connectionTargetAnchors,
            statusBar,
            statusBarText);

        UpdateVisualState(state, context);
        return new GraphNodeVisual(border, portAnchors, connectionTargetAnchors, state);
    }

    public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
    {
        ArgumentNullException.ThrowIfNull(visual);
        ArgumentNullException.ThrowIfNull(context);

        var state = visual.PresenterState as DefaultNodeVisualState
            ?? throw new InvalidOperationException("DefaultGraphNodeVisualPresenter requires its own visual state payload.");

        UpdateVisualState(state, context);
    }

    private static void UpdateVisualState(DefaultNodeVisualState state, GraphNodeVisualContext context)
    {
        var node = context.Node;
        var renderedSize = ResolveRenderedNodeSize(context);
        var nodeStyle = GetNodeCardStyle(context);
        var isSelected = node.IsSelected;
        var isInspected = context.InteractionFocus.IsNodeInspected(node.Id);
        var isEditing = context.InteractionFocus.IsNodeEditing(node.Id);
        var isOutputNode = IsOutputNode(node);

        state.Border.Width = renderedSize.Width;
        state.Border.Background = BrushFactory.Solid(
            isSelected || isInspected
                ? nodeStyle.SelectedBackgroundHex
                : nodeStyle.BackgroundHex,
            isEditing
                ? 0.98d
                : isOutputNode
                    ? 0.94d
                    : 1d);
        state.Border.BorderBrush = BrushFactory.Solid(
            isSelected || isInspected || isOutputNode
                ? node.AccentHex
                : nodeStyle.BorderHex,
            isEditing
                ? 1d
                : isOutputNode
                    ? 0.84d
                    : 0.98d);
        state.Header.Background = BrushFactory.Solid(
            node.AccentHex,
            isEditing
                ? 0.96d
                : isInspected
                    ? 0.82d
                    : isSelected
                        ? nodeStyle.SelectedHeaderOpacity
                        : isOutputNode
                            ? Math.Max(nodeStyle.HeaderOpacity, 0.66d)
                        : nodeStyle.HeaderOpacity);
        state.Border.BorderThickness = new Thickness(
            isEditing
                ? nodeStyle.SelectedBorderThickness + 1d
                : isInspected || isSelected
                    ? nodeStyle.SelectedBorderThickness
                    : isOutputNode
                        ? nodeStyle.BorderThickness + 0.5d
                    : nodeStyle.BorderThickness);
        SetStateClass(state.Border, "astergraph-node-selected", isSelected);
        SetStateClass(state.Border, "astergraph-node-inspected", isInspected);
        SetStateClass(state.Border, "astergraph-node-editing", isEditing);
        SetStateClass(state.Border, "astergraph-node-output", isOutputNode);
        state.Subtitle.Text = node.DisplaySubtitle;
        state.Description.Text = node.DisplayDescription;
        state.Description.IsVisible = node.ActiveSurfaceTier.ShowsSection(NodeSurfaceSectionKeys.Description);

        state.BadgePanel.Children.Clear();
        foreach (var focusBadge in CreateSystemBadges(node, isInspected, isEditing, isOutputNode))
        {
            state.BadgePanel.Children.Add(focusBadge);
        }

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

        UpdateParameterEndpointRows(state, context);
        UpdateParameterRail(state, context, nodeStyle, renderedSize.Width);
        state.Border.Height = ResolveRenderedNodeHeight(node, renderedSize.Height, state.StatusBar.IsVisible);
    }

    private static void UpdateParameterEndpointRows(DefaultNodeVisualState state, GraphNodeVisualContext context)
    {
        var node = context.Node;
        state.ConnectionTargetAnchors.Clear();
        state.ParameterEndpointStack.Children.Clear();

        foreach (var endpoint in node.ParameterEndpoints)
        {
            state.ParameterEndpointStack.Children.Add(CreateParameterEndpointButton(context, endpoint, state.ConnectionTargetAnchors));
        }
    }

    private static void UpdateParameterRail(DefaultNodeVisualState state, GraphNodeVisualContext context, NodeCardStyleOptions nodeStyle, double renderedWidth)
    {
        var node = context.Node;
        var railVisible = node.ParameterEndpoints.Count > 0 && node.ActiveSurfaceTier.ShowsSection(NodeSurfaceSectionKeys.ParameterRail);
        var editorsVisible = railVisible && node.ActiveSurfaceTier.ShowsSection(NodeSurfaceSectionKeys.ParameterEditors);

        state.ParameterRailStack.Children.Clear();
        state.ParameterRail.IsVisible = railVisible;
        state.ContentGrid.ColumnDefinitions = railVisible
            ? new ColumnDefinitions("*,Auto")
            : new ColumnDefinitions("*");
        Grid.SetColumnSpan(state.StatusBar, railVisible ? 2 : 1);

        if (!railVisible)
        {
            return;
        }

        state.ParameterRail.Width = ResolveParameterRailWidth(node, renderedWidth);
        state.ParameterRail.Background = BrushFactory.Solid("#0F1A26", 0.94);
        state.ParameterRail.BorderBrush = BrushFactory.Solid(
            node.AccentHex,
            context.InteractionFocus.IsNodeEditing(node.Id)
                ? 0.94d
                : node.IsSelected || context.InteractionFocus.IsNodeInspected(node.Id)
                    ? 0.84d
                    : 0.56d);

        foreach (var endpoint in node.ParameterEndpoints)
        {
            state.ParameterRailStack.Children.Add(CreateParameterRailSection(context, endpoint, nodeStyle, editorsVisible));
        }
    }

    private static Control CreateParameterRailSection(
        GraphNodeVisualContext context,
        NodeParameterEndpointViewModel endpoint,
        NodeCardStyleOptions nodeStyle,
        bool editorsVisible)
    {
        var header = new TextBlock
        {
            Text = endpoint.Parameter.DisplayName,
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Foreground = BrushFactory.Solid("#F1F7FA", 0.98),
        };

        if (!editorsVisible)
        {
            return new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    header,
                    new TextBlock
                    {
                        Text = endpoint.IsConnected
                            ? endpoint.ConnectedDisplayText ?? "Connected"
                            : DescribeParameterValue(endpoint.Parameter),
                        FontSize = 10,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = BrushFactory.Solid("#A5C3CE", 0.92),
                    },
                },
            };
        }

        var section = new StackPanel { Spacing = 6 };
        section.Children.Add(header);
        if (endpoint.IsConnected)
        {
            section.Children.Add(CreateConnectedParameterBinding(context, endpoint));
            return section;
        }

        var editorHost = new NodeParameterEditorHost
        {
            Parameter = endpoint.Parameter,
            NodeParameterEditorRegistry = context.NodeParameterEditorRegistry,
            Usage = NodeParameterEditorUsage.NodeSurface,
        };
        AutomationProperties.SetName(editorHost, $"{context.Node.Title} parameter editor {endpoint.Parameter.DisplayName}");
        section.Children.Add(editorHost);

        if (endpoint.Parameter.HasHelpText)
        {
            section.Children.Add(new TextBlock
            {
                Text = endpoint.Parameter.HelpText,
                FontSize = 10,
                TextWrapping = TextWrapping.Wrap,
                Foreground = BrushFactory.Solid(nodeStyle.SubtitleTextHex, 0.72),
            });
        }

        return section;
    }

    private static Button CreateParameterEndpointButton(
        GraphNodeVisualContext context,
        NodeParameterEndpointViewModel endpoint,
        Dictionary<GraphConnectionTargetRef, Control> connectionTargetAnchors)
    {
        var dot = new Border
        {
            Width = 10,
            Height = 10,
            CornerRadius = new CornerRadius(999),
            Background = BrushFactory.Solid("#F3B36B"),
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 4, 0, 0),
        };
        connectionTargetAnchors[endpoint.Target] = dot;

        var summary = endpoint.IsConnected
            ? endpoint.ConnectedDisplayText ?? "Connected"
            : DescribeParameterValue(endpoint.Parameter);
        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            ColumnSpacing = 8,
        };
        content.Children.Add(dot);

        var textStack = new StackPanel { Spacing = 2 };
        textStack.Children.Add(new TextBlock
        {
            Text = endpoint.Parameter.DisplayName,
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Foreground = BrushFactory.Solid("#F1F7FA", 0.98),
        });
        textStack.Children.Add(new TextBlock
        {
            Text = summary,
            FontSize = 10,
            TextWrapping = TextWrapping.Wrap,
            Foreground = BrushFactory.Solid("#A5C3CE", 0.92),
        });
        Grid.SetColumn(textStack, 1);
        content.Children.Add(textStack);

        var button = new Button
        {
            DataContext = endpoint,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Content = content,
        };
        AutomationProperties.SetName(button, $"{context.Node.Title} parameter endpoint {endpoint.Parameter.DisplayName}");
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
            context.ActivateConnectionTarget(context.Node, endpoint.Target);
        };
        return button;
    }

    private static Border CreateConnectedParameterBinding(GraphNodeVisualContext context, NodeParameterEndpointViewModel endpoint)
    {
        var bindingSummary = new Border
        {
            Background = BrushFactory.Solid("#152434", 0.98),
            BorderBrush = BrushFactory.Solid("#F3B36B", 0.5),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(8, 6),
            Child = new TextBlock
            {
                Text = endpoint.ConnectedDisplayText ?? "Connected",
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Foreground = BrushFactory.Solid("#F8FBFD", 0.96),
            },
        };
        AutomationProperties.SetName(bindingSummary, $"{context.Node.Title} parameter binding {endpoint.Parameter.DisplayName}");
        bindingSummary.PointerPressed += (_, args) => args.Handled = true;
        return bindingSummary;
    }

    private static string DescribeParameterValue(NodeParameterViewModel parameter)
    {
        if (parameter.HasMixedValues)
        {
            return parameter.MixedValueHint;
        }

        if (parameter.IsBoolean)
        {
            return parameter.BoolValue is true ? "Enabled" : "Disabled";
        }

        if (parameter.IsEnum)
        {
            return parameter.SelectedOption?.Label ?? parameter.StringValue;
        }

        return string.IsNullOrWhiteSpace(parameter.StringValue)
            ? parameter.TypeId.Value
            : parameter.StringValue;
    }

    private static double ResolveMinimumChromeHeight(NodeViewModel node)
    {
        var visiblePortRows = Math.Max(Math.Max(node.Inputs.Count + node.ParameterEndpoints.Count, node.Outputs.Count), 1);
        return MinimumNodeChromeHeight + (Math.Max(0, visiblePortRows - 1) * AdditionalPortRowHeight);
    }

    private static double ResolveInteractiveMinimumHeight(NodeViewModel node)
        => Math.Max(MinimumNodeHeight, ResolveMinimumChromeHeight(node));

    private static double ResolveRenderedNodeHeight(NodeViewModel node, double renderedHeight, bool hasStatusBar)
    {
        renderedHeight = Math.Max(renderedHeight, ResolveInteractiveMinimumHeight(node));
        if (hasStatusBar)
        {
            renderedHeight += StatusBarReserveHeight;
        }

        return renderedHeight;
    }

    private static double ResolveParameterRailWidth(NodeViewModel node, double renderedWidth)
    {
        var desiredWidth = node.ActiveSurfaceTier.ShowsSection(NodeSurfaceSectionKeys.ParameterEditors)
            ? EditorParameterRailWidth
            : MinimumParameterRailWidth;
        return Math.Min(
            MaximumParameterRailWidth,
            Math.Max(desiredWidth, renderedWidth * 0.32d));
    }

    private static GraphSize ResolveRenderedNodeSize(GraphNodeVisualContext context)
        => context.SurfacePreviewSize ?? new GraphSize(context.Node.Width, context.Node.Height);

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
                Text = GraphTypeCueFormatter.FormatPortToken(port),
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

    private static Thumb CreateResizeThumb(
        string automationName,
        HorizontalAlignment horizontalAlignment,
        VerticalAlignment verticalAlignment,
        double width,
        double height,
        Thickness? margin = null)
    {
        var thumb = new Thumb
        {
            Background = Brushes.Transparent,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = verticalAlignment,
            Width = width,
            Height = height,
            Margin = margin ?? default,
        };
        AutomationProperties.SetName(thumb, automationName);
        return thumb;
    }

    private static NodeCardStyleOptions GetNodeCardStyle(GraphNodeVisualContext context)
        => context.StyleOptions.NodeOverrides.FirstOrDefault(overrideStyle => overrideStyle.DefinitionId == context.Node.DefinitionId)?.Style
           ?? context.StyleOptions.NodeCard;

    private static PortStyleOptions GetPortStyle(GraphNodeVisualContext context, PortViewModel? port)
        => port is null
            ? context.StyleOptions.Port
            : context.StyleOptions.PortOverrides.FirstOrDefault(overrideStyle => overrideStyle.TypeId == port.TypeId)?.Style
              ?? context.StyleOptions.Port;

    private static bool IsOutputNode(NodeViewModel node)
        => string.Equals(node.Category, "Output", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<Border> CreateSystemBadges(
        NodeViewModel node,
        bool isInspected,
        bool isEditing,
        bool isOutputNode)
    {
        if (isOutputNode)
        {
            yield return CreateFocusBadge("Output", node.AccentHex, "Terminal output node");
        }

        if (isEditing)
        {
            yield return CreateFocusBadge("Editing", node.AccentHex, "Inspector editing focus");
        }
        else if (isInspected)
        {
            yield return CreateFocusBadge("Inspect", node.AccentHex, "Primary inspected node");
        }
    }

    private static Border CreateFocusBadge(string text, string accentHex, string toolTip)
    {
        var badge = new Border
        {
            CornerRadius = new CornerRadius(999),
            BorderThickness = new Thickness(1),
            BorderBrush = BrushFactory.SolidSafe(accentHex, "#FFFFFF", 0.88),
            Background = BrushFactory.SolidSafe(accentHex, "#FFFFFF", 0.24),
            Padding = new Thickness(7, 2),
            Child = new TextBlock
            {
                Text = text,
                FontSize = 10,
                FontWeight = FontWeight.SemiBold,
                Foreground = BrushFactory.Solid("#FFFFFF", 0.95),
                TextWrapping = TextWrapping.NoWrap,
            },
        };
        ToolTip.SetTip(badge, toolTip);
        return badge;
    }

    private static void SetStateClass(StyledElement element, string className, bool isActive)
    {
        if (isActive)
        {
            if (!element.Classes.Contains(className))
            {
                element.Classes.Add(className);
            }

            return;
        }

        element.Classes.Remove(className);
    }

    private sealed record DefaultNodeVisualState(
        Border Border,
        Border Header,
        TextBlock Subtitle,
        TextBlock Description,
        StackPanel BadgePanel,
        Grid ContentGrid,
        StackPanel ParameterEndpointStack,
        Border ParameterRail,
        StackPanel ParameterRailStack,
        Dictionary<GraphConnectionTargetRef, Control> ConnectionTargetAnchors,
        Border StatusBar,
        TextBlock StatusBarText);
}
