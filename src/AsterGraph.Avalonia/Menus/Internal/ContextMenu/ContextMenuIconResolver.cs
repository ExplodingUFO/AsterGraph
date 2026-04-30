using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Styling;

namespace AsterGraph.Avalonia.Menus.Internal;

/// <summary>
/// 映射图标 key 到右键菜单文本图标。
/// </summary>
internal static class ContextMenuIconResolver
{
    internal static Border? CreateIcon(string? iconKey, bool isEnabled, ContextMenuStyleOptions style)
    {
        var glyph = ResolveIconGlyph(iconKey);
        if (glyph is null)
        {
            return null;
        }

        return new Border
        {
            Width = 18,
            Height = 18,
            CornerRadius = new CornerRadius(style.ItemCornerRadius),
            Background = BrushFactory.Solid(style.HoverHex),
            Child = new TextBlock
            {
                Text = glyph,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontSize = style.IconFontSize,
                FontWeight = FontWeight.Bold,
                Foreground = BrushFactory.Solid(isEnabled ? style.ForegroundHex : style.DisabledForegroundHex),
            },
        };
    }

    private static string? ResolveIconGlyph(string? iconKey)
        => iconKey switch
        {
            "add" => "+",
            "node" => "N",
            "fit" => "F",
            "reset" => "R",
            "save" => "S",
            "load" => "L",
            "copy" => "C",
            "import" => "I",
            "export" => "E",
            "paste" => "P",
            "cancel" => "X",
            "inspect" => "I",
            "center" => "C",
            "delete" => "-",
            "duplicate" => "D",
            "disconnect" => "/",
            "connect" => ">",
            "compatible" => "=",
            "type" => "T",
            "conversion" => "~",
            "align" => "|",
            "align-left" => "L",
            "align-center" => "C",
            "align-right" => "R",
            "align-top" => "T",
            "align-middle" => "M",
            "align-bottom" => "B",
            "distribute" => "#",
            "distribute-horizontal" => "H",
            "distribute-vertical" => "V",
            "snap" => ".",
            "info" => "i",
            _ => null,
        };
}
