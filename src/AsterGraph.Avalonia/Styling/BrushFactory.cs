using Avalonia.Media;

namespace AsterGraph.Avalonia.Styling;

internal static class BrushFactory
{
    public static SolidColorBrush Solid(string hex, double opacity = 1.0)
        => new(Color(hex, opacity));

    public static Color Color(string hex, double opacity = 1.0)
    {
        var parsed = global::Avalonia.Media.Color.Parse(hex);
        if (opacity >= 0.999)
        {
            return parsed;
        }

        return new Color(
            (byte)Math.Round(255 * opacity),
            parsed.R,
            parsed.G,
            parsed.B);
    }
}
