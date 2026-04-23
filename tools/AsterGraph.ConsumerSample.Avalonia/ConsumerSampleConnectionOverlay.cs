using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.ConsumerSample;

internal sealed class ConsumerSampleConnectionOverlay : Canvas
{
    private readonly IGraphEditorSession _session;

    public ConsumerSampleConnectionOverlay(IGraphEditorSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));

        Name = "PART_AuthoringEdgeOverlay";
        IsHitTestVisible = false;
        ClipToBounds = false;

        _session.Events.DocumentChanged += HandleStateChanged;
        _session.Events.ViewportChanged += HandleStateChanged;
        DetachedFromVisualTree += (_, _) =>
        {
            _session.Events.DocumentChanged -= HandleStateChanged;
            _session.Events.ViewportChanged -= HandleStateChanged;
        };
        RebuildBadges();
    }

    private void HandleStateChanged(object? sender, EventArgs e)
        => RebuildBadges();

    private void RebuildBadges()
    {
        Children.Clear();

        var scene = _session.Queries.GetSceneSnapshot();
        var connectionsById = scene.Document.Connections.ToDictionary(connection => connection.Id, StringComparer.Ordinal);

        foreach (var geometry in scene.ConnectionGeometries)
        {
            if (!connectionsById.TryGetValue(geometry.ConnectionId, out var connection))
            {
                continue;
            }

            var midpoint = ResolveMidpoint(geometry);
            var screenX = (midpoint.X * scene.Viewport.Zoom) + scene.Viewport.PanX;
            var screenY = (midpoint.Y * scene.Viewport.Zoom) + scene.Viewport.PanY;
            var text = $"{connection.Label} • {geometry.Source.EndpointId} -> {geometry.Target.EndpointId}";

            var textBlock = new TextBlock
            {
                Name = $"PART_AuthoringEdgeText_{geometry.ConnectionId}",
                Text = text,
                Foreground = Brush.Parse("#F7FBFD"),
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 220,
            };

            var badge = new Border
            {
                Name = $"PART_AuthoringEdgeBadge_{geometry.ConnectionId}",
                Background = Brush.Parse("#122334"),
                BorderBrush = Brush.Parse(connection.AccentHex),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(10, 6),
                Child = textBlock,
            };

            SetLeft(badge, Math.Max(0d, screenX - 84d));
            SetTop(badge, Math.Max(0d, screenY - 18d));
            Children.Add(badge);
        }
    }

    private static GraphPoint ResolveMidpoint(GraphEditorConnectionGeometrySnapshot geometry)
    {
        var route = geometry.Route;
        var segmentIndex = route.Vertices.Count == 0 ? 0 : route.Vertices.Count / 2;
        return ConnectionPathBuilder.ResolveSegmentMidpoint(
            geometry.Source.Position,
            route,
            geometry.Target.Position,
            segmentIndex);
    }
}
