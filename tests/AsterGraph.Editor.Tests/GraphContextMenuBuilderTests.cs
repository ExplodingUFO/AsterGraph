using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Events;
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

}
