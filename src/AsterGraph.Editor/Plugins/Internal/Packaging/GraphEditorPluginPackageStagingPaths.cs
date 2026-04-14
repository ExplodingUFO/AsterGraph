using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class GraphEditorPluginPackageIdentityResolver
{
    public static GraphEditorPluginPackageIdentity Resolve(GraphEditorPluginCandidateSnapshot candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        return candidate.ProvenanceEvidence.PackageIdentity
            ?? (!string.IsNullOrWhiteSpace(candidate.Manifest.Provenance.PackageId)
                ? new GraphEditorPluginPackageIdentity(
                    candidate.Manifest.Provenance.PackageId,
                    candidate.Manifest.Provenance.PackageVersion ?? candidate.Manifest.Version)
                : new GraphEditorPluginPackageIdentity(
                    candidate.Manifest.Id,
                    candidate.Manifest.Version));
    }

    public static string BuildCacheKey(
        GraphEditorPluginPackageIdentity packageIdentity,
        string packagePath,
        GraphEditorPluginCandidateSnapshot candidate)
    {
        ArgumentNullException.ThrowIfNull(packageIdentity);
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentNullException.ThrowIfNull(candidate);

        var fileInfo = new FileInfo(packagePath);
        var signerFingerprint = candidate.ProvenanceEvidence.Signature.Signer?.Fingerprint ?? string.Empty;
        var fingerprint = string.Join(
            "|",
            packageIdentity.Id,
            packageIdentity.Version ?? string.Empty,
            fileInfo.Length.ToString(CultureInfo.InvariantCulture),
            fileInfo.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture),
            candidate.ProvenanceEvidence.Signature.Status,
            candidate.TrustEvaluation.Decision,
            signerFingerprint);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fingerprint))).ToLowerInvariant();
    }
}

internal static class GraphEditorPluginPackageStagingPathPlanner
{
    public static string GetStagingDirectory(
        string stagingRoot,
        GraphEditorPluginPackageIdentity packageIdentity,
        string packagePath,
        GraphEditorPluginCandidateSnapshot candidate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stagingRoot);
        ArgumentNullException.ThrowIfNull(packageIdentity);
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentNullException.ThrowIfNull(candidate);

        var fullRoot = Path.GetFullPath(stagingRoot);
        var cacheKey = GraphEditorPluginPackageIdentityResolver.BuildCacheKey(packageIdentity, packagePath, candidate);

        return Path.Combine(
            fullRoot,
            GraphEditorPluginArchivePathUtility.SanitizeSegment(packageIdentity.Id),
            GraphEditorPluginArchivePathUtility.SanitizeSegment(packageIdentity.Version ?? "unknown-version"),
            cacheKey);
    }

    public static string GetArchiveDirectoryName(string normalizedArchivePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedArchivePath);

        var separatorIndex = normalizedArchivePath.LastIndexOf('/');
        return separatorIndex < 0
            ? string.Empty
            : normalizedArchivePath[..separatorIndex];
    }

    public static string GetDestinationPath(string rootDirectory, string normalizedArchivePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedArchivePath);

        var fullRoot = Path.GetFullPath(rootDirectory);
        var candidatePath = Path.GetFullPath(Path.Combine(fullRoot, normalizedArchivePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!candidatePath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Package archive entry '{normalizedArchivePath}' resolves outside the staging directory.");
        }

        return candidatePath;
    }
}

internal static class GraphEditorPluginArchivePathUtility
{
    public static string Normalize(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var normalized = path.Trim().Replace('\\', '/').Trim('/');
        if (string.IsNullOrWhiteSpace(normalized)
            || normalized.StartsWith("../", StringComparison.Ordinal)
            || normalized.Contains("/../", StringComparison.Ordinal)
            || normalized == ".."
            || Path.IsPathRooted(normalized))
        {
            throw new InvalidDataException($"Package archive entry '{path}' is not a valid relative path.");
        }

        return normalized;
    }

    public static string SanitizeSegment(string segment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(segment);

        var builder = new StringBuilder(segment.Length);
        foreach (var character in segment.Trim())
        {
            builder.Append(Path.GetInvalidFileNameChars().Contains(character) ? '_' : character);
        }

        return builder.Length == 0 ? "_" : builder.ToString();
    }
}
