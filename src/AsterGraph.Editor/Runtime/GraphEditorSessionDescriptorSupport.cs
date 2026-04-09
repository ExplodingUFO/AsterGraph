using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Runtime;

internal sealed class GraphEditorSessionDescriptorSupport
{
    private readonly Func<string, string, string> _localize;

    public GraphEditorSessionDescriptorSupport(
        INodeCatalog nodeCatalog,
        Func<string, string, string>? localize = null,
        GraphEditorViewModel? compatibilityEditor = null,
        bool hasFragmentWorkspaceService = false,
        bool hasFragmentLibraryService = false,
        bool hasClipboardPayloadSerializer = false,
        bool hasPluginLoader = false,
        bool hasPluginTrustPolicy = false,
        bool hasContextMenuAugmentor = false,
        bool hasNodePresentationProvider = false,
        bool hasLocalizationProvider = false)
    {
        NodeCatalog = nodeCatalog ?? throw new ArgumentNullException(nameof(nodeCatalog));
        _localize = localize ?? ((_, fallback) => fallback);
        CompatibilityEditor = compatibilityEditor;
        HasFragmentWorkspaceService = hasFragmentWorkspaceService;
        HasFragmentLibraryService = hasFragmentLibraryService;
        HasClipboardPayloadSerializer = hasClipboardPayloadSerializer;
        HasPluginLoader = hasPluginLoader;
        HasPluginTrustPolicy = hasPluginTrustPolicy;
        HasContextMenuAugmentor = hasContextMenuAugmentor;
        HasNodePresentationProvider = hasNodePresentationProvider;
        HasLocalizationProvider = hasLocalizationProvider;
    }

    public INodeCatalog NodeCatalog { get; }

    public GraphEditorViewModel? CompatibilityEditor { get; }

    public bool HasFragmentWorkspaceService { get; }

    public bool HasFragmentLibraryService { get; }

    public bool HasClipboardPayloadSerializer { get; }

    public bool HasPluginLoader { get; }

    public bool HasPluginTrustPolicy { get; }

    public bool HasContextMenuAugmentor { get; }

    public bool HasNodePresentationProvider { get; }

    public bool HasLocalizationProvider { get; }

    public IReadOnlyCollection<INodeDefinition> Definitions => NodeCatalog.Definitions;

    public string Localize(string key, string fallback)
        => _localize(key, fallback);
}
