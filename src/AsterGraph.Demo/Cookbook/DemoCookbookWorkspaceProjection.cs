namespace AsterGraph.Demo.Cookbook;
public sealed record DemoCookbookWorkspaceSnapshot(
    IReadOnlyList<DemoCookbookWorkspaceNavigationGroup> NavigationGroups,
    DemoCookbookWorkspaceRecipeContent SelectedRecipe);
public sealed record DemoCookbookWorkspaceNavigationGroup(
    DemoCookbookRecipeCategory Category,
    string DisplayName,
    IReadOnlyList<DemoCookbookWorkspaceNavigationItem> Recipes);
public sealed record DemoCookbookWorkspaceNavigationItem(
    string RecipeId,
    string Title,
    DemoCookbookRecipeCategory Category,
    bool IsSelected);
public sealed record DemoCookbookWorkspaceRecipeContent(
    string RecipeId,
    string Title,
    string Summary,
    DemoCookbookRecipeCategory Category,
    string RouteStatus,
    string RouteStatusDescription,
    string UnavailableActionDescription,
    IReadOnlyList<DemoCookbookWorkspaceAnchor> GraphAnchors,
    IReadOnlyList<DemoCookbookWorkspaceAnchor> CodeExamples,
    IReadOnlyList<DemoCookbookWorkspaceAnchor> DocumentationLinks,
    IReadOnlyList<DemoCookbookWorkspaceScenarioPoint> ScenarioPoints,
    IReadOnlyList<DemoCookbookWorkspaceInteractionFacet> InteractionFacets,
    IReadOnlyList<DemoCookbookWorkspaceWorkflowStep> WorkflowSteps,
    IReadOnlyList<string> ComponentShowcaseLines,
    IReadOnlyList<string> ProofMarkers,
    IReadOnlyList<string> DeferredGaps,
    DemoCookbookRouteClarity RouteClarity,
    string SupportBoundary,
    string CodeSample);
public sealed record DemoCookbookWorkspaceAnchor(
    string Label,
    string Path,
    string Evidence);
public sealed record DemoCookbookWorkspaceScenarioPoint(
    string Key,
    DemoCookbookScenarioKind Kind,
    string Label,
    string Evidence,
    string GraphCueLabel,
    string GraphCueTarget,
    string ContentCue);
public sealed record DemoCookbookWorkspaceInteractionFacet(
    string Key,
    DemoCookbookInteractionKind Kind,
    string Label,
    string Evidence,
    string FocusLabel,
    string FocusTarget);
public static partial class DemoCookbookWorkspaceProjection
{
    public static DemoCookbookWorkspaceSnapshot Create(
        string? selectedRecipeId = null,
        IReadOnlyList<DemoCookbookRecipe>? navigationRecipes = null)
    {
        var recipes = DemoCookbookCatalog.Recipes;
        if (recipes.Count == 0)
        {
            throw new InvalidOperationException("Demo cookbook workspace requires at least one recipe.");
        }

        var selectedRecipe = string.IsNullOrWhiteSpace(selectedRecipeId)
            ? recipes[0]
            : recipes.Single(recipe => string.Equals(recipe.Id, selectedRecipeId, StringComparison.Ordinal));

        return new DemoCookbookWorkspaceSnapshot(
            CreateNavigationGroups(navigationRecipes ?? recipes, selectedRecipe.Id),
            CreateRecipeContent(selectedRecipe));
    }
    private static IReadOnlyList<DemoCookbookWorkspaceNavigationGroup> CreateNavigationGroups(
        IReadOnlyList<DemoCookbookRecipe> recipes,
        string selectedRecipeId)
        => DemoCookbookCatalog.RequiredCategories
            .Where(category => recipes.Any(recipe => recipe.Category == category))
            .Select(category => new DemoCookbookWorkspaceNavigationGroup(
                category,
                FormatCategory(category),
                recipes
                    .Where(recipe => recipe.Category == category)
                    .Select(recipe => new DemoCookbookWorkspaceNavigationItem(
                        recipe.Id,
                        recipe.Title,
                        recipe.Category,
                        string.Equals(recipe.Id, selectedRecipeId, StringComparison.Ordinal)))
                    .ToArray()))
            .ToArray();
    private static DemoCookbookWorkspaceRecipeContent CreateRecipeContent(DemoCookbookRecipe recipe)
    {
        var posture = CreateRoutePosture(recipe.Category);

        return new DemoCookbookWorkspaceRecipeContent(
            recipe.Id,
            recipe.Title,
            recipe.Summary,
            recipe.Category,
            posture.Status,
            posture.Description,
            posture.UnavailableActionDescription,
            ConvertAnchors(recipe.DemoAnchors),
            ConvertAnchors(recipe.CodeAnchors),
            ConvertAnchors(recipe.DocumentationAnchors),
            ConvertScenarioPoints(recipe),
            ConvertInteractionFacets(recipe),
            ConvertWorkflowSteps(recipe),
            CreateComponentShowcaseLines(recipe),
            recipe.ProofMarkers.ToArray(),
            posture.DeferredGaps,
            recipe.RouteClarity,
            recipe.SupportBoundary,
            recipe.CodeSample);
    }

    private static DemoCookbookRoutePosture CreateRoutePosture(DemoCookbookRecipeCategory category)
        => category switch
        {
            DemoCookbookRecipeCategory.StarterHost => new DemoCookbookRoutePosture(
                "Supported SDK route",
                "Uses the shipped Avalonia host path and canonical startup/onboarding route.",
                "This recipe is display guidance; no runtime code execution is enabled.",
                ["WPF cookbook parity remains deferred."]),
            DemoCookbookRecipeCategory.Authoring => new DemoCookbookRoutePosture(
                "Supported SDK route",
                "Uses public authoring, command, node, port, and parameter extension seams.",
                "This recipe is display guidance; no generated workflow feature is enabled.",
                ["Retained/migration cookbook depth remains deferred."]),
            DemoCookbookRecipeCategory.PerformanceViewport => new DemoCookbookRoutePosture(
                "Proof/demo route",
                "Uses public viewport projection, minimap cadence, and layout command seams.",
                "This recipe is display guidance; no second renderer, background graph index, or runtime execution mode is enabled.",
                ["Background graph indexing, alternate renderers, and runtime execution modes remain deferred."]),
            DemoCookbookRecipeCategory.GroupsSubgraphs => new DemoCookbookRoutePosture(
                "Proof/demo route",
                "Demonstrates persisted groups, composite scopes, collapsed projection, and boundary-edge evidence.",
                "Group/subgraph proof is display guidance; no generated workflow feature is enabled.",
                ["Nested group mutation and specialized boundary-edge styling remain deferred."]),
            DemoCookbookRecipeCategory.PluginTrust => new DemoCookbookRoutePosture(
                "Proof/demo route",
                "Demonstrates trusted in-process plugin decisions and evidence without sandbox claims.",
                "Deferred plugin gaps are informational; no sandbox or marketplace feature is enabled.",
                ["Marketplace, remote distribution, unload lifecycle, and sandboxing remain deferred."]),
            DemoCookbookRecipeCategory.DiagnosticsSupport => new DemoCookbookRoutePosture(
                "Proof/demo route",
                "Demonstrates local diagnostics and support-bundle handoff evidence.",
                "Support evidence stays local; no telemetry or remote sync feature is enabled.",
                ["Remote telemetry, remote sync, and stronger support-scope claims remain deferred."]),
            DemoCookbookRecipeCategory.ReviewHelp => new DemoCookbookRoutePosture(
                "Proof/demo route",
                "Demonstrates validation, repair/help, and review-loop evidence without adding a workflow engine.",
                "Review/help gaps are informational; no macro or scheduling feature is enabled.",
                ["Workflow execution scheduling and macro systems remain deferred."]),
            _ => new DemoCookbookRoutePosture(
                "Proof/demo route",
                "Demonstrates existing cookbook evidence for this route.",
                "Additional route depth is not enabled without adopter evidence.",
                ["Additional route depth requires adopter evidence."]),
        };

    private sealed record DemoCookbookRoutePosture(
        string Status,
        string Description,
        string UnavailableActionDescription,
        IReadOnlyList<string> DeferredGaps);

    private static IReadOnlyList<DemoCookbookWorkspaceAnchor> ConvertAnchors(IReadOnlyList<DemoCookbookAnchor> anchors)
        => anchors
            .Select(anchor => new DemoCookbookWorkspaceAnchor(anchor.Label, anchor.Path, anchor.Evidence))
            .ToArray();

    private static IReadOnlyList<DemoCookbookWorkspaceScenarioPoint> ConvertScenarioPoints(DemoCookbookRecipe recipe)
        => recipe.ScenarioPoints
            .Select((point, index) => new DemoCookbookWorkspaceScenarioPoint(
                CreateScenarioPointKey(recipe.Id, index),
                point.Kind,
                point.Label,
                point.Evidence,
                FormatScenarioCueLabel(point.Kind),
                ResolveEvidenceTarget(recipe, point.Evidence, "scenario"),
                FormatScenarioContentCue(point.Kind, recipe.Id)))
            .ToArray();

    private static string CreateScenarioPointKey(string recipeId, int index)
        => recipeId + ":scenario-" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string FormatScenarioCueLabel(DemoCookbookScenarioKind kind)
        => kind switch
        {
            DemoCookbookScenarioKind.GraphOperations => "Graph operation cue",
            DemoCookbookScenarioKind.NodeMetadata => "Node metadata cue",
            DemoCookbookScenarioKind.ValidationRuntimeOverlay => "Runtime overlay cue",
            DemoCookbookScenarioKind.SupportEvidence => "Support evidence cue",
            DemoCookbookScenarioKind.HostCodeExample => "Host code cue",
            _ => kind + " cue",
        };

    private static string FormatScenarioContentCue(DemoCookbookScenarioKind kind, string routeId)
        => routeId + " / " + kind;

    private static IReadOnlyList<DemoCookbookWorkspaceInteractionFacet> ConvertInteractionFacets(
        DemoCookbookRecipe recipe)
        => recipe.InteractionFacets
            .Select((facet, index) => new DemoCookbookWorkspaceInteractionFacet(
                CreateInteractionFacetKey(recipe.Id, index),
                facet.Kind,
                facet.Label,
                facet.Evidence,
                FormatInteractionFocusLabel(facet.Kind),
                ResolveEvidenceTarget(recipe, facet.Evidence, "interaction")))
            .ToArray();

    private static string CreateInteractionFacetKey(string recipeId, int index)
        => recipeId + ":interaction-" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string ResolveEvidenceTarget(DemoCookbookRecipe recipe, string evidence, string itemKind)
    {
        var anchor = recipe.CodeAnchors
            .Concat(recipe.DemoAnchors)
            .Concat(recipe.DocumentationAnchors)
            .FirstOrDefault(anchor => string.Equals(anchor.Evidence, evidence, StringComparison.Ordinal));

        if (anchor is not null)
        {
            return anchor.Path + "#" + evidence;
        }

        if (recipe.ProofMarkers.Contains(evidence, StringComparer.Ordinal))
        {
            return "proof:" + evidence;
        }

        throw new InvalidOperationException(
            $"Cookbook {itemKind} evidence '{evidence}' is not owned by recipe '{recipe.Id}'.");
    }

    private static string FormatInteractionFocusLabel(DemoCookbookInteractionKind kind)
        => kind switch
        {
            DemoCookbookInteractionKind.Selection => "Selection focus",
            DemoCookbookInteractionKind.Connection => "Connection focus",
            DemoCookbookInteractionKind.LayoutReadability => "Layout/readability focus",
            DemoCookbookInteractionKind.Inspection => "Inspection focus",
            DemoCookbookInteractionKind.ValidationRuntimeFeedback => "Validation/runtime feedback focus",
            _ => kind + " focus",
        };

    private static string FormatCategory(DemoCookbookRecipeCategory category)
        => category switch
        {
            DemoCookbookRecipeCategory.StarterHost => "Starter Host",
            DemoCookbookRecipeCategory.Authoring => "Authoring",
            DemoCookbookRecipeCategory.PerformanceViewport => "Performance And Viewport",
            DemoCookbookRecipeCategory.GroupsSubgraphs => "Groups And Subgraphs",
            DemoCookbookRecipeCategory.PluginTrust => "Plugin Trust",
            DemoCookbookRecipeCategory.DiagnosticsSupport => "Diagnostics Support",
            DemoCookbookRecipeCategory.ReviewHelp => "Review Help",
            _ => category.ToString(),
        };
}
