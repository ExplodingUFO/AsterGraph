using AsterGraph.Editor.Diagnostics;

namespace AsterGraph.Editor.ViewModels;

internal sealed class GraphEditorFragmentWorkspaceCommands
{
    private readonly GraphEditorViewModel.IGraphEditorFragmentCommandHost _host;
    private readonly GraphEditorFragmentTransferSupport _transferSupport;

    public GraphEditorFragmentWorkspaceCommands(
        GraphEditorViewModel.IGraphEditorFragmentCommandHost host,
        GraphEditorFragmentTransferSupport transferSupport)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _transferSupport = transferSupport ?? throw new ArgumentNullException(nameof(transferSupport));
    }

    public void ExportSelectionFragment()
    {
        if (!_host.CommandPermissions.Fragments.AllowExport)
        {
            _host.SetStatus("editor.status.fragment.export.disabledByPermissions", "Fragment export is disabled by host permissions.");
            return;
        }

        var fragment = _transferSupport.CreateSelectionFragment();
        if (fragment is null)
        {
            _host.SetStatus("editor.status.fragment.export.selectNodeFirst", "Select at least one node before exporting a fragment.");
            return;
        }

        _host.FragmentWorkspaceService.Save(fragment);
        _host.RaiseComputedPropertyChanges();
        var status = _host.SetStatus("editor.status.fragment.export.savedToPath", "Exported fragment to {0}.", _host.FragmentWorkspaceService.FragmentPath);
        _host.PublishRuntimeDiagnostic(
            "fragment.export.succeeded",
            "fragment.export",
            status,
            GraphEditorDiagnosticSeverity.Info);
        _host.RaiseFragmentExported(_host.FragmentWorkspaceService.FragmentPath, fragment);
    }

    public bool ExportSelectionFragmentTo(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!_host.CommandPermissions.Fragments.AllowExport)
        {
            _host.SetStatus("editor.status.fragment.export.disabledByPermissions", "Fragment export is disabled by host permissions.");
            return false;
        }

        var fragment = _transferSupport.CreateSelectionFragment();
        if (fragment is null)
        {
            _host.SetStatus("editor.status.fragment.export.selectNodeFirst", "Select at least one node before exporting a fragment.");
            return false;
        }

        _host.FragmentWorkspaceService.Save(fragment, path);
        _host.SetStatus("editor.status.fragment.export.savedToPath", "Exported fragment to {0}.", path);
        _host.RaiseFragmentExported(path, fragment);
        return true;
    }

    public void ImportFragment()
    {
        if (!_host.CommandPermissions.Fragments.AllowImport)
        {
            _host.SetStatus("editor.status.fragment.import.disabledByPermissions", "Fragment import is disabled by host permissions.");
            return;
        }

        if (!_host.FragmentWorkspaceService.Exists())
        {
            var status = _host.SetStatus("editor.status.fragment.import.noExportedFile", "No exported fragment file is available yet.");
            _host.PublishRuntimeDiagnostic(
                "fragment.import.missing",
                "fragment.import",
                status,
                GraphEditorDiagnosticSeverity.Warning);
            return;
        }

        var fragment = _host.FragmentWorkspaceService.Load();
        _host.StoreSelectionClipboard(fragment);
        _host.RaiseComputedPropertyChanges();

        var importedStatus = _transferSupport.PasteFragment(fragment, "Imported");
        if (importedStatus is null)
        {
            var status = _host.SetStatus("editor.status.fragment.import.noNodesInFile", "Fragment file did not contain any nodes.");
            _host.PublishRuntimeDiagnostic(
                "fragment.import.empty",
                "fragment.import",
                status,
                GraphEditorDiagnosticSeverity.Warning);
        }
        else
        {
            _host.PublishRuntimeDiagnostic(
                "fragment.import.succeeded",
                "fragment.import",
                importedStatus,
                GraphEditorDiagnosticSeverity.Info);
            _host.RaiseFragmentImported(_host.FragmentWorkspaceService.FragmentPath, fragment);
        }
    }

    public void ClearFragment()
    {
        if (!_host.CommandPermissions.Fragments.AllowClearWorkspaceFragment)
        {
            _host.SetStatus("editor.status.fragment.clear.disabledByPermissions", "Fragment clearing is disabled by host permissions.");
            return;
        }

        if (!_host.FragmentWorkspaceService.Exists())
        {
            _host.SetStatus("editor.status.fragment.import.noExportedFile", "No exported fragment file is available yet.");
            return;
        }

        _host.FragmentWorkspaceService.Delete();
        _host.RaiseComputedPropertyChanges();
        _host.SetStatus("editor.status.fragment.clear.cleared", "Cleared the saved fragment file.");
    }

    public bool ImportFragmentFrom(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!_host.CommandPermissions.Fragments.AllowImport)
        {
            _host.SetStatus("editor.status.fragment.import.disabledByPermissions", "Fragment import is disabled by host permissions.");
            return false;
        }

        if (!_host.FragmentWorkspaceService.Exists(path))
        {
            var status = _host.SetStatus("editor.status.fragment.import.fileNotFound", "Fragment file '{0}' was not found.", path);
            _host.PublishRuntimeDiagnostic(
                "fragment.import.fileMissing",
                "fragment.import",
                status,
                GraphEditorDiagnosticSeverity.Warning);
            return false;
        }

        var fragment = _host.FragmentWorkspaceService.Load(path);
        _host.StoreSelectionClipboard(fragment);
        _host.RaiseComputedPropertyChanges();
        var importedStatus = _transferSupport.PasteFragment(fragment, "Imported");
        if (importedStatus is not null)
        {
            _host.PublishRuntimeDiagnostic(
                "fragment.import.succeeded",
                "fragment.import",
                importedStatus,
                GraphEditorDiagnosticSeverity.Info);
            _host.RaiseFragmentImported(path, fragment);
        }

        return importedStatus is not null;
    }
}
