namespace AsterGraph.Editor.Plugins.Internal;

internal sealed record AsterGraphPluginPreloadEvaluation(
    GraphEditorPluginManifest Manifest,
    GraphEditorPluginCompatibilityEvaluation Compatibility,
    GraphEditorPluginTrustEvaluation TrustEvaluation,
    GraphEditorPluginProvenanceEvidence ProvenanceEvidence)
{
    public bool IsTrustBlocked
        => TrustEvaluation.Decision == GraphEditorPluginTrustDecision.Blocked;

    public bool IsCompatibilityBlocked
        => Compatibility.Status == GraphEditorPluginCompatibilityStatus.Incompatible;
}
