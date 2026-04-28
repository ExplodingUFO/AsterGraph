using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Provides a thin fluent facade for the common hosted Avalonia composition path.
/// </summary>
/// <remarks>
/// This builder delegates to <see cref="AsterGraphEditorFactory.Create(AsterGraphEditorOptions)" /> and
/// <see cref="AsterGraphAvaloniaViewFactory.Create(AsterGraphAvaloniaViewOptions)" />. It does not introduce
/// a second runtime model; runtime authority remains on <see cref="GraphEditorViewModel.Session" />.
/// </remarks>
public sealed class AsterGraphHostBuilder
{
    private GraphDocument? _document;
    private INodeCatalog? _catalog;
    private IPortCompatibilityService? _compatibilityService;
    private readonly List<GraphEditorPluginRegistration> _pluginRegistrations = [];
    private IGraphEditorPluginTrustPolicy? _pluginTrustPolicy;
    private IGraphLocalizationProvider? _localizationProvider;
    private IGraphEditorDiagnosticsSink? _diagnosticsSink;
    private string? _storageRootPath;
    private GraphEditorViewChromeMode _chromeMode = GraphEditorViewChromeMode.Default;
    private bool _enableDefaultContextMenu = true;
    private AsterGraphCommandShortcutPolicy _commandShortcutPolicy = AsterGraphCommandShortcutPolicy.Default;
    private AsterGraphWorkbenchOptions _workbench = AsterGraphWorkbenchOptions.Default;
    private AsterGraphPresentationOptions? _presentation;

    private AsterGraphHostBuilder()
    {
    }

    /// <summary>
    /// Starts a new hosted Avalonia builder.
    /// </summary>
    public static AsterGraphHostBuilder Create()
        => new();

    /// <summary>
    /// Sets the initial document for the hosted editor.
    /// </summary>
    public AsterGraphHostBuilder UseDocument(GraphDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        _document = document;
        return this;
    }

    /// <summary>
    /// Sets the node catalog visible to the hosted editor.
    /// </summary>
    public AsterGraphHostBuilder UseCatalog(INodeCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
        return this;
    }

    /// <summary>
    /// Sets the port-compatibility service used by the hosted editor.
    /// </summary>
    public AsterGraphHostBuilder UseCompatibility(IPortCompatibilityService compatibilityService)
    {
        ArgumentNullException.ThrowIfNull(compatibilityService);
        _compatibilityService = compatibilityService;
        return this;
    }

    /// <summary>
    /// Uses the default port-compatibility service.
    /// </summary>
    public AsterGraphHostBuilder UseDefaultCompatibility()
    {
        _compatibilityService = new DefaultPortCompatibilityService();
        return this;
    }

    /// <summary>
    /// Adds plugin registrations to the hosted editor.
    /// </summary>
    public AsterGraphHostBuilder UsePluginRegistrations(IEnumerable<GraphEditorPluginRegistration> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);
        _pluginRegistrations.Clear();
        _pluginRegistrations.AddRange(registrations);
        return this;
    }

    /// <summary>
    /// Adds plugin registrations to the hosted editor.
    /// </summary>
    public AsterGraphHostBuilder UsePluginRegistrations(params GraphEditorPluginRegistration[] registrations)
        => UsePluginRegistrations((IEnumerable<GraphEditorPluginRegistration>)registrations);

    /// <summary>
    /// Sets the host-owned plugin trust policy.
    /// </summary>
    public AsterGraphHostBuilder UsePluginTrustPolicy(IGraphEditorPluginTrustPolicy pluginTrustPolicy)
    {
        ArgumentNullException.ThrowIfNull(pluginTrustPolicy);
        _pluginTrustPolicy = pluginTrustPolicy;
        return this;
    }

    /// <summary>
    /// Sets the localization provider for built-in editor strings.
    /// </summary>
    public AsterGraphHostBuilder UseLocalization(IGraphLocalizationProvider localizationProvider)
    {
        ArgumentNullException.ThrowIfNull(localizationProvider);
        _localizationProvider = localizationProvider;
        return this;
    }

    /// <summary>
    /// Sets the diagnostics sink used by the runtime session.
    /// </summary>
    public AsterGraphHostBuilder UseDiagnostics(IGraphEditorDiagnosticsSink diagnosticsSink)
    {
        ArgumentNullException.ThrowIfNull(diagnosticsSink);
        _diagnosticsSink = diagnosticsSink;
        return this;
    }

    /// <summary>
    /// Sets the optional storage root forwarded to the editor factory.
    /// </summary>
    public AsterGraphHostBuilder UseStorageRoot(string storageRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageRootPath);
        _storageRootPath = storageRootPath;
        return this;
    }

    /// <summary>
    /// Sets the hosted Avalonia chrome mode.
    /// </summary>
    public AsterGraphHostBuilder UseChromeMode(GraphEditorViewChromeMode chromeMode)
    {
        _chromeMode = chromeMode;
        return this;
    }

    /// <summary>
    /// Enables or disables the stock context menu.
    /// </summary>
    public AsterGraphHostBuilder UseDefaultContextMenu(bool isEnabled)
    {
        _enableDefaultContextMenu = isEnabled;
        return this;
    }

    /// <summary>
    /// Sets the hosted command-shortcut policy.
    /// </summary>
    public AsterGraphHostBuilder UseCommandShortcutPolicy(AsterGraphCommandShortcutPolicy commandShortcutPolicy)
    {
        ArgumentNullException.ThrowIfNull(commandShortcutPolicy);
        _commandShortcutPolicy = commandShortcutPolicy;
        return this;
    }

    /// <summary>
    /// Uses the stock hosted workbench composition for toolbar, node palette, inspector, mini-map, fragments, diagnostics, and status chrome.
    /// </summary>
    public AsterGraphHostBuilder UseDefaultWorkbench()
    {
        _chromeMode = GraphEditorViewChromeMode.Default;
        _enableDefaultContextMenu = true;
        _commandShortcutPolicy = AsterGraphCommandShortcutPolicy.Default;
        _workbench = AsterGraphWorkbenchOptions.Default;
        return this;
    }

    /// <summary>
    /// Sets the hosted workbench composition options.
    /// </summary>
    public AsterGraphHostBuilder UseWorkbench(AsterGraphWorkbenchOptions workbench)
    {
        ArgumentNullException.ThrowIfNull(workbench);
        _workbench = workbench;
        return this;
    }

    /// <summary>
    /// Sets optional Avalonia presentation overrides.
    /// </summary>
    public AsterGraphHostBuilder UsePresentation(AsterGraphPresentationOptions presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        _presentation = presentation;
        return this;
    }

    /// <summary>
    /// Builds the editor factory options represented by this builder.
    /// </summary>
    public AsterGraphEditorOptions BuildEditorOptions()
        => new()
        {
            Document = _document,
            NodeCatalog = _catalog,
            CompatibilityService = _compatibilityService,
            PluginRegistrations = _pluginRegistrations.ToArray(),
            PluginTrustPolicy = _pluginTrustPolicy,
            LocalizationProvider = _localizationProvider,
            DiagnosticsSink = _diagnosticsSink,
            StorageRootPath = _storageRootPath,
        };

    /// <summary>
    /// Builds a hosted editor facade by using the canonical editor factory.
    /// </summary>
    public GraphEditorViewModel BuildEditor()
        => AsterGraphEditorFactory.Create(BuildEditorOptions());

    /// <summary>
    /// Builds the Avalonia view-factory options for an existing editor facade.
    /// </summary>
    public AsterGraphAvaloniaViewOptions BuildViewOptions(GraphEditorViewModel editor)
    {
        ArgumentNullException.ThrowIfNull(editor);
        return new AsterGraphAvaloniaViewOptions
        {
            Editor = editor,
            ChromeMode = _chromeMode,
            EnableDefaultContextMenu = _enableDefaultContextMenu,
            CommandShortcutPolicy = _commandShortcutPolicy,
            Workbench = _workbench,
            Presentation = _presentation,
        };
    }

    /// <summary>
    /// Builds the stock Avalonia hosted view by using the canonical editor and view factories.
    /// </summary>
    public GraphEditorView BuildAvaloniaView()
    {
        var editor = BuildEditor();
        return AsterGraphAvaloniaViewFactory.Create(BuildViewOptions(editor));
    }
}
