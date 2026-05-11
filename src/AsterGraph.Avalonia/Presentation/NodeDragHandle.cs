using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// Provides the public attached property used to mark node sub-surfaces that can start a node drag.
/// </summary>
public sealed class NodeDragHandle
{
    /// <summary>
    /// Identifies whether a control is an explicit node drag handle.
    /// </summary>
    public static readonly AttachedProperty<bool> IsDragHandleProperty =
        AvaloniaProperty.RegisterAttached<NodeDragHandle, Control, bool>("IsDragHandle");

    private NodeDragHandle()
    {
    }

    /// <summary>
    /// Marks a control as an explicit node drag handle.
    /// </summary>
    public static void SetIsDragHandle(Control element, bool value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(IsDragHandleProperty, value);
    }

    /// <summary>
    /// Gets whether a control is marked as an explicit node drag handle.
    /// </summary>
    public static bool GetIsDragHandle(Control element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return element.GetValue(IsDragHandleProperty);
    }

    internal static bool ShouldStartNodeDrag(Control nodeRoot, object? source)
    {
        ArgumentNullException.ThrowIfNull(nodeRoot);

        if (!ContainsDragHandle(nodeRoot))
        {
            return true;
        }

        if (source is not Control sourceControl)
        {
            return false;
        }

        for (Visual? current = sourceControl; current is not null; current = current.GetVisualParent())
        {
            if (current is Control currentControl && GetIsDragHandle(currentControl))
            {
                return true;
            }

            if (ReferenceEquals(current, nodeRoot))
            {
                return false;
            }
        }

        return false;
    }

    private static bool ContainsDragHandle(Control nodeRoot)
    {
        if (GetIsDragHandle(nodeRoot))
        {
            return true;
        }

        return nodeRoot
            .GetVisualDescendants()
            .OfType<Control>()
            .Any(GetIsDragHandle);
    }
}
