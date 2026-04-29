using System.ComponentModel;

namespace AsterGraph.Editor.Viewport;

/// <summary>
/// Immutable zoom and pan state for the editor viewport.
/// </summary>
/// <param name="Zoom">Current zoom factor.</param>
/// <param name="PanX">Horizontal pan offset in screen space.</param>
/// <param name="PanY">Vertical pan offset in screen space.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly record struct ViewportState(double Zoom, double PanX, double PanY);
