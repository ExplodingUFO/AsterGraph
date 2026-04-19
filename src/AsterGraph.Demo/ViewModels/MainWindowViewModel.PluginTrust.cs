using CommunityToolkit.Mvvm.Input;
using AsterGraph.Demo.Integration;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Plugins;

namespace AsterGraph.Demo.ViewModels;

public sealed record PluginCandidateEntry(
    string PluginId,
    string DisplayName,
    string Version,
    string TargetFramework,
    string Source,
    string TrustFingerprint,
    string TrustReason,
    string Publisher,
    string PackageId,
    string PackageVersion,
    string SignerFingerprint,
    bool IsAllowed,
    bool IsBlocked,
    string SummaryLine,
    string ProvenanceLine,
    string TrustLine);

public partial class MainWindowViewModel
{
    private readonly DemoPluginShowcase.DemoPluginShowcaseConfiguration _pluginShowcase;
    private IReadOnlyList<GraphEditorPluginCandidateSnapshot> _pluginCandidates = [];

    public IReadOnlyList<GraphEditorPluginCandidateSnapshot> PluginCandidates => _pluginCandidates;

    public IReadOnlyList<PluginCandidateEntry> PluginCandidateEntries
        => PluginCandidates
            .Select(CreatePluginCandidateEntry)
            .ToArray();

    public IReadOnlyList<string> PluginAllowlistLines
    {
        get
        {
            var entries = _pluginShowcase.TrustPolicy.Entries;
            var lines = new List<string>
            {
                T("Allowlist 决策：", "Allowlist decisions: ") + entries.Count,
                T("Allowlist 导出路径：", "Allowlist export path: ") + ResolvePluginAllowlistExchangePath(),
            };

            if (entries.Count == 0)
            {
                lines.Add(T(
                    "当前没有持久化 allowlist 决策。",
                    "There are no persisted allowlist decisions yet."));
            }
            else
            {
                lines.AddRange(entries.Select(entry =>
                    $"{entry.DisplayName} · {entry.PackageId ?? entry.PluginId}@{entry.PackageVersion ?? entry.Version ?? "?"} · fingerprint {FormatFingerprint(entry.TrustFingerprint)}"));
            }

            return lines;
        }
    }

    public bool TrustPluginCandidate(string pluginId)
    {
        var candidate = FindPluginCandidate(pluginId);
        if (candidate is null)
        {
            return false;
        }

        var changed = _pluginShowcase.TrustPolicy.Allow(candidate.Manifest, candidate.ProvenanceEvidence);
        RefreshPluginCandidates();
        RefreshRuntimeProjection();
        return changed;
    }

    public bool BlockPluginCandidate(string pluginId)
    {
        var candidate = FindPluginCandidate(pluginId);
        if (candidate is null)
        {
            return false;
        }

        var changed = _pluginShowcase.TrustPolicy.Block(candidate.Manifest, candidate.ProvenanceEvidence);
        RefreshPluginCandidates();
        RefreshRuntimeProjection();
        return changed;
    }

    public bool ExportPluginAllowlist(string? path = null)
        => _pluginShowcase.TrustPolicy.Export(string.IsNullOrWhiteSpace(path) ? ResolvePluginAllowlistExchangePath() : path);

    public bool ImportPluginAllowlist(string? path = null)
    {
        var changed = _pluginShowcase.TrustPolicy.Import(string.IsNullOrWhiteSpace(path) ? ResolvePluginAllowlistExchangePath() : path);
        if (!changed)
        {
            return false;
        }

        RefreshPluginCandidates();
        RefreshRuntimeProjection();
        return true;
    }

    [RelayCommand]
    private void TrustPluginCandidateShell(string pluginId)
        => TrustPluginCandidate(pluginId);

    [RelayCommand]
    private void BlockPluginCandidateShell(string pluginId)
        => BlockPluginCandidate(pluginId);

    [RelayCommand]
    private void ExportPluginAllowlistShell()
        => ExportPluginAllowlist();

    [RelayCommand]
    private void ImportPluginAllowlistShell()
        => ImportPluginAllowlist();

    private void RefreshPluginCandidates()
        => _pluginCandidates = AsterGraphEditorFactory.DiscoverPluginCandidates(_pluginShowcase.DiscoveryOptions);

    private GraphEditorPluginCandidateSnapshot? FindPluginCandidate(string pluginId)
        => PluginCandidates.SingleOrDefault(candidate => string.Equals(candidate.Manifest.Id, pluginId, StringComparison.Ordinal));

    private PluginCandidateEntry CreatePluginCandidateEntry(GraphEditorPluginCandidateSnapshot candidate)
    {
        var manifest = candidate.Manifest;
        var provenance = candidate.ProvenanceEvidence;
        var trustFingerprint = DemoPluginAllowlistTrustPolicy.BuildTrustFingerprint(manifest, provenance);
        var version = manifest.Version ?? "?";
        var targetFramework = manifest.Compatibility.TargetFramework ?? "?";
        var publisher = manifest.Provenance.Publisher ?? "?";
        var packageId = provenance.PackageIdentity?.Id ?? manifest.Provenance.PackageId ?? manifest.Id;
        var packageVersion = provenance.PackageIdentity?.Version ?? manifest.Provenance.PackageVersion ?? version;
        var signerFingerprint = provenance.Signature.Signer?.Fingerprint ?? provenance.Signature.Status.ToString();
        var trustReason = candidate.TrustEvaluation.ReasonMessage
            ?? candidate.TrustEvaluation.ReasonCode
            ?? T("无策略说明。", "No policy note.");
        var isAllowed = candidate.TrustEvaluation.Decision == GraphEditorPluginTrustDecision.Allowed;
        var decisionText = TrustDecisionText(candidate.TrustEvaluation.Decision);

        return new PluginCandidateEntry(
            manifest.Id,
            manifest.DisplayName,
            version,
            targetFramework,
            candidate.SourceKind.ToString(),
            trustFingerprint,
            trustReason,
            publisher,
            packageId,
            packageVersion,
            signerFingerprint,
            isAllowed,
            !isAllowed,
            $"{manifest.Id} · {version} · tfm {targetFramework} · {candidate.SourceKind}",
            $"publisher {publisher} · package {packageId}@{packageVersion} · signer {signerFingerprint}",
            $"{decisionText} · fingerprint {FormatFingerprint(trustFingerprint)} · {trustReason}");
    }

    private string ResolvePluginAllowlistExchangePath()
        => Path.Combine(_shellStateStore.StorageRootPath, "plugin-allowlist-export.json");

    private static string FormatFingerprint(string fingerprint)
        => fingerprint.Length <= 12 ? fingerprint : fingerprint[..12] + "…";
}
