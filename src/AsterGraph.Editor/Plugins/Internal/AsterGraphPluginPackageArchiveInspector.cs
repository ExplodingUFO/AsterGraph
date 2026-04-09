using NuGet.Packaging;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class AsterGraphPluginPackageArchiveInspector
{
    private const string PackageSignatureEntryName = ".signature.p7s";

    public static AsterGraphPluginPackageArchiveInspection Inspect(string packagePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);

        var fullPath = Path.GetFullPath(packagePath);
        using var stream = File.OpenRead(fullPath);
        using var reader = new PackageArchiveReader(stream);
        var nuspec = reader.NuspecReader;

        var packageId = nuspec.GetId();
        var version = nuspec.GetVersion()?.ToNormalizedString();
        var displayName = FirstNonEmpty(nuspec.GetTitle(), packageId) ?? packageId;
        var description = Normalize(nuspec.GetDescription());
        var publisher = Normalize(nuspec.GetAuthors());
        var hasPackageSignature = reader.GetFiles().Any(file => string.Equals(file, PackageSignatureEntryName, StringComparison.OrdinalIgnoreCase));

        var manifest = new GraphEditorPluginManifest(
            packageId,
            displayName,
            new GraphEditorPluginManifestProvenance(
                GraphEditorPluginManifestSourceKind.PackageArchive,
                fullPath,
                publisher: publisher,
                packageId: packageId,
                packageVersion: version),
            description: description,
            version: version);

        var signature = hasPackageSignature
            ? new GraphEditorPluginSignatureEvidence(
                GraphEditorPluginSignatureStatus.Unknown,
                reasonCode: "signature.validation.unavailable",
                reasonMessage: "Package signature presence was detected, but signature verification is not implemented yet.")
            : new GraphEditorPluginSignatureEvidence(
                GraphEditorPluginSignatureStatus.Unsigned,
                reasonCode: "signature.unsigned",
                reasonMessage: "Package archive does not contain a NuGet package signature.");

        return new AsterGraphPluginPackageArchiveInspection(
            fullPath,
            manifest,
            new GraphEditorPluginProvenanceEvidence(
                new GraphEditorPluginPackageIdentity(packageId, version),
                signature));
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values
            .Select(Normalize)
            .FirstOrDefault(value => value is not null);

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
