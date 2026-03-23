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

    public void Save(GraphSelectionFragment fragment)
    {
        ArgumentNullException.ThrowIfNull(fragment);

        var directory = Path.GetDirectoryName(FragmentPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(FragmentPath, GraphClipboardPayloadSerializer.Serialize(fragment));
    }

    public GraphSelectionFragment Load()
    {
        var text = File.ReadAllText(FragmentPath);
        if (!GraphClipboardPayloadSerializer.TryDeserialize(text, out var fragment) || fragment is null)
        {
            throw new InvalidOperationException("Failed to deserialize the saved selection fragment.");
        }

        return fragment;
    }

    public bool Exists()
        => File.Exists(FragmentPath);
}
