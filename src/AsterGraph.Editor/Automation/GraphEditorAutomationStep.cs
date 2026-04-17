using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Automation;

/// <summary>
/// Describes one automation step.
/// </summary>
public sealed record GraphEditorAutomationStep
{
    /// <summary>
    /// Initializes one automation step.
    /// </summary>
    /// <param name="stepId">A stable step identifier.</param>
    /// <param name="command">The stable command invocation executed by the step.</param>
    public GraphEditorAutomationStep(string stepId, GraphEditorCommandInvocationSnapshot command)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepId);
        ArgumentNullException.ThrowIfNull(command);

        StepId = stepId.Trim();
        Command = command;
    }

    /// <summary>
    /// Gets the stable step identifier.
    /// </summary>
    public string StepId { get; }

    /// <summary>
    /// Gets the stable command invocation executed by the step.
    /// </summary>
    public GraphEditorCommandInvocationSnapshot Command { get; }
}
