using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AsterGraph.Editor.Plugins;

namespace AsterGraph.ConsumerSample;

public sealed record ConsumerSamplePluginCandidateEntry(
    string PluginId,
    string DisplayName,
    string Version,
    string TargetFramework,
    string TrustFingerprint,
    string TrustReason,
    bool IsAllowed,
    bool IsBlocked,
    bool IsLoaded,
    string LoadState,
    string ManifestLine,
    string GalleryLine,
    string SummaryLine,
    string ProvenanceLine,
    string TrustLine);

internal sealed record ConsumerSamplePluginAllowlistEntry(
    string PluginId,
    string DisplayName,
    string? Version,
    string TrustFingerprint,
    string? Publisher,
    string? PackageId,
    string? PackageVersion);

internal sealed class ConsumerSamplePluginAllowlistStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public ConsumerSamplePluginAllowlistStore(string storageRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageRootPath);
        Directory.CreateDirectory(storageRootPath);
        StorageRootPath = storageRootPath;
        AllowlistPath = Path.Combine(storageRootPath, "plugin-allowlist.json");
    }

    public string StorageRootPath { get; }

    public string AllowlistPath { get; }

    public IReadOnlyList<ConsumerSamplePluginAllowlistEntry> Load()
    {
        if (!File.Exists(AllowlistPath))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<ConsumerSamplePluginAllowlistEntry>>(File.ReadAllText(AllowlistPath), JsonOptions)
                ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Save(IReadOnlyList<ConsumerSamplePluginAllowlistEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        File.WriteAllText(AllowlistPath, JsonSerializer.Serialize(entries, JsonOptions));
    }

    public bool Export(string path, IReadOnlyList<ConsumerSamplePluginAllowlistEntry> entries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(entries);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, JsonSerializer.Serialize(entries, JsonOptions));
        return true;
    }

    public IReadOnlyList<ConsumerSamplePluginAllowlistEntry> Import(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<ConsumerSamplePluginAllowlistEntry>>(File.ReadAllText(path), JsonOptions)
                ?? [];
        }
        catch
        {
            return [];
        }
    }
}

internal sealed class ConsumerSamplePluginAllowlistTrustPolicy : IGraphEditorPluginTrustPolicy
{
    private readonly ConsumerSamplePluginAllowlistStore _store;
    private readonly Dictionary<string, ConsumerSamplePluginAllowlistEntry> _entriesByFingerprint = new(StringComparer.OrdinalIgnoreCase);

    public ConsumerSamplePluginAllowlistTrustPolicy(
        ConsumerSamplePluginAllowlistStore store,
        IReadOnlyList<ConsumerSamplePluginAllowlistEntry> seedEntries)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));

        var persistedEntries = _store.Load();
        if (persistedEntries.Count == 0)
        {
            persistedEntries = seedEntries;
            Persist();
        }

        ReplaceEntries(persistedEntries);
    }

    public IReadOnlyList<ConsumerSamplePluginAllowlistEntry> Entries
        => _entriesByFingerprint.Values.OrderBy(entry => entry.PluginId, StringComparer.Ordinal).ToArray();

    public GraphEditorPluginTrustEvaluation Evaluate(GraphEditorPluginTrustPolicyContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var fingerprint = BuildTrustFingerprint(context.Manifest, context.ProvenanceEvidence);
        if (_entriesByFingerprint.TryGetValue(fingerprint, out var entry))
        {
            return new GraphEditorPluginTrustEvaluation(
                GraphEditorPluginTrustDecision.Allowed,
                GraphEditorPluginTrustEvaluationSource.HostPolicy,
                reasonCode: "consumer.sample.allowlist.allowed",
                reasonMessage: $"Host allowlist accepted fingerprint '{entry.TrustFingerprint}' for '{entry.PluginId}'.");
        }

        return new GraphEditorPluginTrustEvaluation(
            GraphEditorPluginTrustDecision.Blocked,
            GraphEditorPluginTrustEvaluationSource.HostPolicy,
            reasonCode: "consumer.sample.allowlist.blocked",
            reasonMessage: $"Host allowlist does not contain fingerprint '{fingerprint}' for '{context.Manifest.Id}'.");
    }

    public bool Allow(GraphEditorPluginManifest manifest, GraphEditorPluginProvenanceEvidence provenanceEvidence)
    {
        var entry = CreateEntry(manifest, provenanceEvidence);
        _entriesByFingerprint[entry.TrustFingerprint] = entry;
        Persist();
        return true;
    }

    public bool Block(GraphEditorPluginManifest manifest, GraphEditorPluginProvenanceEvidence provenanceEvidence)
    {
        var removed = _entriesByFingerprint.Remove(BuildTrustFingerprint(manifest, provenanceEvidence));
        if (removed)
        {
            Persist();
        }

        return removed;
    }

    public bool Export(string path)
        => _store.Export(path, Entries);

    public bool Import(string path)
    {
        var imported = _store.Import(path);
        if (imported.Count == 0)
        {
            return false;
        }

        ReplaceEntries(imported);
        Persist();
        return true;
    }

    public static ConsumerSamplePluginAllowlistEntry CreateEntry(
        GraphEditorPluginManifest manifest,
        GraphEditorPluginProvenanceEvidence provenanceEvidence)
        => new(
            manifest.Id,
            manifest.DisplayName,
            manifest.Version,
            BuildTrustFingerprint(manifest, provenanceEvidence),
            manifest.Provenance.Publisher,
            provenanceEvidence.PackageIdentity?.Id ?? manifest.Provenance.PackageId,
            provenanceEvidence.PackageIdentity?.Version ?? manifest.Provenance.PackageVersion ?? manifest.Version);

    public static string BuildTrustFingerprint(
        GraphEditorPluginManifest manifest,
        GraphEditorPluginProvenanceEvidence provenanceEvidence)
    {
        var signerFingerprint = provenanceEvidence.Signature.Signer?.Fingerprint ?? string.Empty;
        var packageId = provenanceEvidence.PackageIdentity?.Id ?? manifest.Provenance.PackageId ?? manifest.Id;
        var packageVersion = provenanceEvidence.PackageIdentity?.Version ?? manifest.Provenance.PackageVersion ?? manifest.Version ?? string.Empty;
        var targetFramework = manifest.Compatibility.TargetFramework ?? string.Empty;

        var payload = string.Join(
            "|",
            manifest.Id,
            manifest.DisplayName,
            packageId,
            packageVersion,
            signerFingerprint,
            provenanceEvidence.Signature.Status,
            targetFramework);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }

    private void ReplaceEntries(IReadOnlyList<ConsumerSamplePluginAllowlistEntry> entries)
    {
        _entriesByFingerprint.Clear();
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.TrustFingerprint))
            {
                continue;
            }

            _entriesByFingerprint[entry.TrustFingerprint] = entry;
        }
    }

    private void Persist()
        => _store.Save(Entries);
}
