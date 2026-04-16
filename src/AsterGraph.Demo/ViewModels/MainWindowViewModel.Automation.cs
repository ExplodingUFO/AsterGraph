using CommunityToolkit.Mvvm.Input;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    private readonly List<GraphEditorAutomationStepExecutionSnapshot> _automationProgressSteps = [];

    public GraphEditorAutomationRunRequest? LastAutomationRequest { get; private set; }

    public GraphEditorAutomationExecutionSnapshot? LastAutomationResult { get; private set; }

    public IReadOnlyList<GraphEditorAutomationStepExecutionSnapshot> AutomationProgressSteps
        => _automationProgressSteps;

    public IReadOnlyList<string> AutomationRequestLines =>
        LastAutomationRequest is null
            ?
            [
                "选择一个 automation run 以查看 request 和 step 列表。",
            ]
            :
            [
                $"RunId：{LastAutomationRequest.RunId}",
                $"Mutation scope：{BoolText(LastAutomationRequest.RunInMutationScope)}",
                $"Stop on failure：{BoolText(LastAutomationRequest.StopOnFailure)}",
                .. LastAutomationRequest.Steps.Select(step => $"Step · {step.StepId} · {step.Command.CommandId}"),
            ];

    public IReadOnlyList<string> AutomationProgressLines =>
        _automationProgressSteps.Count == 0
            ?
            [
                "当前没有自动化进度事件。",
            ]
            : _automationProgressSteps.Select(step =>
                step.Succeeded
                    ? $"Completed · {step.StepId} · {step.CommandId}"
                    : $"Failed · {step.StepId} · {step.CommandId} · {step.FailureCode ?? step.FailureMessage ?? "Unknown failure"}")
                .ToArray();

    public IReadOnlyList<string> AutomationResultLines =>
        LastAutomationResult is null
            ?
            [
                "当前没有 automation result。",
            ]
            :
            [
                $"RunId：{LastAutomationResult.RunId}",
                $"Succeeded：{BoolText(LastAutomationResult.Succeeded)}",
                $"Executed steps：{LastAutomationResult.ExecutedStepCount}/{LastAutomationResult.TotalStepCount}",
                $"Inspection nodes：{LastAutomationResult.Inspection.Document.Nodes.Count}",
                $"Inspection connections：{LastAutomationResult.Inspection.Document.Connections.Count}",
                LastAutomationResult.FailureMessage is null
                    ? "Failure：none"
                    : $"Failure：{LastAutomationResult.FailureMessage}",
            ];

    [RelayCommand]
    public void RunSelectionAutomation()
        => ExecuteAutomation(new GraphEditorAutomationRunRequest(
            "demo.focus-output",
            [
                CreateAutomationStep("select-output", "selection.set", ("nodeId", "output"), ("primaryNodeId", "output"), ("updateStatus", "false")),
                CreateAutomationStep("center-output", "viewport.center-node", ("nodeId", "output")),
                CreateAutomationStep("fit-view", "viewport.fit", ("updateStatus", "false")),
            ],
            mutationLabel: "demo.focus-output"));

    [RelayCommand]
    public void RunPluginAutomation()
        => ExecuteAutomation(new GraphEditorAutomationRunRequest(
            "demo.plugin-showcase",
            [
                CreateAutomationStep("select-light", "selection.set", ("nodeId", "light"), ("primaryNodeId", "light"), ("updateStatus", "false")),
                CreateAutomationStep("add-plugin-node", "nodes.add", ("definitionId", "aster.demo.plugin.showcase-node"), ("worldX", "1700"), ("worldY", "220")),
            ],
            mutationLabel: "demo.plugin-showcase"));

    [RelayCommand]
    public void RunWorkspaceAutomation()
        => ExecuteAutomation(new GraphEditorAutomationRunRequest(
            "demo.workspace-proof",
            [
                CreateAutomationStep("select-source", "selection.set", ("nodeId", "time"), ("primaryNodeId", "time"), ("updateStatus", "false")),
                CreateAutomationStep("save-workspace", "workspace.save"),
            ],
            mutationLabel: "demo.workspace-proof"));

    private void ExecuteAutomation(GraphEditorAutomationRunRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        LastAutomationRequest = request;
        LastAutomationResult = null;
        _automationProgressSteps.Clear();
        Editor.Session.Automation.Execute(request);
        RefreshRuntimeProjection();
    }

    private void OnAutomationStarted(GraphEditorAutomationStartedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);
        RefreshRuntimeProjection();
    }

    private void OnAutomationProgress(GraphEditorAutomationProgressEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);
        _automationProgressSteps.Add(args.Step);
        RefreshRuntimeProjection();
    }

    private void OnAutomationCompleted(GraphEditorAutomationCompletedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);
        LastAutomationResult = args.Result;
        RefreshRuntimeProjection();
    }

    private static GraphEditorAutomationStep CreateAutomationStep(
        string stepId,
        string commandId,
        params (string Name, string Value)[] arguments)
        => new(
            stepId,
            new GraphEditorCommandInvocationSnapshot(
                commandId,
                arguments.Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value)).ToList()));
}
