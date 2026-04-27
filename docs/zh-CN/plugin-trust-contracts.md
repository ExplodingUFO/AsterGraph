# 插件信任契约 v1

这份文档只覆盖当前这版 thin contract：插件清单、provenance / signature evidence、host-owned trust policy、implicit allow、以及 blocked-before-activation 和 package staging 的边界。它不定义沙箱，也不定义未发布的后续 trust 模型。

## 插件清单字段

`GraphEditorPluginManifest` 是可在加载前读取的插件清单。它只负责把 host 需要看的稳定信息放到一处，不承担信任决策本身。

| 字段 | 含义 |
| --- | --- |
| `Id` | 稳定插件标识 |
| `DisplayName` | 宿主可读名称 |
| `Description` | 可选描述 |
| `Version` | 可选版本文本 |
| `Compatibility` | 兼容性摘要 |
| `CapabilitySummary` | 能力摘要 |
| `Provenance` | 来源和分发线索 |

`Compatibility` 里的 v1 字段是：

- `MinimumAsterGraphVersion`
- `MaximumAsterGraphVersion`
- `TargetFramework`
- `RuntimeSurface`

## Provenance 和 Signature Evidence

`GraphEditorPluginManifestProvenance` 只描述来源和分发线索：

- `SourceKind`
- `Source`
- `Publisher`
- `PackageId`
- `PackageVersion`

`GraphEditorPluginProvenanceEvidence` 只承载额外证据：

- `PackageIdentity`
- `Signature`

`Signature` 的 v1 证据面只包含这些稳定字段：

- `Status`
- `Kind`
- `Signer`
- `TimestampUtc`
- `TimestampAuthority`
- `ReasonCode`
- `ReasonMessage`

这里的规则很简单：manifest 和 provenance / signature evidence 都是给 host 看的输入，不是自动放行信号。

## Trust Policy 归属

插件信任策略由 host 自己拥有。`IGraphEditorPluginTrustPolicy` 只定义 host-supplied policy 的评估入口，AsterGraph 负责把候选、manifest 和 provenance evidence 交给 host。

host 在候选进入加载前做一次 `Evaluate(...)`，决策结果通过 `GraphEditorPluginTrustEvaluation` 返回。这里的 trust boundary 仍然是 host-owned。

## Trust Review Short Path

评估可信 in-process 插件时，按这条短路径走：

1. 用 [Plugin 与自定义节点 Recipe](./plugin-recipe.md) 编写或生成插件。
2. 用 `AsterGraph.PluginTool validate` 验证 `.dll`、`.nupkg` 或插件目录。
3. 复核 PluginTool 输出里的 manifest、compatibility、provenance、signature evidence 和 SHA-256 hash。
4. 在 activation 前应用宿主自有的 `IGraphEditorPluginTrustPolicy`。
5. 验证真实宿主 flow 时，把 [Consumer Sample](./consumer-sample.md) 当作受防守的 hosted trust hop。

PluginTool validation 是给 host policy 使用的证据。它不是 marketplace approval、sandbox decision，也不是自动加载授权。

## Host Policy Examples

这些是 host-owned policy 示例，不是 runtime fallback mode：

| Pattern | 典型场景 | Policy input |
| --- | --- | --- |
| Allow all local dev | 已知机器上的本地开发循环。 | 固定本地插件目录，以及明确的 local-dev reason。 |
| Allow by hash | 小团队共享已知插件二进制。 | PluginTool SHA-256 hash 必须匹配宿主 allowlist。 |
| Allow by manifest or publisher | 组织内部发布的插件。 | manifest id、package id/version、publisher metadata 和 signature evidence 必须匹配宿主策略。 |
| Block unknown source | 默认 prerelease 或企业姿态。 | 没有 allowlist、hash 或可接受签名匹配的候选，在 activation 前阻止。 |
| Enterprise fixed plugin directory | 受管桌面部署。 | 只从管理员控制的目录发现插件，并保留 allowlist import/export 记录用于审计。 |

## Implicit Allow 合同

如果 host 没有配置 trust policy，默认结果是 implicit allow。

- `GraphEditorPluginTrustEvaluation.ImplicitAllow(...)` 返回 `Allowed`
- `Source` 是 `ImplicitAllow`
- 默认 `ReasonCode` 是 `trust.policy.not-configured`
- 默认 `ReasonMessage` 是 `No plugin trust policy was configured.`

这表示“没有 host policy 时允许继续”，而不是“清单或签名本身自动授信”。

## Blocked Before Activation

被 trust policy 拒绝的候选必须在任何 contribution code 执行前停下。

- `GraphEditorPluginTrustDecision.Blocked` 表示插件在 activation 之前被拦截
- blocked candidate 仍然可以保留 manifest、provenance 和 reason 信息，供 host inspection 或 diagnostics 使用
- trust blocked 不应该进入插件激活路径

## Package Staging

package registration 不能直接跳过 staging。

- package registration 必须先经过 `AsterGraphEditorFactory.StagePluginPackage(...)`
- staging 结果会携带 `GraphEditorPluginStageSnapshot`
- 如果 trust policy 或 signature evidence 不通过，staging 会返回 refused / blocked 结果
- package registration 只有在 staging 之后，才会进入后续加载路径

这条规则的目的，是把“候选发现”“包暂存”“插件激活”分成三步，而不是把 package 直接当成可执行加载源。

## Non-goals

v1 不做这些事：

- 不提供 sandbox
- 不提供不受信任代码隔离
- 不把 provenance 或 signature evidence 当成自动授权
- 不定义额外的 adapter-specific trust 规则
- 不承诺未来的 remote trust、网络签名链或其它未发布模型

## 相关文档

- [Host Integration](./host-integration.md)
- [Plugin 与自定义节点 Recipe](./plugin-recipe.md)
- [Consumer Sample](./consumer-sample.md)
- [Extension Contracts](./extension-contracts.md)
