using System;
using AsterGraph.Demo.Cookbook;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookBuiltInBatchTests
{
    [Fact]
    public void CookbookCatalog_AddsBuiltInWorkbenchRecipeBatch()
    {
        Assert.True(DemoCookbookCatalog.Recipes.Count >= 19);
        AssertBuiltInRecipe(
            "builtin-minimap-workbench-route",
            DemoCookbookRecipeCategory.PerformanceViewport,
            "MINIMAP_LIGHTWEIGHT_PROJECTION_OK",
            "minimap-workbench");
        AssertBuiltInRecipe(
            "builtin-background-grid-route",
            DemoCookbookRecipeCategory.PerformanceViewport,
            "GRID_BACKGROUND_DENSITY_OK",
            "background-grid-density");
        AssertBuiltInRecipe(
            "builtin-hosted-controls-route",
            DemoCookbookRecipeCategory.Authoring,
            "HOSTED_CONTROLS_PANEL_COMPOSITION_OK",
            "hosted-controls-panel");
    }

    [Fact]
    public void CookbookCatalog_AddsStandaloneBuiltInRecipeBatch()
    {
        AssertStandaloneBuiltInRecipe(
            "builtin-standalone-controls-route",
            "BUILTIN_STANDALONE_CONTROLS_OK",
            "keyboard-navigation-lab",
            "AsterGraphControls");
        AssertStandaloneBuiltInRecipe(
            "builtin-standalone-panel-route",
            "BUILTIN_STANDALONE_PANEL_OK",
            "host-event-inspector",
            "AsterGraphPanel");
        AssertStandaloneBuiltInRecipe(
            "builtin-node-toolbar-route",
            "BUILTIN_NODE_TOOLBAR_OK",
            "selection-marquee-workbench",
            "NodeToolbar");
        AssertStandaloneBuiltInRecipe(
            "builtin-edge-toolbar-route",
            "BUILTIN_EDGE_TOOLBAR_OK",
            "clipboard-fragment-roundtrip",
            "EdgeToolbar");
        AssertStandaloneBuiltInRecipe(
            "builtin-node-resizer-route",
            "BUILTIN_NODE_RESIZER_OK",
            "validation-prevent-cycle",
            "NodeResizer");
    }

    [Fact]
    public void CookbookCatalog_AddsWhiteboardAuthoringCookbookRoute()
    {
        var recipe = Assert.Single(DemoCookbookCatalog.Recipes, candidate =>
            string.Equals(candidate.Id, "whiteboard-authoring-cookbook-route", StringComparison.Ordinal));

        Assert.Equal(DemoCookbookRecipeCategory.Authoring, recipe.Category);
        Assert.Contains(recipe.CodeAnchors, anchor =>
            anchor.Path == "src/AsterGraph.Avalonia/Hosting/AsterGraphAuthoringToolActionFactory.cs"
            && anchor.Evidence == "CreateWhiteboardDrawingToolActions");
        Assert.Contains(recipe.DemoAnchors, anchor =>
            anchor.Path == "tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs"
            && anchor.Evidence == "AuthoringToolsChrome_ProjectsWhiteboardDrawingActionsThroughNodeCanvasDrawingMode");
        Assert.Contains(recipe.DemoAnchors, anchor =>
            anchor.Path == "src/AsterGraph.Demo/DemoGraphFactory.cs"
            && anchor.Evidence == "selection-marquee-workbench");
        Assert.Contains("WHITEBOARD_AUTHORING_COOKBOOK_UX_OK", recipe.ProofMarkers);
        Assert.Contains("PART_WhiteboardDrawingRectangleButton", recipe.RouteClarity.SupportedRoute, StringComparison.Ordinal);
        Assert.Contains("PART_WhiteboardDrawingFreehandButton", recipe.RouteClarity.SupportedRoute, StringComparison.Ordinal);
        Assert.DoesNotContain("GraphDocument schema", recipe.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("full React Flow whiteboard parity", recipe.RouteClarity.SupportedRoute, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertBuiltInRecipe(
        string id,
        DemoCookbookRecipeCategory category,
        string proofMarker,
        string scenarioId)
    {
        var recipe = Assert.Single(DemoCookbookCatalog.Recipes, candidate =>
            string.Equals(candidate.Id, id, StringComparison.Ordinal));

        Assert.Equal(category, recipe.Category);
        Assert.Contains(recipe.DemoAnchors, anchor =>
            anchor.Path == "src/AsterGraph.Demo/DemoGraphFactory.cs"
            && anchor.Evidence == scenarioId);
        Assert.Contains(recipe.ProofMarkers, marker => marker == proofMarker);
        Assert.Contains(scenarioId, recipe.RouteClarity.SupportedRoute, StringComparison.Ordinal);
        Assert.DoesNotContain("React Flow", recipe.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("React Flow", recipe.RouteClarity.SupportedRoute, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertStandaloneBuiltInRecipe(
        string id,
        string proofMarker,
        string scenarioId,
        string entryPoint)
    {
        var recipe = Assert.Single(DemoCookbookCatalog.Recipes, candidate =>
            string.Equals(candidate.Id, id, StringComparison.Ordinal));

        Assert.Equal(DemoCookbookRecipeCategory.Authoring, recipe.Category);
        Assert.Contains(recipe.DemoAnchors, anchor =>
            anchor.Path == "src/AsterGraph.Demo/DemoGraphFactory.cs"
            && anchor.Evidence == scenarioId);
        Assert.Contains(recipe.ProofMarkers, marker => marker == proofMarker);
        Assert.Contains(recipe.CodeAnchors, anchor => anchor.Evidence == entryPoint);
        Assert.Contains(entryPoint, recipe.RouteClarity.SupportedRoute, StringComparison.Ordinal);
        Assert.DoesNotContain("UseDefaultWorkbench", recipe.RouteClarity.SupportedRoute, StringComparison.Ordinal);
    }
}
