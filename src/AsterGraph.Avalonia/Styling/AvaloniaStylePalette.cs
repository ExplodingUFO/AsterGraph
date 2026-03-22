using Avalonia.Media;

namespace AsterGraph.Avalonia.Styling;

public sealed record AvaloniaStylePalette(
    IBrush ShellBackgroundBrush,
    SolidColorBrush PanelBrush,
    SolidColorBrush PanelBorderBrush,
    SolidColorBrush CanvasFrameBrush,
    SolidColorBrush BadgeBrush,
    SolidColorBrush HeadlineBrush,
    SolidColorBrush BodyBrush,
    SolidColorBrush EyebrowBrush,
    SolidColorBrush HighlightBrush,
    SolidColorBrush InspectorCardBrush,
    SolidColorBrush InputBackgroundBrush,
    SolidColorBrush InputBorderBrush,
    SolidColorBrush ValidationErrorBrush);
