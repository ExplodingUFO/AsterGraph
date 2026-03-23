namespace AsterGraph.Editor.Services;

internal sealed class GraphFragmentWorkspaceService
{
    public GraphFragmentWorkspaceService(string? fragmentPath = null)
    {
        FragmentPath = fragmentPath ?? GetDefaultFragmentPath();
    }

    public string FragmentPath { get; }

    public static string GetDefaultFragmentPath()
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AsterGraphDemo",
            "selection-fragment.json");

    public void Save(GraphSelectionFragment fragment, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(fragment);

        var resolvedPath = ResolvePath(path);
        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(resolvedPath, GraphClipboardPayloadSerializer.Serialize(fragment));
    }

    public GraphSelectionFragment Load(string? path = null)
    {
        var text = File.ReadAllText(ResolvePath(path));
        if (!GraphClipboardPayloadSerializer.TryDeserialize(text, out var fragment) || fragment is null)
        {
            throw new InvalidOperationException("Failed to deserialize the saved selection fragment.");
        }

        return fragment;
    }

    public bool Exists(string? path = null)
        => File.Exists(ResolvePath(path));

    private string ResolvePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? FragmentPath : path.Trim();
}
