using NuGet.Packaging;
using System.Text.Json;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class AsterGraphPluginPackageArchiveInspector
{
    private const string PackageSignatureEntryName = ".signature.p7s";
    private const string PluginMetadataEntryName = "astergraph.plugin.json";

    public static AsterGraphPluginPackageArchiveInspection Inspect(string packagePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);

        var fullPath = Path.GetFullPath(packagePath);
        using var stream = File.OpenRead(fullPath);
        using var reader = new PackageArchiveReader(stream);
        var nuspec = reader.NuspecReader;
        var packageFiles = reader.GetFiles().ToList();

        var packageId = nuspec.GetId();
        var version = nuspec.GetVersion()?.ToNormalizedString();
        var displayName = FirstNonEmpty(nuspec.GetTitle(), packageId) ?? packageId;
        var description = Normalize(nuspec.GetDescription());
        var publisher = Normalize(nuspec.GetAuthors());
        var hasPackageSignature = packageFiles.Any(file => string.Equals(file, PackageSignatureEntryName, StringComparison.OrdinalIgnoreCase));
        var payload = ReadDeclaredPayload(reader, packageFiles);

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
                signature),
            payload.DeclaredPluginAssemblyPath,
            payload.DeclaredPluginTypeName,
            payload.ReasonCode,
            payload.ReasonMessage);
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values
            .Select(Normalize)
            .FirstOrDefault(value => value is not null);

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();

    private static DeclaredPayload ReadDeclaredPayload(PackageArchiveReader reader, IReadOnlyCollection<string> packageFiles)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(packageFiles);

        var metadataEntry = packageFiles.FirstOrDefault(file => string.Equals(file, PluginMetadataEntryName, StringComparison.OrdinalIgnoreCase));
        if (metadataEntry is null)
        {
            return new DeclaredPayload(
                null,
                null,
                "stage.payload.not-declared",
                "Package archive does not contain a root 'astergraph.plugin.json' payload declaration.");
        }

        try
        {
            using var metadataStream = reader.GetStream(metadataEntry);
            using var document = JsonDocument.Parse(metadataStream);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return InvalidPayload("Plugin payload metadata must be a JSON object.");
            }

            if (!TryGetRequiredString(document.RootElement, "pluginAssembly", out var pluginAssembly, out var pluginAssemblyError))
            {
                return InvalidPayload(pluginAssemblyError);
            }

            var normalizedAssemblyPath = NormalizeArchivePath(pluginAssembly!);
            if (!packageFiles.Any(file => string.Equals(NormalizeArchivePath(file), normalizedAssemblyPath, StringComparison.OrdinalIgnoreCase)))
            {
                return InvalidPayload($"Declared plugin assembly '{normalizedAssemblyPath}' was not found in the package archive.");
            }

            string? pluginTypeName = null;
            if (document.RootElement.TryGetProperty("pluginTypeName", out var pluginTypeNameElement))
            {
                if (!TryGetOptionalString(pluginTypeNameElement, out pluginTypeName))
                {
                    return InvalidPayload("Declared plugin type name must be a non-empty JSON string when provided.");
                }
            }

            return new DeclaredPayload(normalizedAssemblyPath, pluginTypeName, null, null);
        }
        catch (JsonException exception)
        {
            return InvalidPayload($"Plugin payload metadata could not be parsed: {exception.Message}");
        }
        catch (InvalidDataException exception)
        {
            return InvalidPayload(exception.Message);
        }
    }

    private static bool TryGetRequiredString(
        JsonElement element,
        string propertyName,
        out string? value,
        out string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        value = null;
        errorMessage = string.Empty;

        if (!element.TryGetProperty(propertyName, out var property))
        {
            errorMessage = $"Plugin payload metadata is missing required '{propertyName}' field.";
            return false;
        }

        if (!TryGetOptionalString(property, out value))
        {
            errorMessage = $"Plugin payload metadata field '{propertyName}' must be a non-empty JSON string.";
            return false;
        }

        return true;
    }

    private static bool TryGetOptionalString(JsonElement element, out string? value)
    {
        value = null;
        if (element.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var text = Normalize(element.GetString());
        if (text is null)
        {
            return false;
        }

        value = text;
        return true;
    }

    private static string NormalizeArchivePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var normalized = path.Trim().Replace('\\', '/').Trim('/');
        if (string.IsNullOrWhiteSpace(normalized)
            || normalized.StartsWith("../", StringComparison.Ordinal)
            || normalized.Contains("/../", StringComparison.Ordinal)
            || normalized == ".."
            || Path.IsPathRooted(normalized))
        {
            throw new InvalidDataException($"Plugin payload metadata declared invalid archive path '{path}'.");
        }

        return normalized;
    }

    private static DeclaredPayload InvalidPayload(string message)
        => new(
            null,
            null,
            "stage.payload.invalid",
            message);

    private sealed record DeclaredPayload(
        string? DeclaredPluginAssemblyPath,
        string? DeclaredPluginTypeName,
        string? ReasonCode,
        string? ReasonMessage);
}
