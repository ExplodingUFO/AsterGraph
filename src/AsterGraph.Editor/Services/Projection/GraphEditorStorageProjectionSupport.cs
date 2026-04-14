using System.Collections.ObjectModel;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorStorageProjectionHost
{
    bool IsDirty { get; }

    string WorkspacePath { get; }

    ObservableCollection<FragmentTemplateViewModel> FragmentTemplates { get; }

    IGraphFragmentWorkspaceService FragmentWorkspaceService { get; }

    IGraphFragmentLibraryService FragmentLibraryService { get; }

    void SetSelectedFragmentTemplate(FragmentTemplateViewModel? value);

    string LocalizeText(string key, string fallback);

    string LocalizeFormat(string key, string fallback, params object?[] arguments);

    void RaiseComputedPropertyChanges();
}

internal sealed class GraphEditorStorageProjectionSupport
{
    private readonly IGraphEditorStorageProjectionHost _host;

    public GraphEditorStorageProjectionSupport(IGraphEditorStorageProjectionHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public string GetWorkspaceCaption()
    {
        var workspaceState = _host.IsDirty
            ? _host.LocalizeText("editor.workspace.state.unsaved", "Unsaved changes")
            : _host.LocalizeText("editor.workspace.state.synced", "Snapshot synced");
        return _host.LocalizeFormat(
            "editor.workspace.caption",
            "{0}  ·  {1}",
            workspaceState,
            _host.WorkspacePath);
    }

    public string GetFragmentCaption()
    {
        var availability = _host.FragmentWorkspaceService.Exists()
            ? _host.LocalizeText("editor.fragment.state.available", "Fragment available")
            : _host.LocalizeText("editor.fragment.state.missing", "No fragment file");
        return _host.LocalizeFormat(
            "editor.fragment.caption",
            "{0}  ·  {1}",
            availability,
            _host.FragmentWorkspaceService.FragmentPath);
    }

    public string GetFragmentStatusCaption()
        => !_host.FragmentWorkspaceService.Exists()
            ? _host.LocalizeText("editor.fragment.status.missing", "No saved fragment file.")
            : _host.LocalizeFormat(
                "editor.fragment.status.updated",
                "Last updated {0:yyyy-MM-dd HH:mm:ss}",
                File.GetLastWriteTime(_host.FragmentWorkspaceService.FragmentPath));

    public string GetFragmentLibraryCaption()
    {
        var templateState = _host.FragmentTemplates.Count > 0
            ? _host.LocalizeFormat("editor.fragmentLibrary.state.hasTemplates", "{0} templates", _host.FragmentTemplates.Count)
            : _host.LocalizeText("editor.fragmentLibrary.state.noTemplates", "No templates");
        return _host.LocalizeFormat(
            "editor.fragmentLibrary.caption",
            "{0}  ·  {1}",
            templateState,
            _host.FragmentLibraryService.LibraryPath);
    }

    public void RefreshFragmentTemplates()
    {
        _host.FragmentTemplates.Clear();
        foreach (var template in _host.FragmentLibraryService.EnumerateTemplates())
        {
            _host.FragmentTemplates.Add(new FragmentTemplateViewModel(template));
        }

        _host.SetSelectedFragmentTemplate(_host.FragmentTemplates.FirstOrDefault());
        _host.RaiseComputedPropertyChanges();
    }
}
