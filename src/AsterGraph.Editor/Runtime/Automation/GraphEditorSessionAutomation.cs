using AsterGraph.Editor;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    public GraphEditorAutomationExecutionSnapshot Execute(GraphEditorAutomationRunRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var startedArgs = new GraphEditorAutomationStartedEventArgs(
            request.RunId,
            request.Steps.Count,
            request.RunInMutationScope,
            request.MutationLabel);

        PublishDiagnostic(new GraphEditorDiagnostic(
            "automation.run.started",
            "automation.run",
            $"Automation run '{request.RunId}' started with {request.Steps.Count} step(s).",
            GraphEditorDiagnosticSeverity.Info));
        AutomationStarted?.Invoke(this, startedArgs);

        var stepResults = new List<GraphEditorAutomationStepExecutionSnapshot>(request.Steps.Count);
        string? failureCode = null;
        string? failureMessage = null;

        IGraphEditorMutationScope? mutationScope = null;
        try
        {
            if (request.RunInMutationScope)
            {
                mutationScope = BeginMutation(request.MutationLabel);
            }

            for (var stepIndex = 0; stepIndex < request.Steps.Count; stepIndex++)
            {
                var step = request.Steps[stepIndex];
                var stepResult = ExecuteAutomationStep(step, stepIndex);
                stepResults.Add(stepResult);

                if (!stepResult.Succeeded)
                {
                    failureCode ??= stepResult.FailureCode;
                    failureMessage ??= stepResult.FailureMessage;

                    PublishDiagnostic(new GraphEditorDiagnostic(
                        "automation.step.failed",
                        "automation.run",
                        stepResult.FailureMessage
                            ?? $"Automation step '{step.StepId}' ({step.Command.CommandId}) failed.",
                        GraphEditorDiagnosticSeverity.Error));
                }

                AutomationProgress?.Invoke(
                    this,
                    new GraphEditorAutomationProgressEventArgs(
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
        PublishDiagnostic(new GraphEditorDiagnostic(
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
            CaptureInspectionSnapshot(),
            failureCode,
            failureMessage);

        AutomationCompleted?.Invoke(this, new GraphEditorAutomationCompletedEventArgs(result));
        return result;
    }

    private GraphEditorAutomationStepExecutionSnapshot ExecuteAutomationStep(GraphEditorAutomationStep step, int stepIndex)
    {
        var descriptors = GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        if (!descriptors.TryGetValue(step.Command.CommandId, out var descriptor))
        {
            return CreateAutomationStepFailure(
                step,
                stepIndex,
                "automation.step.command-unknown",
                $"Automation command '{step.Command.CommandId}' is not discoverable in the current session.");
        }

        if (!descriptor.IsEnabled)
        {
            return CreateAutomationStepFailure(
                step,
                stepIndex,
                "automation.step.command-disabled",
                descriptor.DisabledReason
                    ?? $"Automation command '{step.Command.CommandId}' is disabled in the current session.");
        }

        try
        {
            if (!Commands.TryExecuteCommand(step.Command))
            {
                return CreateAutomationStepFailure(
                    step,
                    stepIndex,
                    "automation.step.dispatch-failed",
                    $"Automation command '{step.Command.CommandId}' could not be executed.");
            }
        }
        catch (Exception exception)
        {
            return CreateAutomationStepFailure(
                step,
                stepIndex,
                "automation.step.exception",
                $"Automation command '{step.Command.CommandId}' threw an exception: {exception.Message}");
        }

        return new(stepIndex, step.StepId, step.Command.CommandId, true);
    }

    private static GraphEditorAutomationStepExecutionSnapshot CreateAutomationStepFailure(
        GraphEditorAutomationStep step,
        int stepIndex,
        string failureCode,
        string failureMessage)
        => new(stepIndex, step.StepId, step.Command.CommandId, false, failureCode, failureMessage);
}
