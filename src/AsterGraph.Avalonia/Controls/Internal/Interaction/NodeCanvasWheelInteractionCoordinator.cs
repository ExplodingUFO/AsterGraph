using Avalonia;
using Avalonia.Input;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal interface INodeCanvasWheelInteractionHost
{
    GraphEditorViewModel? ViewModel { get; }

    bool EnableDefaultWheelViewportGestures { get; }

    NodeCanvasInteractionSession InteractionSession { get; }
}

internal sealed class NodeCanvasWheelInteractionCoordinator
{
    private const double ZoomFactor = 1.12;
    private const double ScrollSpeedMultiplier = 40.0;
    private readonly INodeCanvasWheelInteractionHost _host;

    public NodeCanvasWheelInteractionCoordinator(INodeCanvasWheelInteractionHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public bool HandleWheel(Point point, Vector delta, KeyModifiers modifiers)
    {
        if (_host.ViewModel is null || !_host.EnableDefaultWheelViewportGestures)
        {
            return false;
        }

        _host.InteractionSession.UpdatePointerPosition(point);

        // 多数精度触控板的捏合会转成带 Control 修饰符的滚轮事件。
        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            var factor = delta.Y >= 0 ? ZoomFactor : 1 / ZoomFactor;
            _host.ViewModel.ZoomAt(factor, new GraphPoint(point.X, point.Y));
        }
        else
        {
            _host.ViewModel.PanBy(delta.X * ScrollSpeedMultiplier, delta.Y * ScrollSpeedMultiplier);
        }

        return true;
    }
}
