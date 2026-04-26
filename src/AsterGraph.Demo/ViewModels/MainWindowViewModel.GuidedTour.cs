using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Demo.ViewModels;

public sealed record ScenarioTourStep(
    string Key,
    string Title,
    string Summary,
    string RelatedHostGroup);

public partial class MainWindowViewModel
{
    private const string TourStepCreateNode = "create-node";
    private const string TourStepConnectPorts = "connect-ports";
    private const string TourStepEditParameters = "edit-parameters";
    private const string TourStepTrustPlugin = "trust-plugin";
    private const string TourStepRunAutomation = "run-automation";
    private const string TourStepSaveAndExport = "save-and-export";
    private IReadOnlyList<ScenarioTourStep> _scenarioTourSteps = [];
    private string? _tourCreatedNodeId;
    private string? _lastScenarioTourWorkspacePath;
    private string? _lastScenarioTourExportPath;

    [ObservableProperty]
    private ScenarioTourStep selectedScenarioTourStep = null!;

    [ObservableProperty]
    private string lastScenarioTourActionResult = string.Empty;

    public IReadOnlyList<ScenarioTourStep> ScenarioTourSteps => _scenarioTourSteps;

    public string ScenarioTourProgressText
        => T("步骤 ", "Step ") + $"{ResolveSelectedScenarioTourStepIndex() + 1}/{ScenarioTourSteps.Count}";

    public IReadOnlyList<string> ScenarioTourSignalLines
    {
        get
        {
            var document = Session.Queries.CreateDocumentSnapshot();
            var aiNodeCount = document.Nodes.Count(node => node.Category == "AI Pipeline");
            var hasPromptToLlm = document.Connections.Any(connection =>
                connection.SourceNodeId == "prompt" &&
                connection.SourcePortId == "prompt" &&
                connection.TargetNodeId == "llm" &&
                connection.TargetPortId == "prompt");
            var prompt = document.Nodes.FirstOrDefault(node => node.Id == "prompt");
            var systemPrompt = prompt?.ParameterValues?.FirstOrDefault(parameter => parameter.Key == "systemPrompt")?.Value?.ToString();
            var allowedPlugins = PluginCandidateEntries.Count(entry => entry.IsAllowed);
            var blockedPlugins = PluginCandidateEntries.Count(entry => entry.IsBlocked);

            return
            [
                T("自定义节点：", "Custom nodes: ") + aiNodeCount + T(" 个 AI Pipeline 节点", " AI Pipeline nodes"),
                T("连接校验：", "Connection validation: ") + (hasPromptToLlm ? "prompt.prompt -> llm.prompt" : T("缺少 prompt 到 LLM 的 typed connection", "missing prompt to LLM typed connection")),
                T("参数编辑：", "Parameter editing: ") + (string.IsNullOrWhiteSpace(systemPrompt) ? T("未找到 systemPrompt", "systemPrompt missing") : $"systemPrompt={systemPrompt}"),
                T("插件信任：", "Plugin trust: ") + $"allowed={allowedPlugins}; blocked={blockedPlugins}",
                T("自动化证明：", "Automation proof: ") + (LastAutomationResult is null ? T("尚未运行", "not run yet") : $"{LastAutomationResult.RunId}; succeeded={LastAutomationResult.Succeeded}"),
                T("保存 / 加载：", "Save / load: ") + (string.IsNullOrWhiteSpace(_lastScenarioTourWorkspacePath) ? T("尚未运行", "not run yet") : _lastScenarioTourWorkspacePath),
                T("导出：", "Export: ") + (string.IsNullOrWhiteSpace(_lastScenarioTourExportPath) ? T("尚未运行", "not run yet") : _lastScenarioTourExportPath),
            ];
        }
    }

    [RelayCommand]
    public void SelectScenarioTourStep(ScenarioTourStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        SelectedScenarioTourStep = step;
    }

    [RelayCommand]
    public void PreviousScenarioTourStep()
    {
        var index = ResolveSelectedScenarioTourStepIndex();
        SelectedScenarioTourStep = ScenarioTourSteps[Math.Max(0, index - 1)];
    }

    [RelayCommand]
    public void NextScenarioTourStep()
    {
        var index = ResolveSelectedScenarioTourStepIndex();
        SelectedScenarioTourStep = ScenarioTourSteps[Math.Min(ScenarioTourSteps.Count - 1, index + 1)];
    }

    [RelayCommand]
    public void RunSelectedScenarioTourStep()
        => RunScenarioTourStep(SelectedScenarioTourStep.Key);

    [RelayCommand]
    public void OpenSelectedScenarioTourStepPanel()
        => OpenHostMenuGroup(SelectedScenarioTourStep.RelatedHostGroup);

    public void RunScenarioTourStep(string stepKey)
    {
        LastScenarioTourActionResult = stepKey switch
        {
            TourStepCreateNode => RunCreateNodeTourStep(),
            TourStepConnectPorts => RunConnectPortsTourStep(),
            TourStepEditParameters => RunEditParametersTourStep(),
            TourStepTrustPlugin => RunTrustPluginTourStep(),
            TourStepRunAutomation => RunAutomationTourStep(),
            TourStepSaveAndExport => RunSaveAndExportTourStep(),
            _ => throw new ArgumentException($"Unknown scenario tour step '{stepKey}'.", nameof(stepKey)),
        };

        RefreshRuntimeProjection();
        RefreshScenarioTourProjection();
    }

    partial void OnSelectedScenarioTourStepChanged(ScenarioTourStep value)
        => RefreshScenarioTourProjection();

    private void UpdateScenarioTour()
    {
        var selectedKey = SelectedScenarioTourStep?.Key;
        _scenarioTourSteps = CreateScenarioTourSteps();
        OnPropertyChanged(nameof(ScenarioTourSteps));

        SelectedScenarioTourStep = _scenarioTourSteps.SingleOrDefault(step => step.Key == selectedKey) ?? _scenarioTourSteps[0];
        if (string.IsNullOrWhiteSpace(LastScenarioTourActionResult))
        {
            LastScenarioTourActionResult = T("尚未运行导览步骤。", "No tour step has run yet.");
        }

        RefreshScenarioTourProjection();
    }

    private IReadOnlyList<ScenarioTourStep> CreateScenarioTourSteps()
        =>
        [
            new ScenarioTourStep(
                TourStepCreateNode,
                T("创建节点", "Create Node"),
                T("通过 session command 插入一个新的 Prompt 节点，证明场景使用真实自定义节点定义。", "Insert a new Prompt node through a session command to prove the scenario uses real custom node definitions."),
                DemoHostMenuGroups.Showcase),
            new ScenarioTourStep(
                TourStepConnectPorts,
                T("连接端口", "Connect Ports"),
                T("把 Input 的 text 输出连接到新 Prompt 的 context 输入，走 canonical typed connection path。", "Connect the Input text output to the new Prompt context input through the canonical typed connection path."),
                DemoHostMenuGroups.Runtime),
            new ScenarioTourStep(
                TourStepEditParameters,
                T("编辑参数", "Edit Parameters"),
                T("选择 Prompt 节点并写入 systemPrompt，证明参数检查器和 session parameter command 共享同一路径。", "Select the Prompt node and write systemPrompt to prove inspector parameters and session parameter commands share the same path."),
                DemoHostMenuGroups.Showcase),
            new ScenarioTourStep(
                TourStepTrustPlugin,
                T("信任插件", "Trust Plugin"),
                T("把被阻止的示例插件加入 allowlist，展示宿主拥有的 trust decision。", "Allow the blocked sample plugin to show host-owned trust decisions."),
                DemoHostMenuGroups.Extensions),
            new ScenarioTourStep(
                TourStepRunAutomation,
                T("运行自动化", "Run Automation"),
                T("执行插件自动化并把 request、progress 和 result 投影回同一个 session。", "Run plugin automation and project request, progress, and result back onto the same session."),
                DemoHostMenuGroups.Automation),
            new ScenarioTourStep(
                TourStepSaveAndExport,
                T("保存并导出", "Save And Export"),
                T("保存再重新加载当前工作区，并导出 SVG 场景证明宿主工作流闭环。", "Save and reload the current workspace, then export SVG to prove the host workflow closes the loop."),
                DemoHostMenuGroups.Runtime),
        ];

    private string RunCreateNodeTourStep()
    {
        var before = Editor.Nodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
        Session.Commands.AddNode(new NodeDefinitionId("aster.demo.prompt-builder"), new GraphPoint(1880, 120));
        var created = Editor.Nodes.First(node => !before.Contains(node.Id));
        _tourCreatedNodeId = created.Id;
        Session.Commands.SetSelection([created.Id], created.Id, updateStatus: false);
        return T("已创建 Prompt 节点：", "Created Prompt node: ") + created.Id;
    }

    private string RunConnectPortsTourStep()
    {
        var targetNodeId = EnsureTourPromptNode();
        var before = Session.Queries.CreateDocumentSnapshot().Connections.Count;
        Session.Commands.StartConnection("input", "text");
        Session.Commands.CompleteConnection(targetNodeId, "context");
        var after = Session.Queries.CreateDocumentSnapshot().Connections.Count;
        return T("已连接 Input.text -> Prompt.context；连线数：", "Connected Input.text -> Prompt.context; connections: ") + $"{before}->{after}";
    }

    private string RunEditParametersTourStep()
    {
        Session.Commands.SetSelection(["prompt"], "prompt", updateStatus: false);
        var edited = Session.Commands.TrySetSelectedNodeParameterValue(
            "systemPrompt",
            "You are an AsterGraph tour agent. Return concise actions and cited evidence.");
        return T("systemPrompt 编辑结果：", "systemPrompt edit result: ") + BoolText(edited);
    }

    private string RunTrustPluginTourStep()
    {
        var changed = TrustPluginCandidate("aster.demo.plugin.blocked");
        return T("插件 allowlist 更新：", "Plugin allowlist updated: ") + BoolText(changed);
    }

    private string RunAutomationTourStep()
    {
        RunPluginAutomation();
        return LastAutomationResult is null
            ? T("自动化未返回结果。", "Automation did not return a result.")
            : T("自动化完成：", "Automation completed: ") + $"{LastAutomationResult.RunId}; succeeded={LastAutomationResult.Succeeded}";
    }

    private string RunSaveAndExportTourStep()
    {
        _lastScenarioTourWorkspacePath = Path.Combine(_shellStateStore.StorageRootPath, "scenario-tour-workspace.json");
        _lastScenarioTourExportPath = Path.Combine(_shellStateStore.StorageRootPath, "scenario-tour-export.svg");

        SaveWorkspaceAs(_lastScenarioTourWorkspacePath);
        var loaded = TryOpenWorkspacePath(_lastScenarioTourWorkspacePath);
        var exported = Session.Commands.TryExportSceneAsSvg(_lastScenarioTourExportPath);

        return T("保存 / 加载 / 导出：", "Save / load / export: ") + $"load={loaded}; export={exported}";
    }

    private string EnsureTourPromptNode()
    {
        if (!string.IsNullOrWhiteSpace(_tourCreatedNodeId) && Editor.FindNode(_tourCreatedNodeId) is not null)
        {
            return _tourCreatedNodeId;
        }

        RunCreateNodeTourStep();
        return _tourCreatedNodeId!;
    }

    private int ResolveSelectedScenarioTourStepIndex()
    {
        var index = ScenarioTourSteps
            .Select((step, stepIndex) => new { step, stepIndex })
            .FirstOrDefault(item => string.Equals(item.step.Key, SelectedScenarioTourStep.Key, StringComparison.Ordinal))
            ?.stepIndex;

        return index ?? 0;
    }

    private void RefreshScenarioTourProjection()
    {
        OnPropertyChanged(nameof(SelectedScenarioTourStep));
        OnPropertyChanged(nameof(ScenarioTourProgressText));
        OnPropertyChanged(nameof(ScenarioTourSignalLines));
        OnPropertyChanged(nameof(LastScenarioTourActionResult));
    }
}
