using AsterGraph.Editor.Services;

namespace AsterGraph.Demo.ViewModels;

public sealed record MainWindowShellOptions(
    string? StorageRootPath = null,
    bool EnableStatePersistence = false,
    bool RestoreLastWorkspaceOnStartup = false,
    string? SaveAsWorkspacePath = null,
    string? InitialScenario = null)
{
    public static MainWindowShellOptions CreatePersistentDefault()
        => new(
            StorageRootPath: Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AsterGraph.Demo"),
            EnableStatePersistence: true,
            RestoreLastWorkspaceOnStartup: true,
            SaveAsWorkspacePath: null);

    internal string ResolveStorageRootPath()
    {
        if (!string.IsNullOrWhiteSpace(StorageRootPath))
        {
            Directory.CreateDirectory(StorageRootPath);
            return StorageRootPath;
        }

        var root = EnableStatePersistence
            ? Path.Combine(Path.GetTempPath(), "AsterGraph.Demo")
            : Path.Combine(Path.GetTempPath(), "AsterGraph.Demo.Tests", Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);
        return root;
    }

    internal string ResolveDefaultWorkspacePath(string storageRootPath)
        => GraphEditorStorageDefaults.GetWorkspacePath(storageRootPath);
}
