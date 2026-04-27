using System.Globalization;
using System.Security;
using System.Text;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Services;

internal static class GraphSceneSvgDocumentBuilder
{
    private const double EmptySceneWidth = 960d;
    private const double EmptySceneHeight = 640d;
    private const double ScenePadding = 72d;
    private const double CornerRadius = 18d;
    private const double GroupCornerRadius = 24d;
    private const double PortRadius = 5d;
    private const string DefaultBackgroundHex = "#09141C";

    public static string Build(GraphEditorSceneSnapshot scene, string? backgroundHex = null)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var bounds = MeasureScene(scene);
        var builder = new StringBuilder();
        var width = bounds.Width.ToString("0.##", CultureInfo.InvariantCulture);
        var height = bounds.Height.ToString("0.##", CultureInfo.InvariantCulture);
        var viewBox = string.Create(
            CultureInfo.InvariantCulture,
            $"{bounds.MinX:0.##} {bounds.MinY:0.##} {bounds.Width:0.##} {bounds.Height:0.##}");

        builder.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""");
        builder.AppendLine(
            $"""<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="{viewBox}" fill="none">""");
        builder.AppendLine($"""  <rect width="100%" height="100%" fill="{ResolveBackgroundHex(backgroundHex)}" />""");
        builder.AppendLine("""  <g id="groups">""");
        foreach (var group in scene.NodeGroups)
        {
            AppendGroup(builder, group);
        }

        builder.AppendLine("""  </g>""");
        builder.AppendLine("""  <g id="connections">""");
        var nodesById = scene.Document.Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        foreach (var connection in scene.Document.Connections)
        {
            AppendConnection(builder, nodesById, connection);
        }

        builder.AppendLine("""  </g>""");
        builder.AppendLine("""  <g id="nodes">""");
        foreach (var node in scene.Document.Nodes)
        {
            AppendNode(builder, node);
        }

        builder.AppendLine("""  </g>""");
        builder.AppendLine("</svg>");
        return builder.ToString();
    }

    private static GraphBounds MeasureScene(GraphEditorSceneSnapshot scene)
    {
        if (scene.Document.Nodes.Count == 0 && scene.NodeGroups.Count == 0)
        {
            return new GraphBounds(0d, 0d, EmptySceneWidth, EmptySceneHeight);
        }

        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;

        foreach (var node in scene.Document.Nodes)
        {
            minX = Math.Min(minX, node.Position.X);
            minY = Math.Min(minY, node.Position.Y);
            maxX = Math.Max(maxX, node.Position.X + node.Size.Width);
            maxY = Math.Max(maxY, node.Position.Y + node.Size.Height);
        }

        foreach (var group in scene.NodeGroups)
        {
            minX = Math.Min(minX, group.Position.X);
            minY = Math.Min(minY, group.Position.Y);
            maxX = Math.Max(maxX, group.Position.X + group.Size.Width);
            maxY = Math.Max(maxY, group.Position.Y + group.Size.Height);
        }

        return new GraphBounds(
            minX - ScenePadding,
            minY - ScenePadding,
            Math.Max(EmptySceneWidth, (maxX - minX) + (ScenePadding * 2d)),
            Math.Max(EmptySceneHeight, (maxY - minY) + (ScenePadding * 2d)));
    }

    private static void AppendGroup(StringBuilder builder, GraphEditorNodeGroupSnapshot group)
    {
        builder.AppendLine(
            string.Create(
                CultureInfo.InvariantCulture,
                $"""    <rect x="{group.Position.X:0.##}" y="{group.Position.Y:0.##}" width="{group.Size.Width:0.##}" height="{group.Size.Height:0.##}" rx="{GroupCornerRadius:0.##}" fill="#0F2230" fill-opacity="0.55" stroke="#4DA6BF" stroke-width="2" stroke-dasharray="10 8" />"""));
        builder.AppendLine(
            string.Create(
                CultureInfo.InvariantCulture,
                $"""    <text x="{group.Position.X + 18d:0.##}" y="{group.Position.Y + 30d:0.##}" fill="#9FDDEE" font-size="18" font-family="Segoe UI, Arial, sans-serif">{Escape(group.Title)}</text>"""));
    }

    private static void AppendConnection(
        StringBuilder builder,
        IReadOnlyDictionary<string, GraphNode> nodesById,
        GraphConnection connection)
    {
        if (!nodesById.TryGetValue(connection.SourceNodeId, out var sourceNode)
            || !nodesById.TryGetValue(connection.TargetNodeId, out var targetNode))
        {
            return;
        }

        var start = new GraphPoint(sourceNode.Position.X + sourceNode.Size.Width, sourceNode.Position.Y + (sourceNode.Size.Height / 2d));
        var end = new GraphPoint(targetNode.Position.X, targetNode.Position.Y + (targetNode.Size.Height / 2d));
        var delta = Math.Max(56d, (end.X - start.X) / 2d);
        var stroke = string.IsNullOrWhiteSpace(connection.AccentHex) ? "#8ED8FF" : connection.AccentHex;
        builder.AppendLine(
            string.Create(
                CultureInfo.InvariantCulture,
                $"""    <path d="M {start.X:0.##} {start.Y:0.##} C {start.X + delta:0.##} {start.Y:0.##} {end.X - delta:0.##} {end.Y:0.##} {end.X:0.##} {end.Y:0.##}" stroke="{stroke}" stroke-width="4" stroke-linecap="round" stroke-linejoin="round" fill="none" />"""));

        if (!string.IsNullOrWhiteSpace(connection.Label))
        {
            var labelX = (start.X + end.X) / 2d;
            var labelY = Math.Min(start.Y, end.Y) - 14d;
            builder.AppendLine(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"""    <text x="{labelX:0.##}" y="{labelY:0.##}" fill="#DCEFF5" font-size="14" text-anchor="middle" font-family="Segoe UI, Arial, sans-serif">{Escape(connection.Label)}</text>"""));
        }
    }

    private static void AppendNode(StringBuilder builder, GraphNode node)
    {
        var accent = string.IsNullOrWhiteSpace(node.AccentHex) ? "#6AD5C4" : node.AccentHex;
        var titleY = node.Position.Y + 34d;
        var subtitleY = titleY + 22d;

        builder.AppendLine(
            string.Create(
                CultureInfo.InvariantCulture,
                $"""    <rect x="{node.Position.X:0.##}" y="{node.Position.Y:0.##}" width="{node.Size.Width:0.##}" height="{node.Size.Height:0.##}" rx="{CornerRadius:0.##}" fill="#112431" stroke="{accent}" stroke-width="2" />"""));
        builder.AppendLine(
            string.Create(
                CultureInfo.InvariantCulture,
                $"""    <text x="{node.Position.X + 20d:0.##}" y="{titleY:0.##}" fill="#F5FBFF" font-size="20" font-weight="600" font-family="Segoe UI, Arial, sans-serif">{Escape(node.Title)}</text>"""));

        if (!string.IsNullOrWhiteSpace(node.Subtitle))
        {
            builder.AppendLine(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"""    <text x="{node.Position.X + 20d:0.##}" y="{subtitleY:0.##}" fill="#A5C3CF" font-size="13" font-family="Segoe UI, Arial, sans-serif">{Escape(node.Subtitle)}</text>"""));
        }

        AppendPorts(builder, node, node.Inputs, true);
        AppendPorts(builder, node, node.Outputs, false);
    }

    private static void AppendPorts(StringBuilder builder, GraphNode node, IReadOnlyList<GraphPort> ports, bool isInput)
    {
        if (ports.Count == 0)
        {
            return;
        }

        var startY = node.Position.Y + 82d;
        var gap = ports.Count == 1
            ? 0d
            : Math.Min(28d, Math.Max(18d, (node.Size.Height - 108d) / Math.Max(1, ports.Count - 1)));

        for (var index = 0; index < ports.Count; index++)
        {
            var port = ports[index];
            var y = startY + (gap * index);
            var x = isInput ? node.Position.X : node.Position.X + node.Size.Width;
            var textX = isInput ? x + 14d : x - 14d;
            var anchor = isInput ? "start" : "end";
            var accent = string.IsNullOrWhiteSpace(port.AccentHex) ? "#9FDDEE" : port.AccentHex;
            builder.AppendLine(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"""    <circle cx="{x:0.##}" cy="{y:0.##}" r="{PortRadius:0.##}" fill="{accent}" stroke="#081118" stroke-width="2" />"""));
            builder.AppendLine(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"""    <text x="{textX:0.##}" y="{y + 4d:0.##}" fill="#DCEFF5" font-size="13" text-anchor="{anchor}" font-family="Segoe UI, Arial, sans-serif">{Escape(port.Label)}</text>"""));
        }
    }

    private static string ResolveBackgroundHex(string? backgroundHex)
    {
        if (string.IsNullOrWhiteSpace(backgroundHex))
        {
            return DefaultBackgroundHex;
        }

        var normalized = backgroundHex.Trim();
        if ((normalized.Length != 7 && normalized.Length != 9)
            || normalized[0] != '#'
            || !normalized.AsSpan(1).ToString().All(Uri.IsHexDigit))
        {
            throw new ArgumentException("BackgroundHex must use #RRGGBB or #AARRGGBB format.", nameof(backgroundHex));
        }

        return normalized;
    }

    private static string Escape(string? value)
        => SecurityElement.Escape(value) ?? string.Empty;

    private readonly record struct GraphBounds(double MinX, double MinY, double Width, double Height);
}
