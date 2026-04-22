# AsterGraph 快速开始

这份文档只讲一件事：怎样从空白宿主最快跑起 AsterGraph。

## 1. 先选起始包

| 宿主目标 | 起始包 | 原因 |
| --- | --- | --- |
| hosted starter 脚手架 | `AsterGraph.Starter.Avalonia` | 最小端到端 Avalonia 脚手架；cookbook 里的第一个 hosted 跳板 |
| 默认 Avalonia UI 宿主 | `AsterGraph.Avalonia` | 主 UI 入口，包含默认壳层和 view factory |
| 仅运行时 / 自定义 UI 宿主 | `AsterGraph.Editor` | 面向自定义 UI 或原生壳层的 canonical session/runtime surface |
| 契约优先集成 | `AsterGraph.Abstractions` | 稳定的标识符、定义和 provider 契约 |

只有当宿主还需要直接处理 `GraphDocument`、序列化或兼容性 API 时，再额外加 `AsterGraph.Core`。

## 2. 从 NuGet 安装

公开 alpha 包已经发到 nuget.org，所以默认的 `dotnet restore` 加 `--prerelease` 就够了。

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

如果你只想先看到最小仅运行时路径，直接运行：

```powershell
dotnet run --project tools/AsterGraph.HelloWorld/AsterGraph.HelloWorld.csproj --nologo
```

如果你想先跑最小默认 UI 路线，执行：

```powershell
dotnet run --project tools/AsterGraph.HelloWorld.Avalonia/AsterGraph.HelloWorld.Avalonia.csproj --nologo
```

如果你想先看一个更真实的宿主集成，带宿主动作、参数编辑和一个可信插件，可以运行：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo
```

`Starter.Avalonia` 适合第一个 hosted 入口和最小端到端 Avalonia 脚手架；`HelloWorld` 适合最简单的 runtime-only 第一跑；`HelloWorld.Avalonia` 适合在 starter 之后看的最小默认 UI 第一跑；`ConsumerSample.Avalonia` 适合在跳到 `Demo` 之前先看一个真实宿主；`HostSample` 只适合做推荐路线验证，不是上手入口。

这个样例自己的 README 是 [`tools/AsterGraph.ConsumerSample.Avalonia/README.md`](../../tools/AsterGraph.ConsumerSample.Avalonia/README.md)。

## 4. 推荐接入路线

默认新接入优先走 Avalonia hosted 路线；只有需要原生自定义 UI 时再走 runtime/session 自定义路线。

| 宿主需要什么 | 从哪里开始 | 第一个样例 |
| --- | --- | --- |
| hosted starter 脚手架 | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `tools/AsterGraph.Starter.Avalonia` |
| 只要运行时 / 自定义 UI | `AsterGraphEditorFactory.CreateSession(...)` | `tools/AsterGraph.HelloWorld` |
| 默认 Avalonia UI | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `tools/AsterGraph.HelloWorld.Avalonia` |
| plugin trust / discovery | `AsterGraphEditorFactory.DiscoverPluginCandidates(...)` + `AsterGraphEditorOptions.PluginTrustPolicy` | [`tools/AsterGraph.ConsumerSample.Avalonia`](../../tools/AsterGraph.ConsumerSample.Avalonia/) |
| automation | `IGraphEditorSession.Automation.Execute(...)` | [Host Integration](./host-integration.md) |
| retained 迁移 | `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }` | [Host Integration](./host-integration.md) |

新代码优先使用 runtime/session 路线或默认 Avalonia 路线；retained 路线只用于迁移。
如果宿主管的是自己的 UI，那么 runtime/session 路线就是 canonical 的原生路径；`Editor.Session` 仍然负责宿主动作、诊断、automation 和 proof 逻辑。
默认 onboarding 继续走 Avalonia-first。
Quick Start 当前仍然是 Avalonia-first。`v0.9.0-beta` 会在同一条 canonical 路线上验证 `WPF` 作为 adapter 2；这部分合同见 [Adapter Capability Matrix](./adapter-capability-matrix.md)，不要把它当成第二条上手路径。

WPF 仅是 adapter-2 portability validation：`partial` / `fallback` 都指向同一条 canonical session/query 路线 + host 自有投影，不是 retained MVVM，也不是 WPF 专属 runtime API。

## 5. 最小 Hosted-UI 组合

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

## 6. 插件信任边界

插件加载当前是进程内执行。宿主可以发现候选、做 allow/block 信任策略、检查加载结果，但 AsterGraph 目前不提供沙箱或不受信任代码隔离。

## 7. 超过“第一跑”之后看哪里

- [Host Integration](./host-integration.md) = 包边界、路线矩阵、迁移说明
- [Architecture](./architecture.md) = editor-kernel / scene-interaction / adapter split 与公开 stability level
- [Adapter Capability Matrix](./adapter-capability-matrix.md) = 锁定的 `WPF` adapter-2 合同，以及 `supported` / `partial` / `fallback`
- [Consumer Sample](./consumer-sample.md) = 介于 HelloWorld 和 Demo 之间的真实宿主样例
- [Alpha Status](./alpha-status.md) = 当前范围、非目标、已知限制
- [Demo Guide](./demo-guide.md) = 完整展示宿主
- [HostSample](../../tools/AsterGraph.HostSample/) = 路线验证用 proof harness，不是上手入口
- [ScaleSmoke 基线](./scale-baseline.md) = 公开的规模分层与防回归红线
- [Authoring Inspector Recipe](./authoring-inspector-recipe.md) = definition-driven 参数、分组、校验与 shipped inspector editor
- [Plugin 与自定义节点 Recipe](./plugin-recipe.md) = 最小可复制 plugin/custom-node 路线
- [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md) = 老宿主的渐进迁移指南

## 8. 维护者与源码验证入口

如果你现在做的是仓库验证，而不是直接消费公开包：

- 维护流程与 lane 说明：[CONTRIBUTING.md](../../CONTRIBUTING.md)
- release sign-off 与手动 NuGet 发布流程：[Public Launch Checklist](./public-launch-checklist.md)
- 历史 tag 与包版本的关系：[Versioning](./versioning.md)
- proof harness：[`tools/AsterGraph.HostSample`](../../tools/AsterGraph.HostSample/)、[`tools/AsterGraph.PackageSmoke`](../../tools/AsterGraph.PackageSmoke/)、[`tools/AsterGraph.ScaleSmoke`](../../tools/AsterGraph.ScaleSmoke/)
- 中等样例：[`tools/AsterGraph.ConsumerSample.Avalonia`](../../tools/AsterGraph.ConsumerSample.Avalonia/)
