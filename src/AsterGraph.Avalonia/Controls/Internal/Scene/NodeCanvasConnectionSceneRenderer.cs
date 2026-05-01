using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Editor.Viewport;

namespace AsterGraph.Avalonia.Controls.Internal;

internal readonly record struct NodeCanvasConnectionSceneContext(
    GraphEditorViewModel? ViewModel,
    Canvas? ConnectionLayer,
    Canvas? NodeLayer,
    Control CoordinateRoot,
    IReadOnlyDictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> NodeVisuals,
    IReadOnlyDictionary<string, GraphEditorConnectionGeometrySnapshot> ConnectionGeometries,
    GraphEditorHierarchyStateSnapshot HierarchyState,
    ViewportVisibleSceneProjection? VisibleSceneProjection,
    bool ApplyVisibleSceneBudget,
    Point? PointerScreenPosition,
    Func<ConnectionViewModel, ConnectionStyleOptions> ResolveConnectionStyle,
    Func<NodeCanvasContextMenuSnapshot> CreateContextMenuSnapshot,
    Func<ContextRequestedEventArgs, Control, GraphPoint> ResolveWorldPosition,
    Func<Control, ContextMenuContext, bool> OpenContextMenu,
    Func<NodeViewModel, GraphSize?> ResolveNodePreviewSize);

internal sealed record NodeCanvasRenderedNodeVisual(
    Control Root,
    IGraphNodeVisualPresenter Presenter,
    GraphNodeVisual Visual);

internal sealed class NodeCanvasConnectionSceneRenderer
{
    public void RenderConnections(NodeCanvasConnectionSceneContext context)
    {
        if (context.ConnectionLayer is null || context.ViewModel is null)
        {
            return;
        }

        context.ConnectionLayer.Children.Clear();

        foreach (var connection in context.ViewModel.Connections)
        {
            var hierarchyConnection = context.HierarchyState.Connections
                .FirstOrDefault(snapshot => string.Equals(snapshot.ConnectionId, connection.Id, StringComparison.Ordinal));
            if (hierarchyConnection is not null && !hierarchyConnection.IsVisibleInActiveScope)
            {
                continue;
            }

            if (!context.ConnectionGeometries.TryGetValue(connection.Id, out var geometry))
            {
                continue;
            }

            if (!IntersectsVisibleSceneBudget(context, geometry))
            {
                continue;
            }

            var sourceNode = context.ViewModel.FindNode(connection.SourceNodeId);
            if (sourceNode is null)
            {
                continue;
            }

            var sourcePort = sourceNode.GetPort(connection.SourcePortId);
            if (sourcePort is null)
            {
                continue;
            }

            var targetNode = context.ViewModel.FindNode(connection.TargetNodeId);
            if (targetNode is null)
            {
                continue;
            }

            DrawConnection(
                context,
                GetConnectionEndpointAnchor(
                    context,
                    sourceNode,
                    sourcePort,
                    geometry.Target.Position,
                    hierarchyConnection?.SourceCollapsedByGroupId),
                geometry.Route,
                geometry.RouteStyle,
                GetConnectionTargetEndpointAnchor(
                    context,
                    targetNode,
                    connection.Target,
                    geometry.Source.Position,
                    hierarchyConnection?.TargetCollapsedByGroupId),
                connection,
                sourcePort);
        }

        if (context.ViewModel.HasPendingConnection
            && context.ViewModel.PendingSourceNode is not null
            && context.ViewModel.PendingSourcePort is not null
            && context.PointerScreenPosition is not null)
        {
            var source = GetPortAnchor(context, context.ViewModel.PendingSourceNode, context.ViewModel.PendingSourcePort);
            var end = context.ViewModel.ScreenToWorld(
                new GraphPoint(
                    context.PointerScreenPosition.Value.X,
                    context.PointerScreenPosition.Value.Y));

            DrawConnection(
                context,
                source,
                GraphConnectionRoute.Empty,
                GraphEditorConnectionRouteStyle.Bezier,
                end,
                new ConnectionViewModel(
                    "pending",
                    context.ViewModel.PendingSourceNode.Id,
                    context.ViewModel.PendingSourcePort.Id,
                    string.Empty,
                    string.Empty,
                    "pending",
                    context.ViewModel.PendingSourcePort.AccentHex),
                context.ViewModel.PendingSourcePort,
                isPreview: true);
        }
    }

    public GraphPoint GetPortAnchor(NodeCanvasConnectionSceneContext context, NodeViewModel node, PortViewModel port)
    {
        var previewSize = context.ResolveNodePreviewSize(node);

        // 吸附拖动时，节点外层 Canvas 的绝对布局可能还没完成新一轮 Arrange。
        // 因此这里优先读取“端口圆点在节点卡内部的局部坐标”，再叠加当前节点的 X/Y，
        // 这样连线能跟随最新的节点位置，而不会因为布局时序晚一拍出现偏移。
        if (context.NodeVisuals.TryGetValue(node, out var visual)
            && previewSize is GraphSize resolvedPreviewSize
            && ShouldPreferPreviewAnchor(visual.Root, resolvedPreviewSize))
        {
            return ResolvePreviewPortAnchor(node, port, resolvedPreviewSize);
        }

        if (context.NodeVisuals.TryGetValue(node, out visual)
            && visual.Visual.PortAnchors.TryGetValue(port.Id, out var anchorDot))
        {
            var center = new Point(anchorDot.Bounds.Width / 2, anchorDot.Bounds.Height / 2);
            var localToNode = anchorDot.TranslatePoint(center, visual.Root);
            if (localToNode is not null)
            {
                return new GraphPoint(
                    node.X + localToNode.Value.X,
                    node.Y + localToNode.Value.Y);
            }

            if (context.NodeLayer is not null)
            {
                var translated = anchorDot.TranslatePoint(center, context.NodeLayer);
                if (translated is not null)
                {
                    return new GraphPoint(translated.Value.X, translated.Value.Y);
                }
            }
        }

        if (previewSize is GraphSize fallbackPreviewSize)
        {
            return ResolvePreviewPortAnchor(node, port, fallbackPreviewSize);
        }

        return node.GetPortAnchor(port);
    }

    private GraphPoint GetConnectionEndpointAnchor(
        NodeCanvasConnectionSceneContext context,
        NodeViewModel node,
        PortViewModel port,
        GraphPoint projectedEndpoint,
        string? collapsedByGroupId)
        => ResolveCollapsedGroupBoundaryAnchor(context, collapsedByGroupId, projectedEndpoint)
           ?? GetPortAnchor(context, node, port);

    private GraphPoint GetConnectionTargetEndpointAnchor(
        NodeCanvasConnectionSceneContext context,
        NodeViewModel node,
        GraphConnectionTargetRef target,
        GraphPoint projectedEndpoint,
        string? collapsedByGroupId)
        => ResolveCollapsedGroupBoundaryAnchor(context, collapsedByGroupId, projectedEndpoint)
           ?? GetConnectionTargetAnchor(context, node, target);

    public GraphPoint GetConnectionTargetAnchor(NodeCanvasConnectionSceneContext context, NodeViewModel node, GraphConnectionTargetRef target)
    {
        if (target.Kind == GraphConnectionTargetKind.Port)
        {
            var port = node.GetPort(target.TargetId);
            if (port is not null)
            {
                return GetPortAnchor(context, node, port);
            }
        }

        if (context.NodeVisuals.TryGetValue(node, out var visual)
            && visual.Visual.ConnectionTargetAnchors.TryGetValue(target, out var anchorControl))
        {
            var center = new Point(anchorControl.Bounds.Width / 2, anchorControl.Bounds.Height / 2);
            var localToNode = anchorControl.TranslatePoint(center, visual.Root);
            if (localToNode is not null)
            {
                return new GraphPoint(
                    node.X + localToNode.Value.X,
                    node.Y + localToNode.Value.Y);
            }

            if (context.NodeLayer is not null)
            {
                var translated = anchorControl.TranslatePoint(center, context.NodeLayer);
                if (translated is not null)
                {
                    return new GraphPoint(translated.Value.X, translated.Value.Y);
                }
            }
        }

        var renderedWidth = context.ResolveNodePreviewSize(node)?.Width ?? node.Width;
        return new GraphPoint(
            node.X + Math.Max(24d, renderedWidth - 18d),
            node.Y + 56d);
    }

    private static GraphPoint ResolvePreviewPortAnchor(NodeViewModel node, PortViewModel port, GraphSize previewSize)
        => PortAnchorCalculator.GetAnchor(
            new NodeBounds(node.X, node.Y, previewSize.Width, previewSize.Height),
            port.Direction,
            port.Index,
            port.Total);

    private static GraphPoint? ResolveCollapsedGroupBoundaryAnchor(
        NodeCanvasConnectionSceneContext context,
        string? groupId,
        GraphPoint projectedEndpoint)
    {
        if (string.IsNullOrWhiteSpace(groupId))
        {
            return null;
        }

        var group = context.HierarchyState.NodeGroups
            .FirstOrDefault(candidate => string.Equals(candidate.Id, groupId, StringComparison.Ordinal));
        if (group is null)
        {
            return null;
        }

        var left = group.Position.X;
        var top = group.Position.Y;
        var width = NodeCanvasGroupChromeMetrics.ResolveRenderedWidth(group);
        var height = NodeCanvasGroupChromeMetrics.ResolveRenderedHeight(group);
        var right = left + width;
        var bottom = top + height;
        var centerX = left + (width / 2d);
        var centerY = top + (height / 2d);
        var deltaX = projectedEndpoint.X - centerX;
        var deltaY = projectedEndpoint.Y - centerY;

        if (Math.Abs(deltaX) * height > Math.Abs(deltaY) * width)
        {
            var x = deltaX >= 0d ? right : left;
            var y = centerY + (deltaY * (width / 2d) / Math.Max(Math.Abs(deltaX), 0.001d));
            return new GraphPoint(x, Math.Clamp(y, top, bottom));
        }

        var boundaryY = deltaY >= 0d ? bottom : top;
        var boundaryX = centerX + (deltaX * (height / 2d) / Math.Max(Math.Abs(deltaY), 0.001d));
        return new GraphPoint(Math.Clamp(boundaryX, left, right), boundaryY);
    }

    private static bool ShouldPreferPreviewAnchor(Control root, GraphSize previewSize)
        => Math.Abs(root.Bounds.Width - previewSize.Width) > 0.5d
           || Math.Abs(root.Bounds.Height - previewSize.Height) > 0.5d;

    private static bool IntersectsVisibleSceneBudget(
        NodeCanvasConnectionSceneContext context,
        GraphEditorConnectionGeometrySnapshot geometry)
    {
        if (!context.ApplyVisibleSceneBudget || context.VisibleSceneProjection is not ViewportVisibleSceneProjection projection)
        {
            return true;
        }

        var bounds = ResolveRouteBounds(geometry.Source.Position, geometry.Route, geometry.RouteStyle, geometry.Target.Position);
        return bounds.Left <= projection.WorldBottomRight.X
               && bounds.Right >= projection.WorldTopLeft.X
               && bounds.Top <= projection.WorldBottomRight.Y
               && bounds.Bottom >= projection.WorldTopLeft.Y;
    }

    private static RouteBounds ResolveRouteBounds(
        GraphPoint start,
        GraphConnectionRoute route,
        GraphEditorConnectionRouteStyle routeStyle,
        GraphPoint end)
    {
        var minX = Math.Min(start.X, end.X);
        var minY = Math.Min(start.Y, end.Y);
        var maxX = Math.Max(start.X, end.X);
        var maxY = Math.Max(start.Y, end.Y);

        foreach (var segment in ConnectionPathBuilder.BuildRoute(start, route, end, routeStyle))
        {
            Include(segment.Control1);
            Include(segment.Control2);
            Include(segment.End);
        }

        return new RouteBounds(minX, minY, maxX, maxY);

        void Include(GraphPoint point)
        {
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);
        }
    }

    private readonly record struct RouteBounds(double Left, double Top, double Right, double Bottom);

    private void DrawConnection(
        NodeCanvasConnectionSceneContext context,
        GraphPoint start,
        GraphConnectionRoute route,
        GraphEditorConnectionRouteStyle routeStyle,
        GraphPoint end,
        ConnectionViewModel connection,
        PortViewModel? sourcePort = null,
        bool isPreview = false)
    {
        if (context.ConnectionLayer is null || context.ViewModel is null)
        {
            return;
        }

        var connectionStyle = context.ResolveConnectionStyle(connection);
        var focusKind = context.ViewModel.InteractionFocus.GetConnectionFocusKind(connection);
        var hasInspectionFocus = context.ViewModel.InteractionFocus.HasInspection;
        var segments = ConnectionPathBuilder.BuildRoute(start, route, end, routeStyle);
        var path = new global::Avalonia.Controls.Shapes.Path
        {
            Data = CreateRouteGeometry(start, segments),
            Stroke = BrushFactory.Solid(
                connection.AccentHex,
                isPreview
                    ? connectionStyle.PreviewStrokeOpacity
                    : ResolveStrokeOpacity(connectionStyle, focusKind, hasInspectionFocus)),
            StrokeThickness = isPreview
                ? connectionStyle.PreviewThickness
                : ResolveStrokeThickness(connectionStyle, focusKind),
            StrokeLineCap = PenLineCap.Round,
        };
        if (!isPreview)
        {
            path.PointerPressed += (_, args) =>
            {
                if (!args.GetCurrentPoint(context.CoordinateRoot).Properties.IsLeftButtonPressed)
                {
                    return;
                }

                args.Handled = TrySelectConnection(context, connection.Id);
            };
        }

        context.ConnectionLayer.Children.Add(path);

        if (isPreview)
        {
            return;
        }

        var labelAnchor = ConnectionPathBuilder.ResolveSegmentMidpoint(start, route, end, route.Vertices.Count / 2);
        var midpoint = new Point(labelAnchor.X, labelAnchor.Y);
        var typeToken = sourcePort is null ? string.Empty : GraphTypeCueFormatter.FormatPortToken(sourcePort);
        var displayText = GetDisplayedChipText(connection, connection.NoteText, typeToken, focusKind);
        var edgeHelpText = context.ViewModel.NodeDocumentationProvider
            .GetEdgeDocumentation(connection.ToModel(), context.ViewModel.Session.Queries.CreateDocumentSnapshot())
            ?.HelpText;

        var chip = new Border
        {
            Background = BrushFactory.Solid(
                connectionStyle.LabelBackgroundHex,
                ResolveLabelBackgroundOpacity(connectionStyle, focusKind, hasInspectionFocus)),
            BorderBrush = BrushFactory.Solid(
                connection.AccentHex,
                ResolveLabelBorderOpacity(connectionStyle, focusKind, hasInspectionFocus)),
            BorderThickness = new Thickness(connectionStyle.LabelBorderThickness),
            CornerRadius = new CornerRadius(connectionStyle.LabelCornerRadius),
            Padding = new Thickness(connectionStyle.LabelHorizontalPadding, connectionStyle.LabelVerticalPadding),
            Focusable = true,
            Child = CreateLabelBlock(connectionStyle, displayText),
        };
        AutomationProperties.SetName(chip, $"{displayText} connection");
        SetConnectionHelp(chip, displayText, connection.Label, edgeHelpText);
        chip.KeyDown += (_, args) =>
        {
            if (string.IsNullOrWhiteSpace(connection.TargetNodeId))
            {
                return;
            }

            if (args.Key == Key.Apps || (args.Key == Key.F10 && args.KeyModifiers.HasFlag(KeyModifiers.Shift)))
            {
                args.Handled = context.OpenContextMenu(
                    chip,
                    NodeCanvasContextMenuContextFactory.CreateConnectionContext(
                        context.CreateContextMenuSnapshot(),
                        new GraphPoint(0, 0),
                        connection.Id,
                        hostContext: context.ViewModel.HostContext));
            }
        };
        chip.ContextRequested += (_, args) =>
        {
            if (string.IsNullOrWhiteSpace(connection.TargetNodeId))
            {
                return;
            }

            args.Handled = context.OpenContextMenu(
                chip,
                NodeCanvasContextMenuContextFactory.CreateConnectionContext(
                    context.CreateContextMenuSnapshot(),
                    context.ResolveWorldPosition(args, context.CoordinateRoot),
                    connection.Id,
                    hostContext: context.ViewModel.HostContext));
        };
        chip.DoubleTapped += (_, args) =>
        {
            if (chip.Child is TextBox || context.ViewModel is null)
            {
                return;
            }

            var editor = new TextBox
            {
                Text = connection.NoteText ?? string.Empty,
                MinWidth = 160,
            };
            var finalized = false;

            void RestoreLabel(string? noteText)
            {
                var restoredText = GetDisplayedChipText(connection, noteText, typeToken, focusKind);
                chip.Child = CreateLabelBlock(connectionStyle, restoredText);
                AutomationProperties.SetName(chip, $"{restoredText} connection");
                SetConnectionHelp(chip, restoredText, string.IsNullOrWhiteSpace(noteText) ? connection.Label : null, edgeHelpText);
            }

            void Complete(bool commit)
            {
                if (finalized)
                {
                    return;
                }

                finalized = true;
                if (commit)
                {
                    context.ViewModel.TrySetConnectionNoteText(connection.Id, editor.Text, updateStatus: true);
                    RestoreLabel(editor.Text);
                    return;
                }

                RestoreLabel(connection.NoteText);
            }

            editor.KeyDown += (_, keyArgs) =>
            {
                switch (keyArgs.Key)
                {
                    case Key.Enter:
                        keyArgs.Handled = true;
                        Complete(commit: true);
                        break;
                    case Key.Escape:
                        keyArgs.Handled = true;
                        Complete(commit: false);
                        break;
                }
            };
            editor.LostFocus += (_, _) => Complete(commit: true);

            chip.Child = editor;
            editor.Focus();
            args.Handled = true;
        };

        Canvas.SetLeft(chip, midpoint.X + connectionStyle.LabelOffsetX);
        Canvas.SetTop(chip, midpoint.Y + connectionStyle.LabelOffsetY);
        context.ConnectionLayer.Children.Add(chip);
    }

    private static Geometry CreateRouteGeometry(GraphPoint start, IReadOnlyList<BezierConnection> segments)
    {
        var commands = string.Join(
            " ",
            segments.Select(segment =>
                $"C {segment.Control1.X:0.##},{segment.Control1.Y:0.##} " +
                $"{segment.Control2.X:0.##},{segment.Control2.Y:0.##} " +
                $"{segment.End.X:0.##},{segment.End.Y:0.##}"));
        return Geometry.Parse($"M {start.X:0.##},{start.Y:0.##} {commands}");
    }

    private static bool TrySelectConnection(NodeCanvasConnectionSceneContext context, string connectionId)
    {
        if (context.ViewModel is null || string.IsNullOrWhiteSpace(connectionId))
        {
            return false;
        }

        return context.ViewModel.Session.Commands.TryExecuteCommand(
            new GraphEditorCommandInvocationSnapshot(
                "selection.connections.set",
                [
                    new GraphEditorCommandArgumentSnapshot("connectionId", connectionId),
                    new GraphEditorCommandArgumentSnapshot("primaryConnectionId", connectionId),
                ]));
    }

    private static double ResolveStrokeOpacity(
        ConnectionStyleOptions connectionStyle,
        GraphEditorConnectionFocusKind focusKind,
        bool hasInspectionFocus)
        => focusKind switch
        {
            GraphEditorConnectionFocusKind.Editing => Math.Max(connectionStyle.StrokeOpacity, 0.98d),
            GraphEditorConnectionFocusKind.Inspected => Math.Max(connectionStyle.StrokeOpacity, 0.86d),
            _ when hasInspectionFocus => Math.Min(connectionStyle.StrokeOpacity, 0.24d),
            _ => connectionStyle.StrokeOpacity,
        };

    private static double ResolveStrokeThickness(
        ConnectionStyleOptions connectionStyle,
        GraphEditorConnectionFocusKind focusKind)
        => focusKind switch
        {
            GraphEditorConnectionFocusKind.Editing => connectionStyle.Thickness + 1.5d,
            GraphEditorConnectionFocusKind.Inspected => connectionStyle.Thickness + 0.75d,
            _ => connectionStyle.Thickness,
        };

    private static double ResolveLabelBackgroundOpacity(
        ConnectionStyleOptions connectionStyle,
        GraphEditorConnectionFocusKind focusKind,
        bool hasInspectionFocus)
        => focusKind switch
        {
            GraphEditorConnectionFocusKind.Editing => Math.Max(connectionStyle.LabelBackgroundOpacity, 0.94d),
            GraphEditorConnectionFocusKind.Inspected => Math.Max(connectionStyle.LabelBackgroundOpacity, 0.84d),
            _ when hasInspectionFocus => Math.Min(connectionStyle.LabelBackgroundOpacity, 0.22d),
            _ => connectionStyle.LabelBackgroundOpacity,
        };

    private static double ResolveLabelBorderOpacity(
        ConnectionStyleOptions connectionStyle,
        GraphEditorConnectionFocusKind focusKind,
        bool hasInspectionFocus)
        => focusKind switch
        {
            GraphEditorConnectionFocusKind.Editing => Math.Max(connectionStyle.LabelBorderOpacity, 0.98d),
            GraphEditorConnectionFocusKind.Inspected => Math.Max(connectionStyle.LabelBorderOpacity, 0.88d),
            _ when hasInspectionFocus => Math.Min(connectionStyle.LabelBorderOpacity, 0.24d),
            _ => connectionStyle.LabelBorderOpacity,
        };

    private static TextBlock CreateLabelBlock(ConnectionStyleOptions connectionStyle, string text)
        => new()
        {
            Text = text,
            FontSize = connectionStyle.LabelFontSize,
            Foreground = BrushFactory.Solid(connectionStyle.LabelForegroundHex, connectionStyle.LabelForegroundOpacity),
        };

    private static string GetDisplayedChipText(
        ConnectionViewModel connection,
        string? noteText,
        string typeToken,
        GraphEditorConnectionFocusKind focusKind)
    {
        if (!string.IsNullOrWhiteSpace(noteText))
        {
            return noteText.Trim();
        }

        if (string.IsNullOrWhiteSpace(typeToken))
        {
            return connection.Label;
        }

        return focusKind == GraphEditorConnectionFocusKind.None || string.IsNullOrWhiteSpace(connection.Label)
            ? typeToken
            : $"{typeToken} · {connection.Label}";
    }

    private static void SetConnectionHelp(Control chip, string displayText, string? label, string? edgeHelpText)
    {
        var labelHelp = !string.IsNullOrWhiteSpace(label) && !string.Equals(displayText, label, StringComparison.Ordinal)
            ? label
            : null;
        var helpText = JoinHelpText(edgeHelpText, labelHelp);
        ToolTip.SetTip(chip, helpText);
        AutomationProperties.SetHelpText(chip, helpText);
    }

    private static string? JoinHelpText(params string?[] segments)
    {
        var normalized = segments
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .Select(segment => segment!.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return normalized.Count == 0
            ? null
            : string.Join("  ·  ", normalized);
    }
}
