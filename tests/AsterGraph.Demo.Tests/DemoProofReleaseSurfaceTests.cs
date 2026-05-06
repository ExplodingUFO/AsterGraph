using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoProofReleaseSurfaceTests
{
    private static readonly string[] AdapterMatrixProofMarkerLines =
    [
    ];

    private static readonly string[] OfficialCapabilityModules =
    [
        "Selection",
        "History",
        "Clipboard",
        "Shortcut Policy",
        "Layout",
        "MiniMap",
        "Stencil",
        "Fragment Library",
        "Export",
        "Baseline Edge Authoring",
    ];

    private static readonly string[] AdvancedEditingCapabilityModules =
    [
        "Node Surface Authoring",
        "Hierarchy Semantics",
        "Composite Scope Authoring",
        "Edge Semantics",
        "Edge Geometry Tooling",
    ];

    private static readonly string[] RepairHelpReviewProofMarkerIds =
    [
        "GRAPH_ERROR_HELP_TARGET_OK",
        "GRAPH_PROBLEM_INSPECTOR_HELP_TARGET_OK",
        "REPAIR_HELP_REVIEW_LOOP_OK",
    ];

    [Fact]
    public void QuickStart_UsesAvaloniaAsDefaultOnboardingPath()
    {
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");

        AssertAppearsBefore(quickStart, "[Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)", "## 1. Pick Your Starting Package");
        AssertAppearsBefore(quickStart, "[Beta Support Bundle](./support-bundle.md)", "## 1. Pick Your Starting Package");
        AssertAppearsBefore(quickStartZh, "[插件信任契约 v1](./plugin-trust-contracts.md)", "## 1. 先选起始包");
        AssertAppearsBefore(quickStartZh, "[Beta Support Bundle](./support-bundle.md)", "## 1. 先选起始包");
        Assert.Contains("onboarding", quickStart, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("default", quickStart, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Avalonia", quickStart, StringComparison.Ordinal);
        Assert.False(
            HasLineWith(quickStart, "WPF", "onboarding"),
            "Quick Start docs must not describe WPF as an onboarding path.");

        Assert.Contains("默认", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("第一跑", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("Avalonia", quickStartZh, StringComparison.Ordinal);
        Assert.False(
            HasLineWith(quickStartZh, "WPF", "第一跑"),
            "Quick Start docs must not describe WPF as a first-run path.");
    }

    [Fact]
    public void PublicEntryDocs_LinkDedicatedEvaluationPathGuide()
    {
        var readme = ReadRepoFile("README.md");
        var readmeZh = ReadRepoFile("README.zh-CN.md");
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");
        var projectStatus = ReadRepoFile("docs/en/project-status.md");
        var projectStatusZh = ReadRepoFile("docs/zh-CN/project-status.md");
        var checklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var checklistZh = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");

        Assert.Contains("evaluation-path.md", readme, StringComparison.Ordinal);
        Assert.Contains("Beta Evaluation Path", readme, StringComparison.Ordinal);
        Assert.Contains("evaluation-path.md", readmeZh, StringComparison.Ordinal);
        Assert.Contains("公开 Beta 评估路径", readmeZh, StringComparison.Ordinal);
        Assert.Contains("evaluation-path.md", quickStart, StringComparison.Ordinal);
        Assert.Contains("Beta Evaluation Path", quickStart, StringComparison.Ordinal);
        Assert.Contains("ConsumerSample.Avalonia -- --proof", quickStart, StringComparison.Ordinal);
        Assert.Contains("HostSample", quickStart, StringComparison.Ordinal);
        Assert.Contains("evaluation-path.md", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("公开 Beta 评估路径", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("ConsumerSample.Avalonia -- --proof", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("HostSample", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("evaluation-path.md", projectStatus, StringComparison.Ordinal);
        Assert.Contains("single route ladder", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("evaluation-path.md", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("单一路径", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("evaluation-path.md", checklist, StringComparison.Ordinal);
        Assert.Contains("single route ladder", checklist, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("evaluation-path.md", checklistZh, StringComparison.Ordinal);
        Assert.Contains("单一路径", checklistZh, StringComparison.Ordinal);
    }

    [Fact]
    public void EvaluationPathGuide_DefinesOneRouteLadderWithoutWideningSupportBoundary()
    {
        var evaluationPath = ReadRepoFile("docs/en/evaluation-path.md");
        var evaluationPathZh = ReadRepoFile("docs/zh-CN/evaluation-path.md");

        AssertAppearsBefore(evaluationPath, "[Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)", "## Boundary First");
        AssertAppearsBefore(evaluationPath, "[Beta Support Bundle](./support-bundle.md)", "## Boundary First");
        AssertAppearsBefore(evaluationPathZh, "[插件信任契约 v1](./plugin-trust-contracts.md)", "## 先锁边界");
        AssertAppearsBefore(evaluationPathZh, "[Beta Support Bundle](./support-bundle.md)", "## 先锁边界");

        foreach (var contents in new[] { evaluationPath, evaluationPathZh })
        {
            Assert.Contains("Starter.Avalonia", contents, StringComparison.Ordinal);
            Assert.Contains("HelloWorld.Avalonia", contents, StringComparison.Ordinal);
            Assert.Contains("ConsumerSample.Avalonia", contents, StringComparison.Ordinal);
            Assert.Contains("HostSample", contents, StringComparison.Ordinal);
            Assert.Contains("CONSUMER_SAMPLE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("HOST_SAMPLE_OK:True", contents, StringComparison.Ordinal);
        }

        Assert.True(HasLineWith(evaluationPath, "WPF", "validation"));
        Assert.Contains("AsterGraph.HelloWorld.Wpf", evaluationPath, StringComparison.Ordinal);
        Assert.Contains("adapter-2-accessibility-recipe.md", evaluationPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adapter-2-performance-recipe.md", evaluationPath, StringComparison.OrdinalIgnoreCase);
        Assert.True(HasLineWith(evaluationPath, "retained", "migration"));
        Assert.True(HasLineWith(evaluationPath, "HostSample", "proof"));
        Assert.True(HasLineWith(evaluationPath, "HostSample", "after"));
        Assert.True(HasLineWith(evaluationPathZh, "WPF", "验证"));
        Assert.Contains("AsterGraph.HelloWorld.Wpf", evaluationPathZh, StringComparison.Ordinal);
        Assert.Contains("adapter-2-accessibility-recipe.md", evaluationPathZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adapter-2-performance-recipe.md", evaluationPathZh, StringComparison.OrdinalIgnoreCase);
        Assert.True(HasLineWith(evaluationPathZh, "retained", "迁移"));
        Assert.True(HasLineWith(evaluationPathZh, "HostSample", "proof"));
        Assert.True(HasLineWith(evaluationPathZh, "HostSample", "之后"));
    }

    [Fact]
    public void Adapter2AccessibilityRecipe_DocumentsBoundedValidationHandoff()
    {
        var recipe = ReadRepoFile("docs/en/adapter-2-accessibility-recipe.md");
        var recipeZh = ReadRepoFile("docs/zh-CN/adapter-2-accessibility-recipe.md");

        Assert.Contains("ConsumerSample.Avalonia -- --proof", recipe, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.HelloWorld.Wpf", recipe, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK:True", recipe, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_BASELINE_OK:True", recipe, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_OK:True", recipe, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PERFORMANCE_ACCESSIBILITY_HANDOFF_OK:True", recipe, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_RECIPE_ALIGNMENT_OK:True", recipe, StringComparison.Ordinal);
        Assert.True(HasLineWith(recipe, "WPF", "validation-only"));
        Assert.True(HasLineWith(recipe, "Avalonia", "defended"));

        Assert.Contains("ConsumerSample.Avalonia -- --proof", recipeZh, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.HelloWorld.Wpf", recipeZh, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK:True", recipeZh, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_BASELINE_OK:True", recipeZh, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_OK:True", recipeZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PERFORMANCE_ACCESSIBILITY_HANDOFF_OK:True", recipeZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_RECIPE_ALIGNMENT_OK:True", recipeZh, StringComparison.Ordinal);
        Assert.True(HasLineWith(recipeZh, "WPF", "验证"));
        Assert.True(HasLineWith(recipeZh, "Avalonia", "受防守"));
    }

    [Fact]
    public void Adapter2PerformanceRecipe_DocumentsBoundedValidationHandoff()
    {
        var recipe = ReadRepoFile("docs/en/adapter-2-performance-recipe.md");
        var recipeZh = ReadRepoFile("docs/zh-CN/adapter-2-performance-recipe.md");

        Assert.Contains("ConsumerSample.Avalonia -- --proof", recipe, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.HelloWorld.Wpf", recipe, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PERFORMANCE_BASELINE_OK:True", recipe, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_EXPORT_BREADTH_OK:True", recipe, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PROJECTION_BUDGET_OK:True:none", recipe, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_COMMAND_BUDGET_OK:True:none", recipe, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_SCENE_BUDGET_OK:True:none", recipe, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PROOF_BUDGET_OK:True", recipe, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PERFORMANCE_ACCESSIBILITY_HANDOFF_OK:True", recipe, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_RECIPE_ALIGNMENT_OK:True", recipe, StringComparison.Ordinal);
        Assert.True(HasLineWith(recipe, "WPF", "validation-only"));
        Assert.True(HasLineWith(recipe, "Avalonia", "defended"));

        Assert.Contains("ConsumerSample.Avalonia -- --proof", recipeZh, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.HelloWorld.Wpf", recipeZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PERFORMANCE_BASELINE_OK:True", recipeZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_EXPORT_BREADTH_OK:True", recipeZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PROJECTION_BUDGET_OK:True:none", recipeZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_COMMAND_BUDGET_OK:True:none", recipeZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_SCENE_BUDGET_OK:True:none", recipeZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PROOF_BUDGET_OK:True", recipeZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PERFORMANCE_ACCESSIBILITY_HANDOFF_OK:True", recipeZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_RECIPE_ALIGNMENT_OK:True", recipeZh, StringComparison.Ordinal);
        Assert.True(HasLineWith(recipeZh, "WPF", "验证"));
        Assert.True(HasLineWith(recipeZh, "Avalonia", "受防守"));
    }

    [Fact]
    public void ProjectStatus_DocumentsExternalCapabilityReadinessGate()
    {
        var projectStatus = ReadRepoFile("docs/en/project-status.md");
        var projectStatusZh = ReadRepoFile("docs/zh-CN/project-status.md");

        Assert.Contains("External Capability Readiness Gate", projectStatus, StringComparison.Ordinal);
        Assert.Contains("Externally proven now", projectStatus, StringComparison.Ordinal);
        Assert.Contains("Validation-only or bounded claims", projectStatus, StringComparison.Ordinal);
        Assert.Contains("Deferred until more adopter evidence", projectStatus, StringComparison.Ordinal);
        Assert.Contains("route-level evidence", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("HOST_SAMPLE_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("DEMO_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("PACKAGE_SMOKE_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_BASELINE_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_FOCUS_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PERFORMANCE_BASELINE_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PROJECTION_BUDGET_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_COMMAND_BUDGET_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_SCENE_BUDGET_OK", projectStatus, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:baseline:True", projectStatus, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:large:True", projectStatus, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:stress:True", projectStatus, StringComparison.Ordinal);
        Assert.Contains("SCALE_EXPORT_BUDGET:stress:svg=informational:png<=120000:jpeg<=100000:reload<=800", projectStatus, StringComparison.Ordinal);
        Assert.Contains("SCALE_RASTER_EXPORT_STRESS_OK:True", projectStatus, StringComparison.Ordinal);
        Assert.Contains("SCALE_TIER_BUDGET:xlarge:nodes=10000:selection=512:moves=128:budget=informational-only", projectStatus, StringComparison.Ordinal);
        Assert.Contains("telemetry-only", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("stabilization-support-matrix.md", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adapter-capability-matrix.md", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-feedback.md", projectStatus, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("外部能力就绪闸门", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("当前已被外部证据证明", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("仅验证通过或受边界约束的声明", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("在更多采用者证据出现前继续延后", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("路线级证据", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("HOST_SAMPLE_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("DEMO_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("PACKAGE_SMOKE_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_BASELINE_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_FOCUS_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PERFORMANCE_BASELINE_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PROJECTION_BUDGET_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_COMMAND_BUDGET_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_SCENE_BUDGET_OK", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:baseline:True", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:large:True", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:stress:True", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_EXPORT_BUDGET:stress:svg=informational:png<=120000:jpeg<=100000:reload<=800", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_RASTER_EXPORT_STRESS_OK:True", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_TIER_BUDGET:xlarge:nodes=10000:selection=512:moves=128:budget=informational-only", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("telemetry-only", projectStatusZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("stabilization-support-matrix.md", projectStatusZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adapter-capability-matrix.md", projectStatusZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-feedback.md", projectStatusZh, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProjectStatus_AndAdoptionFeedback_ShareNextBetaLineCriteria()
    {
        var projectStatus = ReadRepoFile("docs/en/project-status.md");
        var projectStatusZh = ReadRepoFile("docs/zh-CN/project-status.md");
        var adoptionFeedback = ReadRepoFile("docs/en/adoption-feedback.md");
        var adoptionFeedbackZh = ReadRepoFile("docs/zh-CN/adoption-feedback.md");

        Assert.Contains("## External Capability Readiness Gate", projectStatus, StringComparison.Ordinal);
        Assert.Contains("Maintainer-seeded rehearsal evidence", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("3-5 gate", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("real external reports", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("same bounded risk", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Repairability, Help, and Review UX", projectStatus, StringComparison.Ordinal);
        Assert.Contains("v0.67 Repairability, Help, and Review UX proof is completed evidence", projectStatus, StringComparison.Ordinal);
        Assert.Contains("next recommended action: v0.68 release packaging/readiness", projectStatus, StringComparison.Ordinal);
        Assert.Contains("ADOPTION_RECOMMENDATION_CURRENT_OK:True", projectStatus, StringComparison.Ordinal);
        Assert.Contains("CLAIM_HYGIENE_BOUNDARY_OK:True", projectStatus, StringComparison.Ordinal);
        Assert.Contains("RELEASE_READINESS_GATE_OK:True", projectStatus, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BOUNDARY_GATE_OK:True", projectStatus, StringComparison.Ordinal);
        Assert.Contains("BETA_CLAIM_ALIGNMENT_OK:True", projectStatus, StringComparison.Ordinal);
        Assert.Contains("validation repair", projectStatus, StringComparison.Ordinal);
        Assert.Contains("contextual help", projectStatus, StringComparison.Ordinal);
        Assert.Contains("support boundary", projectStatus, StringComparison.Ordinal);
        Assert.Contains("release proof", projectStatus, StringComparison.Ordinal);
        Assert.Contains("Repairability, Help, and Review UX", adoptionFeedback, StringComparison.Ordinal);
        Assert.Contains("The completed v0.67 0.xx alpha/beta hardening line", adoptionFeedback, StringComparison.Ordinal);
        Assert.Contains("Next action is v0.68 release packaging/readiness", adoptionFeedback, StringComparison.Ordinal);
        Assert.Contains("ADOPTION_RECOMMENDATION_CURRENT_OK:True", adoptionFeedback, StringComparison.Ordinal);
        Assert.Contains("CLAIM_HYGIENE_BOUNDARY_OK:True", adoptionFeedback, StringComparison.Ordinal);
        Assert.Contains("RELEASE_READINESS_GATE_OK:True", adoptionFeedback, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BOUNDARY_GATE_OK:True", adoptionFeedback, StringComparison.Ordinal);
        Assert.Contains("BETA_CLAIM_ALIGNMENT_OK:True", adoptionFeedback, StringComparison.Ordinal);
        Assert.Contains("previous `Performance / Export Hardening` work and the v0.67 repair/help work are defended evidence", adoptionFeedback, StringComparison.Ordinal);
        Assert.Contains("## Seeded Trial Synthesis", adoptionFeedback, StringComparison.Ordinal);
        Assert.Contains("maintainer-seeded rehearsal evidence", adoptionFeedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("seeded rehearsals do not count", adoptionFeedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("3-5 gate", adoptionFeedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("real external reports", adoptionFeedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("same bounded risk", adoptionFeedback, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("## 外部能力就绪闸门", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("维护者种子预演证据", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("3 到 5 的门槛", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("真实外部报告", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("同一个受限风险", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("Repairability, Help, and Review UX", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("v0.67 Repairability, Help, and Review UX proof 已经是来自", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("下一步推荐动作：v0.68 release packaging/readiness", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("ADOPTION_RECOMMENDATION_CURRENT_OK:True", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("CLAIM_HYGIENE_BOUNDARY_OK:True", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("RELEASE_READINESS_GATE_OK:True", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BOUNDARY_GATE_OK:True", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("BETA_CLAIM_ALIGNMENT_OK:True", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("validation repair", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("contextual help", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("support boundary", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("release proof", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("Repairability, Help, and Review UX", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("已完成的 v0.67 `0.xx` alpha/beta hardening 线", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("下一步动作是 v0.68 release packaging/readiness", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("ADOPTION_RECOMMENDATION_CURRENT_OK:True", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("CLAIM_HYGIENE_BOUNDARY_OK:True", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("RELEASE_READINESS_GATE_OK:True", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BOUNDARY_GATE_OK:True", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("BETA_CLAIM_ALIGNMENT_OK:True", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("之前的 `Performance / Export Hardening` 和 v0.67 repair/help 工作都已经变成 defended evidence", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("## 当前种子试用综合", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("维护者种子预演证据", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("3 到 5 的门槛", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("真实外部报告", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("同一个受限风险", adoptionFeedbackZh, StringComparison.Ordinal);
    }

    [Fact]
    public void ProjectStatus_AndPublicLaunchChecklist_KeepDryRunRecordsInternalOnly()
    {
        var projectStatus = ReadRepoFile("docs/en/project-status.md");
        var projectStatusZh = ReadRepoFile("docs/zh-CN/project-status.md");
        var checklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var checklistZh = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");

        Assert.Contains("adoption-intake-dry-run.md", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("synthetic dry-run", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("internal rehearsal", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not external validation", projectStatus, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("adoption-intake-dry-run.md", projectStatusZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("合成 dry-run", projectStatusZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("内部预演", projectStatusZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("不是外部验证", projectStatusZh, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("adoption-intake-dry-run.md", checklist, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("synthetic dry-run", checklist, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("3-5 real external report gate", checklist, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("do not widen", checklist, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("do not count", checklist, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("adoption-intake-dry-run.md", checklistZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("合成 dry-run", checklistZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("3 到 5 条真实外部报告", checklistZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("不要扩大", checklistZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("不要计入", checklistZh, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PublicEvidenceLoopDocs_CrossLinkStatusChecklistFeedbackBundleAndDryRun()
    {
        var projectStatus = ReadRepoFile("docs/en/project-status.md");
        var projectStatusZh = ReadRepoFile("docs/zh-CN/project-status.md");
        var checklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var checklistZh = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var adoptionFeedback = ReadRepoFile("docs/en/adoption-feedback.md");
        var adoptionFeedbackZh = ReadRepoFile("docs/zh-CN/adoption-feedback.md");
        var supportBundle = ReadRepoFile("docs/en/support-bundle.md");
        var supportBundleZh = ReadRepoFile("docs/zh-CN/support-bundle.md");
        var dryRun = ReadRepoFile("docs/en/adoption-intake-dry-run.md");
        var dryRunZh = ReadRepoFile("docs/zh-CN/adoption-intake-dry-run.md");

        Assert.Contains("public-launch-checklist.md", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-feedback.md", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("support-bundle.md", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-intake-dry-run.md", projectStatus, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("project-status.md", checklist, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-feedback.md", checklist, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("support-bundle.md", checklist, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-intake-dry-run.md", checklist, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("project-status.md", adoptionFeedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("public-launch-checklist.md", adoptionFeedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("support-bundle.md", adoptionFeedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-intake-dry-run.md", adoptionFeedback, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("project-status.md", supportBundle, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("public-launch-checklist.md", supportBundle, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-feedback.md", supportBundle, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-intake-dry-run.md", supportBundle, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("project-status.md", dryRun, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("public-launch-checklist.md", dryRun, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-feedback.md", dryRun, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("support-bundle.md", dryRun, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("public-launch-checklist.md", projectStatusZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-feedback.md", projectStatusZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("support-bundle.md", projectStatusZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-intake-dry-run.md", projectStatusZh, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("project-status.md", checklistZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-feedback.md", checklistZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("support-bundle.md", checklistZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-intake-dry-run.md", checklistZh, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("project-status.md", adoptionFeedbackZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("public-launch-checklist.md", adoptionFeedbackZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("support-bundle.md", adoptionFeedbackZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-intake-dry-run.md", adoptionFeedbackZh, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("project-status.md", supportBundleZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("public-launch-checklist.md", supportBundleZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-feedback.md", supportBundleZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-intake-dry-run.md", supportBundleZh, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("project-status.md", dryRunZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("public-launch-checklist.md", dryRunZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adoption-feedback.md", dryRunZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("support-bundle.md", dryRunZh, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AdopterTriageDocs_LinkSyntheticIntakeDryRunFixtureRecords()
    {
        var triage = ReadRepoFile("docs/en/adopter-triage.md");
        var triageZh = ReadRepoFile("docs/zh-CN/adopter-triage.md");
        var dryRun = ReadRepoFile("docs/en/adoption-intake-dry-run.md");
        var dryRunZh = ReadRepoFile("docs/zh-CN/adoption-intake-dry-run.md");

        Assert.Contains("adoption-intake-dry-run.md", triage, StringComparison.Ordinal);
        Assert.Contains("adoption-intake-dry-run.md", triageZh, StringComparison.Ordinal);
        Assert.Contains("synthetic", dryRun, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dry-run", dryRun, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rehearsal", dryRun, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("synthetic", dryRunZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dry-run", dryRunZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("演练", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK:False", dryRun, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_METADATA_PROJECTION_OK:False", dryRun, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PERSISTENCE_OK:False", dryRun, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK:False", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_METADATA_PROJECTION_OK:False", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PERSISTENCE_OK:False", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("Expected triage classification", dryRun, StringComparison.Ordinal);
        Assert.Contains("预期分流分类", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("parameter projection/write-path failure", dryRun, StringComparison.Ordinal);
        Assert.Contains("metadata projection failure", dryRun, StringComparison.Ordinal);
        Assert.Contains("support-bundle persistence failure", dryRun, StringComparison.Ordinal);
        Assert.Contains("parameter projection / 写入路径失败", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("metadata projection / 元数据投影失败", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("support-bundle persistence / 持久化失败", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("sample/session parameter projection or write-path investigation", dryRun, StringComparison.Ordinal);
        Assert.Contains("definition/inspector metadata projection investigation", dryRun, StringComparison.Ordinal);
        Assert.Contains("persistence/path/environment investigation", dryRun, StringComparison.Ordinal);
        Assert.Contains("进行 sample/session 参数投影或写入路径调查", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("进行定义/inspector 元数据投影调查", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("进行持久化/路径/环境调查", dryRunZh, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(dryRun, "route", "version", "proof markers", "friction", "support-bundle attachment note"));
        Assert.True(HasLineWithAll(dryRunZh, "route", "version", "proof 标记", "摩擦", "support bundle 附件备注"));
        Assert.Contains("claim-expansion status", dryRun, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("claim-expansion status", dryRunZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ADOPTION_INTAKE_EVIDENCE_OK:True", dryRun, StringComparison.Ordinal);
        Assert.Contains("ADOPTION_INTAKE_EVIDENCE_OK:True", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("parameterSnapshots", dryRun, StringComparison.Ordinal);
        Assert.Contains("parameterSnapshots", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("status", dryRun, StringComparison.Ordinal);
        Assert.Contains("owner", dryRun, StringComparison.Ordinal);
        Assert.Contains("priority", dryRun, StringComparison.Ordinal);
        Assert.Contains("status", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("owner", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("priority", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("NO_SUPPORT_BUNDLE:route-cannot-produce-one", dryRun, StringComparison.Ordinal);
        Assert.Contains("NO_SUPPORT_BUNDLE:route-cannot-produce-one", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("synthetic", dryRun, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dry-run", dryRun, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rehearsal", dryRun, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not external validation", dryRun, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("synthetic", dryRunZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dry-run", dryRunZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("演练", dryRunZh, StringComparison.Ordinal);
        Assert.Contains("不是外部验证", dryRunZh, StringComparison.Ordinal);
    }

    [Fact]
    public void IssueTemplates_AndTriageDocs_ShareOneBetaEvidenceContract()
    {
        var packageVersion = GetPackageVersion();
        var publicTag = $"v{packageVersion}";
        var adoptionTemplate = ReadRepoFile(".github/ISSUE_TEMPLATE/adoption_feedback.yml");
        var bugTemplate = ReadRepoFile(".github/ISSUE_TEMPLATE/bug_report.md");
        var triageDoc = ReadRepoFile("docs/en/adopter-triage.md");
        var triageDocZh = ReadRepoFile("docs/zh-CN/adopter-triage.md");
        var checklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var checklistZh = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var adoptionFeedback = ReadRepoFile("docs/en/adoption-feedback.md");
        var adoptionFeedbackZh = ReadRepoFile("docs/zh-CN/adoption-feedback.md");
        var supportBundle = ReadRepoFile("docs/en/support-bundle.md");
        var supportBundleZh = ReadRepoFile("docs/zh-CN/support-bundle.md");
        var versionBlock = ExtractIssueTemplateBlock(adoptionTemplate, "version");
        var routeBlock = ExtractIssueTemplateBlock(adoptionTemplate, "route");
        var proofMarkersBlock = ExtractIssueTemplateBlock(adoptionTemplate, "proof_markers");
        var supportBundleBlock = ExtractIssueTemplateBlock(adoptionTemplate, "support_bundle");
        var routeOptions = ExtractDropdownOptions(adoptionTemplate, "route");

        Assert.Contains("id: version", adoptionTemplate, StringComparison.Ordinal);
        Assert.Contains($"placeholder: {packageVersion} / {publicTag}", versionBlock, StringComparison.Ordinal);
        Assert.DoesNotContain("alpha", versionBlock, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id: route", adoptionTemplate, StringComparison.Ordinal);
        Assert.Contains("id: proof_markers", adoptionTemplate, StringComparison.Ordinal);
        Assert.Contains("id: friction", adoptionTemplate, StringComparison.Ordinal);
        Assert.Contains("id: support_bundle", adoptionTemplate, StringComparison.Ordinal);
        Assert.Contains("Proof markers", adoptionTemplate, StringComparison.Ordinal);
        Assert.Contains("Support-bundle attachment note", adoptionTemplate, StringComparison.Ordinal);
        Assert.Contains("failed", adoptionTemplate, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("failed", proofMarkersBlock, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK", proofMarkersBlock, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_METADATA_PROJECTION_OK", proofMarkersBlock, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PERSISTENCE_OK", proofMarkersBlock, StringComparison.Ordinal);
        Assert.Contains("parameterSnapshots", supportBundleBlock, StringComparison.Ordinal);
        Assert.Contains("status", supportBundleBlock, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("owner", supportBundleBlock, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("priority", supportBundleBlock, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(
            new[]
            {
                "HelloWorld",
                "AsterGraph.Starter.Avalonia",
                "HelloWorld.Avalonia",
                "ConsumerSample.Avalonia",
                "HostSample",
                "PackageSmoke",
                "ScaleSmoke",
                "Demo",
            },
            routeOptions);
        Assert.Contains("options:", routeBlock, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH", supportBundleBlock, StringComparison.Ordinal);
        Assert.Contains("NO_SUPPORT_BUNDLE", supportBundleBlock, StringComparison.Ordinal);
        Assert.DoesNotContain("consumer-support-bundle.json", supportBundleBlock, StringComparison.Ordinal);
        Assert.DoesNotContain("id: persona", adoptionTemplate, StringComparison.Ordinal);
        Assert.DoesNotContain("id: worked", adoptionTemplate, StringComparison.Ordinal);
        Assert.DoesNotContain("id: request", adoptionTemplate, StringComparison.Ordinal);
        Assert.DoesNotContain("id: evidence", adoptionTemplate, StringComparison.Ordinal);
        Assert.DoesNotContain("required: true", supportBundleBlock, StringComparison.Ordinal);

        Assert.Contains("AsterGraph version", bugTemplate, StringComparison.Ordinal);
        Assert.Contains("Route or artifact tried", bugTemplate, StringComparison.Ordinal);
        Assert.Contains("Proof markers", bugTemplate, StringComparison.Ordinal);
        Assert.Contains("Support-bundle attachment note", bugTemplate, StringComparison.Ordinal);

        Assert.Contains("route", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("version", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("proof markers", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("friction", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("support-bundle attachment note", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("parameterSnapshots", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CONSUMER_SAMPLE_METADATA_PROJECTION_OK", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SUPPORT_BUNDLE_PERSISTENCE_OK", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("parameter projection", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("metadata projection", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("persistence", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("status", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("owner", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("priority", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("route", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("version", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("proof 标记", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("摩擦", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("support bundle 附件备注", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("parameterSnapshots", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CONSUMER_SAMPLE_METADATA_PROJECTION_OK", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SUPPORT_BUNDLE_PERSISTENCE_OK", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("parameter projection", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("metadata projection", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("persistence", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("status", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("owner", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("priority", triageDocZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("friction", triageDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("摩擦", triageDocZh, StringComparison.Ordinal);

        foreach (var contents in new[] { adoptionFeedback, adoptionFeedbackZh })
        {
            Assert.True(HasLineWithAll(contents, "HelloWorld", "AsterGraph.Starter.Avalonia", "HelloWorld.Avalonia", "ConsumerSample.Avalonia", "HostSample", "PackageSmoke", "ScaleSmoke", "Demo"));
        }

        Assert.True(HasLineWithAll(supportBundle, "route", "version", "proof markers", "friction", "no support bundle"));
        Assert.True(HasLineWithAll(supportBundleZh, "route", "version", "proof 标记", "摩擦", "不可用"));

        Assert.True(HasLineWithAll(checklist, "route", "version", "proof markers", "friction", "support-bundle attachment note"));
        Assert.True(HasLineWithAll(checklistZh, "route", "version", "proof 标记", "摩擦", "support bundle 附件备注"));
        Assert.Contains("support bundle", checklist, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adopter triage", checklist, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("support bundle", checklistZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adopter-triage", checklistZh, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AdoptionIntakeDocs_DefineRealReportGateAndClaimExpansionStatus()
    {
        var adoptionTemplate = ReadRepoFile(".github/ISSUE_TEMPLATE/adoption_feedback.yml");
        var bugTemplate = ReadRepoFile(".github/ISSUE_TEMPLATE/bug_report.md");
        var adoptionFeedback = ReadRepoFile("docs/en/adoption-feedback.md");
        var adoptionFeedbackZh = ReadRepoFile("docs/zh-CN/adoption-feedback.md");
        var triageDoc = ReadRepoFile("docs/en/adopter-triage.md");
        var triageDocZh = ReadRepoFile("docs/zh-CN/adopter-triage.md");
        var projectStatus = ReadRepoFile("docs/en/project-status.md");
        var projectStatusZh = ReadRepoFile("docs/zh-CN/project-status.md");
        var checklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var checklistZh = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var reportKindOptions = ExtractDropdownOptions(adoptionTemplate, "report_kind");
        var claimExpansionOptions = ExtractDropdownOptions(adoptionTemplate, "claim_expansion_status");

        Assert.Equal(
            new[]
            {
                "Real external adoption report",
                "Maintainer-seeded rehearsal / synthetic dry-run",
            },
            reportKindOptions);
        Assert.Equal(
            new[]
            {
                "No support/capability expansion requested",
                "Candidate support/capability expansion",
                "Unsure / needs maintainer triage",
            },
            claimExpansionOptions);
        Assert.Contains("id: adopter_context", adoptionTemplate, StringComparison.Ordinal);
        Assert.Contains("id: claim_expansion_status", adoptionTemplate, StringComparison.Ordinal);
        Assert.Contains("A real external report must be filed by someone evaluating or embedding AsterGraph outside maintainer rehearsal.", adoptionTemplate, StringComparison.Ordinal);
        Assert.Contains("single report does not widen public claims", adoptionTemplate, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Report type", bugTemplate, StringComparison.Ordinal);
        Assert.Contains("Adopter context", bugTemplate, StringComparison.Ordinal);
        Assert.Contains("Claim-expansion status", bugTemplate, StringComparison.Ordinal);

        foreach (var contents in new[] { adoptionFeedback, triageDoc, projectStatus, checklist })
        {
            Assert.Contains("report type", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("adopter context", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("claim-expansion status", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("single report does not widen public claims", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("3-5 real external reports", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("ADOPTION_INTAKE_EVIDENCE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("SUPPORT_BUNDLE_INTAKE_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("REAL_EXTERNAL_REPORT_GATE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTER_INTAKE_REFRESH_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTER_SUPPORT_BUNDLE_ATTACHMENT_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTER_CLAIM_EXPANSION_GATE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTION_API_STABILIZATION_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTION_API_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("V061_MILESTONE_PROOF_OK:True", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { adoptionFeedbackZh, triageDocZh, projectStatusZh, checklistZh })
        {
            Assert.Contains("报告类型", contents, StringComparison.Ordinal);
            Assert.Contains("采用者上下文", contents, StringComparison.Ordinal);
            Assert.Contains("claim-expansion status", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("单条报告不会扩大公开声明", contents, StringComparison.Ordinal);
            Assert.Contains("3 到 5 条真实外部报告", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTION_INTAKE_EVIDENCE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("SUPPORT_BUNDLE_INTAKE_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("REAL_EXTERNAL_REPORT_GATE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTER_INTAKE_REFRESH_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTER_SUPPORT_BUNDLE_ATTACHMENT_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTER_CLAIM_EXPANSION_GATE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTION_API_STABILIZATION_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTION_API_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("V061_MILESTONE_PROOF_OK:True", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void HostIntegrationDocs_RequireCanonicalRouteThenAdapterForWpf()
    {
        var hostIntegration = ReadRepoFile("docs/en/host-integration.md");
        var hostIntegrationZh = ReadRepoFile("docs/zh-CN/host-integration.md");

        Assert.Contains("canonical", hostIntegration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adapter", hostIntegration, StringComparison.OrdinalIgnoreCase);
        Assert.True(HasLineWith(hostIntegration, "WPF", "partial"), "WPF Partial guidance must appear in host integration docs.");
        Assert.True(HasLineWith(hostIntegration, "WPF", "fallback"), "WPF Fallback guidance must appear in host integration docs.");
        Assert.True(HasLineWith(hostIntegration, "WPF", "host-owned"), "WPF flow must reference host-owned projection.");
        Assert.False(
            HasLineWith(hostIntegration, "WPF", "retained")
            && HasLineWith(hostIntegration, "WPF", "MVVM"),
            "WPF Partial/Fallback guidance must avoid retained-MVVM claim.");
        Assert.Contains("session", hostIntegration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("runtime", hostIntegration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("projection", hostIntegration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("canonical route", hostIntegration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adapter-specific", hostIntegration, StringComparison.OrdinalIgnoreCase);
        Assert.True(HasLineWithAll(hostIntegration, "new adopters", "default", "AsterGraphAvaloniaViewFactory"));
        Assert.True(HasLineWithAll(hostIntegration, "WPF", "adapter-2", "not a separate onboarding path", "parity promise"));

        Assert.True(HasLineWith(hostIntegrationZh, "WPF", "partial"), "WPF Partial guidance must appear in host integration docs.");
        Assert.True(HasLineWith(hostIntegrationZh, "WPF", "fallback"), "WPF Fallback guidance must appear in host integration docs.");
        Assert.True(HasLineWith(hostIntegrationZh, "WPF", "host-owned"), "WPF flow must reference host-owned projection.");
        Assert.False(
            HasLineWith(hostIntegrationZh, "WPF", "retained")
            && HasLineWith(hostIntegrationZh, "WPF", "MVVM"),
            "WPF Partial/Fallback guidance must avoid retained-MVVM claim.");
        Assert.Contains("默认 Avalonia UI", hostIntegrationZh, StringComparison.Ordinal);
        Assert.Contains("迁移期", hostIntegrationZh, StringComparison.Ordinal);
        Assert.Contains("第二适配器", hostIntegrationZh, StringComparison.Ordinal);
        Assert.Contains("投影", hostIntegrationZh, StringComparison.Ordinal);
        Assert.Contains("宿主", hostIntegrationZh, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(hostIntegrationZh, "WPF", "validation-only", "不会变成单独上手路径", "parity"));
    }

    [Fact]
    public void RepositorySurface_UsesAsterGraphSolutionName()
    {
        var repoRoot = GetRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(repoRoot, "AsterGraph.sln")));
        Assert.False(File.Exists(Path.Combine(repoRoot, "avalonia-node-map.sln")));
        Assert.Contains("AsterGraph.sln", ReadRepoFile("CONTRIBUTING.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void CiScript_CapturesDemoProofArtifactForProofRing()
    {
        var script = ReadRepoFile("eng/ci.ps1");

        Assert.Contains("demo-proof.txt", script, StringComparison.Ordinal);
        Assert.Contains("Invoke-DemoProof", script, StringComparison.Ordinal);
        foreach (var requiredProofLine in DemoProofContract.CreatePublicSuccessMarkerLines())
        {
            Assert.Contains(requiredProofLine, script, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ReleaseWorkflowSummaries_SurfaceDemoProofMarkers()
    {
        var ciWorkflow = ReadRepoFile(".github/workflows/ci.yml");
        var releaseWorkflow = ReadRepoFile(".github/workflows/release.yml");

        Assert.Contains("artifacts/proof/demo-proof.txt", ciWorkflow, StringComparison.Ordinal);
        Assert.Contains("DEMO_OK", ciWorkflow, StringComparison.Ordinal);
        foreach (var markerId in DemoProofContract.PublicSuccessMarkerIds)
        {
            Assert.Contains(markerId, ciWorkflow, StringComparison.Ordinal);
        }
        foreach (var markerId in RepairHelpReviewProofMarkerIds)
        {
            Assert.Contains(markerId, ciWorkflow, StringComparison.Ordinal);
        }

        Assert.Contains("artifacts/proof/demo-proof.txt", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("DEMO_OK", releaseWorkflow, StringComparison.Ordinal);
        foreach (var markerId in DemoProofContract.PublicSuccessMarkerIds)
        {
            Assert.Contains(markerId, releaseWorkflow, StringComparison.Ordinal);
        }
        foreach (var markerId in RepairHelpReviewProofMarkerIds)
        {
            Assert.Contains(markerId, releaseWorkflow, StringComparison.Ordinal);
        }

        foreach (var markerLine in AdapterMatrixProofMarkerLines)
        {
            Assert.Contains(markerLine, ciWorkflow, StringComparison.Ordinal);
            Assert.Contains(markerLine, releaseWorkflow, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void CiWorkflow_IncludesMacOsValidationOnCanonicalSolutionPath()
    {
        var ciWorkflow = ReadRepoFile(".github/workflows/ci.yml");

        Assert.Contains("macos-validation", ciWorkflow, StringComparison.Ordinal);
        Assert.Contains("runs-on: macos-latest", ciWorkflow, StringComparison.Ordinal);
        Assert.Contains("./eng/ci.ps1 -Lane all -Framework all -Configuration Release", ciWorkflow, StringComparison.Ordinal);
        Assert.Contains("- macos-validation", ciWorkflow, StringComparison.Ordinal);
    }

    [Fact]
    public void ArchitectureDocs_DescribeKernelSceneAdapterSplitAndStabilityLevels()
    {
        var architectureDoc = ReadRepoFile("docs/en/architecture.md");
        var architectureDocZh = ReadRepoFile("docs/zh-CN/architecture.md");
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");

        foreach (var contents in new[] { architectureDoc, architectureDocZh })
        {
            Assert.Contains("Editor Kernel", contents, StringComparison.Ordinal);
            Assert.Contains("Scene/Interaction", contents, StringComparison.Ordinal);
            Assert.Contains("UI Adapter", contents, StringComparison.Ordinal);
            Assert.Contains("CreateSession(...)", contents, StringComparison.Ordinal);
            Assert.Contains("IGraphEditorSession", contents, StringComparison.Ordinal);
            Assert.Contains("stability", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("architecture", quickStart, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("architecture", quickStartZh, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PublicDocs_DescribeOfficialCapabilityModulesAcrossRoutes()
    {
        var readme = ReadRepoFile("README.md");
        var readmeZh = ReadRepoFile("README.zh-CN.md");
        var hostIntegration = ReadRepoFile("docs/en/host-integration.md");
        var hostIntegrationZh = ReadRepoFile("docs/zh-CN/host-integration.md");

        foreach (var contents in new[] { readme, readmeZh, hostIntegration, hostIntegrationZh })
        {
            foreach (var moduleName in OfficialCapabilityModules)
            {
                Assert.Contains(moduleName, contents, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void FeatureCatalogDocs_DefineGovernedCapabilityManifestAndEntryLinks()
    {
        var featureCatalog = ReadRepoFile("docs/en/feature-catalog.md");
        var featureCatalogZh = ReadRepoFile("docs/zh-CN/feature-catalog.md");
        var readme = ReadRepoFile("README.md");
        var readmeZh = ReadRepoFile("README.zh-CN.md");
        var hostIntegration = ReadRepoFile("docs/en/host-integration.md");
        var hostIntegrationZh = ReadRepoFile("docs/zh-CN/host-integration.md");
        var architecture = ReadRepoFile("docs/en/architecture.md");
        var architectureZh = ReadRepoFile("docs/zh-CN/architecture.md");
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");

        foreach (var contents in new[] { featureCatalog, featureCatalogZh })
        {
            Assert.Contains("FEATURE_CATALOG_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("FEATURE_MANIFEST_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("FEATURE_PACK_GOVERNANCE_OK:True", contents, StringComparison.Ordinal);

            foreach (var fieldName in new[]
            {
                "FeatureId",
                "Pack",
                "Status",
                "Public seam",
                "Avalonia projection",
                "WPF projection",
                "Sample / Demo entry",
                "Proof marker",
                "Perf budget",
                "Docs",
            })
            {
                Assert.Contains(fieldName, contents, StringComparison.Ordinal);
            }

            foreach (var packName in new[] { "Core", "Authoring", "Workbench", "Advanced Graph", "Diagnostics" })
            {
                Assert.Contains(packName, contents, StringComparison.Ordinal);
            }

            foreach (var featureId in new[]
            {
                "core.selection",
                "workbench.stencil.basic",
                "workbench.export.scene",
                "authoring.node-surface",
                "diagnostics.support-bundle",
                "adapter2.performance-accessibility-handoff",
                "adapter2.validation-handoff",
            })
            {
                Assert.Contains(featureId, contents, StringComparison.Ordinal);
            }

            Assert.True(HasLineWith(contents, "WPF projection", "validation-only"));
            Assert.True(HasLineWith(contents, "WPF", "parity"));
            Assert.True(HasLineWith(contents, "marketplace", "sandbox"));
            Assert.True(HasLineWith(contents, "execution engine", "GA"));
        }

        Assert.Contains("does not create a new runtime route", featureCatalog, StringComparison.Ordinal);
        Assert.Contains("不创建新的 runtime route", featureCatalogZh, StringComparison.Ordinal);
        Assert.Contains("Feature Catalog", readme, StringComparison.Ordinal);
        Assert.Contains("docs/en/feature-catalog.md", readme, StringComparison.Ordinal);
        Assert.Contains("Feature Catalog", readmeZh, StringComparison.Ordinal);
        Assert.Contains("docs/zh-CN/feature-catalog.md", readmeZh, StringComparison.Ordinal);

        foreach (var contents in new[] { hostIntegration, hostIntegrationZh, architecture, architectureZh, quickStart, quickStartZh })
        {
            Assert.Contains("feature-catalog.md", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void AdvancedEditingDocs_DefineCanonicalModulesAndProofCoverage()
    {
        var readme = ReadRepoFile("README.md");
        var readmeZh = ReadRepoFile("README.zh-CN.md");
        var editorReadme = ReadRepoFile("src/AsterGraph.Editor/README.md");
        var avaloniaReadme = ReadRepoFile("src/AsterGraph.Avalonia/README.md");
        var advancedEditing = ReadRepoFile("docs/en/advanced-editing.md");
        var advancedEditingZh = ReadRepoFile("docs/zh-CN/advanced-editing.md");
        var demoGuide = ReadRepoFile("docs/en/demo-guide.md");
        var demoGuideZh = ReadRepoFile("docs/zh-CN/demo-guide.md");

        foreach (var contents in new[] { readme, readmeZh, advancedEditing, advancedEditingZh })
        {
            foreach (var moduleName in AdvancedEditingCapabilityModules)
            {
                Assert.Contains(moduleName, contents, StringComparison.Ordinal);
            }
        }

        foreach (var contents in new[] { editorReadme, avaloniaReadme })
        {
            Assert.Contains("advanced-editing", contents, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var contents in new[] { demoGuide, demoGuideZh })
        {
            Assert.Contains("HIERARCHY_SEMANTICS_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("EDGE_GEOMETRY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("Node Surface Authoring", contents, StringComparison.Ordinal);
            Assert.Contains("Hierarchy Semantics", contents, StringComparison.Ordinal);
            Assert.Contains("Edge Geometry Tooling", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void QuickStartAndArchitecture_MapCapabilityModulesToProofLanes()
    {
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");
        var architecture = ReadRepoFile("docs/en/architecture.md");
        var architectureZh = ReadRepoFile("docs/zh-CN/architecture.md");

        foreach (var contents in new[] { quickStart, quickStartZh })
        {
            Assert.Contains("HostSample", contents, StringComparison.Ordinal);
            Assert.Contains("PackageSmoke", contents, StringComparison.Ordinal);
            Assert.Contains("ScaleSmoke", contents, StringComparison.Ordinal);
            Assert.Contains("Demo", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { architecture, architectureZh })
        {
            Assert.Contains("capability modules", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Host Integration", contents, StringComparison.Ordinal);
            Assert.Contains("Quick Start", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ScaleDocs_DistinguishDefendedBudgetsFromStressTelemetry()
    {
        var scaleBaseline = ReadRepoFile("docs/en/scale-baseline.md");
        var scaleBaselineZh = ReadRepoFile("docs/zh-CN/scale-baseline.md");
        var checklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var checklistZh = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");

        foreach (var contents in new[] { scaleBaseline, scaleBaselineZh })
        {
            Assert.Contains("baseline", contents, StringComparison.Ordinal);
            Assert.Contains("large", contents, StringComparison.Ordinal);
            Assert.Contains("stress", contents, StringComparison.Ordinal);
            Assert.Contains("xlarge", contents, StringComparison.Ordinal);
            Assert.Contains("SCALE_TIER_BUDGET:xlarge:nodes=10000:selection=512:moves=128:budget=informational-only", contents, StringComparison.Ordinal);
            Assert.Contains("SCALE_AUTHORING_BUDGET:xlarge:budget=informational-only", contents, StringComparison.Ordinal);
            Assert.Contains("SCALE_EXPORT_BUDGET:xlarge:budget=informational-only", contents, StringComparison.Ordinal);
            Assert.Contains("SCALE_PERF_SUMMARY", contents, StringComparison.Ordinal);
            Assert.Contains("SCALE_AUTHORING_BUDGET_OK", contents, StringComparison.Ordinal);
            Assert.Contains("SCALE_EXPORT_BUDGET_OK", contents, StringComparison.Ordinal);
            Assert.Contains("SCALE_RASTER_EXPORT_STRESS_OK", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("10000-node support", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("blanket virtualization", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:large:True:...", checklist, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:stress:True:...", checklist, StringComparison.Ordinal);
        Assert.Contains("SCALE_EXPORT_BUDGET:stress:svg=informational:png<=120000:jpeg<=100000:reload<=800", checklist, StringComparison.Ordinal);
        Assert.Contains("SCALE_TIER_BUDGET:xlarge:nodes=10000:selection=512:moves=128:budget=informational-only", checklist, StringComparison.Ordinal);
        Assert.Contains("SCALE_RASTER_EXPORT_STRESS_OK:True", checklist, StringComparison.Ordinal);
        Assert.Contains("EXPORT_PROGRESS_OK:True", checklist, StringComparison.Ordinal);
        Assert.Contains("EXPORT_CANCEL_OK:True", checklist, StringComparison.Ordinal);
        Assert.Contains("EXPORT_SCOPE_OK:True", checklist, StringComparison.Ordinal);
        Assert.Contains("EXPORT_SELECTION_SCOPE_OK:True", checklist, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERF_SUMMARY:stress:...", checklist, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:large:True:...", checklistZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:stress:True:...", checklistZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_EXPORT_BUDGET:stress:svg=informational:png<=120000:jpeg<=100000:reload<=800", checklistZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_TIER_BUDGET:xlarge:nodes=10000:selection=512:moves=128:budget=informational-only", checklistZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_RASTER_EXPORT_STRESS_OK:True", checklistZh, StringComparison.Ordinal);
        Assert.Contains("EXPORT_PROGRESS_OK:True", checklistZh, StringComparison.Ordinal);
        Assert.Contains("EXPORT_CANCEL_OK:True", checklistZh, StringComparison.Ordinal);
        Assert.Contains("EXPORT_SCOPE_OK:True", checklistZh, StringComparison.Ordinal);
        Assert.Contains("EXPORT_SELECTION_SCOPE_OK:True", checklistZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERF_SUMMARY:stress:...", checklistZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX_FORMAT:1", checklist, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS", checklist, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS", checklist, StringComparison.Ordinal);
        Assert.DoesNotContain("ADAPTER_CAPABILITY_MATRIX:True", checklist, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK", checklist, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_EXPORT_BREADTH_OK:True", checklist, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(checklist, "AsterGraph.Starter.Wpf", "validation-only", "not onboarding"));
        Assert.True(HasLineWithAll(checklist, "AsterGraph.HelloWorld.Wpf", "validation-only", "not parity"));
        Assert.True(HasLineWith(checklist, "HELLOWORLD_WPF_OK", "adapter-2"));
        Assert.True(HasLineWith(checklist, "HELLOWORLD_WPF_OK", "parity"));
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX_FORMAT:1", checklistZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS", checklistZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS", checklistZh, StringComparison.Ordinal);
        Assert.DoesNotContain("ADAPTER_CAPABILITY_MATRIX:True", checklistZh, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK", checklistZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_EXPORT_BREADTH_OK:True", checklistZh, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(checklistZh, "AsterGraph.Starter.Wpf", "validation-only", "不是上手入口"));
        Assert.True(HasLineWithAll(checklistZh, "AsterGraph.HelloWorld.Wpf", "validation-only", "不代表 parity"));
        Assert.True(HasLineWith(checklistZh, "HELLOWORLD_WPF_OK", "adapter-2"));
        Assert.True(HasLineWith(checklistZh, "HELLOWORLD_WPF_OK", "parity"));
    }

    [Fact]
    public void ReleaseAndStatusDocs_KeepVersionGuidanceAlignedAndGeneric()
    {
        var packageVersion = GetPackageVersion();
        var publicTag = $"v{packageVersion}";
        var readme = ReadRepoFile("README.md");
        var readmeZh = ReadRepoFile("README.zh-CN.md");
        var versioning = ReadRepoFile("docs/en/versioning.md");
        var versioningZh = ReadRepoFile("docs/zh-CN/versioning.md");
        var projectStatus = ReadRepoFile("docs/en/project-status.md");
        var projectStatusZh = ReadRepoFile("docs/zh-CN/project-status.md");
        var alphaStatus = ReadRepoFile("docs/en/alpha-status.md");
        var alphaStatusZh = ReadRepoFile("docs/zh-CN/alpha-status.md");
        var checklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var checklistZh = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");

        foreach (var contents in new[] { readme, readmeZh, versioning, versioningZh, projectStatus, projectStatusZh, alphaStatus, alphaStatusZh })
        {
            Assert.Contains(packageVersion, contents, StringComparison.Ordinal);
            Assert.Contains(publicTag, contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { versioning, projectStatus, alphaStatus })
        {
            Assert.DoesNotContain("v0.9.0-beta", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { versioningZh, projectStatusZh, alphaStatusZh })
        {
            Assert.DoesNotContain("v0.9.0-beta", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { checklist, checklistZh })
        {
            Assert.DoesNotContain(packageVersion, contents, StringComparison.Ordinal);
            Assert.DoesNotContain(publicTag, contents, StringComparison.Ordinal);
        }

        Assert.Contains("package version", checklist, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("public prerelease tag", checklist, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("exact tag-to-package-version match", checklist, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("包版本", checklistZh, StringComparison.Ordinal);
        Assert.Contains("公开 tag", checklistZh, StringComparison.Ordinal);
        Assert.Contains("完全一致", checklistZh, StringComparison.Ordinal);
    }

    [Fact]
    public void VersioningDocs_ClassifyLocalPlanningLabelsAsPrivateNotPublicTags()
    {
        var packageVersion = GetPackageVersion();
        var publicTag = $"v{packageVersion}";
        var versioning = ReadRepoFile("docs/en/versioning.md");
        var versioningZh = ReadRepoFile("docs/zh-CN/versioning.md");

        Assert.Contains($"package version: `{packageVersion}`", versioning, StringComparison.Ordinal);
        Assert.Contains($"public tag: `{publicTag}`", versioning, StringComparison.Ordinal);
        Assert.Contains("local planning-only milestone labels", versioning, StringComparison.Ordinal);
        Assert.Contains("not release identifiers", versioning, StringComparison.Ordinal);
        Assert.DoesNotContain("v0.28.0-beta", versioning, StringComparison.Ordinal);

        Assert.Contains($"包版本：`{packageVersion}`", versioningZh, StringComparison.Ordinal);
        Assert.Contains($"公开 tag：`{publicTag}`", versioningZh, StringComparison.Ordinal);
        Assert.Contains("本地规划专用里程碑标签", versioningZh, StringComparison.Ordinal);
        Assert.Contains("不是发布标识", versioningZh, StringComparison.Ordinal);
        Assert.DoesNotContain("v0.28.0-beta", versioningZh, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicReadmes_ShowPrebuiltScenarioInFirstView()
    {
        var readme = ReadRepoFile("README.md");
        var readmeZh = ReadRepoFile("README.zh-CN.md");
        var scenarioAsset = ReadRepoFile("docs/assets/astergraph-ai-pipeline-demo.svg");

        AssertAppearsBefore(readme, "![AsterGraph AI workflow scenario](./docs/assets/astergraph-ai-pipeline-demo.svg)", "## Start Here");
        AssertAppearsBefore(readme, "Launch the prebuilt AI workflow scenario", "## Public Beta");
        AssertAppearsBefore(readme, "dotnet run --project src/AsterGraph.Demo -- --scenario ai-pipeline", "## Public Beta");
        AssertAppearsBefore(readme, "The scenario shows the SDK as an embeddable authoring surface", "## Public Beta");
        AssertAppearsBefore(readme, "dotnet new astergraph-avalonia", "## Public Beta");
        AssertAppearsBefore(readme, "dotnet new astergraph-plugin", "## Public Beta");
        AssertAppearsBefore(readme, "tools/AsterGraph.PluginTool", "## Public Beta");
        AssertAppearsBefore(readme, "## Start Here", "## Public Beta");
        Assert.Contains("parameter editing", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trusted plugin context", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("automation", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("export", readme, StringComparison.OrdinalIgnoreCase);

        AssertAppearsBefore(readmeZh, "![AsterGraph AI workflow 场景](./docs/assets/astergraph-ai-pipeline-demo.svg)", "## 从哪里开始");
        AssertAppearsBefore(readmeZh, "启动预置 AI workflow 场景", "## 公开 Beta");
        AssertAppearsBefore(readmeZh, "dotnet run --project src/AsterGraph.Demo -- --scenario ai-pipeline", "## 公开 Beta");
        AssertAppearsBefore(readmeZh, "这个场景把 SDK 展示成一个可嵌入的 authoring surface", "## 公开 Beta");
        AssertAppearsBefore(readmeZh, "dotnet new astergraph-avalonia", "## 公开 Beta");
        AssertAppearsBefore(readmeZh, "dotnet new astergraph-plugin", "## 公开 Beta");
        AssertAppearsBefore(readmeZh, "tools/AsterGraph.PluginTool", "## 公开 Beta");
        AssertAppearsBefore(readmeZh, "## 从哪里开始", "## 公开 Beta");
        Assert.Contains("参数编辑", readmeZh, StringComparison.Ordinal);
        Assert.Contains("可信插件上下文", readmeZh, StringComparison.Ordinal);
        Assert.Contains("自动化", readmeZh, StringComparison.Ordinal);
        Assert.Contains("导出", readmeZh, StringComparison.Ordinal);

        Assert.Contains("<title id=\"title\">AsterGraph AI pipeline demo scenario</title>", scenarioAsset, StringComparison.Ordinal);
        Assert.Contains("Input", scenarioAsset, StringComparison.Ordinal);
        Assert.Contains("Prompt", scenarioAsset, StringComparison.Ordinal);
        Assert.Contains("Tool", scenarioAsset, StringComparison.Ordinal);
        Assert.Contains("LLM", scenarioAsset, StringComparison.Ordinal);
        Assert.Contains("Parser", scenarioAsset, StringComparison.Ordinal);
    }

    [Fact]
    public void DemoScenarioPresetDocs_SurfaceHostOwnedPresetProof()
    {
        var demoGuide = ReadRepoFile("docs/en/demo-guide.md");
        var demoGuideZh = ReadRepoFile("docs/zh-CN/demo-guide.md");
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");
        var projectStatus = ReadRepoFile("docs/en/project-status.md");
        var projectStatusZh = ReadRepoFile("docs/zh-CN/project-status.md");

        foreach (var contents in new[] { demoGuide, demoGuideZh, quickStart, quickStartZh, projectStatus, projectStatusZh })
        {
            Assert.Contains("DEMO_SCENARIO_PRESETS_OK:True", contents, StringComparison.Ordinal);
        }

        Assert.True(HasLineWithAll(demoGuide, "Scenario presets", "host-owned", "--scenario terrain-shader"));
        Assert.True(HasLineWithAll(demoGuide, "runtime marketplace", "preset API"));
        Assert.True(HasLineWithAll(demoGuideZh, "scenario preset", "宿主自管", "--scenario terrain-shader"));
        Assert.True(HasLineWithAll(demoGuideZh, "runtime marketplace", "preset API"));
    }

    [Fact]
    public void RuntimeFeedbackDocs_SurfaceProofMarkersWithoutSupportClaimExpansion()
    {
        var readme = ReadRepoFile("README.md");
        var readmeZh = ReadRepoFile("README.zh-CN.md");
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");
        var supportBundle = ReadRepoFile("docs/en/support-bundle.md");
        var supportBundleZh = ReadRepoFile("docs/zh-CN/support-bundle.md");
        var projectStatus = ReadRepoFile("docs/en/project-status.md");
        var projectStatusZh = ReadRepoFile("docs/zh-CN/project-status.md");
        var launchChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var launchChecklistZh = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");

        foreach (var contents in new[] { readme, readmeZh, quickStart, quickStartZh, supportBundle, supportBundleZh, projectStatus, projectStatusZh, launchChecklist, launchChecklistZh })
        {
            Assert.Contains("RUNTIME_DEBUG_PANEL_INTERACTION_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("RUNTIME_LOG_LOCATE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("RUNTIME_LOG_EXPORT_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("AI_PIPELINE_MOCK_RUNNER_POLISH_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("AI_PIPELINE_PAYLOAD_PREVIEW_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("AI_PIPELINE_ERROR_DEBUG_EVIDENCE_OK:True", contents, StringComparison.Ordinal);
        }

        Assert.True(HasLineWithAll(readme, "does not execute graphs", "workflow scripting UI"));
        Assert.True(HasLineWithAll(readme, "algorithm execution engine", "marketplace", "sandbox", "WPF parity", "GA"));
        Assert.True(HasLineWithAll(supportBundle, "host-owned", "not a workflow scripting UI", "marketplace", "sandbox", "WPF parity", "GA"));
        Assert.True(HasLineWithAll(launchChecklist, "does not imply", "algorithm execution engine", "workflow scripting UI", "plugin marketplace", "sandboxing", "WPF parity", "GA"));
        Assert.True(HasLineWithAll(projectStatus, "algorithm execution engine", "workflow scripting UI", "host-owned runtime feedback"));
    }

    private static string GetPackageVersion()
    {
        var props = XDocument.Load(Path.Combine(GetRepositoryRoot(), "Directory.Build.props"));
        var version = props.Root?
            .Elements("PropertyGroup")
            .Elements("Version")
            .Select(node => node.Value)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new InvalidOperationException("Could not read package version from Directory.Build.props.");
        }

        return version.Trim();
    }

    [Fact]
    public void AdapterContractDocs_AvoidPinningValidationStoriesToSpecificBetaTags()
    {
        var publicTag = $"v{GetPackageVersion()}";
        var readme = ReadRepoFile("README.md");
        var readmeZh = ReadRepoFile("README.zh-CN.md");
        var architecture = ReadRepoFile("docs/en/architecture.md");
        var architectureZh = ReadRepoFile("docs/zh-CN/architecture.md");
        var hostIntegration = ReadRepoFile("docs/en/host-integration.md");
        var hostIntegrationZh = ReadRepoFile("docs/zh-CN/host-integration.md");
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");

        foreach (var contents in new[] { architecture, architectureZh, hostIntegration, hostIntegrationZh, quickStart, quickStartZh })
        {
            Assert.False(HasLineWith(contents, "WPF", "v0.9.0-beta"));
            Assert.False(HasLineWith(contents, "WPF", publicTag));
        }

        Assert.False(HasLineWith(readme, "WPF", publicTag));
        Assert.False(HasLineWith(readmeZh, "WPF", publicTag));
        Assert.Contains("WPF", architecture, StringComparison.Ordinal);
        Assert.Contains("WPF", architectureZh, StringComparison.Ordinal);
        Assert.Contains("adapter 2", hostIntegration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adapter 2", hostIntegrationZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("validation", architecture, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("validation", architectureZh, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProductizedAdoptionDocs_DefendHostBuilderAndProofGate()
    {
        var readme = ReadRepoFile("README.md");
        var readmeZh = ReadRepoFile("README.zh-CN.md");
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");
        var hostIntegration = ReadRepoFile("docs/en/host-integration.md");
        var hostIntegrationZh = ReadRepoFile("docs/zh-CN/host-integration.md");
        var avaloniaReadme = ReadRepoFile("src/AsterGraph.Avalonia/README.md");

        foreach (var contents in new[] { readme, readmeZh, quickStart, quickStartZh, hostIntegration, hostIntegrationZh, avaloniaReadme })
        {
            Assert.Contains("AsterGraphHostBuilder", contents, StringComparison.Ordinal);
            Assert.Contains("BuildAvaloniaView", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { quickStart, quickStartZh, hostIntegration, hostIntegrationZh, avaloniaReadme })
        {
            Assert.Contains("Hosted Builder Cookbook", contents, StringComparison.Ordinal);
            Assert.Contains("UseDocument(document)", contents, StringComparison.Ordinal);
            Assert.Contains("UseCatalog(catalog)", contents, StringComparison.Ordinal);
            Assert.Contains("UseDefaultCompatibility()", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphAvaloniaViewFactory.Create", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { hostIntegration, hostIntegrationZh })
        {
            Assert.Contains("UseDefaultWorkbench()", contents, StringComparison.Ordinal);
            Assert.True(HasLineWithAll(contents, "UseDefaultWorkbench()", "toolbar", "command palette"));
            Assert.True(HasLineWithAll(contents, "UseDefaultWorkbench()", "stencil", "inspector"));
            Assert.True(HasLineWithAll(contents, "UseDefaultWorkbench()", "mini-map", "fragment"));
            Assert.True(HasLineWithAll(contents, "UseDefaultWorkbench()", "diagnostics", "status chrome"));
            Assert.True(HasLineWithAll(contents, "UseDefaultWorkbench()", "does not create", "runtime model")
                || HasLineWithAll(contents, "UseDefaultWorkbench()", "不会创建", "runtime model"));
        }

        Assert.Contains("CreateSession(...)", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("CreateSession(...)", hostIntegrationZh, StringComparison.Ordinal);
        Assert.Contains("AsterGraphEditorFactory.Create(...)", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("AsterGraphEditorFactory.Create(...)", hostIntegrationZh, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(quickStart, "AsterGraphHostBuilder", "thin hosted helper", "not a second runtime model"));
        Assert.True(HasLineWithAll(quickStartZh, "AsterGraphHostBuilder", "thin hosted helper", "不是第二套 runtime model"));
        Assert.True(HasLineWithAll(hostIntegration, "builder delegates", "editor/session", "Avalonia view factories"));
        Assert.True(HasLineWithAll(hostIntegrationZh, "builder", "editor/session", "Avalonia view factories"));
        Assert.True(HasLineWithAll(avaloniaReadme, "Both routes", "same editor/session owner"));
        Assert.True(HasLineWithAll(hostIntegration, "UseNodePresentationProvider", "AsterGraphEditorOptions.NodePresentationProvider", "AsterGraphPresentationOptions"));
        Assert.True(HasLineWithAll(hostIntegrationZh, "UseNodePresentationProvider", "AsterGraphEditorOptions.NodePresentationProvider", "AsterGraphPresentationOptions"));
        Assert.True(HasLineWithAll(quickStart, "UseRuntimeOverlayProvider", "UseLayoutProvider"));
        Assert.True(HasLineWithAll(quickStartZh, "UseRuntimeOverlayProvider", "UseLayoutProvider"));
        Assert.Contains("dotnet run --project src/AsterGraph.Demo -- --scenario ai-pipeline", readme, StringComparison.Ordinal);
        Assert.Contains("dotnet run --project src/AsterGraph.Demo -- --scenario ai-pipeline", readmeZh, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(readme, "30 seconds", "AI workflow", "scenario"));
        Assert.True(HasLineWithAll(readme, "5 minutes", "ConsumerSample.Avalonia", "--proof --support-bundle"));
        Assert.True(HasLineWithAll(readme, "30 minutes", "Quick Start", "Host Integration"));
        Assert.True(HasLineWithAll(readme, "Maintainer", "Public Launch Checklist", "Adapter Capability Matrix", "Beta Support Bundle"));
        Assert.True(HasLineWithAll(readmeZh, "30 秒", "AI workflow", "场景"));
        Assert.True(HasLineWithAll(readmeZh, "5 分钟", "ConsumerSample.Avalonia", "--proof --support-bundle"));
        Assert.True(HasLineWithAll(readmeZh, "30 分钟", "Quick Start", "Host Integration"));
        Assert.True(HasLineWithAll(readmeZh, "维护者", "Public Launch Checklist", "Adapter Capability Matrix", "Beta Support Bundle"));
        Assert.True(HasLineWithAll(quickStart, "30 seconds", "src/AsterGraph.Demo", "scenario"));
        Assert.True(HasLineWithAll(quickStart, "5 minutes", "FIVE_MINUTE_ONBOARDING_OK", "ONBOARDING_CONFIGURATION_OK"));
        Assert.True(HasLineWithAll(quickStart, "30 minutes", "hosted UI", "runtime-only", "plugin"));
        Assert.True(HasLineWithAll(quickStartZh, "30 秒", "src/AsterGraph.Demo", "场景"));
        Assert.True(HasLineWithAll(quickStartZh, "5 分钟", "FIVE_MINUTE_ONBOARDING_OK", "ONBOARDING_CONFIGURATION_OK"));
        Assert.True(HasLineWithAll(quickStartZh, "30 分钟", "hosted UI", "runtime-only", "plugin"));

        foreach (var contents in new[] { quickStart, quickStartZh })
        {
            Assert.Contains("CONSUMER_SAMPLE_SCENARIO_GRAPH_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("FIVE_MINUTE_ONBOARDING_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ONBOARDING_CONFIGURATION_OK:True", contents, StringComparison.Ordinal);
        }
    }

    private static string ReadRepoFile(string relativePath)
        => File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Directory.Build.props")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Failed to locate repository root from test base directory.");
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "AsterGraph.Demo.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void WriteProofFile(string proofRoot, string fileName, string contents)
        => File.WriteAllText(Path.Combine(proofRoot, fileName), contents.Replace("`n", Environment.NewLine, StringComparison.Ordinal));

    private static bool LineHasAdapterStatus(string contents, string subject, string requiredStatus)
    {
        return contents
            .Split('\n')
            .Any(line =>
                line.Contains(subject, StringComparison.OrdinalIgnoreCase) &&
                line.Contains(requiredStatus, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasLineWith(string contents, string requiredTerm, string requiredCompanion)
    {
        return contents
            .Split('\n', StringSplitOptions.TrimEntries)
            .Any(line =>
                line.Contains(requiredTerm, StringComparison.OrdinalIgnoreCase) &&
                line.Contains(requiredCompanion, StringComparison.OrdinalIgnoreCase));
    }

    private static void AssertAppearsBefore(string contents, string requiredText, string requiredHeading)
    {
        var textIndex = contents.IndexOf(requiredText, StringComparison.Ordinal);
        var headingIndex = contents.IndexOf(requiredHeading, StringComparison.Ordinal);

        Assert.True(textIndex >= 0, $"Expected to find '{requiredText}'.");
        Assert.True(headingIndex >= 0, $"Expected to find '{requiredHeading}'.");
        Assert.True(textIndex < headingIndex, $"Expected '{requiredText}' to appear before '{requiredHeading}'.");
    }

    private static bool HasLineWithAll(string contents, params string[] requiredTerms)
    {
        return contents
            .Split('\n', StringSplitOptions.TrimEntries)
            .Any(line => requiredTerms.All(term => line.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    private static bool HasLineWithOrderedTerms(string contents, params string[] orderedTerms)
    {
        return contents
            .Split('\n', StringSplitOptions.TrimEntries)
            .Any(line =>
            {
                var nextIndex = 0;
                foreach (var term in orderedTerms)
                {
                    var index = line.IndexOf(term, nextIndex, StringComparison.OrdinalIgnoreCase);
                    if (index < 0)
                    {
                        return false;
                    }

                    nextIndex = index + term.Length;
                }

                return true;
            });
    }

    private static string ExtractSection(string contents, string heading, string nextHeading)
    {
        var start = contents.IndexOf(heading, StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException($"Could not find section heading '{heading}'.");
        }

        var next = contents.IndexOf(nextHeading, start + heading.Length, StringComparison.Ordinal);
        if (next < 0)
        {
            throw new InvalidOperationException($"Could not find next section heading '{nextHeading}'.");
        }

        return contents[start..next];
    }

    private static string ExtractIssueTemplateBlock(string contents, string id)
    {
        var lines = contents.Split('\n');
        var start = Array.FindIndex(lines, line => line.Contains($"id: {id}", StringComparison.Ordinal));
        if (start < 0)
        {
            throw new InvalidOperationException($"Could not find issue template block for '{id}'.");
        }

        var end = start + 1;
        while (end < lines.Length && !lines[end].StartsWith("  - type:", StringComparison.Ordinal))
        {
            end++;
        }

        return string.Join(Environment.NewLine, lines.Skip(start).Take(end - start));
    }

    private static string[] ExtractDropdownOptions(string contents, string id)
    {
        var block = ExtractIssueTemplateBlock(contents, id);
        var lines = block.Split('\n');
        var optionsStart = Array.FindIndex(lines, line => line.TrimStart().StartsWith("options:", StringComparison.Ordinal));

        if (optionsStart < 0)
        {
            throw new InvalidOperationException($"Could not find options list for '{id}'.");
        }

        return lines[(optionsStart + 1)..]
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("- ", StringComparison.Ordinal))
            .Select(line => line[2..])
            .ToArray();
    }
}
