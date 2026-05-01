using System;
using System.Linq;
using AsterGraph.Demo.Cookbook;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookV078CatalogTests
{
    [Fact]
    public void CookbookCatalog_CoversV078RenderingAndViewportRecipe()
    {
        var recipe = GetRecipe("v078-rendering-viewport-route");

        Assert.Equal(DemoCookbookRecipeCategory.PerformanceViewport, recipe.Category);
        Assert.Contains(recipe.CodeAnchors, anchor => anchor.Evidence == "GetSceneSnapshot");
        Assert.Contains(recipe.CodeAnchors, anchor => anchor.Evidence == "GetViewportSnapshot");
        Assert.Contains(recipe.CodeAnchors, anchor => anchor.Evidence == "ApplyVisibleSceneBudget");
        Assert.Contains(recipe.DemoAnchors, anchor => anchor.Evidence == "RenderConnections_VisibleSceneBudgetScopesCommittedConnectionsButPreservesPendingPreview");
        Assert.Contains(recipe.ProofMarkers, marker => marker == "VIEWPORT_LOD_POLICY_OK");
        Assert.Contains(recipe.ProofMarkers, marker => marker == "EDGE_RENDERING_SCOPE_BOUNDARY_OK");
        AssertScenarioAndInteractionEvidence(recipe, "ApplyVisibleSceneBudget");
        AssertScenarioAndInteractionEvidence(recipe, "EDGE_RENDERING_SCOPE_BOUNDARY_OK");
    }

    [Fact]
    public void CookbookCatalog_CoversV078CustomizationRecipe()
    {
        var recipe = GetRecipe("v078-customization-route");

        Assert.Equal(DemoCookbookRecipeCategory.Authoring, recipe.Category);
        Assert.Contains(recipe.CodeAnchors, anchor => anchor.Evidence == "CreatePresentationOptions");
        Assert.Contains(recipe.CodeAnchors, anchor => anchor.Evidence == "CreateEdgeOverlay");
        Assert.Contains(recipe.CodeAnchors, anchor => anchor.Evidence == "GetConnectionGeometrySnapshots");
        Assert.Contains(recipe.DocumentationAnchors, anchor => anchor.Evidence == "INodeParameterEditorRegistry");
        Assert.Contains(recipe.ProofMarkers, marker => marker == "CUSTOM_EXTENSION_SURFACE_OK");
        Assert.Contains(recipe.ProofMarkers, marker => marker == "CUSTOM_EXTENSION_SCOPE_BOUNDARY_OK");
        AssertScenarioAndInteractionEvidence(recipe, "CUSTOM_EXTENSION_EDGE_OVERLAY_OK");
        AssertScenarioAndInteractionEvidence(recipe, "CUSTOM_EXTENSION_RUNTIME_INSPECTOR_OK");
    }

    [Fact]
    public void CookbookCatalog_CoversV078SpatialAuthoringRecipe()
    {
        var recipe = GetRecipe("v078-spatial-authoring-route");

        Assert.Equal(DemoCookbookRecipeCategory.Authoring, recipe.Category);
        Assert.Contains(recipe.CodeAnchors, anchor => anchor.Evidence == "GetNodeSurfaceSnapshots");
        Assert.Contains(recipe.CodeAnchors, anchor => anchor.Evidence == "TrySetNodeSize");
        Assert.Contains(recipe.CodeAnchors, anchor => anchor.Evidence == "TryWrapSelectionToComposite");
        Assert.Contains(recipe.CodeAnchors, anchor => anchor.Evidence == "TryInsertConnectionRouteVertex");
        Assert.Contains(recipe.DemoAnchors, anchor => anchor.Evidence == "SessionCommands_NodeGroups_PromoteToComposite_AndExposeBoundaryPorts");
        Assert.Contains(recipe.ProofMarkers, marker => marker == "TIERED_NODE_SURFACE_OK");
        Assert.Contains(recipe.ProofMarkers, marker => marker == "COMPOSITE_SCOPE_OK");
        Assert.Contains(recipe.ProofMarkers, marker => marker == "EDGE_GEOMETRY_OK");
        AssertScenarioAndInteractionEvidence(recipe, "GetScopeNavigationSnapshot");
        AssertScenarioAndInteractionEvidence(recipe, "EDGE_GEOMETRY_OK");
    }

    private static DemoCookbookRecipe GetRecipe(string id)
        => Assert.Single(DemoCookbookCatalog.Recipes, recipe => string.Equals(recipe.Id, id, StringComparison.Ordinal));

    private static void AssertScenarioAndInteractionEvidence(DemoCookbookRecipe recipe, string evidence)
    {
        Assert.Contains(
            recipe.ScenarioPoints.Select(point => point.Evidence)
                .Concat(recipe.InteractionFacets.Select(facet => facet.Evidence)),
            item => item == evidence);
    }
}
