using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorWorkspaceLoadCoordinatorHost
{
    bool CanLoadWorkspace { get; }

    bool WorkspaceExists { get; }

    string WorkspacePath { get; }

    GraphDocument LoadWorkspaceDocument();

    void SetWorkspaceLoadDisabledStatus();

    void HandleWorkspaceLoadMissing(string statusMessage);

    void ApplyLoadedWorkspaceDocument(GraphDocument document, string statusMessage);

    void HandleWorkspaceLoadSucceeded(string diagnosticMessage);

    void HandleWorkspaceLoadFailed(string statusMessage, Exception exception);
}

internal sealed class GraphEditorWorkspaceLoadCoordinator
{
    private readonly IGraphEditorWorkspaceLoadCoordinatorHost _host;

    public GraphEditorWorkspaceLoadCoordinator(IGraphEditorWorkspaceLoadCoordinatorHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public bool LoadWorkspace()
    {
        if (!_host.CanLoadWorkspace)
        {
            _host.SetWorkspaceLoadDisabledStatus();
            return false;
        }

        try
        {
            if (!_host.WorkspaceExists)
            {
                _host.HandleWorkspaceLoadMissing("No saved snapshot yet. Save once to create one.");
                return false;
            }

            var document = _host.LoadWorkspaceDocument();
            _host.ApplyLoadedWorkspaceDocument(document, "Workspace loaded from disk.");
            _host.HandleWorkspaceLoadSucceeded(_host.WorkspacePath);
            return true;
        }
        catch (Exception exception)
        {
            _host.HandleWorkspaceLoadFailed($"Load failed: {exception.Message}", exception);
            return false;
        }
    }
}
