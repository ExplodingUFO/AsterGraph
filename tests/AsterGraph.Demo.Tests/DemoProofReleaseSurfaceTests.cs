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
        "HELLOWORLD_WPF_OK:True",
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
    public void HostedRouteLadder_IsNamedInEntryDocsAndSampleReadmes()
    {
        var entryDocs = new[]
        {
            ReadRepoFile("README.md"),
            ReadRepoFile("README.zh-CN.md"),
            ReadRepoFile("docs/en/quick-start.md"),
            ReadRepoFile("docs/zh-CN/quick-start.md"),
            ReadRepoFile("docs/en/evaluation-path.md"),
            ReadRepoFile("docs/zh-CN/evaluation-path.md"),
            ReadRepoFile("docs/en/project-status.md"),
            ReadRepoFile("docs/zh-CN/project-status.md"),
            ReadRepoFile("docs/en/host-integration.md"),
            ReadRepoFile("docs/zh-CN/host-integration.md"),
            ReadRepoFile("tools/AsterGraph.Starter.Avalonia/README.md"),
            ReadRepoFile("tools/AsterGraph.HelloWorld.Avalonia/README.md"),
            ReadRepoFile("tools/AsterGraph.ConsumerSample.Avalonia/README.md"),
        };

        foreach (var contents in entryDocs)
        {
            Assert.Contains("hosted route ladder", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Starter.Avalonia", contents, StringComparison.Ordinal);
            Assert.Contains("HelloWorld.Avalonia", contents, StringComparison.Ordinal);
            Assert.Contains("ConsumerSample.Avalonia", contents, StringComparison.Ordinal);
            Assert.True(HasLineWithOrderedTerms(contents, "Starter.Avalonia", "HelloWorld.Avalonia", "ConsumerSample.Avalonia"));
        }
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
        Assert.True(HasLineWith(evaluationPath, "retained", "migration"));
        Assert.True(HasLineWith(evaluationPath, "HostSample", "proof"));
        Assert.True(HasLineWith(evaluationPath, "HostSample", "after"));
        Assert.True(HasLineWith(evaluationPathZh, "WPF", "验证"));
        Assert.True(HasLineWith(evaluationPathZh, "retained", "迁移"));
        Assert.True(HasLineWith(evaluationPathZh, "HostSample", "proof"));
        Assert.True(HasLineWith(evaluationPathZh, "HostSample", "之后"));
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
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:baseline:True", projectStatus, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:large:True", projectStatus, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERF_SUMMARY:stress", projectStatus, StringComparison.Ordinal);
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
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:baseline:True", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:large:True", projectStatusZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERF_SUMMARY:stress", projectStatusZh, StringComparison.Ordinal);
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
        Assert.Contains("## 当前种子试用综合", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("维护者种子预演证据", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("3 到 5 的门槛", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("真实外部报告", adoptionFeedbackZh, StringComparison.Ordinal);
        Assert.Contains("同一个受限风险", adoptionFeedbackZh, StringComparison.Ordinal);
    }

    [Fact]
    public void SupportBundleDocs_DefineLocalConsumerSampleContract()
    {
        var supportBundle = ReadRepoFile("docs/en/support-bundle.md");
        var supportBundleZh = ReadRepoFile("docs/zh-CN/support-bundle.md");
        var consumerSampleDoc = ReadRepoFile("docs/en/consumer-sample.md");
        var consumerSampleReadme = ReadRepoFile("tools/AsterGraph.ConsumerSample.Avalonia/README.md");
        var adoptionFeedback = ReadRepoFile("docs/en/adoption-feedback.md");
        var adoptionFeedbackZh = ReadRepoFile("docs/zh-CN/adoption-feedback.md");

        foreach (var contents in new[] { supportBundle, supportBundleZh })
        {
            Assert.Contains("ConsumerSample.Avalonia", contents, StringComparison.Ordinal);
            Assert.Contains("--support-bundle", contents, StringComparison.Ordinal);
            Assert.Contains("SUPPORT_BUNDLE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("packageVersion", contents, StringComparison.Ordinal);
            Assert.Contains("publicTag", contents, StringComparison.Ordinal);
            Assert.Contains("route", contents, StringComparison.Ordinal);
            Assert.Contains("proofLines", contents, StringComparison.Ordinal);
            Assert.Contains("environment", contents, StringComparison.Ordinal);
            Assert.Contains("reproduction", contents, StringComparison.Ordinal);
        }

        Assert.Contains("bounded intake record", supportBundle, StringComparison.OrdinalIgnoreCase);
        Assert.True(HasLineWithAll(supportBundleZh, "support bundle", "附件"));
        AssertAppearsBefore(supportBundle, "Beta Evaluation Path", "## Canonical Producer");
        AssertAppearsBefore(supportBundleZh, "公开 Beta 评估路径", "## Canonical 生成入口");
        Assert.Contains("support-bundle", consumerSampleDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("support-bundle", consumerSampleReadme, StringComparison.OrdinalIgnoreCase);
        Assert.True(HasLineWithAll(adoptionFeedback, "route", "version", "proof markers", "friction", "support-bundle attachment note"));
        Assert.True(HasLineWithAll(adoptionFeedbackZh, "route", "version", "proof 标记", "摩擦", "support bundle 附件备注"));
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
    public void IntakeHandoffDocs_UseOneBoundedIntakeVocabularyAndKeepQuickReferenceSummaryOnly()
    {
        var evaluationPath = ReadRepoFile("docs/en/evaluation-path.md");
        var evaluationPathZh = ReadRepoFile("docs/zh-CN/evaluation-path.md");
        var consumerSample = ReadRepoFile("docs/en/consumer-sample.md");
        var consumerSampleZh = ReadRepoFile("docs/zh-CN/consumer-sample.md");
        var supportBundle = ReadRepoFile("docs/en/support-bundle.md");
        var supportBundleZh = ReadRepoFile("docs/zh-CN/support-bundle.md");
        var adoptionFeedback = ReadRepoFile("docs/en/adoption-feedback.md");
        var adoptionFeedbackZh = ReadRepoFile("docs/zh-CN/adoption-feedback.md");
        var adoptionTemplate = ReadRepoFile(".github/ISSUE_TEMPLATE/adoption_feedback.yml");
        var quickReferenceEn = ExtractSection(
            consumerSample,
            "## Trust and proof quick reference",
            "## Run It");
        var quickReferenceZh = ExtractSection(
            consumerSampleZh,
            "## 信任与证明速查",
            "## 如何运行");
        var proofHandoffEn = ExtractSection(
            consumerSample,
            "## Proof Handoff",
            "## When To Use This Sample");
        var proofHandoffZh = ExtractSection(
            consumerSampleZh,
            "## Proof Handoff",
            "## 什么时候看它");
        var supportBundleBlock = ExtractIssueTemplateBlock(adoptionTemplate, "support_bundle");

        Assert.Contains("Boundary First", evaluationPath, StringComparison.Ordinal);
        Assert.Contains("先锁边界", evaluationPathZh, StringComparison.Ordinal);
        Assert.Contains("defended route", evaluationPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("bounded intake record", evaluationPath, StringComparison.OrdinalIgnoreCase);
        Assert.True(HasLineWithOrderedTerms(evaluationPath, "defended route", "bounded intake record"));
        Assert.Contains("受防守路线", evaluationPathZh, StringComparison.Ordinal);
        Assert.Contains("受限 intake 记录", evaluationPathZh, StringComparison.Ordinal);
        Assert.True(HasLineWithOrderedTerms(evaluationPathZh, "受防守路线", "受限 intake 记录"));
        Assert.Contains("summary-only", quickReferenceEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("summary-only", quickReferenceZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Beta Support Bundle", quickReferenceEn, StringComparison.Ordinal);
        Assert.Contains("Adoption Feedback Loop", quickReferenceEn, StringComparison.Ordinal);
        Assert.Contains("Beta Support Bundle", quickReferenceZh, StringComparison.Ordinal);
        Assert.Contains("Adoption Feedback Loop", quickReferenceZh, StringComparison.Ordinal);
        Assert.True(quickReferenceEn.IndexOf("Beta Support Bundle", StringComparison.Ordinal) < quickReferenceEn.IndexOf("Adoption Feedback Loop", StringComparison.Ordinal));
        Assert.True(quickReferenceZh.IndexOf("Beta Support Bundle", StringComparison.Ordinal) < quickReferenceZh.IndexOf("Adoption Feedback Loop", StringComparison.Ordinal));
        Assert.DoesNotContain("Public Launch Checklist", quickReferenceEn, StringComparison.Ordinal);
        Assert.DoesNotContain("Public Launch Checklist", quickReferenceZh, StringComparison.Ordinal);
        Assert.Contains("Proof Handoff", proofHandoffEn, StringComparison.Ordinal);
        Assert.Contains("Proof Handoff", proofHandoffZh, StringComparison.Ordinal);
        Assert.Contains("--support-bundle", proofHandoffEn, StringComparison.Ordinal);
        Assert.Contains("--support-bundle", proofHandoffZh, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", proofHandoffEn, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", proofHandoffZh, StringComparison.Ordinal);
        Assert.Contains("NO_SUPPORT_BUNDLE:route-cannot-produce-one", proofHandoffEn, StringComparison.Ordinal);
        Assert.Contains("NO_SUPPORT_BUNDLE:route-cannot-produce-one", proofHandoffZh, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK", proofHandoffEn, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_METADATA_PROJECTION_OK", proofHandoffEn, StringComparison.Ordinal);
        Assert.Contains("parameterSnapshots", proofHandoffEn, StringComparison.Ordinal);
        Assert.Contains("failed proof-marker", proofHandoffEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK", proofHandoffZh, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_METADATA_PROJECTION_OK", proofHandoffZh, StringComparison.Ordinal);
        Assert.Contains("parameterSnapshots", proofHandoffZh, StringComparison.Ordinal);
        Assert.Contains("失败的 proof-marker", proofHandoffZh, StringComparison.OrdinalIgnoreCase);
        Assert.True(proofHandoffEn.IndexOf("AsterGraph.ConsumerSample.Avalonia -- --proof", StringComparison.Ordinal) < proofHandoffEn.IndexOf("SUPPORT_BUNDLE_PATH:...", StringComparison.Ordinal));
        Assert.True(proofHandoffEn.IndexOf("SUPPORT_BUNDLE_PATH:...", StringComparison.Ordinal) < proofHandoffEn.IndexOf("NO_SUPPORT_BUNDLE:route-cannot-produce-one", StringComparison.Ordinal));
        Assert.True(proofHandoffZh.IndexOf("AsterGraph.ConsumerSample.Avalonia -- --proof", StringComparison.Ordinal) < proofHandoffZh.IndexOf("SUPPORT_BUNDLE_PATH:...", StringComparison.Ordinal));
        Assert.True(proofHandoffZh.IndexOf("SUPPORT_BUNDLE_PATH:...", StringComparison.Ordinal) < proofHandoffZh.IndexOf("NO_SUPPORT_BUNDLE:route-cannot-produce-one", StringComparison.Ordinal));
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK", evaluationPath, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_METADATA_PROJECTION_OK", evaluationPath, StringComparison.Ordinal);
        Assert.Contains("parameterSnapshots", evaluationPath, StringComparison.Ordinal);
        Assert.Contains("failed proof-marker", evaluationPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK", evaluationPathZh, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_METADATA_PROJECTION_OK", evaluationPathZh, StringComparison.Ordinal);
        Assert.Contains("parameterSnapshots", evaluationPathZh, StringComparison.Ordinal);
        Assert.Contains("失败的 proof-marker", evaluationPathZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("bounded intake record", supportBundleBlock, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("attachment note", supportBundleBlock, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("bounded intake record", supportBundle, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("受限 intake 记录", supportBundleZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK", supportBundle, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_METADATA_PROJECTION_OK", supportBundle, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PERSISTENCE_OK", supportBundle, StringComparison.Ordinal);
        Assert.Contains("parameterSnapshots", supportBundle, StringComparison.Ordinal);
        Assert.Contains("status", supportBundle, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("owner", supportBundle, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("priority", supportBundle, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK", supportBundleZh, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_METADATA_PROJECTION_OK", supportBundleZh, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PERSISTENCE_OK", supportBundleZh, StringComparison.Ordinal);
        Assert.Contains("parameterSnapshots", supportBundleZh, StringComparison.Ordinal);
        Assert.Contains("status", supportBundleZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("owner", supportBundleZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("priority", supportBundleZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("bounded intake vocabulary", adoptionFeedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("受限 intake 词汇", adoptionFeedbackZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("proof markers", adoptionFeedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("proof 标记", adoptionFeedbackZh, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("screenshot reference", adoptionFeedback, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("截图", adoptionFeedback, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("screenshot reference", adoptionFeedbackZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("bounded intake record", adoptionTemplate, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", adoptionTemplate, StringComparison.Ordinal);
        Assert.Contains("NO_SUPPORT_BUNDLE:route-cannot-produce-one", adoptionTemplate, StringComparison.Ordinal);
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
    public void CiScript_PublishesStressTelemetrySummaryAlongsideDefendedScaleProof()
    {
        var script = ReadRepoFile("eng/ci.ps1");

        Assert.Contains("'baseline'", script, StringComparison.Ordinal);
        Assert.Contains("'large'", script, StringComparison.Ordinal);
        Assert.Contains("'stress'", script, StringComparison.Ordinal);
        Assert.Contains("'--samples'", script, StringComparison.Ordinal);
        Assert.Contains("'3'", script, StringComparison.Ordinal);
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

        Assert.Contains("artifacts/proof/demo-proof.txt", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("DEMO_OK", releaseWorkflow, StringComparison.Ordinal);
        foreach (var markerId in DemoProofContract.PublicSuccessMarkerIds)
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
    public void CiScript_CapturesWpfAdapterMatrixProofArtifact()
    {
        var script = ReadRepoFile("eng/ci.ps1");

        Assert.Contains("helloWorldWpfProofPath", script, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK:True", script, StringComparison.Ordinal);
        Assert.Contains("Validate WPF richer sample proof", script, StringComparison.Ordinal);
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
    public void SecondAdapterDocs_LockWpfAdapterMatrixContractAcrossLanguages()
    {
        var readme = ReadRepoFile("README.md");
        var readmeZh = ReadRepoFile("README.zh-CN.md");
        var hostIntegration = ReadRepoFile("docs/en/host-integration.md");
        var hostIntegrationZh = ReadRepoFile("docs/zh-CN/host-integration.md");
        var architecture = ReadRepoFile("docs/en/architecture.md");
        var architectureZh = ReadRepoFile("docs/zh-CN/architecture.md");
        var projectStatus = ReadRepoFile("docs/en/project-status.md");
        var projectStatusZh = ReadRepoFile("docs/zh-CN/project-status.md");
        var alphaStatus = ReadRepoFile("docs/en/alpha-status.md");
        var alphaStatusZh = ReadRepoFile("docs/zh-CN/alpha-status.md");
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");
        var adapterMatrix = ReadRepoFile("docs/en/adapter-capability-matrix.md");
        var adapterMatrixZh = ReadRepoFile("docs/zh-CN/adapter-capability-matrix.md");
        var extensionContracts = ReadRepoFile("docs/en/extension-contracts.md");
        var extensionContractsZh = ReadRepoFile("docs/zh-CN/extension-contracts.md");

        foreach (var contents in new[]
                 {
                     readme,
                     readmeZh,
                     hostIntegration,
                     hostIntegrationZh,
                     architecture,
                     architectureZh,
                     projectStatus,
                     projectStatusZh,
                     alphaStatus,
                     alphaStatusZh,
                     quickStart,
                     quickStartZh,
                     adapterMatrix,
                     adapterMatrixZh,
                 })
        {
            Assert.Contains("WPF", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { extensionContracts, extensionContractsZh })
        {
            Assert.Contains("### Stable canonical surfaces", contents, StringComparison.Ordinal);
            Assert.Contains("### Supported hosted-UI composition helper", contents, StringComparison.Ordinal);
            Assert.Contains("### Retained migration surfaces", contents, StringComparison.Ordinal);
            Assert.Contains("### Compatibility-only shims", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphEditorFactory.CreateSession(...)", contents, StringComparison.Ordinal);
        Assert.Contains("IGraphEditorSession", contents, StringComparison.Ordinal);
        Assert.Contains("AsterGraphEditorFactory.Create(...)", contents, StringComparison.Ordinal);
        Assert.Contains("GraphEditorViewModel", contents, StringComparison.Ordinal);
        Assert.Contains("GraphEditorView", contents, StringComparison.Ordinal);
        Assert.Contains("GraphEditorViewModel.Session", contents, StringComparison.Ordinal);
        Assert.Contains("GetCompatiblePortTargets(...)", contents, StringComparison.Ordinal);
        Assert.Contains("IGraphEditorQueries.GetCompatibleTargets(...)", contents, StringComparison.Ordinal);
        Assert.Contains("CompatiblePortTarget", contents, StringComparison.Ordinal);
        Assert.Contains("stable canonical surfaces", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("Use the retained surfaces only as migration bridges.", extensionContracts, StringComparison.Ordinal);
        Assert.Contains("canonical-first 指导是", extensionContractsZh, StringComparison.Ordinal);
        Assert.Contains("new work should start on the stable canonical surfaces", extensionContracts, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("新工作应优先从 `Stable canonical surfaces` 起步", extensionContractsZh, StringComparison.Ordinal);

        foreach (var contents in new[] { readme, readmeZh, hostIntegration, hostIntegrationZh, architecture, architectureZh, projectStatus, projectStatusZh, alphaStatus, alphaStatusZh, quickStart, quickStartZh })
        {
            Assert.Contains("adapter-capability-matrix.md", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("adapter 2", adapterMatrix, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Supported", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("Partial", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("Fallback", adapterMatrix, StringComparison.Ordinal);
        Assert.DoesNotContain("Phase 154", adapterMatrix, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CreateSession(...)", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("IGraphEditorSession", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.Editor", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("adapter-specific runtime APIs", adapterMatrix, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("on top of the existing canonical session/runtime route", adapterMatrix, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("layered on the same runtime owner", adapterMatrix, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Matrix Vocabulary", adapterMatrix, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Matrix Categories", adapterMatrix, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Retained migration is not", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("COMMAND_SURFACE_OK", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("HOST_SAMPLE_OK", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("DEMO_OK", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("Fallback Rule", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("lower-level documented path", adapterMatrix, StringComparison.OrdinalIgnoreCase);
        Assert.True(LineHasAdapterStatus(adapterMatrix, "Avalonia", "supported"));
        Assert.True(LineHasAdapterStatus(adapterMatrix, "WPF", "partial"));
        Assert.True(LineHasAdapterStatus(adapterMatrix, "WPF", "fallback"));

        Assert.Contains("adapter 2", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("supported", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("partial", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fallback", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("must not exceed", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("must not exceed", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("同一条 canonical session/runtime 路线", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("公开文档描述 Avalonia/WPF 支持情况时，只使用下面这三种标签", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("Matrix Vocabulary", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Matrix Categories", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("HELLOWORLD_WPF_OK", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("COMMAND_SURFACE_OK", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("HOST_SAMPLE_OK", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("DEMO_OK", adapterMatrixZh, StringComparison.Ordinal);
        Assert.DoesNotContain("Phase 154", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CreateSession(...)", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("IGraphEditorSession", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.Editor", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("retained", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fallback", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("proof", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.True(LineHasAdapterStatus(adapterMatrixZh, "Avalonia", "supported"));
        Assert.True(LineHasAdapterStatus(adapterMatrixZh, "WPF", "partial"));
        Assert.True(LineHasAdapterStatus(adapterMatrixZh, "WPF", "fallback"));
    }

    [Fact]
    public void WritePrereleaseNotes_IncludeDemoProofMarkersInProofSummary()
    {
        var tempRoot = CreateTempDirectory();
        var proofRoot = Path.Combine(tempRoot, "proof");
        Directory.CreateDirectory(proofRoot);

        WriteProofFile(proofRoot, "public-repo-hygiene.txt", "PUBLIC_REPO_HYGIENE_OK:True");
        WriteProofFile(proofRoot, "hostsample-packed.txt", "HOST_SAMPLE_OK:True");
        WriteProofFile(proofRoot, "consumer-sample.txt", "CONSUMER_SAMPLE_OK:True");
        WriteProofFile(proofRoot, "hostsample-net10-packed.txt", "HOST_SAMPLE_NET10_OK:True");
        WriteProofFile(proofRoot, "package-smoke.txt", "PACKAGE_SMOKE_OK:True");
        WriteProofFile(
            proofRoot,
            "scale-smoke.txt",
            "SCALE_TIER_BUDGET:baseline`nSCALE_PERFORMANCE_BUDGET_OK:baseline:True:none`nSCALE_TIER_BUDGET:large`nSCALE_PERFORMANCE_BUDGET_OK:large:True:none`nSCALE_TIER_BUDGET:stress:nodes=5000:selection=256:moves=96:budget=informational-only`nSCALE_PERFORMANCE_BUDGET_OK:stress:True:informational-only`nSCALE_PERF_SUMMARY:stress:samples=3:setup-p50=311:setup-p95=569:selection-p50=18:selection-p95=41:connection-p50=582:connection-p95=820:history-p50=912:history-p95=946:viewport-p50=2:viewport-p95=5:save-p50=149:save-p95=156:reload-p50=62:reload-p95=65`nSCALE_HISTORY_CONTRACT_OK:True");
        WriteProofFile(
            proofRoot,
            "demo-proof.txt",
            string.Join("`n", ["DEMO_OK:True", .. DemoProofContract.CreatePublicSuccessMarkerLines()]));
        WriteProofFile(proofRoot, "hello-world-wpf-proof.txt", string.Join("`n", AdapterMatrixProofMarkerLines));

        var coveragePath = Path.Combine(tempRoot, "coverage-summary.json");
        File.WriteAllText(
            coveragePath,
            """
            {
              "coveredLines": 10,
              "totalLines": 20,
              "lineRate": 0.5
            }
            """);

        var outputPath = Path.Combine(tempRoot, "prerelease-notes.md");
        var startInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments =
                $"-NoProfile -ExecutionPolicy Bypass -File \"{Path.Combine(GetRepositoryRoot(), "eng", "write-prerelease-notes.ps1")}\" " +
                $"-RepoRoot \"{GetRepositoryRoot()}\" " +
                $"-ProofRoot \"{proofRoot}\" " +
                $"-OutputPath \"{outputPath}\" " +
                $"-CoverageSummaryPath \"{coveragePath}\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        process!.WaitForExit();

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        Assert.True(
            process.ExitCode == 0,
            $"write-prerelease-notes.ps1 failed with exit code {process.ExitCode}.{Environment.NewLine}STDOUT:{Environment.NewLine}{stdout}{Environment.NewLine}STDERR:{Environment.NewLine}{stderr}");

        var notes = File.ReadAllText(outputPath);
        foreach (var requiredProofLine in new[] { "DEMO_OK:True" }.Concat(DemoProofContract.CreatePublicSuccessMarkerLines()))
        {
            Assert.Contains(requiredProofLine, notes, StringComparison.Ordinal);
        }

        foreach (var requiredMatrixProofLine in AdapterMatrixProofMarkerLines)
        {
            Assert.Contains(requiredMatrixProofLine, notes, StringComparison.Ordinal);
        }

        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:baseline:True:none", notes, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:large:True:none", notes, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERF_SUMMARY:stress:samples=3", notes, StringComparison.Ordinal);
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
            Assert.Contains("SCALE_PERF_SUMMARY", contents, StringComparison.Ordinal);
        }

        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:large:True:...", checklist, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERF_SUMMARY:stress:...", checklist, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:large:True:...", checklistZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERF_SUMMARY:stress:...", checklistZh, StringComparison.Ordinal);

        Assert.Contains("ADAPTER_CAPABILITY_MATRIX_FORMAT:1", checklist, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS", checklist, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS", checklist, StringComparison.Ordinal);
        Assert.DoesNotContain("ADAPTER_CAPABILITY_MATRIX:True", checklist, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK", checklist, StringComparison.Ordinal);
        Assert.True(HasLineWith(checklist, "HELLOWORLD_WPF_OK", "adapter-2"));
        Assert.True(HasLineWith(checklist, "HELLOWORLD_WPF_OK", "parity"));
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX_FORMAT:1", checklistZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS", checklistZh, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS", checklistZh, StringComparison.Ordinal);
        Assert.DoesNotContain("ADAPTER_CAPABILITY_MATRIX:True", checklistZh, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK", checklistZh, StringComparison.Ordinal);
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
