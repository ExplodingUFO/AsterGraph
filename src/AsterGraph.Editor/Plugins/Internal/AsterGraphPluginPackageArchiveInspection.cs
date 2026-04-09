namespace AsterGraph.Editor.Plugins.Internal;

internal sealed record AsterGraphPluginPackageArchiveInspection(
    string PackagePath,
    GraphEditorPluginManifest Manifest,
    GraphEditorPluginProvenanceEvidence ProvenanceEvidence);
