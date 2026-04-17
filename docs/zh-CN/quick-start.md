# AsterGraph 快速开始

这是从空白宿主到 public alpha 接入的最短路径。

相关文档：

- [Alpha 状态](./alpha-status.md)
- [对外发布检查清单](./public-launch-checklist.md)
- [Host Integration](./host-integration.md)
- [State Contracts](./state-contracts.md)
- [Extension Contracts](./extension-contracts.md)
- [Demo Guide](./demo-guide.md)

## 包入口

| 宿主目标 | 起始包 | 原因 |
| --- | --- | --- |
| 默认 Avalonia UI 宿主 | `AsterGraph.Avalonia` | 主 UI 入口，包含默认壳层和 view factory |
| 契约优先集成 | `AsterGraph.Abstractions` | 稳定的 definitions / identifiers / provider contracts |
| runtime-first 自定义宿主 | `AsterGraph.Editor` | canonical session/runtime surface |

`AsterGraph.Demo` 只是 sample host，不是 SDK 边界的一部分。

## 包来源

当前三种来源形态：

- 仓库内本地 feed：`artifacts/packages`
- 可选的 GitHub Packages
- 维护者完成发布检查后触发的 tag-driven prerelease workflow

从源码验证当前分支时：

```powershell
copy NuGet.config.sample NuGet.config
```

## Canonical Adoption Path

| 宿主需要什么 | 从哪里开始 | 用什么验证 |
| --- | --- | --- |
| 只要 runtime / 自定义 UI | `AsterGraphEditorFactory.CreateSession(...)` | `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -p:UsePackedAsterGraphPackages=true --nologo` |
| 默认 Avalonia UI | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | 同一个 `HostSample` 命令 |
| plugin trust / discovery | `AsterGraphEditorFactory.DiscoverPluginCandidates(...)` + `AsterGraphEditorOptions.PluginTrustPolicy` | `eng/ci.ps1 -Lane contract` |
| automation | `IGraphEditorSession.Automation.Execute(...)` | `eng/ci.ps1 -Lane contract` |
| retained migration | `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }` | `eng/ci.ps1 -Lane contract` |

新集成优先使用 runtime/session 路线或 shipped Avalonia 路线；retained 路线只用于迁移。

## 最小 UI 组合

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

## 验证

推荐入口：

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

在切公开仓库可见性，或者推第一个公开 prerelease tag 之前，先按 [对外发布检查清单](./public-launch-checklist.md) 走一遍。

原始 proof tools：

```powershell
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -p:UsePackedAsterGraphPackages=true --nologo
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -f net10.0 -p:EnableNet10ConsumerProof=true -p:UsePackedAsterGraphPackages=true --nologo
dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj -p:UsePackedAsterGraphPackages=true --nologo
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj --nologo
```
