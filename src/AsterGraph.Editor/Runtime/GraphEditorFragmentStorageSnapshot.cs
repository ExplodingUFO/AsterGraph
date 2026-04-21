namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable host-facing snapshot of fragment workspace and template-library storage state.
/// </summary>
public sealed record GraphEditorFragmentStorageSnapshot(
    string WorkspaceFragmentPath,
    bool HasWorkspaceFragment,
    DateTime? WorkspaceFragmentLastModified,
    string TemplateLibraryPath,
    bool IsTemplateLibraryEnabled,
    bool CanExportSelectionFragment,
    bool CanImportFragment,
    bool CanClearWorkspaceFragment,
    bool CanExportSelectionAsTemplate,
    bool CanImportFragmentTemplate,
    bool CanDeleteFragmentTemplate);
