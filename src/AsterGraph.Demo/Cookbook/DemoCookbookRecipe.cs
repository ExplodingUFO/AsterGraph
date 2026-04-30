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
    IReadOnlyList<string> ProofMarkers,
    string SupportBoundary);

public sealed record DemoCookbookAnchor(
    string Label,
    string Path,
    string Evidence);

public sealed record DemoCookbookScenarioPoint(
    DemoCookbookScenarioKind Kind,
    string Label,
    string Evidence);

public enum DemoCookbookScenarioKind
{
    GraphOperations,
    NodeMetadata,
    ValidationRuntimeOverlay,
    SupportEvidence,
    HostCodeExample,
}
