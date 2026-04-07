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
        GraphEditorViewModel? compatibilityEditor = null)
    {
        NodeCatalog = nodeCatalog ?? throw new ArgumentNullException(nameof(nodeCatalog));
        _localize = localize ?? ((_, fallback) => fallback);
        CompatibilityEditor = compatibilityEditor;
    }

    public INodeCatalog NodeCatalog { get; }

    public GraphEditorViewModel? CompatibilityEditor { get; }

    public IReadOnlyCollection<INodeDefinition> Definitions => NodeCatalog.Definitions;

    public string Localize(string key, string fallback)
        => _localize(key, fallback);
}
