using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AsterGraph.Abstractions.Styling;

namespace AsterGraph.Avalonia.Styling;

public sealed class GraphEditorStyleAdapter
{
    public GraphEditorStyleAdapter(GraphEditorStyleOptions options)
    {
        Options = options;
        Palette = CreatePalette(options);
    }

    public GraphEditorStyleOptions Options { get; }

    public AvaloniaStylePalette Palette { get; }

    public void ApplyResources(IResourceDictionary resources)
    {
        resources["AsterGraph.ShellBackgroundBrush"] = Palette.ShellBackgroundBrush;
        resources["AsterGraph.PanelBrush"] = Palette.PanelBrush;
        resources["AsterGraph.PanelBorderBrush"] = Palette.PanelBorderBrush;
        resources["AsterGraph.CanvasFrameBrush"] = Palette.CanvasFrameBrush;
        resources["AsterGraph.BadgeBrush"] = Palette.BadgeBrush;
        resources["AsterGraph.HeadlineBrush"] = Palette.HeadlineBrush;
        resources["AsterGraph.BodyBrush"] = Palette.BodyBrush;
        resources["AsterGraph.EyebrowBrush"] = Palette.EyebrowBrush;
        resources["AsterGraph.HighlightBrush"] = Palette.HighlightBrush;
        resources["AsterGraph.InspectorCardBrush"] = Palette.InspectorCardBrush;
        resources["AsterGraph.InputBackgroundBrush"] = Palette.InputBackgroundBrush;
        resources["AsterGraph.InputBorderBrush"] = Palette.InputBorderBrush;
        resources["AsterGraph.ValidationErrorBrush"] = Palette.ValidationErrorBrush;
        resources["AsterGraph.DefaultCornerRadius"] = new CornerRadius(Options.Shell.DefaultCornerRadius);
        resources["AsterGraph.DefaultSpacing"] = Options.Shell.DefaultSpacing;
        resources["AsterGraph.InspectorCardCornerRadius"] = new CornerRadius(Options.Inspector.CardCornerRadius);
        resources["AsterGraph.ContextMenuCornerRadius"] = new CornerRadius(Options.ContextMenu.CornerRadius);
    }

    private static AvaloniaStylePalette CreatePalette(GraphEditorStyleOptions options)
    {
        var shell = options.Shell;
        var gradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.Parse(shell.BackgroundStartHex), 0),
                new GradientStop(Color.Parse(shell.BackgroundMidHex), 0.45),
                new GradientStop(Color.Parse(shell.BackgroundEndHex), 1),
            ],
        };

        return new AvaloniaStylePalette(
            gradient,
            BrushFactory.Solid(shell.PanelBackgroundHex),
            BrushFactory.Solid(shell.PanelBorderHex),
            BrushFactory.Solid(options.Canvas.FrameBackgroundHex),
            BrushFactory.Solid(shell.BadgeBackgroundHex),
            BrushFactory.Solid(shell.HeadlineHex),
            BrushFactory.Solid(shell.BodyHex),
            BrushFactory.Solid(shell.EyebrowHex),
            BrushFactory.Solid(shell.HighlightHex),
            BrushFactory.Solid(options.Inspector.CardBackgroundHex),
            BrushFactory.Solid(options.Inspector.InputBackgroundHex),
            BrushFactory.Solid(options.Inspector.InputBorderHex),
            BrushFactory.Solid(options.Inspector.ValidationErrorHex));
    }
}
