# AsterGraph NuGet Readiness Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make `AsterGraph.Abstractions` / `AsterGraph.Core` / `AsterGraph.Editor` / `AsterGraph.Avalonia` consumable as versioned NuGet packages by external hosts such as `VibraVision.Integrated`.

**Architecture:** The work is divided into four layers: freeze package surface and dependency boundaries, add packaging metadata and target-framework support, provide README and local package-source guidance, then verify the full `pack -> restore -> build` consumer loop with a dedicated smoke host. The editor/runtime architecture itself stays intact; this plan only makes that architecture publishable and consumable.

**Tech Stack:** .NET 8/9 multi-targeting, Avalonia 11, CommunityToolkit.Mvvm, NuGet packaging, MSBuild, markdown README

---

## Implementation Note

This plan focuses on packaging/configuration/documentation verification rather than feature TDD.  
Where behavior is configuration-driven, the verification loop is `build -> pack -> restore -> build smoke` rather than red-green business tests.

### Task 1: Freeze package surface and host dependency policy

**Files:**
- Modify: `README.md`
- Modify: `src/AsterGraph.Abstractions/README.md`
- Modify: `src/AsterGraph.Avalonia/README.md`
- Create: `src/AsterGraph.Core/README.md`
- Create: `src/AsterGraph.Editor/README.md`
- Modify: `docs/plans/2026-03-23-astergraph-nuget-readiness-checklist.md`

**Step 1: Write the consumer-facing dependency policy**

Document and freeze:

- default direct host dependencies: `AsterGraph.Avalonia` + `AsterGraph.Abstractions`
- explicit optional dependencies: `AsterGraph.Core` and `AsterGraph.Editor`
- non-consumable package: `AsterGraph.Demo`

**Step 2: Align all README files with the package story**

Each README must clearly explain:

- what the package owns
- what it intentionally does not own
- who should reference it directly

**Step 3: Verify README coverage**

Run:

```powershell
rg -n "^#|^## |NuGet|AsterGraph.Abstractions|AsterGraph.Avalonia|AsterGraph.Core|AsterGraph.Editor|host|consumer" README.md src/AsterGraph.Abstractions/README.md src/AsterGraph.Core/README.md src/AsterGraph.Editor/README.md src/AsterGraph.Avalonia/README.md docs/plans/2026-03-23-astergraph-nuget-readiness-checklist.md
```

Expected:

- all package boundaries and host dependency recommendations are present and consistent

### Task 2: Add shared packaging metadata and make the library projects packable

**Files:**
- Modify: `Directory.Build.props`
- Modify: `src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj`
- Modify: `src/AsterGraph.Core/AsterGraph.Core.csproj`
- Modify: `src/AsterGraph.Editor/AsterGraph.Editor.csproj`
- Modify: `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`
- Modify: `src/AsterGraph.Demo/AsterGraph.Demo.csproj`

**Step 1: Move from “buildable libraries” to “packable libraries”**

Add or centralize:

- `PackageId`
- `Version`
- `Authors`
- `Description`
- `RepositoryUrl`
- `PackageReadmeFile`
- `PackageTags`
- `IsPackable`

Keep `AsterGraph.Demo` explicitly non-packable.

**Step 2: Generate XML docs and package-friendly outputs**

For the four library packages, add:

- `GenerateDocumentationFile`
- symbol/source package settings if appropriate

**Step 3: Verify project metadata presence**

Run:

```powershell
rg -n "PackageId|Version|Authors|Description|RepositoryUrl|PackageReadmeFile|PackageTags|IsPackable|GenerateDocumentationFile" Directory.Build.props src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj src/AsterGraph.Core/AsterGraph.Core.csproj src/AsterGraph.Editor/AsterGraph.Editor.csproj src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj src/AsterGraph.Demo/AsterGraph.Demo.csproj
```

Expected:

- the four library packages expose explicit packaging metadata
- demo is clearly excluded from package publication

### Task 3: Adjust target frameworks so the packages can be consumed by `net8.0` hosts

**Files:**
- Modify: `Directory.Build.props`
- Modify: `src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj`
- Modify: `src/AsterGraph.Core/AsterGraph.Core.csproj`
- Modify: `src/AsterGraph.Editor/AsterGraph.Editor.csproj`
- Modify: `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`
- Modify: `src/AsterGraph.Demo/AsterGraph.Demo.csproj`

**Step 1: Stop forcing a single global `TargetFramework` for every project**

Refactor the shared props so library targets can diverge from demo-host targets.

**Step 2: Multi-target the consumable packages**

Make the four packages target:

- `net8.0`
- `net9.0`

Keep `AsterGraph.Demo` on the most convenient target unless there is a concrete reason to multi-target it.

**Step 3: Verify framework configuration**

Run:

```powershell
rg -n "TargetFramework|TargetFrameworks" Directory.Build.props src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj src/AsterGraph.Core/AsterGraph.Core.csproj src/AsterGraph.Editor/AsterGraph.Editor.csproj src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj src/AsterGraph.Demo/AsterGraph.Demo.csproj
```

Expected:

- the four packages support `net8.0`
- demo remains intentionally scoped

### Task 4: Add local package-source guidance and a package smoke consumer

**Files:**
- Create: `NuGet.config.sample`
- Create: `tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj`
- Create: `tools/AsterGraph.PackageSmoke/Program.cs`
- Modify: `avalonia-node-map.sln`
- Modify: `README.md`

**Step 1: Create a sample NuGet config**

Document a local feed layout such as:

- `artifacts/packages`

and show how a downstream consumer would point at it.

**Step 2: Add a minimal smoke consumer**

Create a tiny host project that restores from the local feed and compiles against:

- `AsterGraph.Avalonia`
- `AsterGraph.Abstractions`

It only needs to prove package restore and compile-time consumption.

**Step 3: Verify the smoke project is solution-visible**

Run:

```powershell
dotnet build avalonia-node-map.sln
```

Expected:

- the smoke project restores and builds along with the rest of the solution

### Task 5: Verify the full pack -> restore -> build loop

**Files:**
- Modify: any files required for final polish after verification

**Step 1: Build the full solution**

Run:

```powershell
dotnet build avalonia-node-map.sln -c Release
```

Expected:

- PASS

**Step 2: Pack the four publishable packages**

Run:

```powershell
dotnet pack src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj -c Release -o artifacts/packages
dotnet pack src/AsterGraph.Core/AsterGraph.Core.csproj -c Release -o artifacts/packages
dotnet pack src/AsterGraph.Editor/AsterGraph.Editor.csproj -c Release -o artifacts/packages
dotnet pack src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj -c Release -o artifacts/packages
```

Expected:

- PASS
- four `.nupkg` files emitted

**Step 3: Restore and build the smoke consumer against the packed artifacts**

Run:

```powershell
dotnet restore tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj --configfile NuGet.config.sample
dotnet build tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj -c Release
```

Expected:

- PASS

**Step 4: Record residual gaps**

Document anything still deferred, especially:

- package signing
- CI publish automation
- SourceLink / symbols publication
- public API baseline automation

## 代码规范

- 不因打包工作破坏 `AsterGraph.Abstractions -> Core -> Editor -> Avalonia -> Demo` 现有依赖方向。
- 不允许把 Demo 特定逻辑、样例定义或宿主特定代码塞回可发布包。
- 默认宿主依赖边界必须稳定：`AsterGraph.Avalonia` + `AsterGraph.Abstractions`。

## 注释规范

- 新增 public 类型、public 方法、public 属性统一补清晰 XML 注释。
- 配置和打包相关注释只解释“为什么需要该元数据/多目标/包边界”，不写流水账。
- 如果某个 API 仍在快速变化，README 和注释都必须显式写清稳定性预期。

## README 规范

- README 必须与 `csproj` 中的包元数据一致。
- 根 README 必须写清包边界、宿主默认依赖建议、pack/restore 最小命令。
- 包相关文档必须包含 `mermaid`、验证命令和宿主消费说明。

## 下一阶段工作

- 为四个正式包补 CI 发布流程与版本治理自动化。
- 增加 symbols/SourceLink/公共 API 稳定性校验。
- 在真实外部宿主仓库中验证 NuGet 接入闭环。

