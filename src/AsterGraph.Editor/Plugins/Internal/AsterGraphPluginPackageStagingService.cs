using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class AsterGraphPluginPackageStagingService
{
    public static GraphEditorPluginStageSnapshot Stage(GraphEditorPluginPackageStageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var candidate = request.Candidate;
        if (string.IsNullOrWhiteSpace(candidate.PackagePath))
        {
            throw new ArgumentException("Plugin candidate must expose a package path before it can be staged.", nameof(request));
        }

        var packagePath = Path.GetFullPath(candidate.PackagePath);
        var packageIdentity = ResolvePackageIdentity(candidate);

        try
        {
            var trustRefusal = GetTrustRefusal(candidate, packagePath, packageIdentity);
            if (trustRefusal is not null)
            {
                return trustRefusal;
            }

            var signatureRefusal = GetSignatureRefusal(candidate, packagePath, packageIdentity);
            if (signatureRefusal is not null)
            {
                return signatureRefusal;
            }

            var inspection = AsterGraphPluginPackageArchiveInspector.Inspect(packagePath);
            if (string.IsNullOrWhiteSpace(inspection.DeclaredPluginAssemblyPath))
            {
                return new GraphEditorPluginStageSnapshot(
                    GraphEditorPluginStageOutcome.Refused,
                    packagePath,
                    packageIdentity,
                    pluginTypeName: null,
                    usedCache: false,
                    reasonCode: inspection.PayloadReasonCode ?? "stage.payload.not-declared",
                    reasonMessage: inspection.PayloadReasonMessage ?? "Package archive did not declare a staged plugin payload.");
            }

            var stagingRoot = string.IsNullOrWhiteSpace(request.StagingRootPath)
                ? GraphEditorStorageDefaults.GetPluginStagingPath()
                : request.StagingRootPath;
            var stagingDirectory = GetStagingDirectory(stagingRoot, packageIdentity, packagePath, candidate);
            var stagedEntries = GetStagedEntries(packagePath, inspection.DeclaredPluginAssemblyPath);
            var mainAssemblyPath = GetDestinationPath(stagingDirectory, inspection.DeclaredPluginAssemblyPath);

            if (IsCacheHit(stagingDirectory, stagedEntries))
            {
                return new GraphEditorPluginStageSnapshot(
                    GraphEditorPluginStageOutcome.CacheHit,
                    packagePath,
                    packageIdentity,
                    stagingDirectory,
                    mainAssemblyPath,
                    inspection.DeclaredPluginTypeName,
                    usedCache: true);
            }

            ExtractEntries(packagePath, stagingDirectory, stagedEntries);

            return new GraphEditorPluginStageSnapshot(
                GraphEditorPluginStageOutcome.Staged,
                packagePath,
                packageIdentity,
                stagingDirectory,
                mainAssemblyPath,
                inspection.DeclaredPluginTypeName,
                usedCache: false);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            return new GraphEditorPluginStageSnapshot(
                GraphEditorPluginStageOutcome.Failed,
                packagePath,
                packageIdentity,
                pluginTypeName: null,
                usedCache: false,
                reasonCode: "stage.failed",
                reasonMessage: $"Failed to stage package '{packagePath}': {exception.Message}");
        }
    }

    private static GraphEditorPluginStageSnapshot? GetTrustRefusal(
        GraphEditorPluginCandidateSnapshot candidate,
        string packagePath,
        GraphEditorPluginPackageIdentity packageIdentity)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentNullException.ThrowIfNull(packageIdentity);

        if (candidate.TrustEvaluation.Decision != GraphEditorPluginTrustDecision.Blocked)
        {
            return null;
        }

        return new GraphEditorPluginStageSnapshot(
            GraphEditorPluginStageOutcome.Refused,
            packagePath,
            packageIdentity,
            pluginTypeName: candidate.PluginTypeName,
            usedCache: false,
            reasonCode: candidate.TrustEvaluation.ReasonCode ?? "stage.trust.blocked",
            reasonMessage: candidate.TrustEvaluation.ReasonMessage ?? "Plugin trust policy blocked verified package staging.");
    }

    private static GraphEditorPluginStageSnapshot? GetSignatureRefusal(
        GraphEditorPluginCandidateSnapshot candidate,
        string packagePath,
        GraphEditorPluginPackageIdentity packageIdentity)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentNullException.ThrowIfNull(packageIdentity);

        var signature = candidate.ProvenanceEvidence.Signature;
        if (signature.Status == GraphEditorPluginSignatureStatus.Valid)
        {
            return null;
        }

        var reasonCode = signature.Status switch
        {
            GraphEditorPluginSignatureStatus.NotProvided => "stage.signature.not-provided",
            GraphEditorPluginSignatureStatus.Unknown => "stage.signature.unknown",
            GraphEditorPluginSignatureStatus.Unsigned => "stage.signature.unsigned",
            GraphEditorPluginSignatureStatus.Invalid => "stage.signature.invalid",
            _ => "stage.signature.not-verified",
        };
        var reasonMessage = signature.ReasonMessage
            ?? signature.Status switch
            {
                GraphEditorPluginSignatureStatus.NotProvided => "Plugin package candidate did not provide signature evidence required for verified staging.",
                GraphEditorPluginSignatureStatus.Unknown => "Plugin package signature could not be verified for staging.",
                GraphEditorPluginSignatureStatus.Unsigned => "Plugin package is unsigned and cannot be staged.",
                GraphEditorPluginSignatureStatus.Invalid => "Plugin package signature is invalid and cannot be staged.",
                _ => "Plugin package signature is not in a verified state for staging.",
            };

        return new GraphEditorPluginStageSnapshot(
            GraphEditorPluginStageOutcome.Refused,
            packagePath,
            packageIdentity,
            pluginTypeName: candidate.PluginTypeName,
            usedCache: false,
            reasonCode: signature.ReasonCode ?? reasonCode,
            reasonMessage: reasonMessage);
    }

    private static GraphEditorPluginPackageIdentity ResolvePackageIdentity(GraphEditorPluginCandidateSnapshot candidate)
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

    private static string GetStagingDirectory(
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
        var cacheKey = BuildCacheKey(packageIdentity, packagePath, candidate);

        return Path.Combine(
            fullRoot,
            SanitizePathSegment(packageIdentity.Id),
            SanitizePathSegment(packageIdentity.Version ?? "unknown-version"),
            cacheKey);
    }

    private static string BuildCacheKey(
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

    private static string SanitizePathSegment(string segment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(segment);

        var builder = new StringBuilder(segment.Length);
        foreach (var character in segment.Trim())
        {
            builder.Append(Path.GetInvalidFileNameChars().Contains(character) ? '_' : character);
        }

        return builder.Length == 0 ? "_" : builder.ToString();
    }

    private static IReadOnlyList<ArchiveEntryInfo> GetStagedEntries(string packagePath, string declaredPluginAssemblyPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(declaredPluginAssemblyPath);

        using var archive = ZipFile.OpenRead(packagePath);
        var normalizedAssemblyPath = NormalizeArchivePath(declaredPluginAssemblyPath);
        var normalizedDirectory = GetArchiveDirectoryName(normalizedAssemblyPath);

        return archive.Entries
            .Where(entry => !string.IsNullOrEmpty(entry.Name))
            .Select(entry => new ArchiveEntryInfo(entry.FullName, NormalizeArchivePath(entry.FullName)))
            .Where(entry => string.Equals(GetArchiveDirectoryName(entry.NormalizedPath), normalizedDirectory, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static bool IsCacheHit(string stagingDirectory, IReadOnlyList<ArchiveEntryInfo> stagedEntries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stagingDirectory);
        ArgumentNullException.ThrowIfNull(stagedEntries);

        if (!Directory.Exists(stagingDirectory))
        {
            return false;
        }

        return stagedEntries.All(entry => File.Exists(GetDestinationPath(stagingDirectory, entry.NormalizedPath)));
    }

    private static void ExtractEntries(string packagePath, string stagingDirectory, IReadOnlyList<ArchiveEntryInfo> stagedEntries)
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
                var destinationPath = GetDestinationPath(temporaryDirectory, stagedEntry.NormalizedPath);
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

    private static string GetDestinationPath(string rootDirectory, string normalizedArchivePath)
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
            throw new InvalidDataException($"Package archive entry '{path}' is not a valid relative path.");
        }

        return normalized;
    }

    private static string GetArchiveDirectoryName(string normalizedArchivePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedArchivePath);

        var separatorIndex = normalizedArchivePath.LastIndexOf('/');
        return separatorIndex < 0
            ? string.Empty
            : normalizedArchivePath[..separatorIndex];
    }

    private sealed record ArchiveEntryInfo(string OriginalPath, string NormalizedPath);
}
