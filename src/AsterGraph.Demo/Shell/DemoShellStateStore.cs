using System.Text.Json;
using System.Text.Json.Serialization;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Demo.Shell;

internal sealed class DemoShellStateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };

    public DemoShellStateStore(string storageRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageRootPath);

        StorageRootPath = storageRootPath;
        StatePath = Path.Combine(storageRootPath, "shell-state.json");
        AutosaveDraftPath = Path.Combine(storageRootPath, "autosave-draft.json");
    }

    public string StorageRootPath { get; }

    public string StatePath { get; }

    public string AutosaveDraftPath { get; }

    public DemoShellState Load()
    {
        if (!File.Exists(StatePath))
        {
            return DemoShellState.Empty;
        }

        try
        {
            return JsonSerializer.Deserialize<DemoShellState>(File.ReadAllText(StatePath), JsonOptions)
                ?? DemoShellState.Empty;
        }
        catch
        {
            return DemoShellState.Empty;
        }
    }

    public void Save(DemoShellState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        Directory.CreateDirectory(StorageRootPath);
        File.WriteAllText(StatePath, JsonSerializer.Serialize(state, JsonOptions));
    }

    public bool HasAutosaveDraft()
        => File.Exists(AutosaveDraftPath);

    public void DeleteAutosaveDraft()
    {
        if (File.Exists(AutosaveDraftPath))
        {
            File.Delete(AutosaveDraftPath);
        }
    }
}

internal sealed record DemoShellState(
    string? LastWorkspacePath,
    IReadOnlyList<string> RecentWorkspacePaths,
    bool ReopenLastWorkspaceOnStartup,
    bool IsHeaderChromeVisible,
    bool IsLibraryChromeVisible,
    bool IsInspectorChromeVisible,
    bool IsStatusChromeVisible,
    bool IsMiniMapVisible,
    bool IsHostPaneOpen,
    double WindowWidth,
    double WindowHeight,
    GraphEditorViewportSnapshot? Viewport,
    string? ThemeVariant)
{
    public static DemoShellState Empty { get; } = new(
        LastWorkspacePath: null,
        RecentWorkspacePaths: [],
        ReopenLastWorkspaceOnStartup: false,
        IsHeaderChromeVisible: false,
        IsLibraryChromeVisible: false,
        IsInspectorChromeVisible: false,
        IsStatusChromeVisible: false,
        IsMiniMapVisible: true,
        IsHostPaneOpen: false,
        WindowWidth: 1480,
        WindowHeight: 900,
        Viewport: null,
        ThemeVariant: null);
}
