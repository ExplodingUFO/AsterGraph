namespace AsterGraph.Editor.Plugins.Internal;

internal sealed record AsterGraphPluginPackageArchiveInspection(
    string PackagePath,
    GraphEditorPluginManifest Manifest,
    GraphEditorPluginProvenanceEvidence ProvenanceEvidence,
    string? DeclaredPluginAssemblyPath = null,
    string? DeclaredPluginTypeName = null,
    string? PayloadReasonCode = null,
    string? PayloadReasonMessage = null);
