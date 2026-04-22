using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class PluginTrustContractDocsTests
{
    [Fact]
    public void PluginTrustContractDocs_DefendManifestTrustPolicyAndImplicitAllowContract()
    {
        var pluginTrustContractsEn = ReadRepoFile("docs/en/plugin-trust-contracts.md");
        var hostIntegrationEn = ReadRepoFile("docs/en/host-integration.md");
        var pluginRecipeEn = ReadRepoFile("docs/en/plugin-recipe.md");
        var consumerSampleEn = ReadRepoFile("docs/en/consumer-sample.md");

        Assert.Contains("# Plugin Manifest and Trust Policy Contract v1", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("## Manifest Fields", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("- `Id`", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("- `Provenance`", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("## Trust Policy Ownership", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("`IGraphEditorPluginTrustPolicy` is the host decision point", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("`GraphEditorPluginTrustPolicyContext` exposes the manifest, provenance evidence, and package path", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("If the host does not configure a policy, the runtime uses `GraphEditorPluginTrustEvaluation.ImplicitAllow()`", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("## Implicit Allow Contract", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("implicit allow is a host/runtime default, not a plugin capability", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("## Blocked Before Activation", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("plugin trust decisions are evaluated before any contribution code is allowed to execute", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("[Host Integration](./host-integration.md)", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("[Plugin And Custom Node Recipe](./plugin-recipe.md)", pluginTrustContractsEn, StringComparison.Ordinal);
        Assert.Contains("[Consumer Sample](./consumer-sample.md)", pluginTrustContractsEn, StringComparison.Ordinal);

        Assert.Contains("[Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)", hostIntegrationEn, StringComparison.Ordinal);
        Assert.Contains("[Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)", pluginRecipeEn, StringComparison.Ordinal);
        Assert.Contains("[Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)", consumerSampleEn, StringComparison.Ordinal);
        Assert.Contains("Plugin trust is host-owned.", hostIntegrationEn, StringComparison.Ordinal);
        Assert.Contains("Plugin loading is not sandboxed.", pluginRecipeEn, StringComparison.Ordinal);
        Assert.Contains("plugin trust stays explicit and host-owned", consumerSampleEn, StringComparison.Ordinal);
        Assert.Contains("allowlist decisions can be exported or imported without rebuilding the host trust-policy flow", consumerSampleEn, StringComparison.Ordinal);
    }

    [Fact]
    public void PluginTrustContractDocs_DefendHostIntegrationPluginRecipeAndConsumerSampleLinks()
    {
        var pluginTrustContractsZh = ReadRepoFile("docs/zh-CN/plugin-trust-contracts.md");
        var hostIntegrationZh = ReadRepoFile("docs/zh-CN/host-integration.md");
        var pluginRecipeZh = ReadRepoFile("docs/zh-CN/plugin-recipe.md");
        var consumerSampleZh = ReadRepoFile("docs/zh-CN/consumer-sample.md");

        Assert.Contains("# 插件信任契约 v1", pluginTrustContractsZh, StringComparison.Ordinal);
        Assert.Contains("## 插件清单字段", pluginTrustContractsZh, StringComparison.Ordinal);
        Assert.Contains("`GraphEditorPluginManifest`", pluginTrustContractsZh, StringComparison.Ordinal);
        Assert.Contains("## Trust Policy 归属", pluginTrustContractsZh, StringComparison.Ordinal);
        Assert.Contains("`IGraphEditorPluginTrustPolicy`", pluginTrustContractsZh, StringComparison.Ordinal);
        Assert.Contains("`GraphEditorPluginTrustEvaluation.ImplicitAllow(...)`", pluginTrustContractsZh, StringComparison.Ordinal);
        Assert.Contains("`trust.policy.not-configured`", pluginTrustContractsZh, StringComparison.Ordinal);
        Assert.Contains("`AsterGraphEditorFactory.StagePluginPackage(...)`", pluginTrustContractsZh, StringComparison.Ordinal);
        Assert.Contains("不提供 sandbox", pluginTrustContractsZh, StringComparison.Ordinal);
        Assert.Contains("不提供不受信任代码隔离", pluginTrustContractsZh, StringComparison.Ordinal);
        Assert.Contains("[插件信任契约 v1](./plugin-trust-contracts.md)", hostIntegrationZh, StringComparison.Ordinal);
        Assert.Contains("[插件信任契约 v1](./plugin-trust-contracts.md)", pluginRecipeZh, StringComparison.Ordinal);
        Assert.Contains("[插件信任契约 v1](./plugin-trust-contracts.md)", consumerSampleZh, StringComparison.Ordinal);
        Assert.Contains("插件信任由宿主负责。", hostIntegrationZh, StringComparison.Ordinal);
        Assert.Contains("插件加载没有 sandbox。", pluginRecipeZh, StringComparison.Ordinal);
        Assert.Contains("插件信任策略保持显式且由宿主管理", consumerSampleZh, StringComparison.Ordinal);
    }

    [Fact]
    public void PluginTrustContractDocs_ElevateConsumerSampleAsHostedTrustHopInQuickStartAndEvaluationPath()
    {
        var quickStartEn = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");
        var evaluationPathEn = ReadRepoFile("docs/en/evaluation-path.md");
        var evaluationPathZh = ReadRepoFile("docs/zh-CN/evaluation-path.md");
        var consumerSampleReadme = ReadRepoFile("tools/AsterGraph.ConsumerSample.Avalonia/README.md");

        const string pluginTrustLinkEn = "[Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)";
        const string pluginRecipeLinkEn = "[Plugin And Custom Node Recipe](./plugin-recipe.md)";
        const string pluginTrustLinkZh = "[插件信任契约 v1](./plugin-trust-contracts.md)";
        const string pluginRecipeLinkZh = "[Plugin 与自定义节点 Recipe](./plugin-recipe.md)";
        const string consumerSampleTrustLinkEn = "[Plugin Manifest and Trust Policy Contract v1](../../docs/en/plugin-trust-contracts.md)";
        const string consumerSampleRecipeLinkEn = "[Plugin And Custom Node Recipe](../../docs/en/plugin-recipe.md)";

        Assert.Contains("ConsumerSample.Avalonia", quickStartEn, StringComparison.Ordinal);
        Assert.Contains("ConsumerSample.Avalonia", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("ConsumerSample.Avalonia", evaluationPathEn, StringComparison.Ordinal);
        Assert.Contains("ConsumerSample.Avalonia", evaluationPathZh, StringComparison.Ordinal);
        Assert.Contains("ConsumerSample.Avalonia", consumerSampleReadme, StringComparison.Ordinal);

        Assert.Contains("hosted trust hop", quickStartEn, StringComparison.Ordinal);
        Assert.Contains("hosted trust hop", evaluationPathEn, StringComparison.Ordinal);
        Assert.Contains("hosted trust hop", consumerSampleReadme, StringComparison.Ordinal);

        Assert.Contains("受防守的 hosted trust hop", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("受防守的 hosted trust hop", evaluationPathZh, StringComparison.Ordinal);

        Assert.Contains(pluginTrustLinkEn, quickStartEn, StringComparison.Ordinal);
        Assert.Contains(pluginRecipeLinkEn, quickStartEn, StringComparison.Ordinal);
        Assert.Contains(pluginTrustLinkEn, evaluationPathEn, StringComparison.Ordinal);
        Assert.Contains(pluginRecipeLinkEn, evaluationPathEn, StringComparison.Ordinal);
        Assert.Contains(pluginTrustLinkZh, quickStartZh, StringComparison.Ordinal);
        Assert.Contains(pluginRecipeLinkZh, quickStartZh, StringComparison.Ordinal);
        Assert.Contains(pluginTrustLinkZh, evaluationPathZh, StringComparison.Ordinal);
        Assert.Contains(pluginRecipeLinkZh, evaluationPathZh, StringComparison.Ordinal);
        Assert.Contains(consumerSampleTrustLinkEn, consumerSampleReadme, StringComparison.Ordinal);
        Assert.Contains(consumerSampleRecipeLinkEn, consumerSampleReadme, StringComparison.Ordinal);

        Assert.True(quickStartEn.IndexOf(pluginTrustLinkEn, StringComparison.Ordinal) < quickStartEn.IndexOf("[Host Integration](./host-integration.md)", StringComparison.Ordinal));
        Assert.True(quickStartZh.IndexOf(pluginTrustLinkZh, StringComparison.Ordinal) < quickStartZh.IndexOf("[Host Integration](./host-integration.md)", StringComparison.Ordinal));
        Assert.True(evaluationPathEn.IndexOf(pluginTrustLinkEn, StringComparison.Ordinal) < evaluationPathEn.IndexOf("[Consumer Sample](./consumer-sample.md)", StringComparison.Ordinal));
        Assert.True(evaluationPathZh.IndexOf(pluginTrustLinkZh, StringComparison.Ordinal) < evaluationPathZh.IndexOf("[Consumer Sample](./consumer-sample.md)", StringComparison.Ordinal));
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
