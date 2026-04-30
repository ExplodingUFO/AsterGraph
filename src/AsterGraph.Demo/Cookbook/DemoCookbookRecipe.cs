namespace AsterGraph.Demo.Cookbook;

public sealed record DemoCookbookRecipe(
    string Id,
    DemoCookbookRecipeCategory Category,
    string Title,
    string Summary,
    IReadOnlyList<DemoCookbookAnchor> CodeAnchors,
    IReadOnlyList<DemoCookbookAnchor> DemoAnchors,
    IReadOnlyList<DemoCookbookAnchor> DocumentationAnchors,
    IReadOnlyList<DemoCookbookScenarioPoint> ScenarioPoints,
    IReadOnlyList<DemoCookbookInteractionFacet> InteractionFacets,
    IReadOnlyList<string> ProofMarkers,
    DemoCookbookRouteClarity RouteClarity,
    string SupportBoundary);

public sealed record DemoCookbookAnchor(
    string Label,
    string Path,
    string Evidence);

public sealed record DemoCookbookScenarioPoint(
    DemoCookbookScenarioKind Kind,
    string Label,
    string Evidence);

public sealed record DemoCookbookInteractionFacet(
    DemoCookbookInteractionKind Kind,
    string Label,
    string Evidence);

public sealed record DemoCookbookRouteClarity(
    string SupportedRoute,
    string PackageBoundary,
    string DemoBoundary);

public enum DemoCookbookScenarioKind
{
    GraphOperations,
    NodeMetadata,
    ValidationRuntimeOverlay,
    SupportEvidence,
    HostCodeExample,
}

public enum DemoCookbookInteractionKind
{
    Selection,
    Connection,
    LayoutReadability,
    Inspection,
    ValidationRuntimeFeedback,
}
