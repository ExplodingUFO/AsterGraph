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
        var spacing = Options.Shell.DefaultSpacing;

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
        resources["AsterGraph.ContextMenuBackgroundBrush"] = Palette.ContextMenuBackgroundBrush;
        resources["AsterGraph.ContextMenuBorderBrush"] = Palette.ContextMenuBorderBrush;
        resources["AsterGraph.ContextMenuForegroundBrush"] = Palette.ContextMenuForegroundBrush;
        resources["AsterGraph.ContextMenuHoverBrush"] = Palette.ContextMenuHoverBrush;
        resources["AsterGraph.ContextMenuDisabledForegroundBrush"] = Palette.ContextMenuDisabledForegroundBrush;
        resources["AsterGraph.ContextMenuSeparatorBrush"] = Palette.ContextMenuSeparatorBrush;
        resources["AsterGraph.DefaultCornerRadius"] = new CornerRadius(Options.Shell.DefaultCornerRadius);
        resources["AsterGraph.DefaultSpacing"] = spacing;
        resources["AsterGraph.LibraryPanelWidth"] = Options.Shell.LibraryPanelWidth;
        resources["AsterGraph.InspectorPanelWidth"] = Options.Shell.InspectorPanelWidth;
        resources["AsterGraph.ShellEyebrowFontSize"] = Options.Shell.EyebrowFontSize;
        resources["AsterGraph.ShellTitleFontSize"] = Options.Shell.TitleFontSize;
        resources["AsterGraph.ShellBodyFontSize"] = Options.Shell.BodyFontSize;
        resources["AsterGraph.InspectorTitleFontSize"] = Options.Inspector.TitleFontSize;
        resources["AsterGraph.InspectorBodyFontSize"] = Options.Inspector.BodyFontSize;
        resources["AsterGraph.InspectorCaptionFontSize"] = Options.Inspector.CaptionFontSize;
        resources["AsterGraph.InspectorSectionSpacing"] = Options.Inspector.SectionSpacing;
        resources["AsterGraph.InspectorCardCornerRadius"] = new CornerRadius(Options.Inspector.CardCornerRadius);
        resources["AsterGraph.InspectorSectionCornerRadius"] = new CornerRadius(Options.Inspector.SectionCornerRadius);
        resources["AsterGraph.InspectorSectionPadding"] = new Thickness(Options.Inspector.SectionPadding);
        resources["AsterGraph.ShellPadding"] = new Thickness(spacing + 2);
        resources["AsterGraph.HeroPanelPadding"] = new Thickness(spacing + 6, spacing + 2);
        resources["AsterGraph.SidePanelPadding"] = new Thickness(spacing + 2);
        resources["AsterGraph.InspectorPanelPadding"] = new Thickness(spacing + 4);
        resources["AsterGraph.CanvasFramePadding"] = new Thickness(Math.Max(4, spacing / 2));
        resources["AsterGraph.StatusPanelPadding"] = new Thickness(spacing + 2, Math.Max(8, spacing - 4));
        resources["AsterGraph.ToolbarButtonPadding"] = new Thickness(spacing, Math.Max(6, spacing - 6));
        resources["AsterGraph.ToolbarButtonMargin"] = new Thickness(0, 0, Math.Max(8, spacing - 6), 0);
        resources["AsterGraph.ToolbarButtonCornerRadius"] = new CornerRadius(Math.Max(8, Options.Shell.DefaultCornerRadius - 8));
        resources["AsterGraph.PillPadding"] = new Thickness(Math.Max(10, spacing - 4), 6);
        resources["AsterGraph.PillCornerRadius"] = new CornerRadius(999);
        resources["AsterGraph.TemplateCardPadding"] = new Thickness(spacing);
        resources["AsterGraph.TemplateCardCornerRadius"] = new CornerRadius(Math.Max(8, Options.Shell.DefaultCornerRadius - 4));
        resources["AsterGraph.ContextMenuBorderThickness"] = new Thickness(Options.ContextMenu.BorderThickness);
        resources["AsterGraph.ContextMenuCornerRadius"] = new CornerRadius(Options.ContextMenu.CornerRadius);
        resources["AsterGraph.ContextMenuItemCornerRadius"] = new CornerRadius(Options.ContextMenu.ItemCornerRadius);
        resources["AsterGraph.ContextMenuItemPadding"] = new Thickness(
            Options.ContextMenu.ItemHorizontalPadding,
            Options.ContextMenu.ItemVerticalPadding);
        resources["AsterGraph.ContextMenuItemFontSize"] = Options.ContextMenu.ItemFontSize;
        resources["AsterGraph.ContextMenuItemMinWidth"] = Options.ContextMenu.ItemMinWidth;
        resources["AsterGraph.ContextMenuIconFontSize"] = Options.ContextMenu.IconFontSize;
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
            BrushFactory.Solid(options.Inspector.ValidationErrorHex),
            BrushFactory.Solid(options.ContextMenu.BackgroundHex),
            BrushFactory.Solid(options.ContextMenu.BorderHex),
            BrushFactory.Solid(options.ContextMenu.ForegroundHex),
            BrushFactory.Solid(options.ContextMenu.HoverHex),
            BrushFactory.Solid(options.ContextMenu.DisabledForegroundHex),
            BrushFactory.Solid(options.ContextMenu.SeparatorHex));
    }
}
