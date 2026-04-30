namespace AsterGraph.Demo.Cookbook;

public sealed record DemoCookbookWorkspaceWorkflowStep(
    string Key,
    DemoCookbookWorkflowKind Kind,
    string Title,
    string CommandId,
    string CodeTarget,
    string DemoTarget,
    string ProofMarker);

public static partial class DemoCookbookWorkspaceProjection
{
    private static IReadOnlyList<DemoCookbookWorkspaceWorkflowStep> ConvertWorkflowSteps(
        DemoCookbookRecipe recipe)
        => recipe.WorkflowSteps
            .Select((step, index) => new DemoCookbookWorkspaceWorkflowStep(
                recipe.Id + ":workflow-" + index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                step.Kind,
                step.Title,
                step.CommandId,
                ResolveEvidenceTarget(recipe, step.CodeEvidence, "workflow code"),
                ResolveEvidenceTarget(recipe, step.DemoEvidence, "workflow demo"),
                step.ProofMarker))
            .ToArray();
}
