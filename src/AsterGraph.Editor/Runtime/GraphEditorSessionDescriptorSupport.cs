using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Runtime;

internal sealed class GraphEditorSessionDescriptorSupport
{
    private readonly Func<string, string, string> _localize;
    private readonly Func<bool> _hasNodePresentationProvider;
    private readonly Func<bool> _hasLocalizationProvider;
    private readonly Func<bool> _canEditNodeParameters;

    public GraphEditorSessionDescriptorSupport(
        INodeCatalog nodeCatalog,
        IPortCompatibilityService compatibilityService,
        Func<string, string, string>? localize = null,
        GraphEditorViewModel? compatibilityEditor = null,
        bool hasFragmentWorkspaceService = false,
        bool hasFragmentLibraryService = false,
        bool hasSceneSvgExportService = false,
        bool hasSceneImageExportService = false,
        bool hasClipboardPayloadSerializer = false,
        bool hasPluginLoader = false,
        bool hasPluginTrustPolicy = false,
        bool hasCommandContributor = false,
        bool hasContextMenuAugmentor = false,
        bool hasToolProvider = false,
        Func<bool>? canEditNodeParameters = null,
        Func<bool>? hasNodePresentationProvider = null,
        Func<bool>? hasLocalizationProvider = null)
    {
        NodeCatalog = nodeCatalog ?? throw new ArgumentNullException(nameof(nodeCatalog));
        CompatibilityService = compatibilityService ?? throw new ArgumentNullException(nameof(compatibilityService));
        _localize = localize ?? ((_, fallback) => fallback);
        _hasNodePresentationProvider = hasNodePresentationProvider ?? (() => false);
        _hasLocalizationProvider = hasLocalizationProvider ?? (() => false);
        _canEditNodeParameters = canEditNodeParameters ?? (() => false);
        CompatibilityEditor = compatibilityEditor;
        HasFragmentWorkspaceService = hasFragmentWorkspaceService;
        HasFragmentLibraryService = hasFragmentLibraryService;
        HasSceneSvgExportService = hasSceneSvgExportService;
        HasSceneImageExportService = hasSceneImageExportService;
        HasClipboardPayloadSerializer = hasClipboardPayloadSerializer;
        HasPluginLoader = hasPluginLoader;
        HasPluginTrustPolicy = hasPluginTrustPolicy;
        HasCommandContributor = hasCommandContributor;
        HasContextMenuAugmentor = hasContextMenuAugmentor;
        HasToolProvider = hasToolProvider;
    }

    public INodeCatalog NodeCatalog { get; }

    public IPortCompatibilityService CompatibilityService { get; }

    public GraphEditorViewModel? CompatibilityEditor { get; }

    public bool HasFragmentWorkspaceService { get; }

    public bool HasFragmentLibraryService { get; }

    public bool HasSceneSvgExportService { get; }

    public bool HasSceneImageExportService { get; }

    public bool HasClipboardPayloadSerializer { get; }

    public bool HasPluginLoader { get; }

    public bool HasPluginTrustPolicy { get; }

    public bool HasCommandContributor { get; }

    public bool HasContextMenuAugmentor { get; }

    public bool HasToolProvider { get; }

    public bool CanEditNodeParameters => _canEditNodeParameters();

    public bool HasNodePresentationProvider => _hasNodePresentationProvider();

    public bool HasLocalizationProvider => _hasLocalizationProvider();

    public IReadOnlyCollection<INodeDefinition> Definitions => NodeCatalog.Definitions;

    public string Localize(string key, string fallback)
        => _localize(key, fallback);
}
