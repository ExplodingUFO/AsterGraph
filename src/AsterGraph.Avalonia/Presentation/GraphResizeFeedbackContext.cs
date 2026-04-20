namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// Describes the stock resize affordance currently resolved under the pointer.
/// </summary>
public sealed record GraphResizeFeedbackContext(
    GraphResizeFeedbackSurfaceKind SurfaceKind,
    GraphResizeFeedbackHandle Handle);
