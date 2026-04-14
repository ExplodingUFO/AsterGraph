using System.IO.Compression;

namespace AsterGraph.Editor.Plugins.Internal;

internal sealed record GraphEditorPluginArchiveEntry(string OriginalPath, string NormalizedPath);

internal static class GraphEditorPluginPackageArchivePlan
{
    public static IReadOnlyList<GraphEditorPluginArchiveEntry> FromPackagePayload(string packagePath, string declaredPluginAssemblyPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(declaredPluginAssemblyPath);

        using var archive = ZipFile.OpenRead(packagePath);
        var normalizedAssemblyPath = GraphEditorPluginArchivePathUtility.Normalize(declaredPluginAssemblyPath);
        var normalizedDirectory = GraphEditorPluginPackageStagingPathPlanner.GetArchiveDirectoryName(normalizedAssemblyPath);

        return archive.Entries
            .Where(entry => !string.IsNullOrEmpty(entry.Name))
            .Select(entry => new GraphEditorPluginArchiveEntry(entry.FullName, GraphEditorPluginArchivePathUtility.Normalize(entry.FullName)))
            .Where(entry => string.Equals(GraphEditorPluginPackageStagingPathPlanner.GetArchiveDirectoryName(entry.NormalizedPath), normalizedDirectory, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}

internal static class GraphEditorPluginPackageArchiveStager
{
    public static void ExtractEntries(string packagePath, string stagingDirectory, IReadOnlyList<GraphEditorPluginArchiveEntry> stagedEntries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(stagingDirectory);
        ArgumentNullException.ThrowIfNull(stagedEntries);

        var fullStagingDirectory = Path.GetFullPath(stagingDirectory);
        var parentDirectory = Path.GetDirectoryName(fullStagingDirectory)
            ?? throw new InvalidOperationException("Staging directory must have a parent directory.");
        Directory.CreateDirectory(parentDirectory);

        var temporaryDirectory = Path.Combine(parentDirectory, $".tmp-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            using var archive = ZipFile.OpenRead(packagePath);
            foreach (var stagedEntry in stagedEntries)
            {
                var sourceEntry = archive.GetEntry(stagedEntry.OriginalPath)
                    ?? throw new InvalidDataException($"Package archive entry '{stagedEntry.OriginalPath}' was not found.");
                var destinationPath = GraphEditorPluginPackageStagingPathPlanner.GetDestinationPath(temporaryDirectory, stagedEntry.NormalizedPath);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                using var sourceStream = sourceEntry.Open();
                using var destinationStream = File.Create(destinationPath);
                sourceStream.CopyTo(destinationStream);
            }

            if (Directory.Exists(fullStagingDirectory))
            {
                Directory.Delete(fullStagingDirectory, recursive: true);
            }

            Directory.Move(temporaryDirectory, fullStagingDirectory);
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
    }
}

internal static class GraphEditorPluginPackageStagingCacheEvaluator
{
    public static bool IsCacheHit(string stagingDirectory, IReadOnlyList<GraphEditorPluginArchiveEntry> stagedEntries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stagingDirectory);
        ArgumentNullException.ThrowIfNull(stagedEntries);

        if (!Directory.Exists(stagingDirectory))
        {
            return false;
        }

        return stagedEntries.All(entry => File.Exists(GraphEditorPluginPackageStagingPathPlanner.GetDestinationPath(stagingDirectory, entry.NormalizedPath)));
    }
}
