using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Presentation;

internal static class GraphTypeCueFormatter
{
    private static readonly char[] TokenSeparators = ['-', '_', ' ', '/', ':', '.'];

    public static string FormatPortToken(PortViewModel port)
    {
        ArgumentNullException.ThrowIfNull(port);
        return FormatTypeToken(port.TypeId.Value, port.DataType);
    }

    public static string FormatTypeToken(string? typeId, string? fallbackText = null)
    {
        var rawType = string.IsNullOrWhiteSpace(typeId) ? fallbackText : typeId;
        if (string.IsNullOrWhiteSpace(rawType))
        {
            return string.Empty;
        }

        var segments = rawType
            .Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(segment => new string(segment.Where(char.IsLetterOrDigit).ToArray()))
            .Where(segment => segment.Length > 0)
            .ToList();

        if (segments.Count == 0)
        {
            return rawType.Trim().ToUpperInvariant();
        }

        if (segments.Count == 1)
        {
            return segments[0].ToUpperInvariant();
        }

        return string.Concat(segments.Select(segment => char.ToUpperInvariant(segment[0])));
    }
}
