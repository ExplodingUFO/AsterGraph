using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphContextMenuBuilderTests
{
    [Fact]
    public void BuildCanvasMenu_DisabledSaveItem_ExposesDisabledReason()
    {
        var editor = CreateEditor(GraphEditorCommandPermissions.ReadOnly);

        var menu = editor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(0, 0)));
        var saveItem = Assert.Single(menu, item => item.Id == "canvas-save");

        Assert.False(saveItem.IsEnabled);
        Assert.Equal("Snapshot saving is disabled by host permissions.", saveItem.DisabledReason);
    }

    [Fact]
    public void EditorNodeTemplates_ExposeCanonicalTemplateSnapshots()
    {
        var editor = CreateEditor(GraphEditorCommandPermissions.Default);

        var template = Assert.Single(editor.NodeTemplates);

        Assert.Equal(new NodeDefinitionId("tests.menu.sample"), template.DefinitionId);
        Assert.Equal("Sample Node", template.Title);
    }

    [Fact]
    public void BuildPortMenu_UsesCanonicalEdgeTemplates_ForParameterTargets()
    {
        var editor = CreateParameterTargetEditor();

        var menu = editor.BuildContextMenu(new ContextMenuContext(
            ContextMenuTargetKind.Port,
            new GraphPoint(0, 0),
            clickedPortNodeId: "tests.menu.source-node",
            clickedPortId: "out"));

        var compatibleTargets = Assert.Single(menu, item => item.Id == "port-compatible-targets");
        var targetNode = Assert.Single(compatibleTargets.Children, item => item.Id == "compatible-node-tests.menu.target-node");
        var parameterTarget = Assert.Single(targetNode.Children, item => item.Id == "compatible-parameter-tests.menu.target-node-gain");

        Assert.Equal("Gain", parameterTarget.Header);
        Assert.True(parameterTarget.IsEnabled);
    }

    private static GraphEditorViewModel CreateEditor(GraphEditorCommandPermissions permissions)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            new NodeDefinitionId("tests.menu.sample"),
            "Sample Node",
            "Tests",
            "Menu",
            [],
            []));

        var behavior = GraphEditorBehaviorOptions.Default with
        {
            Commands = permissions,
        };

        return new GraphEditorViewModel(
            new GraphDocument(
                "Menu Test",
                "Exercise stock menu metadata.",
                [],
                []),
            catalog,
            new DefaultPortCompatibilityService(),
            behaviorOptions: behavior);
    }

    private static GraphEditorViewModel CreateParameterTargetEditor()
    {
        var catalog = new NodeCatalog();
        var definitionId = new NodeDefinitionId("tests.menu.parameter-node");
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Parameter Node",
            "Tests",
            "Menu",
            [new PortDefinition("in", "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition("out", "Output", new PortTypeId("float"), "#6AD5C4")],
            parameters:
            [
                new NodeParameterDefinition(
                    "gain",
                    "Gain",
                    new PortTypeId("float"),
                    ParameterEditorKind.Number,
                    defaultValue: 0.5d),
            ]));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Menu Test",
                "Exercise retained template projection.",
                [
                    new GraphNode(
                        "tests.menu.source-node",
                        "Source Node",
                        "Tests",
                        "Menu",
                        "Provides the source output.",
                        new GraphPoint(80, 120),
                        new GraphSize(220, 140),
                        [],
                        [new GraphPort("out", "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                        "#6AD5C4",
                        definitionId),
                    new GraphNode(
                        "tests.menu.target-node",
                        "Target Node",
                        "Tests",
                        "Menu",
                        "Provides a parameter target.",
                        new GraphPoint(420, 140),
                        new GraphSize(220, 140),
                        [new GraphPort("in", "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                        [],
                        "#F3B36B",
                        definitionId),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService());
    }

}
