namespace AsterGraph.Editor.ViewModels;

internal sealed class GraphEditorFragmentTemplateCommands
{
    private readonly IGraphEditorFragmentCommandHost _host;
    private readonly GraphEditorFragmentTransferSupport _transferSupport;
    private readonly GraphEditorFragmentWorkspaceCommands _workspaceCommands;

    public GraphEditorFragmentTemplateCommands(
        IGraphEditorFragmentCommandHost host,
        GraphEditorFragmentTransferSupport transferSupport,
        GraphEditorFragmentWorkspaceCommands workspaceCommands)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _transferSupport = transferSupport ?? throw new ArgumentNullException(nameof(transferSupport));
        _workspaceCommands = workspaceCommands ?? throw new ArgumentNullException(nameof(workspaceCommands));
    }

    public void ExportSelectionAsTemplate()
    {
        if (!_host.CommandPermissions.Fragments.AllowTemplateManagement || !_host.CommandPermissions.Fragments.AllowExport)
        {
            _host.SetStatus("editor.status.fragmentTemplate.export.disabledByPermissions", "Template export is disabled by host permissions.");
            return;
        }

        if (!_host.BehaviorOptions.Fragments.EnableFragmentLibrary)
        {
            _host.SetStatus("editor.status.fragmentTemplate.library.disabled", "Fragment template library is disabled.");
            return;
        }

        var fragment = _transferSupport.CreateSelectionFragment();
        if (fragment is null)
        {
            _host.SetStatus("editor.status.fragmentTemplate.export.selectNodeFirst", "Select at least one node before exporting a fragment template.");
            return;
        }

        var templateName = _host.SelectedNodeTitle ?? $"selection-{fragment.Nodes.Count}";
        var path = _host.FragmentLibraryService.SaveTemplate(fragment, templateName);
        _host.RefreshFragmentTemplates();
        _host.SetStatus("editor.status.fragmentTemplate.export.savedToPath", "Exported fragment template to {0}.", path);
        _host.RaiseFragmentExported(path, fragment);
    }

    public void ImportSelectedTemplate()
    {
        if (!_host.CommandPermissions.Fragments.AllowTemplateManagement || !_host.CommandPermissions.Fragments.AllowImport)
        {
            _host.SetStatus("editor.status.fragmentTemplate.import.disabledByPermissions", "Template import is disabled by host permissions.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_host.SelectedFragmentTemplatePath))
        {
            _host.SetStatus("editor.status.fragmentTemplate.selectTemplateFirst", "Select a fragment template first.");
            return;
        }

        _ = _workspaceCommands.ImportFragmentFrom(_host.SelectedFragmentTemplatePath);
    }

    public void DeleteSelectedTemplate()
    {
        if (!_host.CommandPermissions.Fragments.AllowTemplateManagement)
        {
            _host.SetStatus("editor.status.fragmentTemplate.delete.disabledByPermissions", "Template deletion is disabled by host permissions.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_host.SelectedFragmentTemplatePath))
        {
            _host.SetStatus("editor.status.fragmentTemplate.selectTemplateFirst", "Select a fragment template first.");
            return;
        }

        var deletedPath = _host.SelectedFragmentTemplatePath;
        _host.FragmentLibraryService.DeleteTemplate(deletedPath);
        _host.RefreshFragmentTemplates();
        _host.SetStatus(
            "editor.status.fragmentTemplate.deleted",
            "Deleted fragment template {0}.",
            Path.GetFileNameWithoutExtension(deletedPath));
    }
}
