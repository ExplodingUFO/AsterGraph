using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// Renders the graph canvas background grid from editor style and viewport state.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GridBackground : Control
{
    /// <summary>
    /// Styled property that supplies the bound editor view model.
    /// </summary>
    public static readonly StyledProperty<GraphEditorViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<GridBackground, GraphEditorViewModel?>(nameof(ViewModel));

    /// <summary>
    /// Bound editor view model used to resolve grid styling and viewport state.
    /// </summary>
    public GraphEditorViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <summary>
    /// Renders the grid background.
    /// </summary>
    /// <param name="context">Drawing context.</param>
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
        var verticalLines = CalculateVisibleLineMetrics(bounds.Width, rawSpacing, ViewModel!.PanX);
        if (verticalLines.Spacing <= 0)
        {
            return;
        }

        var horizontalLines = CalculateVisibleLineMetrics(bounds.Height, rawSpacing, ViewModel.PanY);
        if (horizontalLines.Spacing <= 0)
        {
            return;
        }

        var pen = new Pen(brush, thickness);

        for (var x = verticalLines.Offset; x <= bounds.Width; x += verticalLines.Spacing)
        {
            context.DrawLine(pen, new Point(x, 0), new Point(x, bounds.Height));
        }

        for (var y = horizontalLines.Offset; y <= bounds.Height; y += horizontalLines.Spacing)
        {
            context.DrawLine(pen, new Point(0, y), new Point(bounds.Width, y));
        }
    }

    internal static GridBackgroundLineMetrics CalculateVisibleLineMetrics(double length, double rawSpacing, double offset)
    {
        var spacing = NormalizeSpacing(rawSpacing);
        if (spacing <= 0 || length <= 0)
        {
            return new GridBackgroundLineMetrics(0, 0, 0);
        }

        var normalizedOffset = NormalizeOffset(offset, spacing);
        var lineCount = 0;
        for (var position = normalizedOffset; position <= length; position += spacing)
        {
            lineCount++;
        }

        return new GridBackgroundLineMetrics(spacing, normalizedOffset, lineCount);
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

internal readonly record struct GridBackgroundLineMetrics(double Spacing, double Offset, int LineCount);
