using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;

namespace AsterGraph.Editor.Services;

public sealed class GraphWorkspaceService
{
    public GraphWorkspaceService(string? workspacePath = null)
    {
        WorkspacePath = workspacePath ?? GetDefaultWorkspacePath();
    }

    public string WorkspacePath { get; }

    public static string GetDefaultWorkspacePath()
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AsterGraphDemo",
            "demo-graph.json");

    public void Save(GraphDocument document)
        => GraphDocumentSerializer.Save(document, WorkspacePath);

    public GraphDocument Load()
        => GraphDocumentSerializer.Load(WorkspacePath);

    public bool Exists()
        => File.Exists(WorkspacePath);
}
