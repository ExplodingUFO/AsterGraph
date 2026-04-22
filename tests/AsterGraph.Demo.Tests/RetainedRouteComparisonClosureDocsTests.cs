using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class RetainedRouteComparisonClosureDocsTests
{
    [Fact]
    public void RetainedRouteComparisonGuidance_IsExplicitInEnglishPublicDocs()
    {
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var hostIntegration = ReadRepoFile("docs/en/host-integration.md");
        var retainedRecipe = ReadRepoFile("docs/en/retained-migration-recipe.md");

        Assert.Contains("When To Choose Retained", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("Use `CreateSession(...)` when you are starting new work or own your UI.", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("Use `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` when you want the shipped Avalonia route.", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("Use retained only when the host already constructs `GraphEditorViewModel` or `GraphEditorView` and you are migrating in batches.", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("Retained is not a fourth primary route.", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("The single bounded retained recipe is [Retained-To-Session Migration Recipe](./retained-migration-recipe.md).", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("Choose retained only when you are migrating an existing host in batches", quickStart, StringComparison.Ordinal);
        Assert.Contains("This is the single bounded retained recipe set.", retainedRecipe, StringComparison.Ordinal);
    }

    [Fact]
    public void RetainedRouteComparisonGuidance_IsExplicitInChinesePublicDocs()
    {
        var quickStart = ReadRepoFile("docs/zh-CN/quick-start.md");
        var hostIntegration = ReadRepoFile("docs/zh-CN/host-integration.md");
        var retainedRecipe = ReadRepoFile("docs/zh-CN/retained-migration-recipe.md");

        Assert.Contains("何时选择 retained", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("当你在开始新工作或宿主自己拥有 UI 时，请使用 `CreateSession(...)`", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("当你想使用 shipped Avalonia 路线时，请使用 `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("只有在现有宿主已经构造 `GraphEditorViewModel` 或 `GraphEditorView` 且正在分批迁移时才使用 retained", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("retained 不是第四条主路线", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("唯一的 retained recipe 是 [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)。", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("只有在现有宿主要分批迁移时才选 retained", quickStart, StringComparison.Ordinal);
        Assert.Contains("这是唯一一个 bounded 的 retained recipe 集合。", retainedRecipe, StringComparison.Ordinal);
    }

    [Fact]
    public void RetainedMigrationRecipe_StagedOwnershipIsExplicitInBothLocales()
    {
        var retainedRecipeEn = ReadRepoFile("docs/en/retained-migration-recipe.md");
        var retainedRecipeZh = ReadRepoFile("docs/zh-CN/retained-migration-recipe.md");

        Assert.Contains("## Stage 0: Keep the retained bridge", retainedRecipeEn, StringComparison.Ordinal);
        Assert.Contains("## Stage 1: Move command and query ownership into the session", retainedRecipeEn, StringComparison.Ordinal);
        Assert.Contains("## Stage 2: Replace retained-only helpers and retire bridge-only behavior", retainedRecipeEn, StringComparison.Ordinal);
        Assert.Contains("Keep temporarily:", retainedRecipeEn, StringComparison.Ordinal);
        Assert.Contains("Replace with canonical seams:", retainedRecipeEn, StringComparison.Ordinal);
        Assert.Contains("Ownership after this stage:", retainedRecipeEn, StringComparison.Ordinal);
        Assert.Contains("migration-only, not the preferred long-term extension path", retainedRecipeEn, StringComparison.Ordinal);
        Assert.Contains("## Proof and Evidence Handoff", retainedRecipeEn, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.ConsumerSample.Avalonia -- --proof", retainedRecipeEn, StringComparison.Ordinal);
        Assert.Contains("[Beta Support Bundle](./support-bundle.md)", retainedRecipeEn, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK:True", retainedRecipeEn, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_OK:True", retainedRecipeEn, StringComparison.Ordinal);
        Assert.Contains("does not add a separate support boundary", retainedRecipeEn, StringComparison.OrdinalIgnoreCase);
        Assert.True(retainedRecipeEn.IndexOf("## Stage 0: Keep the retained bridge", StringComparison.Ordinal)
            < retainedRecipeEn.IndexOf("## Stage 1: Move command and query ownership into the session", StringComparison.Ordinal));
        Assert.True(retainedRecipeEn.IndexOf("## Stage 1: Move command and query ownership into the session", StringComparison.Ordinal)
            < retainedRecipeEn.IndexOf("## Stage 2: Replace retained-only helpers and retire bridge-only behavior", StringComparison.Ordinal));
        Assert.True(retainedRecipeEn.IndexOf("## Stage 2: Replace retained-only helpers and retire bridge-only behavior", StringComparison.Ordinal)
            < retainedRecipeEn.IndexOf("## Proof and Evidence Handoff", StringComparison.Ordinal));

        Assert.Contains("## 第 0 阶段：先保留 retained 桥接", retainedRecipeZh, StringComparison.Ordinal);
        Assert.Contains("## 第 1 阶段：把 command 和 query 的归属移入 session", retainedRecipeZh, StringComparison.Ordinal);
        Assert.Contains("## 第 2 阶段：替换 retained-only helper 并退役桥接专属行为", retainedRecipeZh, StringComparison.Ordinal);
        Assert.Contains("暂时保留：", retainedRecipeZh, StringComparison.Ordinal);
        Assert.Contains("替换成 canonical seam：", retainedRecipeZh, StringComparison.Ordinal);
        Assert.Contains("这个阶段之后的归属：", retainedRecipeZh, StringComparison.Ordinal);
        Assert.Contains("迁移期专用", retainedRecipeZh, StringComparison.Ordinal);
        Assert.Contains("## 证据交接", retainedRecipeZh, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.ConsumerSample.Avalonia -- --proof", retainedRecipeZh, StringComparison.Ordinal);
        Assert.Contains("[Beta Support Bundle](./support-bundle.md)", retainedRecipeZh, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK:True", retainedRecipeZh, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_OK:True", retainedRecipeZh, StringComparison.Ordinal);
        Assert.Contains("不会新增单独的 retained 支持边界", retainedRecipeZh, StringComparison.Ordinal);
        Assert.True(retainedRecipeZh.IndexOf("## 第 0 阶段：先保留 retained 桥接", StringComparison.Ordinal)
            < retainedRecipeZh.IndexOf("## 第 1 阶段：把 command 和 query 的归属移入 session", StringComparison.Ordinal));
        Assert.True(retainedRecipeZh.IndexOf("## 第 1 阶段：把 command 和 query 的归属移入 session", StringComparison.Ordinal)
            < retainedRecipeZh.IndexOf("## 第 2 阶段：替换 retained-only helper 并退役桥接专属行为", StringComparison.Ordinal));
        Assert.True(retainedRecipeZh.IndexOf("## 第 2 阶段：替换 retained-only helper 并退役桥接专属行为", StringComparison.Ordinal)
            < retainedRecipeZh.IndexOf("## 证据交接", StringComparison.Ordinal));
    }

    [Fact]
    public void RetainedMigrationCleanup_RemainsCopyableWithoutNewCompatibilityPromise()
    {
        var quickStartEn = ReadRepoFile("docs/en/quick-start.md");
        var hostIntegrationEn = ReadRepoFile("docs/en/host-integration.md");
        var retainedRecipeEn = ReadRepoFile("docs/en/retained-migration-recipe.md");

        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");
        var hostIntegrationZh = ReadRepoFile("docs/zh-CN/host-integration.md");
        var retainedRecipeZh = ReadRepoFile("docs/zh-CN/retained-migration-recipe.md");

        Assert.Contains("Use the retained recipe only as a copyable migration aid for an existing host.", quickStartEn, StringComparison.Ordinal);
        Assert.Contains("Retained stays migration-only and does not add a new compatibility promise.", hostIntegrationEn, StringComparison.Ordinal);
        Assert.Contains("Copy this recipe only for an existing retained host slice.", retainedRecipeEn, StringComparison.Ordinal);

        Assert.Contains("仅把 retained recipe 当成现有宿主可复制的迁移辅助。", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("retained 仍然只用于迁移，不会新增新的兼容性承诺。", hostIntegrationZh, StringComparison.Ordinal);
        Assert.Contains("只有在现有 retained 宿主切片上才复制这份 recipe。", retainedRecipeZh, StringComparison.Ordinal);
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
}
