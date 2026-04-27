using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.Hosting;

/// <summary>
/// Defines the host inputs required by <see cref="AsterGraphEditorFactory" /> when composing an editor runtime.
/// </summary>
/// <remarks>
/// This options contract configures both the canonical <see cref="AsterGraphEditorFactory.CreateSession(AsterGraphEditorOptions)" />
/// route and the hosted-UI <see cref="AsterGraphEditorFactory.Create(AsterGraphEditorOptions)" /> helper.
/// Direct <c>new GraphEditorViewModel(...)</c> usage remains supported as a retained compatibility path for incremental migration.
/// </remarks>
public sealed record AsterGraphEditorOptions
{
    /// <summary>
    /// The initial graph document loaded into the editor.
    /// </summary>
    public GraphDocument? Document { get; init; }

    /// <summary>
    /// The node catalog visible to the editor.
    /// </summary>
    public INodeCatalog? NodeCatalog { get; init; }

    /// <summary>
    /// The port-compatibility service used for connection validation.
    /// </summary>
    public IPortCompatibilityService? CompatibilityService { get; init; }

    /// <summary>
    /// Optional whole-document workspace service.
    /// </summary>
    public IGraphWorkspaceService? WorkspaceService { get; init; }

    /// <summary>
    /// Optional fragment-workspace service.
    /// </summary>
    public IGraphFragmentWorkspaceService? FragmentWorkspaceService { get; init; }

    /// <summary>
    /// Optional explicit storage root. When omitted, the package-neutral default paths are used.
    /// </summary>
    public string? StorageRootPath { get; init; }

    /// <summary>
    /// Optional style configuration.
    /// </summary>
    public GraphEditorStyleOptions? StyleOptions { get; init; }

    /// <summary>
    /// Optional behavior configuration.
    /// </summary>
    public GraphEditorBehaviorOptions? BehaviorOptions { get; init; }

    /// <summary>
    /// Optional fragment-library service.
    /// </summary>
    public IGraphFragmentLibraryService? FragmentLibraryService { get; init; }

    /// <summary>
    /// Optional SVG scene-export service.
    /// </summary>
    public IGraphSceneSvgExportService? SceneSvgExportService { get; init; }

    /// <summary>
    /// Optional raster scene-export service.
    /// </summary>
    public IGraphSceneImageExportService? SceneImageExportService { get; init; }

    /// <summary>
    /// Optional fragment clipboard-payload serializer.
    /// </summary>
    public IGraphClipboardPayloadSerializer? ClipboardPayloadSerializer { get; init; }

    /// <summary>
    /// Optional plugin registrations for directly supplied plugin instances, assemblies, or staged packages.
    /// </summary>
    public IReadOnlyList<GraphEditorPluginRegistration> PluginRegistrations { get; init; } = [];

    /// <summary>
    /// Optional plugin trust policy used to make host-governed decisions before plugin code runs.
    /// </summary>
    public IGraphEditorPluginTrustPolicy? PluginTrustPolicy { get; init; }

    /// <summary>
    /// Optional host context-menu augmentor.
    /// </summary>
    public IGraphContextMenuAugmentor? ContextMenuAugmentor { get; init; }

    /// <summary>
    /// Optional node-presentation provider.
    /// </summary>
    public INodePresentationProvider? NodePresentationProvider { get; init; }

    /// <summary>
    /// Optional contextual tool provider layered on top of the shared command route.
    /// </summary>
    public IGraphEditorToolProvider? ToolProvider { get; init; }

    /// <summary>
    /// Optional host-owned runtime overlay provider. The editor displays or exposes this state but does not execute graphs.
    /// </summary>
    public IGraphRuntimeOverlayProvider? RuntimeOverlayProvider { get; init; }

    /// <summary>
    /// Optional host-owned layout provider. The editor requests plans but does not own a specific layout algorithm.
    /// </summary>
    public IGraphLayoutProvider? LayoutProvider { get; init; }

    /// <summary>
    /// Optional localization provider for built-in editor strings.
    /// </summary>
    public IGraphLocalizationProvider? LocalizationProvider { get; init; }

    /// <summary>
    /// Optional runtime diagnostics sink.
    /// </summary>
    public IGraphEditorDiagnosticsSink? DiagnosticsSink { get; init; }

    /// <summary>
    /// Optional retained host platform services used by the hosted editor facade.
    /// </summary>
    public GraphEditorPlatformServices? PlatformServices { get; init; }

    /// <summary>
    /// Optional runtime logging and tracing configuration.
    /// </summary>
    public GraphEditorInstrumentationOptions? Instrumentation { get; init; }
}
