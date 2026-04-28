# AsterGraph 快速开始

这份文档只讲一件事：怎样从空白宿主最快跑起 AsterGraph。

首次接入默认从 Avalonia 路线开始。
`WPF` 只作为同一条 canonical 路线上的 adapter-2 兼容性验证，不是第二条接入路线，也不是 parity 承诺。
关于冻结的支持边界和面向 `v1.0.0` 的升级指引，见 [稳定化支持矩阵](./stabilization-support-matrix.md)。
如果你要端到端评估当前 public beta，就把 [公开 Beta 评估路径](./evaluation-path.md) 当成从第一次安装到真实宿主 proof 的 hosted route ladder。
如果你要看 plugin trust-policy 和本地证据，就把 [插件信任契约 v1](./plugin-trust-contracts.md) 和 [Beta Support Bundle](./support-bundle.md) 一起放在这条受防守的 hosted 路线上。

## 30 秒 / 5 分钟 / 30 分钟路径

| 时间 | 路径 | 什么时候停 |
| --- | --- | --- |
| 30 秒 | 运行 `src/AsterGraph.Demo -- --scenario ai-pipeline`，或先看 README 场景图。 | 已经能判断应该选哪条接入路线，并且 proof mode 可以输出 `DEMO_SCENARIO_PRESETS_OK:True`。 |
| 5 分钟 | 生成 `dotnet new astergraph-avalonia`，运行 starter，再验证 `ConsumerSample.Avalonia -- --proof --support-bundle <support-bundle-path>`。 | 已经拿到 `FIVE_MINUTE_ONBOARDING_OK:True`、`ONBOARDING_CONFIGURATION_OK:True`、`AUTHORING_FLOW_HANDOFF_OK:True`、`EXPERIENCE_SCOPE_BOUNDARY_OK:True`、`AUTHORING_DEPTH_HANDOFF_OK:True`、`AUTHORING_DEPTH_SCOPE_BOUNDARY_OK:True` 和 `V058_MILESTONE_PROOF_OK:True`。 |
| 30 分钟 | 继续按下面的包、路线、参数、插件和 support-bundle 小节走完。 | 已经知道自己的宿主该复制 hosted UI、runtime-only、plugin，还是 retained migration 指引。 |

最快的项目自有入口现在是模板路线：

```powershell
dotnet new install ./templates
dotnet new astergraph-avalonia -n MyGraphHost
dotnet new astergraph-plugin -n MyGraphPlugin --PluginId my.graph.plugin
```

## 1. 先选起始包

| 宿主目标 | 起始包 | 原因 |
| --- | --- | --- |
| hosted starter 脚手架 | `AsterGraph.Starter.Avalonia` | 最小端到端 Avalonia 脚手架；cookbook 里的第一个 hosted 跳板 |
| 原生生成宿主 | `dotnet new astergraph-avalonia` | 由你的项目拥有的跨平台 Avalonia 桌面宿主 |
| 可信插件 starter | `dotnet new astergraph-plugin` | 面向 in-process 可信扩展的最小插件作者项目 |
| 默认 Avalonia UI 宿主 | `AsterGraph.Avalonia` | 主 UI 入口，包含默认壳层和 view factory |
| 仅运行时 / 自定义 UI 宿主 | `AsterGraph.Editor` | 面向自定义 UI 或原生壳层的 canonical session/runtime surface |
| 契约优先集成 | `AsterGraph.Abstractions` | 稳定的标识符、定义和 provider 契约 |

只有当宿主还需要直接处理 `GraphDocument`、序列化或兼容性 API 时，再额外加 `AsterGraph.Core`。
把 `AsterGraph.Starter.Avalonia` 当作 starter recipe。保留/复制 `AsterGraphEditorFactory.Create(...)`、`AsterGraphAvaloniaViewFactory.Create(...)`、`AsterGraphEditorOptions`，以及 document/catalog/editor/view 的组合流程。替换宿主自己的 top-level window 和它的 title/size，并随着宿主成长逐步替换 sample graph/catalog definitions。复制宿主自管 seam，不复制样例自有展示层。下一步 hosted step 是 `AsterGraph.HelloWorld.Avalonia`。升级到 `AsterGraph.ConsumerSample.Avalonia` 时，继续把 action projection、trust workflow 和选中节点参数读写 seam 放在宿主里。

复制这个 starter scaffold：

- `AsterGraphEditorFactory.Create(...)`
- `AsterGraphAvaloniaViewFactory.Create(...)`
- `AsterGraphEditorOptions`
- document/catalog/editor/view 的组合流程

在你的宿主里替换：

- 宿主自己的 top-level window 和它的 title/size
- 随着宿主成长逐步替换 sample graph/catalog definitions

## 宿主自管参数与元数据复制图

按每个 bounded source 复制它负责的那一部分：

- 从 `AsterGraph.Starter.Avalonia` 复制：保留 `AsterGraphEditorFactory.Create(...)`、`AsterGraphAvaloniaViewFactory.Create(...)`、`AsterGraphEditorOptions` 和 document/catalog/editor/view 的组合流程，然后替换 top-level window、它的 title/size，以及随着宿主成长逐步替换 sample graph/catalog definitions。
- 从 `AsterGraph.ConsumerSample.Avalonia` 复制：继续把 action projection、trust workflow、选中节点参数读写 seam 和 snippet catalog 插入 seam 放在宿主里，但把样例自有的展示、snippet id 和 proof labels 保持在本地。
- 从 [Host Integration](./host-integration.md) 复制：用 route matrix 和 canonical session/runtime 选择来决定哪一层宿主 surface 负责这条 seam。
- 从 [Authoring Inspector Recipe](./authoring-inspector-recipe.md) 复制：用 definition-driven 的元数据词汇（`defaultValue`、`isAdvanced`、`helpText`、`placeholderText`、`constraints.IsReadOnly`）来完成宿主自管参数与元数据工作。

Consumer Sample 证明 seam 分工；Authoring Inspector Recipe 承载元数据词汇。
## 2. 从 NuGet 安装

公开 beta 包已经发到 nuget.org，所以默认的 `dotnet restore` 加 `--prerelease` 就够了。

```powershell
# 使用默认 Avalonia UI
dotnet add package AsterGraph.Avalonia --prerelease

# 只要 runtime / 自定义 UI
dotnet add package AsterGraph.Editor --prerelease

# 节点定义与 provider 契约
dotnet add package AsterGraph.Abstractions --prerelease
```

`AsterGraph.Demo` 只是 sample，不属于公开支持的 SDK 边界。

## 3. 最快的第一跑

如果你想先看第一个 hosted 入口，直接运行：

```powershell
dotnet run --project tools/AsterGraph.Starter.Avalonia/AsterGraph.Starter.Avalonia.csproj --nologo
```

如果你想生成项目自有的原生 Avalonia 脚手架，运行：

```powershell
dotnet new install ./templates
dotnet new astergraph-avalonia -n MyGraphHost
dotnet run --project MyGraphHost/MyGraphHost.csproj
```

如果你只想先看到最小仅运行时路径，直接运行：

```powershell
dotnet run --project tools/AsterGraph.HelloWorld/AsterGraph.HelloWorld.csproj --nologo
```

如果你想先跑最小默认 UI 路线，执行：

```powershell
dotnet run --project tools/AsterGraph.HelloWorld.Avalonia/AsterGraph.HelloWorld.Avalonia.csproj --nologo
```

如果你想先看一个更真实的宿主集成，带宿主动作、选中节点参数读写 seam 和一个可信插件，可以运行：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo
```

Proof handoff 时先跑 `AsterGraph.ConsumerSample.Avalonia -- --proof`；`HostSample` 只作为这条 ladder 之后的 proof harness 使用。

如果你要评估插件能力，受防守的 hosted trust hop 是 `AsterGraph.ConsumerSample.Avalonia`。先看 [插件信任契约 v1](./plugin-trust-contracts.md) 和 [Plugin 与自定义节点 Recipe](./plugin-recipe.md)，再把这条路线当作已经看完。

如果你在写插件，先生成并验证 starter 插件：

```powershell
dotnet new astergraph-plugin -n MyGraphPlugin --PluginId my.graph.plugin
dotnet build MyGraphPlugin/MyGraphPlugin.csproj
dotnet run --project tools/AsterGraph.PluginTool -- validate MyGraphPlugin/bin/Debug/net8.0/MyGraphPlugin.dll
```

`Starter.Avalonia` 适合第一个 hosted 入口和最小端到端 Avalonia 脚手架；`HelloWorld` 适合最简单的 runtime-only 第一跑；`HelloWorld.Avalonia` 适合在 starter 之后看的最小默认 UI 第一跑；`ConsumerSample.Avalonia` 适合在跳到 `Demo` 之前先看一个真实宿主；`HostSample` 只适合做推荐路线验证，不是上手入口。

这条 hosted route ladder 是 `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`。

这个样例自己的 README 是 [`tools/AsterGraph.ConsumerSample.Avalonia/README.md`](../../tools/AsterGraph.ConsumerSample.Avalonia/README.md)。

### 五分钟 hosted 路径

当你想复制一个 Avalonia 宿主，而不是直接研究完整 showcase 时，按这条路径走：

1. 安装 `AsterGraph.Avalonia --prerelease`，或者从源码运行 `tools/AsterGraph.Starter.Avalonia`。
2. 默认够用时，通过 `AsterGraphHostBuilder.Create().UseDocument(document).UseCatalog(catalog).UseDefaultCompatibility().BuildAvaloniaView()` 复制 starter 组合；当每个服务都需要显式接线时，再降到 `AsterGraphEditorFactory.Create(...)`、`AsterGraphAvaloniaViewFactory.Create(...)`、`AsterGraphEditorOptions`，以及 document/catalog/editor/view 流程。
3. 添加第一个自定义节点定义：把 starter 的 sample definition 换成你自己的 `NodeDefinition` id、标题、端口和参数定义。
4. 运行 `tools/AsterGraph.ConsumerSample.Avalonia`，用 hosted action rail 验证图保存/加载、选中节点参数编辑和可信插件路径。
5. 运行 `AsterGraph.ConsumerSample.Avalonia -- --proof --support-bundle <support-bundle-path>`，期待 `CONSUMER_SAMPLE_SCENARIO_GRAPH_OK:True`、`CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK:True`、`CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True`、`GRAPH_SNIPPET_CATALOG_OK:True`、`GRAPH_SNIPPET_INSERT_OK:True`、`FRAGMENT_LIBRARY_SEARCH_OK:True`、`FRAGMENT_LIBRARY_PREVIEW_OK:True`、`FRAGMENT_LIBRARY_RECENTS_FAVORITES_OK:True`、`FRAGMENT_LIBRARY_SCOPE_BOUNDARY_OK:True`、`FIVE_MINUTE_ONBOARDING_OK:True`、`ONBOARDING_CONFIGURATION_OK:True`、`AUTHORING_FLOW_PROOF_OK:True`、`AUTHORING_FLOW_HANDOFF_OK:True`、`AUTHORING_FLOW_SCOPE_BOUNDARY_OK:True`、`EXPERIENCE_POLISH_HANDOFF_OK:True`、`FEATURE_ENHANCEMENT_PROOF_OK:True`、`EXPERIENCE_SCOPE_BOUNDARY_OK:True`、`AUTHORING_DEPTH_HANDOFF_OK:True`、`AUTHORING_DEPTH_SCOPE_BOUNDARY_OK:True` 和 `V058_MILESTONE_PROOF_OK:True`。

Runtime feedback proof 仍然是宿主自管：ConsumerSample 应继续保持 `RUNTIME_DEBUG_PANEL_INTERACTION_OK:True`、`RUNTIME_LOG_LOCATE_OK:True` 和 `RUNTIME_LOG_EXPORT_OK:True`；Demo 应继续保持 `AI_PIPELINE_MOCK_RUNNER_POLISH_OK:True`、`AI_PIPELINE_PAYLOAD_PREVIEW_OK:True` 和 `AI_PIPELINE_ERROR_DEBUG_EVIDENCE_OK:True`。这些都不是 execution-engine、workflow scripting UI、marketplace、sandbox、WPF parity 或 GA 声明。

Graph search proof 也保持 hosted、snapshot-driven：期待 `GRAPH_SEARCH_LOCATE_OK:True`、`GRAPH_SEARCH_SCOPE_FILTER_OK:True` 和 `GRAPH_SEARCH_VIEWPORT_FOCUS_OK:True`，不引入后台图索引服务或 command macro engine。
Command palette proof 继续走同一条 shared command/session route：期待 `COMMAND_PALETTE_GROUPING_OK:True`、`COMMAND_PALETTE_DISABLED_REASON_OK:True`、`COMMAND_PALETTE_RECENT_ACTIONS_OK:True`、`COMMAND_PROJECTION_UNIFIED_OK:True`、`COMMAND_PALETTE_OK:True`、`TOOLBAR_DESCRIPTOR_OK:True`、`CONTEXT_MENU_DESCRIPTOR_OK:True` 和 `COMMAND_DISABLED_REASON_OK:True`，不新增 macro 或 scripting。
Navigation proof 继续作为宿主自管层叠在已有 selection、scope 和 viewport command 上：期待 `NAVIGATION_HISTORY_OK:True`、`SCOPE_BREADCRUMB_NAVIGATION_OK:True`、`FOCUS_RESTORE_OK:True`、`NAVIGATION_PRODUCTIVITY_PROOF_OK:True`、`NAVIGATION_PRODUCTIVITY_HANDOFF_OK:True` 和 `NAVIGATION_SCOPE_BOUNDARY_OK:True`，不新增 runtime navigation API。

release lane 的 template smoke 会验证 `astergraph-avalonia` 和 `astergraph-plugin` 能生成可 build 的 `net8.0` 项目，并且生成的插件能通过 `AsterGraph.PluginTool validate`。

从 `Starter.Avalonia` 复制组合方式，从 `HelloWorld` 看 runtime-only 形状，从 `HelloWorld.Avalonia` 看最小 hosted UI，从 `ConsumerSample.Avalonia` 复制真实宿主动作/参数/插件/support proof，从 `Demo` 查看完整能力 showcase。

## 4. 推荐接入路线

默认新接入优先走 Avalonia hosted 路线；只有需要原生自定义 UI 时再走 runtime/session 自定义路线。

| 宿主需要什么 | 从哪里开始 | 第一个样例 |
| --- | --- | --- |
| hosted starter 脚手架 | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `tools/AsterGraph.Starter.Avalonia` |
| 只要运行时 / 自定义 UI | `AsterGraphEditorFactory.CreateSession(...)` | `tools/AsterGraph.HelloWorld` |
| 默认 Avalonia UI | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `tools/AsterGraph.HelloWorld.Avalonia` |
| plugin trust / discovery | `AsterGraphEditorFactory.DiscoverPluginCandidates(...)` + `AsterGraphEditorOptions.PluginTrustPolicy` | [`tools/AsterGraph.ConsumerSample.Avalonia`](../../tools/AsterGraph.ConsumerSample.Avalonia/) |
| automation | `IGraphEditorSession.Automation.Execute(...)` | [Host Integration](./host-integration.md) |
| retained 兼容桥接 | `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }` | [Host Integration](./host-integration.md) |

新代码优先使用 runtime/session 路线或默认 Avalonia 路线；retained 路线只用于迁移。
只有在现有宿主要分批迁移时才选 retained。仅把 retained recipe 当成现有宿主可复制的迁移辅助。需要这座桥接时，先看 [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)；否则优先从 `CreateSession(...)` 或 `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` 开始。
如果你在选择 retained，就停在这份 recipe，不要自己把多份文档拼起来；它是现有 `GraphEditorViewModel` / `GraphEditorView` 宿主唯一的 bounded retained recipe 集合。
如果宿主管的是自己的 UI，那么 runtime/session 路线就是 canonical 的原生路径；`Editor.Session` 仍然负责宿主动作、诊断、automation 和 proof 逻辑。
默认 onboarding 继续走 Avalonia-first。
Quick Start 当前仍然是 Avalonia-first。当前公开 beta 线会在同一条 canonical 路线上验证 `WPF` 作为 adapter 2；这不是 `WPF` 与 `Avalonia` 已经 parity 的承诺，也不要把它当成第二条上手路径，这部分合同见 [Adapter Capability Matrix](./adapter-capability-matrix.md)。

WPF 仅是 adapter-2 portability validation：`partial` / `fallback` 都指向同一条 canonical session/query 路线 + host 自有投影，不是 retained MVVM，也不是 WPF 专属 runtime API。

## 5. Hosted Builder Cookbook

常见 hosted 路线先用薄 builder：

```csharp
using AsterGraph.Avalonia.Hosting;

var view = AsterGraphHostBuilder
    .Create()
    .UseDocument(document)
    .UseCatalog(catalog)
    .UseDefaultCompatibility()
    .BuildAvaloniaView();
```

需要显式服务接线时，再降到 canonical factories：

```csharp
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Hosting;

INodeCatalog catalog = CreateCatalog();
var document = CreateDocument();

var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = new DefaultPortCompatibilityService(),
});

var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
});
```

`AsterGraphHostBuilder` 是叠在同一套 editor/session 和 Avalonia view factories 上的 thin hosted helper，不是第二套 runtime model。

## 6. 插件信任边界

插件加载当前是进程内执行。宿主可以发现候选、做 allow/block 信任策略、检查加载结果，但 AsterGraph 目前不提供沙箱或不受信任代码隔离。

## 7. 超过“第一跑”之后看哪里

- [Host Integration](./host-integration.md) = 包边界、路线矩阵、迁移说明
- [稳定化支持矩阵](./stabilization-support-matrix.md) = 冻结的支持边界和面向 `v1.0.0` 的升级指引
- [Architecture](./architecture.md) = editor-kernel / scene-interaction / adapter split 与公开 stability level
- [Feature Catalog](./feature-catalog.md) = 治理后的 feature record、pack 分组、adapter projection 状态、proof marker 和 performance budget
- [Adapter Capability Matrix](./adapter-capability-matrix.md) = 锁定的 `WPF` adapter-2 合同，以及 `supported` / `partial` / `fallback`
- [Consumer Sample](./consumer-sample.md) = 介于 HelloWorld 和 Demo 之间的真实宿主样例
- [Alpha Status](./alpha-status.md) = 当前范围、非目标、已知限制
- [Demo Guide](./demo-guide.md) = 完整展示宿主
- [HostSample](../../tools/AsterGraph.HostSample/) = 这条 ladder 之后的路线验证用 proof harness，不是上手入口
- [ScaleSmoke 基线](./scale-baseline.md) = 公开的规模分层与防回归红线
- [Authoring Inspector Recipe](./authoring-inspector-recipe.md) = definition-driven 参数、分组、校验与 shipped inspector editor
- [Plugin 与自定义节点 Recipe](./plugin-recipe.md) = 最小可复制 plugin/custom-node 路线
- [Plugin Host Recipe](./plugin-host-recipe.md) = 宿主视角的插件发现、信任与注册
- [Custom Node Host Recipe](./custom-node-host-recipe.md) = 自定义节点/端口/边注册与样式
- [Host Recipe 阶梯](./host-recipe-ladder.md) = 统一“从这里复制”阶梯
- [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md) = 老宿主的渐进迁移指南

## 8. 维护者与源码验证入口

如果你现在做的是仓库验证，而不是直接消费公开包：

- 维护流程与 lane 说明：[CONTRIBUTING.md](../../CONTRIBUTING.md)
- release sign-off 与手动 NuGet 发布流程：[Public Launch Checklist](./public-launch-checklist.md)
- 历史 tag 与包版本的关系：[Versioning](./versioning.md)
- proof harness：[`tools/AsterGraph.HostSample`](../../tools/AsterGraph.HostSample/)、[`tools/AsterGraph.PackageSmoke`](../../tools/AsterGraph.PackageSmoke/)、[`tools/AsterGraph.ScaleSmoke`](../../tools/AsterGraph.ScaleSmoke/)
- 中等样例：[`tools/AsterGraph.ConsumerSample.Avalonia`](../../tools/AsterGraph.ConsumerSample.Avalonia/)

