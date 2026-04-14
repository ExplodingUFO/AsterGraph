using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Runtime.Internal;

internal interface IGraphEditorSessionAutomationExecutorHost
{
    IGraphEditorMutationScope BeginMutation(string? label);

    IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors();

    bool TryExecuteCommand(GraphEditorCommandInvocationSnapshot command);

    GraphEditorInspectionSnapshot CaptureInspectionSnapshot();

    void PublishDiagnostic(GraphEditorDiagnostic diagnostic);

    void PublishAutomationStarted(GraphEditorAutomationStartedEventArgs args);

    void PublishAutomationProgress(GraphEditorAutomationProgressEventArgs args);

    void PublishAutomationCompleted(GraphEditorAutomationCompletedEventArgs args);
}

internal sealed class GraphEditorSessionAutomationExecutor
{
    private readonly IGraphEditorSessionAutomationExecutorHost _host;

    public GraphEditorSessionAutomationExecutor(IGraphEditorSessionAutomationExecutorHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public GraphEditorAutomationExecutionSnapshot Execute(GraphEditorAutomationRunRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var startedArgs = new GraphEditorAutomationStartedEventArgs(
            request.RunId,
            request.Steps.Count,
            request.RunInMutationScope,
            request.MutationLabel);

        _host.PublishDiagnostic(new GraphEditorDiagnostic(
            "automation.run.started",
            "automation.run",
            $"Automation run '{request.RunId}' started with {request.Steps.Count} step(s).",
            GraphEditorDiagnosticSeverity.Info));
        _host.PublishAutomationStarted(startedArgs);

        var stepResults = new List<GraphEditorAutomationStepExecutionSnapshot>(request.Steps.Count);
        string? failureCode = null;
        string? failureMessage = null;

        IGraphEditorMutationScope? mutationScope = null;
        try
        {
            if (request.RunInMutationScope)
            {
                mutationScope = _host.BeginMutation(request.MutationLabel);
            }

            for (var stepIndex = 0; stepIndex < request.Steps.Count; stepIndex++)
            {
                var step = request.Steps[stepIndex];
                var stepResult = ExecuteStep(step, stepIndex);
                stepResults.Add(stepResult);

                if (!stepResult.Succeeded)
                {
                    failureCode ??= stepResult.FailureCode;
                    failureMessage ??= stepResult.FailureMessage;

                    _host.PublishDiagnostic(new GraphEditorDiagnostic(
                        "automation.step.failed",
                        "automation.run",
                        stepResult.FailureMessage
                            ?? $"Automation step '{step.StepId}' ({step.Command.CommandId}) failed.",
                        GraphEditorDiagnosticSeverity.Error));
                }

                _host.PublishAutomationProgress(new GraphEditorAutomationProgressEventArgs(
                    request.RunId,
                    stepResults.Count,
                    request.Steps.Count,
                    request.RunInMutationScope,
                    request.MutationLabel,
                    stepResult));

                if (!stepResult.Succeeded && request.StopOnFailure)
                {
                    break;
                }
            }
        }
        finally
        {
            mutationScope?.Dispose();
        }

        var succeeded = failureCode is null;
        _host.PublishDiagnostic(new GraphEditorDiagnostic(
            "automation.run.completed",
            "automation.run",
            succeeded
                ? $"Automation run '{request.RunId}' completed successfully ({stepResults.Count}/{request.Steps.Count} steps)."
                : $"Automation run '{request.RunId}' completed with failure after {stepResults.Count}/{request.Steps.Count} steps: {failureMessage ?? failureCode ?? "Unknown failure."}",
            succeeded ? GraphEditorDiagnosticSeverity.Info : GraphEditorDiagnosticSeverity.Warning));

        var result = new GraphEditorAutomationExecutionSnapshot(
            request.RunId,
            succeeded,
            request.RunInMutationScope,
            request.MutationLabel,
            stepResults.Count,
            request.Steps.Count,
            stepResults,
            _host.CaptureInspectionSnapshot(),
            failureCode,
            failureMessage);

        _host.PublishAutomationCompleted(new GraphEditorAutomationCompletedEventArgs(result));
        return result;
    }

    private GraphEditorAutomationStepExecutionSnapshot ExecuteStep(GraphEditorAutomationStep step, int stepIndex)
    {
        var descriptors = _host.GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        if (!descriptors.TryGetValue(step.Command.CommandId, out var descriptor))
        {
            return CreateStepFailure(
                step,
                stepIndex,
                "automation.step.command-unknown",
                $"Automation command '{step.Command.CommandId}' is not discoverable in the current session.");
        }

        if (!descriptor.IsEnabled)
        {
            return CreateStepFailure(
                step,
                stepIndex,
                "automation.step.command-disabled",
                descriptor.DisabledReason
                    ?? $"Automation command '{step.Command.CommandId}' is disabled in the current session.");
        }

        try
        {
            if (!_host.TryExecuteCommand(step.Command))
            {
                return CreateStepFailure(
                    step,
                    stepIndex,
                    "automation.step.dispatch-failed",
                    $"Automation command '{step.Command.CommandId}' could not be executed.");
            }
        }
        catch (Exception exception)
        {
            return CreateStepFailure(
                step,
                stepIndex,
                "automation.step.exception",
                $"Automation command '{step.Command.CommandId}' threw an exception: {exception.Message}");
        }

        return new GraphEditorAutomationStepExecutionSnapshot(stepIndex, step.StepId, step.Command.CommandId, true);
    }

    private static GraphEditorAutomationStepExecutionSnapshot CreateStepFailure(
        GraphEditorAutomationStep step,
        int stepIndex,
        string failureCode,
        string failureMessage)
        => new(stepIndex, step.StepId, step.Command.CommandId, false, failureCode, failureMessage);
}
