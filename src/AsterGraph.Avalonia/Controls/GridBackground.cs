using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

public sealed class GridBackground : Control
{
    public static readonly StyledProperty<GraphEditorViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<GridBackground, GraphEditorViewModel?>(nameof(ViewModel));

    public GraphEditorViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;

        if (ViewModel is null || bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var style = ViewModel.StyleOptions.Canvas;
        context.FillRectangle(BrushFactory.Solid(style.GridBackgroundHex), bounds);
        DrawGrid(context, bounds, style.PrimaryGridSpacing * ViewModel.Zoom, BrushFactory.Solid(style.PrimaryGridHex, style.PrimaryGridOpacity), 1);
        DrawGrid(context, bounds, style.SecondaryGridSpacing * ViewModel.Zoom, BrushFactory.Solid(style.SecondaryGridHex, style.SecondaryGridOpacity), 1.3);
    }

    private void DrawGrid(DrawingContext context, Rect bounds, double rawSpacing, IBrush brush, double thickness)
    {
        var spacing = NormalizeSpacing(rawSpacing);
        if (spacing <= 0)
        {
            return;
        }

        var offsetX = NormalizeOffset(ViewModel!.PanX, spacing);
        var offsetY = NormalizeOffset(ViewModel.PanY, spacing);
        var pen = new Pen(brush, thickness);

        for (var x = offsetX; x <= bounds.Width; x += spacing)
        {
            context.DrawLine(pen, new Point(x, 0), new Point(x, bounds.Height));
        }

        for (var y = offsetY; y <= bounds.Height; y += spacing)
        {
            context.DrawLine(pen, new Point(0, y), new Point(bounds.Width, y));
        }
    }

    private static double NormalizeSpacing(double spacing)
    {
        if (spacing <= 0)
        {
            return 0;
        }

        while (spacing < 22)
        {
            spacing *= 2;
        }

        return spacing;
    }

    private static double NormalizeOffset(double offset, double spacing)
    {
        var normalized = offset % spacing;
        return normalized < 0 ? normalized + spacing : normalized;
    }
}
