using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// 图编辑器缩略图控件，用于概览节点分布与当前视口位置。
/// </summary>
public sealed class GraphMiniMap : Control
{
    /// <summary>
    /// 绑定的编辑器视图模型依赖属性。
    /// </summary>
    public static readonly StyledProperty<GraphEditorViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<GraphMiniMap, GraphEditorViewModel?>(nameof(ViewModel));

    private bool _isDraggingViewport;

    /// <summary>
    /// 初始化缩略图控件。
    /// </summary>
    public GraphMiniMap()
    {
        Focusable = false;
        PointerPressed += HandlePointerPressed;
        PointerMoved += HandlePointerMoved;
        PointerReleased += HandlePointerReleased;
    }

    /// <summary>
    /// 当前绑定的编辑器视图模型。
    /// </summary>
    public GraphEditorViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        var editor = ViewModel;
        if (editor is null || bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var canvasStyle = editor.StyleOptions.Canvas;
        context.FillRectangle(BrushFactory.Solid(canvasStyle.GridBackgroundHex), bounds);

        if (editor.Nodes.Count == 0)
        {
            return;
        }

        var worldBounds = GetWorldBounds(editor);
        var scale = Math.Min(bounds.Width / worldBounds.Width, bounds.Height / worldBounds.Height);
        var offsetX = bounds.X + ((bounds.Width - (worldBounds.Width * scale)) / 2);
        var offsetY = bounds.Y + ((bounds.Height - (worldBounds.Height * scale)) / 2);

        foreach (var node in editor.Nodes)
        {
            var nodeRect = ToMiniMapRect(node.X, node.Y, node.Width, node.Height, worldBounds, scale, offsetX, offsetY);
            context.DrawRectangle(
                BrushFactory.Solid(node.IsSelected ? node.AccentHex : canvasStyle.PrimaryGridHex, node.IsSelected ? 0.7 : 0.45),
                new Pen(BrushFactory.Solid(node.AccentHex, 0.8), 1),
                nodeRect);
        }

        if (editor.ViewportWidth > 0 && editor.ViewportHeight > 0)
        {
            var topLeft = editor.ScreenToWorld(new GraphPoint(0, 0));
            var bottomRight = editor.ScreenToWorld(new GraphPoint(editor.ViewportWidth, editor.ViewportHeight));
            var viewportRect = ToMiniMapRect(
                topLeft.X,
                topLeft.Y,
                bottomRight.X - topLeft.X,
                bottomRight.Y - topLeft.Y,
                worldBounds,
                scale,
                offsetX,
                offsetY);

            context.DrawRectangle(
                null,
                new Pen(BrushFactory.Solid(canvasStyle.GuideHex, canvasStyle.GuideOpacity), 1.2),
                viewportRect);
        }
    }

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs args)
    {
        var editor = ViewModel;
        if (editor is null || editor.Nodes.Count == 0 || args.GetCurrentPoint(this).Properties.IsLeftButtonPressed is false)
        {
            return;
        }

        CenterViewportFromMiniMap(editor, args.GetPosition(this));
        _isDraggingViewport = true;
        args.Pointer.Capture(this);
        args.Handled = true;
    }

    private void HandlePointerMoved(object? sender, PointerEventArgs args)
    {
        if (!_isDraggingViewport || ViewModel is null)
        {
            return;
        }

        CenterViewportFromMiniMap(ViewModel, args.GetPosition(this), updateStatus: false);
        args.Handled = true;
    }

    private void HandlePointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        if (!_isDraggingViewport)
        {
            return;
        }

        _isDraggingViewport = false;
        args.Pointer.Capture(null);
        args.Handled = true;
    }

    private void CenterViewportFromMiniMap(GraphEditorViewModel editor, Point point, bool updateStatus = true)
    {
        var bounds = Bounds;
        var worldBounds = GetWorldBounds(editor);
        var scale = Math.Min(bounds.Width / worldBounds.Width, bounds.Height / worldBounds.Height);
        var offsetX = bounds.X + ((bounds.Width - (worldBounds.Width * scale)) / 2);
        var offsetY = bounds.Y + ((bounds.Height - (worldBounds.Height * scale)) / 2);

        var worldX = worldBounds.X + ((point.X - offsetX) / scale);
        var worldY = worldBounds.Y + ((point.Y - offsetY) / scale);
        editor.CenterViewAt(new GraphPoint(worldX, worldY), updateStatus);
    }

    private static Rect GetWorldBounds(GraphEditorViewModel editor)
    {
        var minX = editor.Nodes.Min(node => node.X);
        var minY = editor.Nodes.Min(node => node.Y);
        var maxX = editor.Nodes.Max(node => node.X + node.Width);
        var maxY = editor.Nodes.Max(node => node.Y + node.Height);
        const double padding = 80;

        return new Rect(
            minX - padding,
            minY - padding,
            Math.Max(1, (maxX - minX) + (padding * 2)),
            Math.Max(1, (maxY - minY) + (padding * 2)));
    }

    private static Rect ToMiniMapRect(
        double worldX,
        double worldY,
        double worldWidth,
        double worldHeight,
        Rect worldBounds,
        double scale,
        double offsetX,
        double offsetY)
    {
        return new Rect(
            offsetX + ((worldX - worldBounds.X) * scale),
            offsetY + ((worldY - worldBounds.Y) * scale),
            Math.Max(2, worldWidth * scale),
            Math.Max(2, worldHeight * scale));
    }
}
