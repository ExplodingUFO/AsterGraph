using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;
using AsterGraph.Editor.Services;

namespace AsterGraph.Demo.Shell;

internal sealed class DemoMutableWorkspaceService : IGraphWorkspaceService
{
    public DemoMutableWorkspaceService(string workspacePath)
    {
        SetWorkspacePath(workspacePath);
    }

    public string WorkspacePath { get; private set; } = string.Empty;

    public void SetWorkspacePath(string workspacePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspacePath);
        WorkspacePath = workspacePath;
    }

    public void Save(GraphDocument document)
        => GraphDocumentSerializer.Save(document, WorkspacePath);

    public GraphDocument Load()
        => GraphDocumentSerializer.Load(WorkspacePath);

    public bool Exists()
        => File.Exists(WorkspacePath);
}
