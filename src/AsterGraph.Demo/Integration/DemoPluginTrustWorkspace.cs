using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AsterGraph.Editor.Plugins;

namespace AsterGraph.Demo.Integration;

internal sealed record DemoPluginAllowlistEntry(
    string PluginId,
    string DisplayName,
    string? Version,
    string TrustFingerprint,
    string? Publisher,
    string? PackageId,
    string? PackageVersion);

internal sealed class DemoPluginAllowlistStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public DemoPluginAllowlistStore(string storageRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageRootPath);
        Directory.CreateDirectory(storageRootPath);
        StorageRootPath = storageRootPath;
        AllowlistPath = Path.Combine(storageRootPath, "plugin-allowlist.json");
    }

    public string StorageRootPath { get; }

    public string AllowlistPath { get; }

    public IReadOnlyList<DemoPluginAllowlistEntry> Load()
    {
        if (!File.Exists(AllowlistPath))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<DemoPluginAllowlistEntry>>(File.ReadAllText(AllowlistPath), JsonOptions)
                ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Save(IReadOnlyList<DemoPluginAllowlistEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        File.WriteAllText(AllowlistPath, JsonSerializer.Serialize(entries, JsonOptions));
    }

    public bool Export(string path, IReadOnlyList<DemoPluginAllowlistEntry> entries)
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

    public IReadOnlyList<DemoPluginAllowlistEntry> Import(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<DemoPluginAllowlistEntry>>(File.ReadAllText(path), JsonOptions)
                ?? [];
        }
        catch
        {
            return [];
        }
    }
}

internal sealed class DemoPluginAllowlistTrustPolicy : IGraphEditorPluginTrustPolicy
{
    private readonly DemoPluginAllowlistStore _store;
    private readonly Dictionary<string, DemoPluginAllowlistEntry> _entriesByFingerprint = new(StringComparer.OrdinalIgnoreCase);

    public DemoPluginAllowlistTrustPolicy(DemoPluginAllowlistStore store, IReadOnlyList<DemoPluginAllowlistEntry> seedEntries)
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

    public IReadOnlyList<DemoPluginAllowlistEntry> Entries => _entriesByFingerprint.Values.OrderBy(entry => entry.PluginId, StringComparer.Ordinal).ToArray();

    public GraphEditorPluginTrustEvaluation Evaluate(GraphEditorPluginTrustPolicyContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var fingerprint = BuildTrustFingerprint(context.Manifest, context.ProvenanceEvidence);
        if (_entriesByFingerprint.TryGetValue(fingerprint, out var entry))
        {
            return new GraphEditorPluginTrustEvaluation(
                GraphEditorPluginTrustDecision.Allowed,
                GraphEditorPluginTrustEvaluationSource.HostPolicy,
                reasonCode: "demo.allowlist.allowed",
                reasonMessage: $"Host allowlist accepted fingerprint '{entry.TrustFingerprint}' for '{entry.PluginId}'.");
        }

        return new GraphEditorPluginTrustEvaluation(
            GraphEditorPluginTrustDecision.Blocked,
            GraphEditorPluginTrustEvaluationSource.HostPolicy,
            reasonCode: "demo.allowlist.blocked",
            reasonMessage: $"Host allowlist does not contain fingerprint '{fingerprint}' for '{context.Manifest.Id}'.");
    }

    public bool Allow(GraphEditorPluginManifest manifest, GraphEditorPluginProvenanceEvidence provenanceEvidence)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(provenanceEvidence);

        var entry = CreateEntry(manifest, provenanceEvidence);
        _entriesByFingerprint[entry.TrustFingerprint] = entry;
        Persist();
        return true;
    }

    public bool Block(GraphEditorPluginManifest manifest, GraphEditorPluginProvenanceEvidence provenanceEvidence)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(provenanceEvidence);

        var fingerprint = BuildTrustFingerprint(manifest, provenanceEvidence);
        var removed = _entriesByFingerprint.Remove(fingerprint);
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

    private void ReplaceEntries(IReadOnlyList<DemoPluginAllowlistEntry> entries)
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

    public static DemoPluginAllowlistEntry CreateEntry(
        GraphEditorPluginManifest manifest,
        GraphEditorPluginProvenanceEvidence provenanceEvidence)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(provenanceEvidence);

        return new DemoPluginAllowlistEntry(
            manifest.Id,
            manifest.DisplayName,
            manifest.Version,
            BuildTrustFingerprint(manifest, provenanceEvidence),
            manifest.Provenance.Publisher,
            provenanceEvidence.PackageIdentity?.Id ?? manifest.Provenance.PackageId,
            provenanceEvidence.PackageIdentity?.Version ?? manifest.Provenance.PackageVersion ?? manifest.Version);
    }

    public static string BuildTrustFingerprint(
        GraphEditorPluginManifest manifest,
        GraphEditorPluginProvenanceEvidence provenanceEvidence)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(provenanceEvidence);

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
}
