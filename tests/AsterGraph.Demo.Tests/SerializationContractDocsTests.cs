using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class SerializationContractDocsTests
{
    [Fact]
    public void SerializationContractDocs_DefendWorkspaceSchemaVersioningAndHostIntegrationLinks()
    {
        var serializationContractEn = ReadRepoFile("docs/en/serialization-contracts.md");
        var serializationContractZh = ReadRepoFile("docs/zh-CN/serialization-contracts.md");
        var hostIntegrationEn = ReadRepoFile("docs/en/host-integration.md");
        var hostIntegrationZh = ReadRepoFile("docs/zh-CN/host-integration.md");

        Assert.Contains("Current workspace/document schema version is `5`", serializationContractEn, StringComparison.Ordinal);
        Assert.Contains("Canonical write envelope for workspace documents is:", serializationContractEn, StringComparison.Ordinal);
        Assert.Contains("- `SchemaVersion`", serializationContractEn, StringComparison.Ordinal);
        Assert.Contains("- `Title`", serializationContractEn, StringComparison.Ordinal);
        Assert.Contains("- `Description`", serializationContractEn, StringComparison.Ordinal);
        Assert.Contains("- `RootGraphId`", serializationContractEn, StringComparison.Ordinal);
        Assert.Contains("- `GraphScopes`", serializationContractEn, StringComparison.Ordinal);
        Assert.Contains("Read behavior accepts:", serializationContractEn, StringComparison.Ordinal);
        Assert.Contains("unversioned legacy payloads", serializationContractEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("`SchemaVersion` values from `1` through `5`", serializationContractEn, StringComparison.Ordinal);
        Assert.Contains("unknown/future schema versions", serializationContractEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Clipboard and fragment payload versioning is separate from workspace-document versioning", serializationContractEn, StringComparison.Ordinal);

        Assert.Contains("写入 payload 的 JSON 字段固定为", serializationContractZh, StringComparison.Ordinal);
        Assert.Contains("`SchemaVersion`", serializationContractZh, StringComparison.Ordinal);
        Assert.Contains("`RootGraphId`", serializationContractZh, StringComparison.Ordinal);
        Assert.Contains("`GraphScopes`", serializationContractZh, StringComparison.Ordinal);
        Assert.Contains("若 JSON 中没有 `SchemaVersion`，按旧版无版本 payload 兼容路径反序列化。", serializationContractZh, StringComparison.Ordinal);
        Assert.Contains("若有 `SchemaVersion`，只接受 `1` 到 `5` 的版本号。", serializationContractZh, StringComparison.Ordinal);
        Assert.Contains("版本号 `< 1` 或 `> 5` 时抛出 `InvalidOperationException`", serializationContractZh, StringComparison.Ordinal);
        Assert.Contains("片段/剪贴板路径使用独立的 payload 契约与版本控制，不复用工作区文档 `SchemaVersion`。", serializationContractZh, StringComparison.Ordinal);
        Assert.Contains("`GraphDocumentSerializer` 写入 payload 的 JSON 字段固定为：", serializationContractZh, StringComparison.Ordinal);

        Assert.Contains("[Serialization Contracts](./serialization-contracts.md)", hostIntegrationEn, StringComparison.Ordinal);
        Assert.Contains("[序列化契约](./serialization-contracts.md)", hostIntegrationZh, StringComparison.Ordinal);
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
