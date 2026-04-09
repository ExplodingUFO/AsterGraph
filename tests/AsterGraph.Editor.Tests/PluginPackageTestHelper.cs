using System.IO.Compression;
using System.Text;

namespace AsterGraph.Editor.Tests;

internal static class PluginPackageTestHelper
{
    private const string SignatureEntryName = ".signature.p7s";

    public static string CreateUnsignedPackage(
        string directory,
        string packageId,
        string version,
        string title,
        string description)
        => CreatePackage(directory, packageId, version, title, description, includeSignatureMarker: false);

    public static string CreateSignedMarkerPackage(
        string directory,
        string packageId,
        string version,
        string title,
        string description)
        => CreatePackage(directory, packageId, version, title, description, includeSignatureMarker: true);

    public static string CreateUnsignedPackageWithPluginPayload(
        string directory,
        string packageId,
        string version,
        string title,
        string description,
        string pluginAssemblyPath,
        string? pluginTypeName = null,
        string packagePayloadDirectory = "plugin",
        bool includePluginMetadata = true,
        string? pluginAssemblyMetadataOverride = null)
        => CreatePackageWithPluginPayload(
            directory,
            packageId,
            version,
            title,
            description,
            pluginAssemblyPath,
            pluginTypeName,
            packagePayloadDirectory,
            includeSignatureMarker: false,
            includePluginMetadata,
            pluginAssemblyMetadataOverride);

    public static string CreateSignedMarkerPackageWithPluginPayload(
        string directory,
        string packageId,
        string version,
        string title,
        string description,
        string pluginAssemblyPath,
        string? pluginTypeName = null,
        string packagePayloadDirectory = "plugin",
        bool includePluginMetadata = true,
        string? pluginAssemblyMetadataOverride = null)
        => CreatePackageWithPluginPayload(
            directory,
            packageId,
            version,
            title,
            description,
            pluginAssemblyPath,
            pluginTypeName,
            packagePayloadDirectory,
            includeSignatureMarker: true,
            includePluginMetadata,
            pluginAssemblyMetadataOverride);

    private static string CreatePackage(
        string directory,
        string packageId,
        string version,
        string title,
        string description,
        bool includeSignatureMarker)
        => CreatePackage(
            directory,
            packageId,
            version,
            title,
            description,
            includeSignatureMarker,
            additionalTextEntries: new Dictionary<string, string>(StringComparer.Ordinal),
            additionalFileEntries: new Dictionary<string, string>(StringComparer.Ordinal));

    private static string CreatePackageWithPluginPayload(
        string directory,
        string packageId,
        string version,
        string title,
        string description,
        string pluginAssemblyPath,
        string? pluginTypeName,
        string packagePayloadDirectory,
        bool includeSignatureMarker,
        bool includePluginMetadata,
        string? pluginAssemblyMetadataOverride)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginAssemblyPath);

        var pluginFiles = GetPluginPayloadFiles(pluginAssemblyPath, packagePayloadDirectory);
        var textEntries = new Dictionary<string, string>(StringComparer.Ordinal);

        if (includePluginMetadata)
        {
            var metadataPluginAssembly = pluginAssemblyMetadataOverride
                ?? JoinEntryPath(packagePayloadDirectory, Path.GetFileName(pluginAssemblyPath));
            textEntries["astergraph.plugin.json"] = BuildPluginMetadata(metadataPluginAssembly, pluginTypeName);
        }

        return CreatePackage(
            directory,
            packageId,
            version,
            title,
            description,
            includeSignatureMarker,
            textEntries,
            pluginFiles);
    }

    private static string CreatePackage(
        string directory,
        string packageId,
        string version,
        string title,
        string description,
        bool includeSignatureMarker,
        IReadOnlyDictionary<string, string> additionalTextEntries,
        IReadOnlyDictionary<string, string> additionalFileEntries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(additionalTextEntries);
        ArgumentNullException.ThrowIfNull(additionalFileEntries);

        Directory.CreateDirectory(directory);
        var packagePath = Path.Combine(directory, $"{packageId}.{version}.nupkg");

        using var stream = File.Create(packagePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

        var nuspecEntry = archive.CreateEntry($"{packageId}.nuspec");
        using (var writer = new StreamWriter(nuspecEntry.Open()))
        {
            writer.Write($$"""
                <?xml version="1.0" encoding="utf-8"?>
                <package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
                  <metadata>
                    <id>{{packageId}}</id>
                    <version>{{version}}</version>
                    <title>{{title}}</title>
                    <authors>AsterGraph Tests</authors>
                    <description>{{description}}</description>
                  </metadata>
                </package>
                """);
        }

        foreach (var entry in additionalTextEntries)
        {
            var packageEntry = archive.CreateEntry(entry.Key);
            using var writer = new StreamWriter(packageEntry.Open(), Encoding.UTF8);
            writer.Write(entry.Value);
        }

        foreach (var entry in additionalFileEntries)
        {
            var packageEntry = archive.CreateEntry(entry.Key);
            using var packageStream = packageEntry.Open();
            using var sourceStream = File.OpenRead(entry.Value);
            sourceStream.CopyTo(packageStream);
        }

        if (includeSignatureMarker)
        {
            var signatureEntry = archive.CreateEntry(SignatureEntryName);
            using var writer = new StreamWriter(signatureEntry.Open());
            writer.Write("signature-marker");
        }

        return Path.GetFullPath(packagePath);
    }

    private static IReadOnlyDictionary<string, string> GetPluginPayloadFiles(string pluginAssemblyPath, string packagePayloadDirectory)
    {
        var fullAssemblyPath = Path.GetFullPath(pluginAssemblyPath);
        var assemblyDirectory = Path.GetDirectoryName(fullAssemblyPath)
            ?? throw new InvalidOperationException("Plugin assembly path must resolve to a directory.");
        var assemblyName = Path.GetFileNameWithoutExtension(fullAssemblyPath);
        var files = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var fileName in new[]
                 {
                     $"{assemblyName}.dll",
                     $"{assemblyName}.deps.json",
                     $"{assemblyName}.runtimeconfig.json",
                     $"{assemblyName}.pdb",
                     $"{assemblyName}.xml",
                 })
        {
            var filePath = Path.Combine(assemblyDirectory, fileName);
            if (File.Exists(filePath))
            {
                files[JoinEntryPath(packagePayloadDirectory, fileName)] = filePath;
            }
        }

        if (files.Count == 0)
        {
            throw new FileNotFoundException("No plugin payload files were found for the specified assembly path.", fullAssemblyPath);
        }

        return files;
    }

    private static string BuildPluginMetadata(string pluginAssemblyPath, string? pluginTypeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginAssemblyPath);

        var quotedAssembly = pluginAssemblyPath.Replace("\\", "/").Replace("\"", "\\\"", StringComparison.Ordinal);
        var quotedTypeName = string.IsNullOrWhiteSpace(pluginTypeName)
            ? null
            : pluginTypeName.Trim().Replace("\"", "\\\"", StringComparison.Ordinal);

        return quotedTypeName is null
            ? $$"""
                {
                  "pluginAssembly": "{{quotedAssembly}}"
                }
                """
            : $$"""
                {
                  "pluginAssembly": "{{quotedAssembly}}",
                  "pluginTypeName": "{{quotedTypeName}}"
                }
                """;
    }

    private static string JoinEntryPath(string packagePayloadDirectory, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var normalizedDirectory = string.IsNullOrWhiteSpace(packagePayloadDirectory)
            ? null
            : packagePayloadDirectory.Trim().Replace('\\', '/').Trim('/');
        return string.IsNullOrWhiteSpace(normalizedDirectory)
            ? fileName
            : $"{normalizedDirectory}/{fileName}";
    }
}
