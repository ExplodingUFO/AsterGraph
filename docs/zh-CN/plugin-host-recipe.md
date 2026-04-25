# Plugin Host Recipe

这份 recipe 展示的是宿主视角的插件发现、信任评估、注册和运行时通信路线。

当宿主需要从磁盘加载插件，并在任何插件代码运行前做显式 allow/block 决策时，使用本文档。

更小的插件作者路线见 [Plugin 与自定义节点 Recipe](./plugin-recipe.md)。v1 manifest 和 trust-policy 合同见 [插件信任契约 v1](./plugin-trust-contracts.md)。

## 包

```powershell
dotnet add package AsterGraph.Editor --prerelease
dotnet add package AsterGraph.Abstractions --prerelease
```

只有宿主还要嵌入默认 UI 时，才再加 `AsterGraph.Avalonia`。

## 1. 发现

用 `AsterGraphEditorFactory.DiscoverPluginCandidates(...)` 枚举候选，而不加载插件程序集。

```csharp
var discoveryOptions = new GraphEditorPluginDiscoveryOptions
{
    DirectorySources =
    [
        new GraphEditorPluginDirectoryDiscoverySource(@"C:\MyHost\Plugins")
    ],
    PackageDirectorySources =
    [
        new GraphEditorPluginPackageDiscoverySource(@"C:\MyHost\PluginPackages")
    ],
    TrustPolicy = myHostTrustPolicy, // 可选预过滤
};

var candidates = AsterGraphEditorFactory.DiscoverPluginCandidates(discoveryOptions);
```

得到的内容：

- 每个候选一个 `GraphEditorPluginCandidateSnapshot`
- `Manifest`（id、display name、version、compatibility、provenance）
- `ProvenanceEvidence`（package identity、signature status、signer fingerprint）
- `TrustEvaluation`（decision、source、reason code、reason message）
- 候选来自 package directory 时的 `PackagePath`

发现阶段不会加载程序集。对不受信任的目录执行也是安全的。

## 2. 信任评估

实现 `IGraphEditorPluginTrustPolicy`，做出宿主自管的决策。

```csharp
public sealed class MyHostTrustPolicy : IGraphEditorPluginTrustPolicy
{
    public GraphEditorPluginTrustEvaluation Evaluate(GraphEditorPluginTrustPolicyContext context)
    {
        // context.Registration — 候选注册信息
        // context.Manifest — 可见 manifest
        // context.ProvenanceEvidence — 签名与包身份
        // context.PackagePath — 绝对本地包路径，存在时可用

        if (IsInHostAllowlist(context.Manifest, context.ProvenanceEvidence))
        {
            return new GraphEditorPluginTrustEvaluation(
                GraphEditorPluginTrustDecision.Allowed,
                GraphEditorPluginTrustEvaluationSource.HostPolicy,
                reasonCode: "myhost.allowlist.allowed",
                reasonMessage: $"通过宿主 allowlist 允许 '{context.Manifest.Id}'。");
        }

        return new GraphEditorPluginTrustEvaluation(
            GraphEditorPluginTrustDecision.Blocked,
            GraphEditorPluginTrustEvaluationSource.HostPolicy,
            reasonCode: "myhost.allowlist.blocked",
            reasonMessage: $"阻止 '{context.Manifest.Id}'：不在宿主 allowlist 中。");
    }
}
```

如果没有配置策略，运行时会返回 `GraphEditorPluginTrustEvaluation.ImplicitAllow()`。

信任决策在任何插件贡献代码执行前就已经评估完毕。

## 3. Allowlist

让 allowlist 保持宿主自管：

- 按 manifest id + fingerprint 持久化条目（fingerprint 由 id、display name、package id、version、signer fingerprint、signature status、target framework 的哈希构成）
- 支持 import/export 路径，使同一份 allowlist 能在不同环境间迁移
- 在 allowlist 条目旁保留 provenance snapshots，用于审计

`tools/AsterGraph.ConsumerSample.Avalonia` 里的 `ConsumerSamplePluginAllowlistTrustPolicy` 是 bounded sample path。复制它的结构，但把存储格式和持久化策略换成宿主自己的。

## 4. 注册

把批准的候选转成 `GraphEditorPluginRegistration`，并通过 `AsterGraphEditorOptions.PluginRegistrations` 传入。

直接实例：

```csharp
var registration = GraphEditorPluginRegistration.FromPlugin(
    new MyPlugin(),
    manifest,
    provenanceEvidence);
```

从程序集路径：

```csharp
var registration = GraphEditorPluginRegistration.FromAssemblyPath(
    assemblyPath,
    pluginTypeName: null,
    manifest,
    provenanceEvidence);
```

从包路径：

```csharp
var registration = GraphEditorPluginRegistration.FromPackagePath(
    packagePath,
    manifest,
    provenanceEvidence);
```

从 staged package（在 `StagePluginPackage` 之后）：

```csharp
var stageResult = AsterGraphEditorFactory.StagePluginPackage(
    new GraphEditorPluginPackageStageRequest(candidate));

if (stageResult.Registration is not null)
{
    // 使用 stageResult.Registration
}
```

组合编辑器：

```csharp
var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = compatibilityService,
    PluginTrustPolicy = myHostTrustPolicy,
    PluginRegistrations = approvedRegistrations,
});
```

## 5. 通信

编辑器运行后，通过 session 检查插件结果。

```csharp
var session = editor.Session;

// 已加载、已信任、被阻止的结果
var loadSnapshots = session.Queries.GetPluginLoadSnapshots();

// Session 诊断包含插件加载事件
var diagnostics = session.Diagnostics.GetRecentDiagnostics();
```

插件贡献的命令走同一条共享 command route：

```csharp
var descriptors = session.Queries.GetCommandDescriptors();
// 插件命令已包含在 descriptor 列表中
```

通过 canonical path 执行：

```csharp
session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(
    commandId, parameters));
```

## 重要边界

插件加载是进程内执行。AsterGraph 不提供沙箱或不受信任代码隔离。

对 public beta 宿主：

- 固定插件目录
- 优先使用 allowlist
- 在宿主策略里做签名或哈希校验
- 不要把插件加载当成隔离边界

## Proof Marker 预期

运行受防守的 hosted proof：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

期待：

- `CONSUMER_SAMPLE_TRUST_OK:True`
- `CONSUMER_SAMPLE_PLUGIN_OK:True`
- `CONSUMER_SAMPLE_OK:True`

## 相关文档

- [Plugin 与自定义节点 Recipe](./plugin-recipe.md)
- [插件信任契约 v1](./plugin-trust-contracts.md)
- [Host Integration](./host-integration.md)
- [Consumer Sample](./consumer-sample.md)
- [Host Recipe 阶梯](./host-recipe-ladder.md)
