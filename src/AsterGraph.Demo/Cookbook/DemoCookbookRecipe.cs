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
    string SupportBoundary,
    IReadOnlyList<DemoCookbookWorkflowStep>? WorkflowSteps = null)
{
    public IReadOnlyList<DemoCookbookWorkflowStep> WorkflowSteps { get; } = WorkflowSteps ?? [];
}

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

public sealed record DemoCookbookWorkflowStep(
    DemoCookbookWorkflowKind Kind,
    string Title,
    string CommandId,
    string CodeEvidence,
    string DemoEvidence,
    string ProofMarker);

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

public enum DemoCookbookWorkflowKind
{
    CommandRegistry,
    SemanticEditing,
    TemplatePreset,
    SelectionTransform,
    NavigationFocus,
}
