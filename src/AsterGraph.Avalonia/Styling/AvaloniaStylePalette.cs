using Avalonia.Media;

namespace AsterGraph.Avalonia.Styling;

/// <summary>
/// Avalonia 视图层使用的已解析调色板，承载宿主样式选项转换后的画刷集合。
/// </summary>
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
    SolidColorBrush ValidationErrorBrush,
    SolidColorBrush ContextMenuBackgroundBrush,
    SolidColorBrush ContextMenuBorderBrush,
    SolidColorBrush ContextMenuForegroundBrush,
    SolidColorBrush ContextMenuHoverBrush,
    SolidColorBrush ContextMenuDisabledForegroundBrush,
    SolidColorBrush ContextMenuSeparatorBrush);
