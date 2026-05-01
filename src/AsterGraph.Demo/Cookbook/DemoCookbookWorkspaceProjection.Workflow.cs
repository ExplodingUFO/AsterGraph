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

    private static IReadOnlyList<string> CreateComponentShowcaseLines(DemoCookbookRecipe recipe)
    {
        var lines = new List<string>();

        AddComponentLines(lines, recipe.CodeAnchors, "code");
        AddComponentLines(lines, recipe.DemoAnchors, "demo");
        AddWorkflowComponentLines(lines, recipe);

        return lines
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static void AddComponentLines(
        ICollection<string> lines,
        IReadOnlyList<DemoCookbookAnchor> anchors,
        string surface)
    {
        foreach (var anchor in anchors)
        {
            var lane = ResolveComponentLane(anchor);
            if (lane is null)
            {
                continue;
            }

            lines.Add(lane + " " + surface + ": " + anchor.Label + " -> " + anchor.Path + "#" + anchor.Evidence);
        }
    }

    private static void AddWorkflowComponentLines(ICollection<string> lines, DemoCookbookRecipe recipe)
    {
        foreach (var step in recipe.WorkflowSteps)
        {
            var lane = ResolveWorkflowComponentLane(step.Kind);
            lines.Add(lane + " workflow: " + step.Title + " -> " + step.CommandId + " / " + step.ProofMarker);
        }
    }

    private static string? ResolveComponentLane(DemoCookbookAnchor anchor)
    {
        var text = anchor.Label + " " + anchor.Path + " " + anchor.Evidence;

        if (ContainsAny(text, "Visible scene", "MiniMap", "Minimap", "Viewport", "projection", "renderer", "rendering"))
        {
            return "Rendering";
        }

        if (ContainsAny(text, "custom", "presentation", "presenter", "parameter", "anchor", "extension", "authoring"))
        {
            return "Customization";
        }

        if (ContainsAny(text, "Layout", "Snap", "spatial", "focus", "SearchGraphItems", "CommandRegistry"))
        {
            return "Spatial";
        }

        return null;
    }

    private static string ResolveWorkflowComponentLane(DemoCookbookWorkflowKind kind)
        => kind switch
        {
            DemoCookbookWorkflowKind.CommandRegistry => "Customization",
            DemoCookbookWorkflowKind.SemanticEditing => "Customization",
            DemoCookbookWorkflowKind.TemplatePreset => "Customization",
            DemoCookbookWorkflowKind.SelectionTransform => "Spatial",
            DemoCookbookWorkflowKind.NavigationFocus => "Spatial",
            _ => "Spatial",
        };

    private static bool ContainsAny(string text, params string[] values)
        => values.Any(value => text.Contains(value, StringComparison.OrdinalIgnoreCase));
}
