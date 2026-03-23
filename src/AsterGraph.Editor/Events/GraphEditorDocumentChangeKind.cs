namespace AsterGraph.Editor.Events;

/// <summary>
/// 编辑器文档变化类型。
/// </summary>
public enum GraphEditorDocumentChangeKind
{
    NodesAdded,
    NodesRemoved,
    ConnectionsChanged,
    LayoutChanged,
    ParametersChanged,
    FragmentPasted,
    WorkspaceLoaded,
    WorkspaceSaved,
    Undo,
    Redo,
}
