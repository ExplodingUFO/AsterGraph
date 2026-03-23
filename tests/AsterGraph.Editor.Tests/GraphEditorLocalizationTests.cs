using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorLocalizationTests
{
    [Fact]
    public void InspectorTitle_UsesLocalizationProviderForStockText()
    {
        var editor = CreateEditor(new TestGraphLocalizationProvider(
            new Dictionary<string, string>
            {
                ["editor.inspector.title.none"] = "未选择节点",
            }));

        Assert.Equal("未选择节点", editor.InspectorTitle);
    }

    [Fact]
    public void BuildContextMenu_UsesLocalizationProviderForStockMenuLabels()
    {
        var editor = CreateEditor(new TestGraphLocalizationProvider(
            new Dictionary<string, string>
            {
                ["editor.menu.canvas.addNode"] = "添加节点",
            }));

        var menu = editor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(0, 0)));
        var addNodeItem = Assert.Single(menu, item => item.Id == "canvas-add-node");

        Assert.Equal("添加节点", addNodeItem.Header);
    }

    [Fact]
    public void StockStrings_FallBackToDefaults_WhenLocalizationProviderIsMissing()
    {
        var editor = CreateEditor(provider: null);

        Assert.Equal("Select A Node", editor.InspectorTitle);

        var menu = editor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(0, 0)));
        var addNodeItem = Assert.Single(menu, item => item.Id == "canvas-add-node");
        Assert.Equal("Add Node", addNodeItem.Header);
    }

    private static GraphEditorViewModel CreateEditor(IGraphLocalizationProvider? provider)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            new NodeDefinitionId("tests.localization.sample"),
            "Sample Node",
            "Tests",
            "Localization",
            [],
            []));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Localization Tests",
                "Covers stock localization seams.",
                [],
                []),
            catalog,
            new DefaultPortCompatibilityService(),
            localizationProvider: provider);
    }

    private sealed class TestGraphLocalizationProvider : IGraphLocalizationProvider
    {
        private readonly IReadOnlyDictionary<string, string> _values;

        public TestGraphLocalizationProvider(IReadOnlyDictionary<string, string> values)
        {
            _values = values;
        }

        public string GetString(string key, string fallback)
            => _values.TryGetValue(key, out var value) ? value : fallback;
    }
}
