using System.IO;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class AsterGraphPluginDiscoveryService
{
    public static IReadOnlyList<GraphEditorPluginCandidateSnapshot> Discover(GraphEditorPluginDiscoveryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var candidates = new List<GraphEditorPluginCandidateSnapshot>();

        foreach (var directorySource in options.DirectorySources ?? [])
        {
            DiscoverDirectorySource(directorySource, options.TrustPolicy, candidates);
        }

        foreach (var manifestSource in options.ManifestSources ?? [])
        {
            DiscoverManifestSource(manifestSource, options.TrustPolicy, candidates);
        }

        return candidates
            .OrderBy(candidate => candidate.SourceKind)
            .ThenBy(candidate => candidate.Source, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.AssemblyPath, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Manifest.Id, StringComparer.Ordinal)
            .ToList();
    }

    private static void DiscoverDirectorySource(
        GraphEditorPluginDirectoryDiscoverySource directorySource,
        IGraphEditorPluginTrustPolicy? trustPolicy,
        ICollection<GraphEditorPluginCandidateSnapshot> candidates)
    {
        ArgumentNullException.ThrowIfNull(directorySource);
        ArgumentNullException.ThrowIfNull(candidates);

        if (!Directory.Exists(directorySource.DirectoryPath))
        {
            return;
        }

        var searchOption = directorySource.IncludeSubdirectories
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        foreach (var assemblyPath in Directory.EnumerateFiles(directorySource.DirectoryPath, directorySource.SearchPattern, searchOption))
        {
            var registration = GraphEditorPluginRegistration.FromAssemblyPath(assemblyPath);
            candidates.Add(CreateCandidateSnapshot(
                GraphEditorPluginCandidateSourceKind.Directory,
                directorySource.DirectoryPath,
                registration,
                trustPolicy));
        }
    }

    private static void DiscoverManifestSource(
        IGraphEditorPluginManifestSource manifestSource,
        IGraphEditorPluginTrustPolicy? trustPolicy,
        ICollection<GraphEditorPluginCandidateSnapshot> candidates)
    {
        ArgumentNullException.ThrowIfNull(manifestSource);
        ArgumentNullException.ThrowIfNull(candidates);

        foreach (var candidate in manifestSource.GetCandidates() ?? [])
        {
            var registration = GraphEditorPluginRegistration.FromAssemblyPath(
                candidate.AssemblyPath,
                candidate.PluginTypeName,
                candidate.Manifest,
                candidate.ProvenanceEvidence);
            candidates.Add(CreateCandidateSnapshot(
                GraphEditorPluginCandidateSourceKind.ManifestSource,
                candidate.Source,
                registration,
                trustPolicy));
        }
    }

    private static GraphEditorPluginCandidateSnapshot CreateCandidateSnapshot(
        GraphEditorPluginCandidateSourceKind sourceKind,
        string source,
        GraphEditorPluginRegistration registration,
        IGraphEditorPluginTrustPolicy? trustPolicy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentNullException.ThrowIfNull(registration);

        var manifest = AsterGraphPluginPreloadEvaluator.ResolveManifest(registration);
        var provenanceEvidence = AsterGraphPluginPreloadEvaluator.ResolveProvenanceEvidence(registration, manifest);
        var compatibility = AsterGraphPluginPreloadEvaluator.EvaluateCompatibility(manifest);
        var trustEvaluation = AsterGraphPluginPreloadEvaluator.EvaluateTrustPolicy(trustPolicy, registration, manifest, provenanceEvidence);

        return new GraphEditorPluginCandidateSnapshot(
            sourceKind,
            source,
            manifest,
            compatibility,
            trustEvaluation,
            provenanceEvidence,
            registration.AssemblyPath,
            registration.PluginTypeName);
    }
}
