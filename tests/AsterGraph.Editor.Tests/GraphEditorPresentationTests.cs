using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorPresentationTests
{
    [Fact]
    public void RefreshNodePresentation_UpdatesNodePresentationSnapshotThroughPublicApi()
    {
        var provider = new TestNodePresentationProvider();
        var editor = CreateEditor(provider, nodeCount: 1);
        var node = editor.Nodes[0];

        provider.SetState(
            node.Id,
            new NodePresentationState(
                SubtitleOverride: "已加载",
                DescriptionOverride: "离线数据已同步",
                TopRightBadges:
                [
                    new NodeAdornmentDescriptor("成功", "#3FB950", "最近一次运行成功"),
                ],
                StatusBar: new NodeStatusBarDescriptor("可预览", "#2F81F7", "已准备好预览")));

        editor.RefreshNodePresentation(node.Id);

        Assert.Equal("已加载", node.DisplaySubtitle);
        Assert.Equal("离线数据已同步", node.DisplayDescription);
        Assert.Single(node.Presentation.TopRightBadges);
        Assert.Equal("成功", node.Presentation.TopRightBadges[0].Text);
        Assert.Equal("可预览", node.Presentation.StatusBar?.Text);
    }

    [Fact]
    public void RefreshNodePresentations_UpdatesEveryNodeUsingProvider()
    {
        var provider = new TestNodePresentationProvider();
        var editor = CreateEditor(provider, nodeCount: 2);

        provider.SetState(editor.Nodes[0].Id, new NodePresentationState(SubtitleOverride: "节点一"));
        provider.SetState(editor.Nodes[1].Id, new NodePresentationState(SubtitleOverride: "节点二"));

        editor.RefreshNodePresentations();

        Assert.Equal("节点一", editor.Nodes[0].DisplaySubtitle);
        Assert.Equal("节点二", editor.Nodes[1].DisplaySubtitle);
    }

    private static GraphEditorViewModel CreateEditor(INodePresentationProvider provider, int nodeCount)
    {
        var definitionId = new NodeDefinitionId("tests.editor.presentation.sample");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Presentation Node",
            "Tests",
            "默认副标题",
            [
                new PortDefinition("in", "输入", new PortTypeId("float"), "#55D8C1"),
            ],
            [
                new PortDefinition("out", "输出", new PortTypeId("float"), "#55D8C1"),
            ],
            description: "默认描述"));

        var nodes = Enumerable.Range(1, nodeCount)
            .Select(index => CreateGraphNode($"node-{index:000}", definitionId))
            .ToList();

        return new GraphEditorViewModel(
            new GraphDocument(
                "Presentation Tests",
                "Covers host-driven node presentation refresh.",
                nodes,
                []),
            catalog,
            new DefaultPortCompatibilityService(),
            nodePresentationProvider: provider);
    }

    private static GraphNode CreateGraphNode(string id, NodeDefinitionId definitionId)
        => new(
            id,
            "Presentation Node",
            "Tests",
            "默认副标题",
            "默认描述",
            new GraphPoint(120, 80),
            new GraphSize(220, 170),
            [
                new GraphPort("in", "输入", PortDirection.Input, "float", "#55D8C1", new PortTypeId("float")),
            ],
            [
                new GraphPort("out", "输出", PortDirection.Output, "float", "#55D8C1", new PortTypeId("float")),
            ],
            "#55D8C1",
            definitionId);

    private sealed class TestNodePresentationProvider : INodePresentationProvider
    {
        private readonly Dictionary<string, NodePresentationState> _states = new(StringComparer.Ordinal);

        public NodePresentationState GetNodePresentation(NodeViewModel node)
            => _states.TryGetValue(node.Id, out var state) ? state : NodePresentationState.Empty;

        public void SetState(string nodeId, NodePresentationState state)
            => _states[nodeId] = state;
    }
}
