using AsterGraph.Abstractions.Styling;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Kernel;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Plugins.Internal;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Hosting;

/// <summary>
/// Provides the canonical host composition entry points for the AsterGraph editor runtime.
/// </summary>
/// <remarks>
/// <see cref="CreateSession(AsterGraphEditorOptions)" /> is the runtime-first entry point for custom UI hosts and
/// integrations that want to work directly with <see cref="IGraphEditorSession" />. <see cref="Create(AsterGraphEditorOptions)" />
/// is the hosted-UI entry point that returns the retained <see cref="GraphEditorViewModel" /> facade for the stock Avalonia shell.
/// Direct <see cref="GraphEditorViewModel" /> construction is still supported, but it should be treated as a compatibility path.
/// </remarks>
public static class AsterGraphEditorFactory
{
    /// <summary>
    /// Discovers plugin candidates by using host-supplied discovery sources and trust policy inputs.
    /// </summary>
    /// <param name="options">The plugin discovery options.</param>
    /// <returns>A stable snapshot collection of discovered candidates.</returns>
    public static IReadOnlyList<GraphEditorPluginCandidateSnapshot> DiscoverPluginCandidates(GraphEditorPluginDiscoveryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return AsterGraphPluginDiscoveryService.Discover(options);
    }

    /// <summary>
    /// Starts an explicit plugin-package staging request for a previously discovered candidate.
    /// </summary>
    /// <param name="request">The package staging request.</param>
    /// <returns>A machine-readable staging result that is safe to inspect before loading the plugin.</returns>
    public static GraphEditorPluginPackageStageResult StagePluginPackage(GraphEditorPluginPackageStageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Candidate.PackagePath))
        {
            throw new ArgumentException("Plugin candidate must expose a package path before it can be staged.", nameof(request));
        }

        var stage = AsterGraphPluginPackageStagingService.Stage(request);
        GraphEditorPluginRegistration? registration = null;
        if ((stage.Outcome == GraphEditorPluginStageOutcome.Staged || stage.Outcome == GraphEditorPluginStageOutcome.CacheHit)
            && !string.IsNullOrWhiteSpace(stage.MainAssemblyPath))
        {
            registration = GraphEditorPluginRegistration.FromStagedPackage(
                request.Candidate.PackagePath,
                stage.MainAssemblyPath,
                stage.PluginTypeName,
                request.Candidate.Manifest,
                request.Candidate.ProvenanceEvidence,
                stage);
        }

        return new GraphEditorPluginPackageStageResult(
            stage,
            request.Candidate.Manifest,
            request.Candidate.ProvenanceEvidence,
            request.Candidate.TrustEvaluation,
            registration);
    }

    /// <summary>
    /// Creates a hosted-UI editor facade from host-supplied options.
    /// </summary>
    /// <param name="options">The host composition options.</param>
    /// <returns>A new graph-editor view model.</returns>
    /// <remarks>
    /// New hosted-UI code should start here and then compose the Avalonia shell through
    /// <c>AsterGraphAvaloniaViewFactory.Create(...)</c> or another retained UI surface factory.
    /// The returned object is still a compatibility facade, but its <see cref="GraphEditorViewModel.Session" />
    /// now sits on top of the shared runtime boundary.
    /// </remarks>
    public static GraphEditorViewModel Create(AsterGraphEditorOptions options)
    {
        var resolved = Resolve(options);

        var editor = new GraphEditorViewModel(
            resolved.Options.Document!,
            resolved.NodeCatalog,
            resolved.Options.CompatibilityService!,
            resolved.WorkspaceService,
            resolved.FragmentWorkspaceService,
            resolved.StyleOptions,
            resolved.BehaviorOptions,
            resolved.FragmentLibraryService,
            resolved.Options.ContextMenuAugmentor,
            resolved.NodePresentationProvider,
            resolved.LocalizationProvider,
            resolved.ClipboardPayloadSerializer,
            resolved.Options.DiagnosticsSink);

        if (editor.Session is GraphEditorSession runtimeSession)
        {
            runtimeSession.ConfigureInstrumentation(resolved.Options.Instrumentation);
            runtimeSession.SetPluginCommandContributors(resolved.PluginCommandContributors);
            runtimeSession.SetPluginLoadSnapshots(resolved.PluginLoadResult.Snapshots);
            runtimeSession.SetPluginTrustPolicyConfigured(resolved.Options.PluginTrustPolicy is not null);
            PublishDiagnostics(runtimeSession, resolved.PluginLoadResult.Diagnostics);
        }

        return editor;
    }

    /// <summary>
    /// Creates an <see cref="IGraphEditorSession" /> from host-supplied options.
    /// </summary>
    /// <param name="options">The host composition options.</param>
    /// <returns>A new graph-editor runtime session.</returns>
    /// <remarks>
    /// This is the canonical entry point for custom UI hosts and for plugin or automation-first integrations.
    /// Unlike <see cref="Create(AsterGraphEditorOptions)" />, this route does not carry the retained
    /// <see cref="GraphEditorViewModel" /> surface or its compatibility-only helpers.
    /// </remarks>
    public static IGraphEditorSession CreateSession(AsterGraphEditorOptions options)
    {
        var resolved = Resolve(options);
        var kernel = new GraphEditorKernel(
            resolved.Options.Document!,
            resolved.NodeCatalog,
            resolved.Options.CompatibilityService!,
            resolved.WorkspaceService,
            resolved.StyleOptions,
            resolved.BehaviorOptions);
        var session = new GraphEditorSession(
            kernel,
            resolved.Options.DiagnosticsSink,
            new GraphEditorSessionDescriptorSupport(
                resolved.NodeCatalog,
                (key, fallback) => resolved.LocalizationProvider?.GetString(key, fallback) ?? fallback,
                hasFragmentWorkspaceService: true,
                hasFragmentLibraryService: true,
                hasClipboardPayloadSerializer: true,
                hasPluginLoader: true,
                hasPluginTrustPolicy: resolved.Options.PluginTrustPolicy is not null,
                hasCommandContributor: resolved.PluginCommandContributors.Count > 0,
                hasContextMenuAugmentor: resolved.Options.ContextMenuAugmentor is not null,
                hasNodePresentationProvider: () => resolved.NodePresentationProvider is not null,
                hasLocalizationProvider: () => resolved.LocalizationProvider is not null));
        session.ConfigureInstrumentation(resolved.Options.Instrumentation);
        session.SetPluginCommandContributors(resolved.PluginCommandContributors);
        session.SetPluginLoadSnapshots(resolved.PluginLoadResult.Snapshots);
        session.SetPluginTrustPolicyConfigured(resolved.Options.PluginTrustPolicy is not null);
        PublishDiagnostics(session, resolved.PluginLoadResult.Diagnostics);
        return session;
    }

    private static ResolvedEditorOptions Resolve(AsterGraphEditorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Document);
        ArgumentNullException.ThrowIfNull(options.NodeCatalog);
        ArgumentNullException.ThrowIfNull(options.CompatibilityService);

        var clipboardPayloadSerializer = options.ClipboardPayloadSerializer ?? new GraphClipboardPayloadSerializer();
        var workspaceService = options.WorkspaceService ?? new GraphWorkspaceService(GraphEditorStorageDefaults.GetWorkspacePath(options.StorageRootPath));
        var fragmentWorkspaceService = options.FragmentWorkspaceService ?? new GraphFragmentWorkspaceService(
            GraphEditorStorageDefaults.GetFragmentPath(options.StorageRootPath),
            clipboardPayloadSerializer);
        var fragmentLibraryService = options.FragmentLibraryService ?? new GraphFragmentLibraryService(
            GraphEditorStorageDefaults.GetFragmentLibraryPath(options.StorageRootPath),
            clipboardPayloadSerializer);
        var styleOptions = options.StyleOptions ?? GraphEditorStyleOptions.Default;
        var behaviorOptions = ResolveBehaviorOptions(options.BehaviorOptions, styleOptions);
        var pluginLoadResult = AsterGraphPluginLoader.Load(options.PluginRegistrations, options.PluginTrustPolicy);
        var nodeCatalog = ComposeNodeCatalog(options.NodeCatalog, pluginLoadResult);
        var localizationProvider = ComposeLocalizationProvider(options.LocalizationProvider, pluginLoadResult);
        var nodePresentationProvider = ComposeNodePresentationProvider(options.NodePresentationProvider, pluginLoadResult);

        return new ResolvedEditorOptions(
            options,
            clipboardPayloadSerializer,
            workspaceService,
            fragmentWorkspaceService,
            fragmentLibraryService,
            styleOptions,
            behaviorOptions,
            pluginLoadResult,
            nodeCatalog,
            nodePresentationProvider,
            localizationProvider,
            pluginLoadResult.Contributions.CommandContributors.ToList());
    }

    private static GraphEditorBehaviorOptions ResolveBehaviorOptions(
        GraphEditorBehaviorOptions? behaviorOptions,
        GraphEditorStyleOptions styleOptions)
    {
        if (behaviorOptions is not null)
        {
            return behaviorOptions;
        }

        return GraphEditorBehaviorOptions.Default with
        {
            DragAssist = GraphEditorBehaviorOptions.Default.DragAssist with
            {
                EnableGridSnapping = styleOptions.Canvas.EnableGridSnapping,
                EnableAlignmentGuides = styleOptions.Canvas.EnableAlignmentGuides,
                SnapTolerance = styleOptions.Canvas.SnapTolerance,
            },
        };
    }

    private static void PublishDiagnostics(GraphEditorSession session, IReadOnlyList<GraphEditorDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(diagnostics);

        foreach (var diagnostic in diagnostics)
        {
            session.PublishDiagnostic(diagnostic);
        }
    }

    private static AsterGraph.Abstractions.Catalog.INodeCatalog ComposeNodeCatalog(
        AsterGraph.Abstractions.Catalog.INodeCatalog baseCatalog,
        GraphEditorPluginLoadResult pluginLoadResult)
    {
        ArgumentNullException.ThrowIfNull(baseCatalog);
        ArgumentNullException.ThrowIfNull(pluginLoadResult);

        return pluginLoadResult.Contributions.NodeDefinitionProviders.Count == 0
            ? baseCatalog
            : new PluginComposedNodeCatalog(baseCatalog, pluginLoadResult.Contributions.NodeDefinitionProviders);
    }

    private static IGraphLocalizationProvider? ComposeLocalizationProvider(
        IGraphLocalizationProvider? hostProvider,
        GraphEditorPluginLoadResult pluginLoadResult)
    {
        ArgumentNullException.ThrowIfNull(pluginLoadResult);
        return CompositePluginLocalizationProvider.Compose(pluginLoadResult.Contributions.LocalizationProviders, hostProvider);
    }

    private static INodePresentationProvider? ComposeNodePresentationProvider(
        INodePresentationProvider? hostProvider,
        GraphEditorPluginLoadResult pluginLoadResult)
    {
        ArgumentNullException.ThrowIfNull(pluginLoadResult);
        return CompositePluginNodePresentationProvider.Compose(pluginLoadResult.Contributions.NodePresentationProviders, hostProvider);
    }

    private sealed record ResolvedEditorOptions(
        AsterGraphEditorOptions Options,
        IGraphClipboardPayloadSerializer ClipboardPayloadSerializer,
        IGraphWorkspaceService WorkspaceService,
        IGraphFragmentWorkspaceService FragmentWorkspaceService,
        IGraphFragmentLibraryService FragmentLibraryService,
        GraphEditorStyleOptions StyleOptions,
        GraphEditorBehaviorOptions BehaviorOptions,
        GraphEditorPluginLoadResult PluginLoadResult,
        AsterGraph.Abstractions.Catalog.INodeCatalog NodeCatalog,
        INodePresentationProvider? NodePresentationProvider,
        IGraphLocalizationProvider? LocalizationProvider,
        IReadOnlyList<AsterGraph.Editor.Plugins.IGraphEditorPluginCommandContributor> PluginCommandContributors);
}
