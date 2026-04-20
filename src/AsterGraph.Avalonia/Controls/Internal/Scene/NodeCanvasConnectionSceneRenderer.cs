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
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal readonly record struct NodeCanvasConnectionSceneContext(
    GraphEditorViewModel? ViewModel,
    Canvas? ConnectionLayer,
    Canvas? NodeLayer,
    Control CoordinateRoot,
    IReadOnlyDictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> NodeVisuals,
    Point? PointerScreenPosition,
    Func<ConnectionViewModel, ConnectionStyleOptions> ResolveConnectionStyle,
    Func<NodeCanvasContextMenuSnapshot> CreateContextMenuSnapshot,
    Func<ContextRequestedEventArgs, Control, GraphPoint> ResolveWorldPosition,
    Func<Control, ContextMenuContext, bool> OpenContextMenu);

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
            var sourceNode = context.ViewModel.FindNode(connection.SourceNodeId);
            var targetNode = context.ViewModel.FindNode(connection.TargetNodeId);
            if (sourceNode is null || targetNode is null)
            {
                continue;
            }

            var sourcePort = sourceNode.GetPort(connection.SourcePortId);
            var targetPort = targetNode.GetPort(connection.TargetPortId);
            if (sourcePort is null || targetPort is null)
            {
                continue;
            }

            DrawConnection(
                context,
                GetPortAnchor(context, sourceNode, sourcePort),
                GetPortAnchor(context, targetNode, targetPort),
                connection);
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
                end,
                new ConnectionViewModel(
                    "pending",
                    context.ViewModel.PendingSourceNode.Id,
                    context.ViewModel.PendingSourcePort.Id,
                    string.Empty,
                    string.Empty,
                    "pending",
                    context.ViewModel.PendingSourcePort.AccentHex),
                isPreview: true);
        }
    }

    public GraphPoint GetPortAnchor(NodeCanvasConnectionSceneContext context, NodeViewModel node, PortViewModel port)
    {
        // 吸附拖动时，节点外层 Canvas 的绝对布局可能还没完成新一轮 Arrange。
        // 因此这里优先读取“端口圆点在节点卡内部的局部坐标”，再叠加当前节点的 X/Y，
        // 这样连线能跟随最新的节点位置，而不会因为布局时序晚一拍出现偏移。
        if (context.NodeVisuals.TryGetValue(node, out var visual)
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

        return node.GetPortAnchor(port);
    }

    private void DrawConnection(
        NodeCanvasConnectionSceneContext context,
        GraphPoint start,
        GraphPoint end,
        ConnectionViewModel connection,
        bool isPreview = false)
    {
        if (context.ConnectionLayer is null || context.ViewModel is null)
        {
            return;
        }

        var connectionStyle = context.ResolveConnectionStyle(connection);
        var curve = ConnectionPathBuilder.Build(start, end);
        var path = new global::Avalonia.Controls.Shapes.Path
        {
            Data = Geometry.Parse(
                $"M {curve.Start.X:0.##},{curve.Start.Y:0.##} " +
                $"C {curve.Control1.X:0.##},{curve.Control1.Y:0.##} " +
                $"{curve.Control2.X:0.##},{curve.Control2.Y:0.##} " +
                $"{curve.End.X:0.##},{curve.End.Y:0.##}"),
            Stroke = BrushFactory.Solid(connection.AccentHex, isPreview ? connectionStyle.PreviewStrokeOpacity : connectionStyle.StrokeOpacity),
            StrokeThickness = isPreview ? connectionStyle.PreviewThickness : connectionStyle.Thickness,
            StrokeLineCap = PenLineCap.Round,
        };
        context.ConnectionLayer.Children.Add(path);

        if (isPreview)
        {
            return;
        }

        var midpoint = new Point((curve.Start.X + curve.End.X) / 2, (curve.Start.Y + curve.End.Y) / 2);

        var chip = new Border
        {
            Background = BrushFactory.Solid(connectionStyle.LabelBackgroundHex, connectionStyle.LabelBackgroundOpacity),
            BorderBrush = BrushFactory.Solid(connection.AccentHex, connectionStyle.LabelBorderOpacity),
            BorderThickness = new Thickness(connectionStyle.LabelBorderThickness),
            CornerRadius = new CornerRadius(connectionStyle.LabelCornerRadius),
            Padding = new Thickness(connectionStyle.LabelHorizontalPadding, connectionStyle.LabelVerticalPadding),
            Focusable = true,
            Child = CreateLabelBlock(connectionStyle, GetDisplayedChipText(connection, connection.NoteText)),
        };
        AutomationProperties.SetName(chip, $"{GetDisplayedChipText(connection, connection.NoteText)} connection");
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
                var displayText = GetDisplayedChipText(connection, noteText);
                chip.Child = CreateLabelBlock(connectionStyle, displayText);
                AutomationProperties.SetName(chip, $"{displayText} connection");
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

    private static TextBlock CreateLabelBlock(ConnectionStyleOptions connectionStyle, string text)
        => new()
        {
            Text = text,
            FontSize = connectionStyle.LabelFontSize,
            Foreground = BrushFactory.Solid(connectionStyle.LabelForegroundHex, connectionStyle.LabelForegroundOpacity),
        };

    private static string GetDisplayedChipText(ConnectionViewModel connection, string? noteText)
        => string.IsNullOrWhiteSpace(noteText)
            ? connection.Label
            : noteText.Trim();
}
