using System.ComponentModel;

namespace AsterGraph.Editor.ViewModels;

/// <summary>
/// Indicates which node-local inline affordance should be shown for an input row at the current surface tier.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public enum NodeSurfaceInlineContentKind
{
    None = 0,
    Summary = 1,
    Editor = 2,
    Connection = 3,
}
