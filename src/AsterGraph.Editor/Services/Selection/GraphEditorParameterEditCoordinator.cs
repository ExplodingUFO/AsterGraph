using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorParameterEditHost
{
    IReadOnlyList<NodeViewModel> SelectedNodes { get; }

    NodeViewModel? PrimarySelectedNode { get; }

    bool CanEditNodeParameters { get; }

    string StatusText(string key, string fallback, params object?[] arguments);

    void SetStatus(string key, string fallback);

    void MarkDirty(string status);
}

internal sealed class GraphEditorParameterEditCoordinator
{
    private readonly IGraphEditorParameterEditHost _host;

    public GraphEditorParameterEditCoordinator(IGraphEditorParameterEditHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void ApplyParameterValue(NodeParameterViewModel parameter, object? value)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        if (_host.SelectedNodes.Count == 0)
        {
            return;
        }

        if (!_host.CanEditNodeParameters)
        {
            _host.SetStatus("editor.status.parameter.edit.disabledByPermissions", "Parameter editing is disabled by host permissions.");
            return;
        }

        foreach (var node in _host.SelectedNodes)
        {
            node.SetParameterValue(parameter.Key, parameter.TypeId, value);
        }

        var status = _host.SelectedNodes.Count == 1
            ? _host.StatusText("editor.status.parameter.updatedSingle", "Updated {0} / {1}.", _host.PrimarySelectedNode!.Title, parameter.DisplayName)
            : _host.StatusText("editor.status.parameter.updatedMultiple", "Updated {0} nodes / {1}.", _host.SelectedNodes.Count, parameter.DisplayName);
        _host.MarkDirty(status);
    }
}
