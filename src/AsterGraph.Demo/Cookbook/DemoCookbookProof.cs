namespace AsterGraph.Demo.Cookbook;

public sealed record DemoCookbookProofResult(
    bool ContractOk,
    bool RequiredCategoriesOk,
    bool SupportBoundaryOk,
    bool WorkspaceLayoutOk,
    bool RouteCoverageOk,
    bool VisualHierarchyOk,
    bool NavigationFeedbackOk,
    bool DetailReadabilityOk,
    bool InteractionStatesOk,
    bool ProfessionalInteractionOk,
    bool ScenarioDepthOk,
    bool OwnershipBoundaryOk,
    int RecipeCount,
    int RequiredCategoryCount)
{
    public bool IsOk =>
        ContractOk
        && RequiredCategoriesOk
        && SupportBoundaryOk
        && WorkspaceLayoutOk
        && RouteCoverageOk
        && VisualHierarchyOk
        && NavigationFeedbackOk
        && DetailReadabilityOk
        && InteractionStatesOk
        && ProfessionalInteractionOk
        && ScenarioDepthOk
        && OwnershipBoundaryOk;
}

public static class DemoCookbookProof
{
    public static IReadOnlyList<string> PublicSuccessMarkerIds { get; } =
    [
        "DEMO_COOKBOOK_CONTRACT_OK",
        "DEMO_COOKBOOK_REQUIRED_CATEGORIES_OK",
        "DEMO_COOKBOOK_SUPPORT_BOUNDARY_OK",
        "DEMO_COOKBOOK_WORKSPACE_LAYOUT_OK",
        "DEMO_COOKBOOK_ROUTE_COVERAGE_OK",
        "DEMO_COOKBOOK_VISUAL_HIERARCHY_OK",
        "DEMO_COOKBOOK_NAVIGATION_FEEDBACK_OK",
        "DEMO_COOKBOOK_DETAIL_READABILITY_OK",
        "DEMO_COOKBOOK_INTERACTION_STATES_OK",
        "DEMO_COOKBOOK_PROFESSIONAL_INTERACTION_OK",
        "DEMO_COOKBOOK_SCENARIO_DEPTH_OK",
        "DEMO_COOKBOOK_OWNERSHIP_BOUNDARY_OK",
    ];

    public static DemoCookbookProofResult Run()
    {
        var contractOk = DemoCookbookCatalog.Validate().Count == 0;
        var requiredCategoriesOk = DemoCookbookCatalog.RequiredCategories.All(category =>
            DemoCookbookCatalog.Recipes.Any(recipe => recipe.Category == category));
        var supportBoundaryOk = DemoCookbookCatalog.Recipes.All(recipe =>
            !string.IsNullOrWhiteSpace(recipe.SupportBoundary)
            && !recipe.SupportBoundary.Contains("runtime marketplace", StringComparison.OrdinalIgnoreCase)
            && !recipe.SupportBoundary.Contains("GA support", StringComparison.OrdinalIgnoreCase));
        var workspaceSnapshots = DemoCookbookCatalog.Recipes
            .Select(recipe => DemoCookbookWorkspaceProjection.Create(recipe.Id))
            .ToArray();
        var workspaceLayoutOk = workspaceSnapshots.All(snapshot =>
            snapshot.NavigationGroups.Count == DemoCookbookCatalog.RequiredCategories.Count
            && snapshot.SelectedRecipe.GraphAnchors.Count > 0
            && snapshot.SelectedRecipe.CodeExamples.Count > 0
            && snapshot.SelectedRecipe.DocumentationLinks.Count > 0
            && snapshot.SelectedRecipe.InteractionFacets.Count > 0
            && snapshot.SelectedRecipe.ProofMarkers.Count > 0
            && !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.SupportBoundary));
        var routeCoverageOk = workspaceSnapshots.All(snapshot =>
            !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.RouteStatus)
            && !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.RouteStatusDescription)
            && !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.RouteClarity.SupportedRoute)
            && !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.RouteClarity.PackageBoundary)
            && !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.RouteClarity.DemoBoundary)
            && snapshot.SelectedRecipe.DeferredGaps.Count > 0)
            && workspaceSnapshots.Any(snapshot => string.Equals(snapshot.SelectedRecipe.RouteStatus, "Supported SDK route", StringComparison.Ordinal))
            && workspaceSnapshots.Any(snapshot => string.Equals(snapshot.SelectedRecipe.RouteStatus, "Proof/demo route", StringComparison.Ordinal))
            && workspaceSnapshots.All(snapshot => IsExpectedRouteStatus(snapshot.SelectedRecipe));
        var visualHierarchyOk = workspaceSnapshots.All(snapshot =>
            snapshot.NavigationGroups.All(group => !string.IsNullOrWhiteSpace(group.DisplayName))
            && !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.Title)
            && !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.Summary)
            && snapshot.SelectedRecipe.GraphAnchors.All(HasReadableAnchor)
            && snapshot.SelectedRecipe.CodeExamples.All(HasReadableAnchor));
        var navigationFeedbackOk = workspaceSnapshots.All(snapshot =>
            snapshot.NavigationGroups.SelectMany(group => group.Recipes).Count(item => item.IsSelected) == 1
            && snapshot.NavigationGroups.All(group => group.Recipes.All(item =>
                !string.IsNullOrWhiteSpace(item.RecipeId)
                && !string.IsNullOrWhiteSpace(item.Title))));
        var detailReadabilityOk = workspaceSnapshots.All(snapshot =>
            snapshot.SelectedRecipe.DocumentationLinks.All(HasReadableAnchor)
            && snapshot.SelectedRecipe.ProofMarkers.All(marker => !string.IsNullOrWhiteSpace(marker))
            && !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.SupportBoundary));
        var interactionStatesOk = workspaceSnapshots.All(snapshot =>
            !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.UnavailableActionDescription)
            && snapshot.SelectedRecipe.DeferredGaps.All(gap => !string.IsNullOrWhiteSpace(gap))
            && !ContainsUnsupportedPromise(snapshot.SelectedRecipe.UnavailableActionDescription)
            && snapshot.SelectedRecipe.DeferredGaps.All(gap => !ContainsUnsupportedPromise(gap)));
        var pluginRecipe = DemoCookbookCatalog.Recipes.Single(recipe => recipe.Id == "plugin-trust-route");
        var diagnosticsRecipes = DemoCookbookCatalog.Recipes
            .Where(recipe => recipe.Category == DemoCookbookRecipeCategory.DiagnosticsSupport)
            .ToArray();
        var filteredSnapshot = DemoCookbookWorkspaceProjection.Create(pluginRecipe.Id, diagnosticsRecipes);
        var emptyFilteredSnapshot = DemoCookbookWorkspaceProjection.Create(pluginRecipe.Id, []);
        var projectedInteractionKinds = workspaceSnapshots
            .SelectMany(snapshot => snapshot.SelectedRecipe.InteractionFacets)
            .Select(facet => facet.Kind)
            .Distinct()
            .ToArray();
        var professionalInteractionOk =
            filteredSnapshot.NavigationGroups.Count == 1
            && filteredSnapshot.NavigationGroups[0].Category == DemoCookbookRecipeCategory.DiagnosticsSupport
            && filteredSnapshot.NavigationGroups[0].Recipes.All(recipe => !recipe.IsSelected)
            && filteredSnapshot.SelectedRecipe.RecipeId == pluginRecipe.Id
            && emptyFilteredSnapshot.NavigationGroups.Count == 0
            && emptyFilteredSnapshot.SelectedRecipe.RecipeId == pluginRecipe.Id
            && DemoCookbookCatalog.RequiredInteractionKinds.All(projectedInteractionKinds.Contains)
            && workspaceSnapshots.All(snapshot =>
                snapshot.SelectedRecipe.GraphAnchors.Count > 0
                && snapshot.SelectedRecipe.InteractionFacets.Count >= 3
                && snapshot.SelectedRecipe.InteractionFacets.All(facet =>
                    !string.IsNullOrWhiteSpace(facet.Label)
                    && !string.IsNullOrWhiteSpace(facet.Evidence)
                    && !string.IsNullOrWhiteSpace(facet.FocusLabel)
                    && HasInteractionEvidence(snapshot.SelectedRecipe, facet))
                && !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.RouteStatus)
                && !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.UnavailableActionDescription));
        var projectedScenarioKinds = workspaceSnapshots
            .SelectMany(snapshot => snapshot.SelectedRecipe.ScenarioPoints)
            .Select(point => point.Kind)
            .Distinct()
            .ToArray();
        var scenarioDepthOk = DemoCookbookCatalog.RequiredScenarioKinds.All(requiredKind =>
                projectedScenarioKinds.Contains(requiredKind))
            && workspaceSnapshots.All(snapshot =>
                snapshot.SelectedRecipe.ScenarioPoints.Count >= 3
                && snapshot.SelectedRecipe.ScenarioPoints.All(point =>
                    !string.IsNullOrWhiteSpace(point.Label)
                    && !string.IsNullOrWhiteSpace(point.Evidence)
                    && HasScenarioEvidence(snapshot.SelectedRecipe, point)));
        var ownershipBoundaryOk = workspaceSnapshots.Length == DemoCookbookCatalog.Recipes.Count
            && PublicSuccessMarkerIds.All(marker => marker.StartsWith("DEMO_COOKBOOK_", StringComparison.Ordinal));

        return new DemoCookbookProofResult(
            contractOk,
            requiredCategoriesOk,
            supportBoundaryOk,
            workspaceLayoutOk,
            routeCoverageOk,
            visualHierarchyOk,
            navigationFeedbackOk,
            detailReadabilityOk,
            interactionStatesOk,
            professionalInteractionOk,
            scenarioDepthOk,
            ownershipBoundaryOk,
            DemoCookbookCatalog.Recipes.Count,
            DemoCookbookCatalog.RequiredCategories.Count);
    }

    public static IReadOnlyList<string> CreateProofLines()
    {
        var result = Run();

        return
        [
            $"DEMO_COOKBOOK_CONTRACT_OK:{result.ContractOk}",
            $"DEMO_COOKBOOK_REQUIRED_CATEGORIES_OK:{result.RequiredCategoriesOk}",
            $"DEMO_COOKBOOK_SUPPORT_BOUNDARY_OK:{result.SupportBoundaryOk}",
            $"DEMO_COOKBOOK_WORKSPACE_LAYOUT_OK:{result.WorkspaceLayoutOk}",
            $"DEMO_COOKBOOK_ROUTE_COVERAGE_OK:{result.RouteCoverageOk}",
            $"DEMO_COOKBOOK_VISUAL_HIERARCHY_OK:{result.VisualHierarchyOk}",
            $"DEMO_COOKBOOK_NAVIGATION_FEEDBACK_OK:{result.NavigationFeedbackOk}",
            $"DEMO_COOKBOOK_DETAIL_READABILITY_OK:{result.DetailReadabilityOk}",
            $"DEMO_COOKBOOK_INTERACTION_STATES_OK:{result.InteractionStatesOk}",
            $"DEMO_COOKBOOK_PROFESSIONAL_INTERACTION_OK:{result.ProfessionalInteractionOk}",
            $"DEMO_COOKBOOK_SCENARIO_DEPTH_OK:{result.ScenarioDepthOk}",
            $"DEMO_COOKBOOK_OWNERSHIP_BOUNDARY_OK:{result.OwnershipBoundaryOk}",
            $"DEMO_COOKBOOK_RECIPE_COUNT:{result.RecipeCount}",
            $"DEMO_COOKBOOK_REQUIRED_CATEGORY_COUNT:{result.RequiredCategoryCount}",
        ];
    }

    private static bool HasReadableAnchor(DemoCookbookWorkspaceAnchor anchor)
        => !string.IsNullOrWhiteSpace(anchor.Label)
           && !string.IsNullOrWhiteSpace(anchor.Path)
           && !string.IsNullOrWhiteSpace(anchor.Evidence);

    private static bool HasScenarioEvidence(
        DemoCookbookWorkspaceRecipeContent recipe,
        DemoCookbookWorkspaceScenarioPoint point)
        => recipe.GraphAnchors
               .Concat(recipe.CodeExamples)
               .Concat(recipe.DocumentationLinks)
               .Any(anchor => string.Equals(anchor.Evidence, point.Evidence, StringComparison.Ordinal))
           || recipe.ProofMarkers.Any(marker => string.Equals(marker, point.Evidence, StringComparison.Ordinal));

    private static bool HasInteractionEvidence(
        DemoCookbookWorkspaceRecipeContent recipe,
        DemoCookbookWorkspaceInteractionFacet facet)
        => recipe.GraphAnchors
               .Concat(recipe.CodeExamples)
               .Concat(recipe.DocumentationLinks)
               .Any(anchor => string.Equals(anchor.Evidence, facet.Evidence, StringComparison.Ordinal))
           || recipe.ProofMarkers.Any(marker => string.Equals(marker, facet.Evidence, StringComparison.Ordinal));

    private static bool ContainsUnsupportedPromise(string text)
        => text.Contains("enabled fallback", StringComparison.OrdinalIgnoreCase)
           || text.Contains("marketplace is enabled", StringComparison.OrdinalIgnoreCase)
           || text.Contains("sandbox is active", StringComparison.OrdinalIgnoreCase)
           || text.Contains("telemetry is active", StringComparison.OrdinalIgnoreCase);

    private static bool IsExpectedRouteStatus(DemoCookbookWorkspaceRecipeContent recipe)
        => recipe.Category switch
        {
            DemoCookbookRecipeCategory.StarterHost or DemoCookbookRecipeCategory.Authoring =>
                string.Equals(recipe.RouteStatus, "Supported SDK route", StringComparison.Ordinal),
            DemoCookbookRecipeCategory.PluginTrust
                or DemoCookbookRecipeCategory.PerformanceViewport
                or DemoCookbookRecipeCategory.DiagnosticsSupport
                or DemoCookbookRecipeCategory.ReviewHelp =>
                string.Equals(recipe.RouteStatus, "Proof/demo route", StringComparison.Ordinal),
            _ => false,
        };
}
