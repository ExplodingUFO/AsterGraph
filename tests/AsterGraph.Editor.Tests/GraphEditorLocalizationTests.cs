using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
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

    [Fact]
    public void UndoStatus_UsesLocalizationProviderForStockStatusMessage()
    {
        var behavior = GraphEditorBehaviorOptions.Default with
        {
            Commands = GraphEditorCommandPermissions.Default with
            {
                History = new HistoryCommandPermissions
                {
                    AllowUndo = false,
                    AllowRedo = true,
                },
            },
        };
        var editor = CreateEditor(
            new TestGraphLocalizationProvider(
                new Dictionary<string, string>
                {
                    ["editor.status.undo.disabledByPermissions"] = "宿主已禁用撤销。",
                }),
            behavior);

        editor.Undo();

        Assert.Equal("宿主已禁用撤销。", editor.StatusMessage);
    }

    [Fact]
    public void SelectionCaption_UsesLocalizationProviderForProjectionCaptions()
    {
        var editor = CreateConnectedEditor(new TestGraphLocalizationProvider(
            new Dictionary<string, string>
            {
                ["editor.selection.none"] = "未选择",
                ["editor.selection.single"] = "输入 {0} / 输出 {1}",
                ["editor.selection.multiple"] = "已选择 {0} 个节点 / 主节点 {1}",
            }));

        Assert.Equal("未选择", editor.SelectionCaption);

        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        Assert.Equal("输入 0 / 输出 1", editor.SelectionCaption);

        editor.SetSelection([editor.Nodes[0], editor.Nodes[1]], editor.Nodes[0], status: null);
        Assert.Equal("已选择 2 个节点 / 主节点 Source", editor.SelectionCaption);
    }

    [Fact]
    public void InspectorProjection_UsesLocalizationProviderForStockProjectionText()
    {
        var editor = CreateConnectedEditor(new TestGraphLocalizationProvider(
            new Dictionary<string, string>
            {
                ["editor.inspector.common.none"] = "无",
                ["editor.inspector.ports.item"] = "{0} => {1}",
                ["editor.inspector.relatedNodes.item"] = "{0} <= {1}",
            }));

        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        Assert.Equal("无", editor.InspectorInputs);
        Assert.Equal("Color => Color", editor.InspectorOutputs);
        Assert.Equal("无", editor.InspectorUpstream);
        Assert.Equal("Sink <= Input", editor.InspectorDownstream);
    }

    private static GraphEditorViewModel CreateEditor(
        IGraphLocalizationProvider? provider,
        GraphEditorBehaviorOptions? behaviorOptions = null)
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
            behaviorOptions: behaviorOptions,
            localizationProvider: provider);
    }

    private static GraphEditorViewModel CreateConnectedEditor(IGraphLocalizationProvider? provider)
    {
        var sourceDefinitionId = new NodeDefinitionId("tests.localization.source");
        var sinkDefinitionId = new NodeDefinitionId("tests.localization.sink");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            sourceDefinitionId,
            "Source",
            "Tests",
            "Localization source",
            [],
            [
                new PortDefinition("out", "Color", new PortTypeId("color"), "#55D8C1"),
            ]));
        catalog.RegisterDefinition(new NodeDefinition(
            sinkDefinitionId,
            "Sink",
            "Tests",
            "Localization sink",
            [
                new PortDefinition("in", "Input", new PortTypeId("color"), "#55D8C1"),
            ],
            []));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Localization Projection Tests",
                "Covers projection localization seams.",
                [
                    new GraphNode(
                        "node-source",
                        "Source",
                        "Tests",
                        "Projection source",
                        "Outputs a color.",
                        new GraphPoint(120, 80),
                        new GraphSize(220, 160),
                        [],
                        [
                            new GraphPort("out", "Color", PortDirection.Output, "Color", "#55D8C1", new PortTypeId("color")),
                        ],
                        "#55D8C1",
                        sourceDefinitionId,
                        []),
                    new GraphNode(
                        "node-sink",
                        "Sink",
                        "Tests",
                        "Projection sink",
                        "Consumes a color.",
                        new GraphPoint(420, 80),
                        new GraphSize(220, 160),
                        [
                            new GraphPort("in", "Input", PortDirection.Input, "Color", "#55D8C1", new PortTypeId("color")),
                        ],
                        [],
                        "#FFB347",
                        sinkDefinitionId,
                        []),
                ],
                [
                    new GraphConnection(
                        "connection-001",
                        "node-source",
                        "out",
                        "node-sink",
                        "in",
                        "link",
                        "#55D8C1"),
                ]),
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
