using System.IO.Compression;

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

    private static string CreatePackage(
        string directory,
        string packageId,
        string version,
        string title,
        string description,
        bool includeSignatureMarker)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

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

        if (includeSignatureMarker)
        {
            var signatureEntry = archive.CreateEntry(SignatureEntryName);
            using var writer = new StreamWriter(signatureEntry.Open());
            writer.Write("signature-marker");
        }

        return Path.GetFullPath(packagePath);
    }
}
