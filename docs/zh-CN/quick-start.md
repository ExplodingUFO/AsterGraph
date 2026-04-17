# AsterGraph 快速开始

这份文档只讲一件事：怎样从空白宿主最快跑起 AsterGraph。

## 1. 先选起始包

| 宿主目标 | 起始包 | 原因 |
| --- | --- | --- |
| 默认 Avalonia UI 宿主 | `AsterGraph.Avalonia` | 主 UI 入口，包含默认壳层和 view factory |
| 仅运行时 / 自定义 UI 宿主 | `AsterGraph.Editor` | 推荐的 session/runtime surface |
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

如果你只想先看到最小仅运行时路径，直接运行：

```powershell
dotnet run --project tools/AsterGraph.HelloWorld/AsterGraph.HelloWorld.csproj --nologo
```

`HelloWorld` 适合第一次上手；`HostSample` 适合证明推荐的仅运行时和默认 UI 两条路线都能成立。

## 4. 推荐接入路线

| 宿主需要什么 | 从哪里开始 | 第一个样例 |
| --- | --- | --- |
| 只要运行时 / 自定义 UI | `AsterGraphEditorFactory.CreateSession(...)` | `tools/AsterGraph.HelloWorld` |
| 默认 Avalonia UI | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `tools/AsterGraph.HostSample` |
| plugin trust / discovery | `AsterGraphEditorFactory.DiscoverPluginCandidates(...)` + `AsterGraphEditorOptions.PluginTrustPolicy` | [Host Integration](./host-integration.md) |
| automation | `IGraphEditorSession.Automation.Execute(...)` | [Host Integration](./host-integration.md) |
| retained 迁移 | `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }` | [Host Integration](./host-integration.md) |

新代码优先使用 runtime/session 路线或默认 Avalonia 路线；retained 路线只用于迁移。

## 5. 最小 Hosted-UI 组合

```csharp
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Hosting;

INodeCatalog catalog = CreateCatalog();
var document = GraphDocument.Empty;

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
- [Alpha Status](./alpha-status.md) = 当前范围、非目标、已知限制
- [Demo Guide](./demo-guide.md) = 完整展示宿主
- [`tools/AsterGraph.HostSample`](../../tools/AsterGraph.HostSample/) = 最小推荐路线验证样例
- [`tools/AsterGraph.PackageSmoke`](../../tools/AsterGraph.PackageSmoke/) = 打包消费验证
- [`tools/AsterGraph.ScaleSmoke`](../../tools/AsterGraph.ScaleSmoke/) = 规模、历史记录与状态连续性验证

## 8. 维护者与源码验证入口

如果你现在做的是仓库验证，而不是直接消费公开包：

- 维护流程与 lane 说明：[CONTRIBUTING.md](../../CONTRIBUTING.md)
- release sign-off 与手动 NuGet 发布流程：[Public Launch Checklist](./public-launch-checklist.md)
- 历史 tag 与包版本的关系：[Versioning](./versioning.md)
