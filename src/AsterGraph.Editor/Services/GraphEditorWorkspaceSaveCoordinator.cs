using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorWorkspaceSaveCoordinatorHost
{
    bool CanSaveWorkspace { get; }

    string WorkspacePath { get; }

    GraphDocument CreateWorkspaceDocumentSnapshot();

    void SaveWorkspaceDocument(GraphDocument document);

    string CreateWorkspaceDocumentSignature(GraphDocument document);

    void SetWorkspaceSaveDisabledStatus();

    void HandleWorkspaceSaveSucceeded(string signature, string statusMessage);

    void HandleWorkspaceSaveFailed(string statusMessage, Exception exception);

    void CompleteWorkspaceSaveAttempt();
}

internal sealed class GraphEditorWorkspaceSaveCoordinator
{
    private readonly IGraphEditorWorkspaceSaveCoordinatorHost _host;

    public GraphEditorWorkspaceSaveCoordinator(IGraphEditorWorkspaceSaveCoordinatorHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void SaveWorkspace()
    {
        if (!_host.CanSaveWorkspace)
        {
            _host.SetWorkspaceSaveDisabledStatus();
            return;
        }

        try
        {
            var document = _host.CreateWorkspaceDocumentSnapshot();
            _host.SaveWorkspaceDocument(document);
            var signature = _host.CreateWorkspaceDocumentSignature(document);
            _host.HandleWorkspaceSaveSucceeded(signature, $"Saved snapshot to {_host.WorkspacePath}.");
        }
        catch (Exception exception)
        {
            _host.HandleWorkspaceSaveFailed($"Save failed: {exception.Message}", exception);
        }

        _host.CompleteWorkspaceSaveAttempt();
    }
}
