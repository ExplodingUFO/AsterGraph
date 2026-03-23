using Avalonia.Media;

namespace AsterGraph.Avalonia.Styling;

internal static class BrushFactory
{
    public static SolidColorBrush Solid(string hex, double opacity = 1.0)
        => new(Color(hex, opacity));

    public static SolidColorBrush SolidSafe(string? hex, string fallbackHex, double opacity = 1.0)
        => new(ColorSafe(hex, fallbackHex, opacity));

    public static Color Color(string hex, double opacity = 1.0)
    {
        var parsed = global::Avalonia.Media.Color.Parse(hex);
        return ApplyOpacity(parsed, opacity);
    }

    public static Color ColorSafe(string? hex, string fallbackHex, double opacity = 1.0)
    {
        var parsed = TryParse(hex, out var primary)
            ? primary
            : TryParse(fallbackHex, out var fallback)
                ? fallback
                : Colors.White;

        return ApplyOpacity(parsed, opacity);
    }

    private static Color ApplyOpacity(Color color, double opacity)
    {
        if (opacity >= 0.999)
        {
            return color;
        }

        return new Color(
            (byte)Math.Round(255 * Math.Clamp(opacity, 0, 1)),
            color.R,
            color.G,
            color.B);
    }

    private static bool TryParse(string? hex, out Color color)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            color = default;
            return false;
        }

        try
        {
            color = global::Avalonia.Media.Color.Parse(hex);
            return true;
        }
        catch
        {
            color = default;
            return false;
        }
    }
}
