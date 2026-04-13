using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Menus;
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
    public void CompatibilityCommands_BuildCanvasMenu_DisabledSaveItem_ExposesDisabledReason()
    {
        var editor = CreateEditor(GraphEditorCommandPermissions.ReadOnly);
        var commands = new GraphEditorViewModel.GraphEditorCompatibilityCommands(editor);

        var menu = commands.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(0, 0)));
        var saveItem = Assert.Single(menu, item => item.Id == "canvas-save");

        Assert.False(saveItem.IsEnabled);
        Assert.Equal("Snapshot saving is disabled by host permissions.", saveItem.DisabledReason);
    }

    [Fact]
    public void CompatibilityCommands_DuplicateNode_CreatesShiftedCopy()
    {
        var editor = CreateConnectedEditor();
        var commands = new GraphEditorViewModel.GraphEditorCompatibilityCommands(editor);
        var original = Assert.Single(editor.Nodes, node => node.Id == "source-node");

        commands.DuplicateNode(original.Id);

        var duplicate = Assert.Single(editor.Nodes, node => node.Id != original.Id && node.Title == original.Title);
        Assert.Equal(original.X + 48, duplicate.X);
        Assert.Equal(original.Y + 48, duplicate.Y);
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

    private static GraphEditorViewModel CreateConnectedEditor()
    {
        var sourceDefinitionId = new NodeDefinitionId("tests.menu.source");
        var sinkDefinitionId = new NodeDefinitionId("tests.menu.sink");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            sourceDefinitionId,
            "Source",
            "Tests",
            "Source node",
            [],
            [
                new PortDefinition("out", "Output", new PortTypeId("number"), "#6AD5C4"),
            ]));
        catalog.RegisterDefinition(new NodeDefinition(
            sinkDefinitionId,
            "Sink",
            "Tests",
            "Sink node",
            [
                new PortDefinition("in", "Input", new PortTypeId("number"), "#6AD5C4"),
            ],
            []));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Menu Compatibility",
                "Exercise compatibility command helpers.",
                [
                    new GraphNode(
                        "source-node",
                        "Source",
                        "Tests",
                        "Source node",
                        "Produces a number.",
                        new GraphPoint(80, 120),
                        new GraphSize(220, 160),
                        [],
                        [
                            new GraphPort("out", "Output", PortDirection.Output, "Number", "#6AD5C4", new PortTypeId("number")),
                        ],
                        "#6AD5C4",
                        sourceDefinitionId,
                        []),
                    new GraphNode(
                        "sink-node",
                        "Sink",
                        "Tests",
                        "Sink node",
                        "Consumes a number.",
                        new GraphPoint(380, 120),
                        new GraphSize(220, 160),
                        [
                            new GraphPort("in", "Input", PortDirection.Input, "Number", "#6AD5C4", new PortTypeId("number")),
                        ],
                        [],
                        "#FFB347",
                        sinkDefinitionId,
                        []),
                ],
                [
                    new GraphConnection(
                        "connection-001",
                        "source-node",
                        "out",
                        "sink-node",
                        "in",
                        "link",
                        "#6AD5C4"),
                ]),
            catalog,
            new DefaultPortCompatibilityService());
    }
}
