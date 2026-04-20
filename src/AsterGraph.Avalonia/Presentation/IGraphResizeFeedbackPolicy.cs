using Avalonia.Input;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// Allows hosts to override or extend stock resize-hover feedback for the shipped Avalonia surfaces.
/// </summary>
public interface IGraphResizeFeedbackPolicy
{
    /// <summary>
    /// Resolves the cursor to apply for the given resize affordance.
    /// Returning <see langword="null"/> keeps the stock cursor for that case.
    /// </summary>
    Cursor? ResolveCursor(GraphResizeFeedbackContext context);
}
