using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.Kernel.Internal;

internal interface IGraphEditorKernelFragmentStorageHost
{
    GraphEditorCommandPermissions CommandPermissions { get; }

    GraphEditorBehaviorOptions BehaviorOptions { get; }

    IGraphFragmentWorkspaceService FragmentWorkspaceService { get; }

    IGraphFragmentLibraryService FragmentLibraryService { get; }

    string? SelectedNodeTitle { get; }

    void SetStatus(string statusMessage);

    void RaiseFragmentExported(string path, GraphSelectionFragment fragment);

    void RaiseFragmentImported(string path, GraphSelectionFragment fragment);
}

internal sealed class GraphEditorKernelFragmentStorageCoordinator
{
    private readonly IGraphEditorKernelFragmentStorageHost _host;
    private readonly GraphEditorKernelClipboardCoordinator _clipboardCoordinator;

    public GraphEditorKernelFragmentStorageCoordinator(
        IGraphEditorKernelFragmentStorageHost host,
        GraphEditorKernelClipboardCoordinator clipboardCoordinator)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _clipboardCoordinator = clipboardCoordinator ?? throw new ArgumentNullException(nameof(clipboardCoordinator));
    }

    public GraphEditorFragmentStorageSnapshot GetStorageSnapshot()
    {
        var resolvedPath = _host.FragmentWorkspaceService.FragmentPath;
        var hasWorkspaceFragment = _host.FragmentWorkspaceService.Exists();
        return new GraphEditorFragmentStorageSnapshot(
            resolvedPath,
            hasWorkspaceFragment,
            hasWorkspaceFragment && File.Exists(resolvedPath)
                ? File.GetLastWriteTime(resolvedPath)
                : null,
            _host.FragmentLibraryService.LibraryPath,
            _host.BehaviorOptions.Fragments.EnableFragmentLibrary,
            CanExportSelectionFragment,
            CanImportFragment,
            CanClearWorkspaceFragment,
            CanExportSelectionAsTemplate,
            CanImportFragmentTemplate,
            CanDeleteFragmentTemplate);
    }

    public IReadOnlyList<GraphEditorFragmentTemplateSnapshot> GetTemplateSnapshots()
        => _host.FragmentLibraryService.EnumerateTemplates()
            .Select(template => new GraphEditorFragmentTemplateSnapshot(
                template.Name,
                template.Path,
                template.NodeCount,
                template.ConnectionCount,
                template.LastModified))
            .OrderByDescending(template => template.LastModified)
            .ThenBy(template => template.Name, StringComparer.Ordinal)
            .ToList();

    public bool TryExportSelectionFragment(string? path)
    {
        if (!CanExportSelectionFragment)
        {
            _host.SetStatus("Fragment export is disabled by host permissions.");
            return false;
        }

        var fragment = _clipboardCoordinator.CreateSelectionFragment();
        if (fragment is null)
        {
            _host.SetStatus("Select at least one node before exporting a fragment.");
            return false;
        }

        var resolvedPath = ResolveWorkspacePath(path);
        _host.FragmentWorkspaceService.Save(fragment, path);
        _host.SetStatus($"Exported fragment to {resolvedPath}.");
        _host.RaiseFragmentExported(resolvedPath, fragment);
        return true;
    }

    public bool TryImportFragment(string? path)
    {
        if (!CanImportFragment)
        {
            _host.SetStatus("Fragment import is disabled by host permissions.");
            return false;
        }

        var resolvedPath = ResolveWorkspacePath(path);
        if (!_host.FragmentWorkspaceService.Exists(path))
        {
            _host.SetStatus($"Fragment file '{resolvedPath}' was not found.");
            return false;
        }

        var fragment = _host.FragmentWorkspaceService.Load(path);
        if (!_clipboardCoordinator.TryPasteFragment(fragment, "Imported"))
        {
            return false;
        }

        _host.RaiseFragmentImported(resolvedPath, fragment);
        return true;
    }

    public bool TryClearWorkspaceFragment(string? path)
    {
        if (!CanClearWorkspaceFragment)
        {
            _host.SetStatus("Fragment clearing is disabled by host permissions.");
            return false;
        }

        var resolvedPath = ResolveWorkspacePath(path);
        if (!_host.FragmentWorkspaceService.Exists(path))
        {
            _host.SetStatus($"Fragment file '{resolvedPath}' was not found.");
            return false;
        }

        _host.FragmentWorkspaceService.Delete(path);
        _host.SetStatus($"Cleared fragment file {resolvedPath}.");
        return true;
    }

    public string TryExportSelectionAsTemplate(string? name)
    {
        if (!CanExportSelectionAsTemplate)
        {
            _host.SetStatus("Template export is disabled by host permissions.");
            return string.Empty;
        }

        var fragment = _clipboardCoordinator.CreateSelectionFragment();
        if (fragment is null)
        {
            _host.SetStatus("Select at least one node before exporting a fragment template.");
            return string.Empty;
        }

        var templateName = string.IsNullOrWhiteSpace(name)
            ? _host.SelectedNodeTitle ?? $"selection-{fragment.Nodes.Count}"
            : name.Trim();
        var savedPath = _host.FragmentLibraryService.SaveTemplate(fragment, templateName);
        _host.SetStatus($"Exported fragment template to {savedPath}.");
        _host.RaiseFragmentExported(savedPath, fragment);
        return savedPath;
    }

    public bool TryImportFragmentTemplate(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!CanImportFragmentTemplate)
        {
            _host.SetStatus("Template import is disabled by host permissions.");
            return false;
        }

        if (!_host.BehaviorOptions.Fragments.EnableFragmentLibrary)
        {
            _host.SetStatus("Fragment template library is disabled.");
            return false;
        }

        var fragment = _host.FragmentLibraryService.LoadTemplate(path);
        if (!_clipboardCoordinator.TryPasteFragment(fragment, "Imported"))
        {
            return false;
        }

        _host.RaiseFragmentImported(path, fragment);
        return true;
    }

    public bool TryDeleteFragmentTemplate(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!CanDeleteFragmentTemplate)
        {
            _host.SetStatus("Template deletion is disabled by host permissions.");
            return false;
        }

        if (!_host.BehaviorOptions.Fragments.EnableFragmentLibrary)
        {
            _host.SetStatus("Fragment template library is disabled.");
            return false;
        }

        _host.FragmentLibraryService.DeleteTemplate(path);
        _host.SetStatus($"Deleted fragment template {Path.GetFileNameWithoutExtension(path)}.");
        return true;
    }

    private bool CanExportSelectionFragment
        => _host.CommandPermissions.Fragments.AllowExport;

    private bool CanImportFragment
        => _host.CommandPermissions.Fragments.AllowImport
        && _host.CommandPermissions.Nodes.AllowCreate;

    private bool CanClearWorkspaceFragment
        => _host.CommandPermissions.Fragments.AllowClearWorkspaceFragment;

    private bool CanExportSelectionAsTemplate
        => _host.CommandPermissions.Fragments.AllowTemplateManagement
        && _host.CommandPermissions.Fragments.AllowExport
        && _host.BehaviorOptions.Fragments.EnableFragmentLibrary;

    private bool CanImportFragmentTemplate
        => _host.CommandPermissions.Fragments.AllowTemplateManagement
        && _host.CommandPermissions.Fragments.AllowImport
        && _host.CommandPermissions.Nodes.AllowCreate;

    private bool CanDeleteFragmentTemplate
        => _host.CommandPermissions.Fragments.AllowTemplateManagement;

    private string ResolveWorkspacePath(string? path)
        => string.IsNullOrWhiteSpace(path)
            ? _host.FragmentWorkspaceService.FragmentPath
            : path.Trim();
}
