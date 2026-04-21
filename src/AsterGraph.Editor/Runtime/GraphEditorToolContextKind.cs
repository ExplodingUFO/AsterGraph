namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Identifies which authoring surface is requesting contextual tools.
/// </summary>
public enum GraphEditorToolContextKind
{
    Selection = 0,
    Node = 1,
    Connection = 2,
}
