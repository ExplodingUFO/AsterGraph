using System.ComponentModel;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// Small helper surface for stock-like focus and accessibility semantics.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GraphPresentationSemantics
{
    /// <summary>
    /// Attached focus bridge for custom inspector presenters.
    /// </summary>
    public static readonly AttachedProperty<string?> InspectorEditingParameterKeyProperty =
        AvaloniaProperty.RegisterAttached<GraphPresentationSemantics, Control, string?>(
            "InspectorEditingParameterKey");

    /// <summary>
    /// Applies the stock node-surface accessibility/focus semantics used by the default canvas shell.
    /// </summary>
    public static void ApplyStockNodeSurfaceSemantics(Control surface, string automationName)
    {
        ArgumentNullException.ThrowIfNull(surface);

        if (string.IsNullOrWhiteSpace(automationName))
        {
            throw new ArgumentException("Automation name must be provided.", nameof(automationName));
        }

        surface.Focusable = true;
        surface.IsTabStop = true;
        AutomationProperties.SetName(surface, automationName);
    }

    /// <summary>
    /// Marks a control as the active inspector editing-parameter focus target.
    /// </summary>
    public static void SetInspectorEditingParameterKey(Control element, string? parameterKey)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(InspectorEditingParameterKeyProperty, NormalizeInspectorEditingParameterKey(parameterKey));
    }

    /// <summary>
    /// Resolves the active inspector editing-parameter key from the focused element or its ancestors.
    /// </summary>
    public static string? ResolveInspectorEditingParameterKey(IInputElement? focusedElement)
    {
        AvaloniaObject? current = focusedElement as AvaloniaObject;
        while (current is not null)
        {
            if (current is Control currentControl)
            {
                var bridgeKey = currentControl.GetValue(InspectorEditingParameterKeyProperty);
                if (!string.IsNullOrWhiteSpace(bridgeKey))
                {
                    return bridgeKey.Trim();
                }

                if (currentControl.Classes.Contains("astergraph-parameter-card")
                    && currentControl.Tag is string parameterKey)
                {
                    return parameterKey;
                }
            }

            current = current switch
            {
                Visual visual => visual.GetVisualParent() as AvaloniaObject,
                StyledElement styledElement => styledElement.Parent as AvaloniaObject,
                _ => null,
            };
        }

        return null;
    }

    private static string? NormalizeInspectorEditingParameterKey(string? parameterKey)
        => string.IsNullOrWhiteSpace(parameterKey) ? null : parameterKey.Trim();
}
