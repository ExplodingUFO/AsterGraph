# Quick Task 260327-rs7: 把最新的库（不含demo）作为私有 NuGet 包发布到 GitHub Packages，并补齐包文档和使用说明 - Context

**Gathered:** 2026-03-27
**Status:** Ready for planning

<domain>
## Task Boundary

把最新的库（不含 Demo）作为私有 NuGet 包发布到 GitHub Packages，并补齐包文档和使用说明。

</domain>

<decisions>
## Implementation Decisions

### 发布范围
- 这次涉及的库包括：`AsterGraph.Abstractions`、`AsterGraph.Core`、`AsterGraph.Editor`、`AsterGraph.Avalonia`。

### 包入口策略
- 对外采用“单主入口 + 协议分离”思路。
- 组件主入口包定位为 `AsterGraph.Avalonia`。
- 协议/抽象作为另外入口包保留，对应 `AsterGraph.Abstractions`。
- 其余库可以继续存在，但文档与使用说明需要清楚表达推荐入口，而不是让使用者自己猜包边界。

### 文档位置
- 主要把包文档和使用说明补到仓库文档中。

### 发布动作
- 这次目标是直接发布到 GitHub Packages，而不是只做本地配置。
- 前提是当前环境中已有可用的 GitHub 凭证/认证条件；如果缺失，需要转为让用户补认证。

### Claude's Discretion
- 仓库文档的具体落点与文件组织方式
- 是否补充包内 README / package metadata 作为辅助，但不改变“仓库文档为主”的决定
- GitHub Packages 的具体发布命令、pack 顺序、以及私有源说明写法

</decisions>

<specifics>
## Specific Ideas

- 用户希望“组件只有一个入口，协议是另外的入口”，所以文档要明确推荐：默认集成从 `AsterGraph.Avalonia` 开始，需要协议/抽象时再引用 `AsterGraph.Abstractions`。
- 发布内容不包含 Demo。
- 这次不是只做技术发布，还要把“怎么用”写清楚。

</specifics>

<canonical_refs>
## Canonical References

- `.planning/PROJECT.md` — 当前项目定位与发布目标
- `.planning/ROADMAP.md` — 已完成的 package/runtime/avalonia surface 相关阶段背景
- `Directory.Build.props` — 版本、包元数据、发布相关通用配置
- `README.md` — 仓库级说明入口（如存在）
- `src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj`
- `src/AsterGraph.Core/AsterGraph.Core.csproj`
- `src/AsterGraph.Editor/AsterGraph.Editor.csproj`
- `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`
- `NuGet.config.sample` — NuGet 源配置示例（如需参考）

</canonical_refs>
