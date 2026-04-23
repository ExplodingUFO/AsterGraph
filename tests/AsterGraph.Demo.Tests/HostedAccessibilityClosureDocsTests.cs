using System;
using System.IO;
using System.Linq;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class HostedAccessibilityClosureDocsTests
{
    [Fact]
    public void HostedAccessibilityRecipeClosureDocs_LinkOneCopyableHostedRecipeAcrossConsumerSampleGuidance()
    {
        var recipeEn = ReadRepoFile("docs/en/hosted-accessibility-recipe.md");
        var recipeZh = ReadRepoFile("docs/zh-CN/hosted-accessibility-recipe.md");
        var consumerSampleEn = ReadRepoFile("docs/en/consumer-sample.md");
        var consumerSampleZh = ReadRepoFile("docs/zh-CN/consumer-sample.md");
        var readme = ReadRepoFile("tools/AsterGraph.ConsumerSample.Avalonia/README.md");

        Assert.Contains("Hosted Accessibility Recipe", readme, StringComparison.Ordinal);
        Assert.Contains("../../docs/en/hosted-accessibility-recipe.md", readme, StringComparison.Ordinal);
        Assert.Contains("[Hosted Accessibility Recipe](./hosted-accessibility-recipe.md)", consumerSampleEn, StringComparison.Ordinal);
        Assert.Contains("[Hosted Accessibility Recipe](./hosted-accessibility-recipe.md)", consumerSampleZh, StringComparison.Ordinal);

        Assert.True(HasLineWithAll(recipeEn, "Step 1", "GraphEditorView", "NodeCanvas", "GraphInspectorView"));
        Assert.True(HasLineWithAll(recipeEn, "Step 2", "Control+Shift+P", "focus", "palette"));
        Assert.True(HasLineWithAll(recipeEn, "Step 3", "header", "palette", "node quick tools", "edge quick tools"));
        Assert.True(HasLineWithAll(recipeEn, "Step 4", "parameter metadata", "connection text editors", "AsterGraph.ConsumerSample.Avalonia -- --proof"));

        Assert.True(HasLineWithAll(recipeZh, "第 1 步", "GraphEditorView", "NodeCanvas", "GraphInspectorView"));
        Assert.True(HasLineWithAll(recipeZh, "第 2 步", "Control+Shift+P", "焦点", "palette"));
        Assert.True(HasLineWithAll(recipeZh, "第 3 步", "header", "palette", "node quick tool", "edge quick tool"));
        Assert.True(HasLineWithAll(recipeZh, "第 4 步", "parameter metadata", "connection text editor", "AsterGraph.ConsumerSample.Avalonia -- --proof"));
    }

    [Fact]
    public void HostedAccessibilityClosureDocs_SurfaceHostedAccessibilityMarkersAcrossProofAndBundleContracts()
    {
        var readme = ReadRepoFile("tools/AsterGraph.ConsumerSample.Avalonia/README.md");
        var consumerSampleEn = ReadRepoFile("docs/en/consumer-sample.md");
        var consumerSampleZh = ReadRepoFile("docs/zh-CN/consumer-sample.md");
        var supportBundleEn = ReadRepoFile("docs/en/support-bundle.md");
        var supportBundleZh = ReadRepoFile("docs/zh-CN/support-bundle.md");

        Assert.Contains("Copyable Hosted Accessibility Handoff", readme, StringComparison.Ordinal);
        Assert.Contains("Copyable Hosted Accessibility Handoff", consumerSampleEn, StringComparison.Ordinal);
        Assert.Contains("可复制的 Hosted Accessibility Handoff", consumerSampleZh, StringComparison.Ordinal);

        foreach (var contents in new[] { readme, consumerSampleEn, consumerSampleZh, supportBundleEn, supportBundleZh })
        {
            Assert.Contains("HOSTED_ACCESSIBILITY_BASELINE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("HOSTED_ACCESSIBILITY_FOCUS_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("HOSTED_ACCESSIBILITY_OK:True", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void HostedAccessibilityClosureDocs_PublishScreenReaderReadyEvaluationPathWithoutWideningBoundary()
    {
        var recipeEn = ReadRepoFile("docs/en/hosted-accessibility-recipe.md");
        var recipeZh = ReadRepoFile("docs/zh-CN/hosted-accessibility-recipe.md");
        var evaluationPathEn = ReadRepoFile("docs/en/evaluation-path.md");
        var evaluationPathZh = ReadRepoFile("docs/zh-CN/evaluation-path.md");
        var supportBundleEn = ReadRepoFile("docs/en/support-bundle.md");
        var supportBundleZh = ReadRepoFile("docs/zh-CN/support-bundle.md");
        var readme = ReadRepoFile("tools/AsterGraph.ConsumerSample.Avalonia/README.md");

        Assert.Contains("Screen-Reader-Ready Evaluation Path", recipeEn, StringComparison.Ordinal);
        Assert.Contains("ConsumerSample.Avalonia -- --proof", recipeEn, StringComparison.Ordinal);
        Assert.Contains("HostSample", recipeEn, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(recipeEn, "Narrator", "NVDA", "VoiceOver"));
        Assert.True(HasLineWithAll(recipeEn, "does not widen", "support promises"));

        Assert.Contains("Screen-Reader-Ready 评估路径", recipeZh, StringComparison.Ordinal);
        Assert.Contains("ConsumerSample.Avalonia -- --proof", recipeZh, StringComparison.Ordinal);
        Assert.Contains("HostSample", recipeZh, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(recipeZh, "Narrator", "NVDA", "VoiceOver"));
        Assert.True(HasLineWithAll(recipeZh, "不扩大支持承诺"));

        Assert.Contains("step-3 support bundle", evaluationPathEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("step-4 `HostSample` proof lines", evaluationPathEn, StringComparison.OrdinalIgnoreCase);
        Assert.True(HasLineWithAll(evaluationPathEn, "HOST_SAMPLE_AUTOMATION_OK:True", "HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True"));
        Assert.Contains("第 3 步", evaluationPathZh, StringComparison.Ordinal);
        Assert.Contains("support bundle", evaluationPathZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("第 4 步 `HostSample` 的 proof 行", evaluationPathZh, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(evaluationPathZh, "HOST_SAMPLE_AUTOMATION_OK:True", "HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True"));

        Assert.True(HasLineWithAll(supportBundleEn, "HOST_SAMPLE_AUTOMATION_OK:True", "HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True", "same bounded intake record"));
        Assert.True(HasLineWithAll(supportBundleZh, "HOST_SAMPLE_AUTOMATION_OK:True", "HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True", "同一条受限 intake 记录"));
        Assert.Contains("screen-reader-ready local evaluation", readme, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasLineWithAll(string contents, params string[] requiredTerms)
        => contents
            .Split('\n', StringSplitOptions.TrimEntries)
            .Any(line => requiredTerms.All(term => line.Contains(term, StringComparison.OrdinalIgnoreCase)));

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
}
