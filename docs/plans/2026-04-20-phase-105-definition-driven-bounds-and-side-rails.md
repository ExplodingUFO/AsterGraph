# Phase 105 Definition-Driven Bounds And Side Rails Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make node default sizing and side-rail disclosure content-driven so baseline nodes fully cover required ports and status chrome while richer content appears only after measured width/height breakpoints.

**Architecture:** Keep semantic authoring facts close to node definitions, compute sizes and disclosure through editor/runtime planning types, and keep Avalonia presenters as renderers that consume a resolved surface plan. Use TDD for every behavior change and keep commits scoped to one task at a time.

**Tech Stack:** C# 13, .NET 8/9, Avalonia, xUnit, existing AsterGraph editor/runtime projection pipeline

---

### Task 1: Lock down adaptive sizing expectations in tests

**Files:**
- Modify: `tests/AsterGraph.Editor.Tests/GraphEditorNodeSurfaceContractsTests.cs`
- Modify: `tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs`
- Test: `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`

**Step 1: Write the failing test**

Add contract tests that prove:
- default node size covers required inputs + outputs + status chrome
- increasing height reveals defaultable input rows
- increasing width reveals summary content before inline editor content
- connected parameter endpoints suppress inline editors

Add standalone presenter tests that assert the visible port/editor counts change at the correct measured stages.

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorNodeSurfaceContractsTests|FullyQualifiedName~NodeCanvasStandaloneTests" -v minimal --no-restore`

Expected: FAIL because the current runtime still relies on simpler size/tier behavior.

**Step 3: Write minimal implementation**

Do not change the presenter yet. Only add the failing tests and any minimal shared test fixtures needed to describe required vs defaultable inputs and inline-editor capability.

**Step 4: Run test to verify it passes/fails as expected**

Run the same command and confirm the new tests are the ones failing.

**Step 5: Commit**

```bash
git add tests/AsterGraph.Editor.Tests/GraphEditorNodeSurfaceContractsTests.cs tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs
git commit -m "test(105): cover adaptive node surface disclosure"
```

### Task 2: Introduce semantic layout facts and planning types

**Files:**
- Modify: `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`
- Modify: `src/AsterGraph.Abstractions/Definitions/NodeParameterDefinition.cs`
- Create: `src/AsterGraph.Abstractions/Definitions/NodeSurfaceLayoutHints.cs`
- Create: `src/AsterGraph.Abstractions/Definitions/NodeSurfaceAuthoringRole.cs`
- Create: `src/AsterGraph.Abstractions/Definitions/NodeInlineEditorCapability.cs`
- Create: `src/AsterGraph.Editor/Runtime/Internal/GraphEditorNodeSurfaceContentPlan.cs`
- Create: `src/AsterGraph.Editor/Runtime/Internal/GraphEditorNodeSurfaceMeasurement.cs`
- Create: `src/AsterGraph.Editor/Runtime/Internal/GraphEditorNodeSurfacePlanner.cs`
- Create: `src/AsterGraph.Editor/Runtime/Internal/GraphEditorNodeSurfaceMeasurer.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorNodeSurfaceContractsTests.cs`

**Step 1: Write the failing test**

Add focused unit tests for the new planner/measurer:
- baseline plan includes required inputs and outputs
- defaultable inputs are excluded from baseline but present in expandable content groups
- width/height breakpoints are derived from content groups rather than hardcoded constants

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorNodeSurfaceContractsTests" -v minimal --no-restore`

Expected: FAIL because the planner/measurer types do not exist yet.

**Step 3: Write minimal implementation**

Add the semantic enums/records and implement planner/measurer types with the smallest API that satisfies the tests. Prefer internal types in editor/runtime unless a true definition seam must be public.

**Step 4: Run test to verify it passes**

Run the same command and confirm the planner/measurer tests pass.

**Step 5: Commit**

```bash
git add src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs src/AsterGraph.Abstractions/Definitions/NodeParameterDefinition.cs src/AsterGraph.Abstractions/Definitions/NodeSurfaceLayoutHints.cs src/AsterGraph.Abstractions/Definitions/NodeSurfaceAuthoringRole.cs src/AsterGraph.Abstractions/Definitions/NodeInlineEditorCapability.cs src/AsterGraph.Editor/Runtime/Internal/GraphEditorNodeSurfaceContentPlan.cs src/AsterGraph.Editor/Runtime/Internal/GraphEditorNodeSurfaceMeasurement.cs src/AsterGraph.Editor/Runtime/Internal/GraphEditorNodeSurfacePlanner.cs src/AsterGraph.Editor/Runtime/Internal/GraphEditorNodeSurfaceMeasurer.cs tests/AsterGraph.Editor.Tests/GraphEditorNodeSurfaceContractsTests.cs
git commit -m "feat(105): add semantic node surface planner"
```

### Task 3: Route node projection through measured disclosure

**Files:**
- Modify: `src/AsterGraph.Editor/Runtime/Internal/GraphEditorNodeSurfaceTierResolver.cs`
- Modify: `src/AsterGraph.Editor/Runtime/GraphEditorNodeSurfaceSnapshot.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/Facade/GraphEditorViewModel.SurfaceTierProjection.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorNodeSurfaceContractsTests.cs`

**Step 1: Write the failing test**

Add tests that prove runtime projection now:
- computes a baseline default size from measured content
- resolves richer disclosure only after width/height breakpoints
- keeps connected parameter endpoints from advertising competing inline editors

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorNodeSurfaceContractsTests" -v minimal --no-restore`

Expected: FAIL because projection still exposes the old simpler tier logic.

**Step 3: Write minimal implementation**

Update the tier/disclosure resolver to consume the planner/measurer output. Extend snapshots only as much as the renderer and tests need.

**Step 4: Run test to verify it passes**

Run the same command and confirm the projection tests pass.

**Step 5: Commit**

```bash
git add src/AsterGraph.Editor/Runtime/Internal/GraphEditorNodeSurfaceTierResolver.cs src/AsterGraph.Editor/Runtime/GraphEditorNodeSurfaceSnapshot.cs src/AsterGraph.Editor/ViewModels/NodeViewModel.cs src/AsterGraph.Editor/ViewModels/Facade/GraphEditorViewModel.SurfaceTierProjection.cs tests/AsterGraph.Editor.Tests/GraphEditorNodeSurfaceContractsTests.cs
git commit -m "feat(105): project measured node surface disclosure"
```

### Task 4: Render adaptive rows and side rails in Avalonia

**Files:**
- Modify: `src/AsterGraph.Avalonia/Presentation/DefaultGraphNodeVisualPresenter.cs`
- Modify: `src/AsterGraph.Avalonia/Presentation/GraphNodeVisualContext.cs`
- Test: `tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs`

**Step 1: Write the failing test**

Add or extend standalone tests that prove:
- baseline nodes show required rows only
- extra height reveals defaultable rows
- extra width reveals summary-only side rail before full editor hosts
- editor hosts do not render for connected parameter endpoints

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~NodeCanvasStandaloneTests" -v minimal --no-restore`

Expected: FAIL because the presenter still renders based on the older simpler assumptions.

**Step 3: Write minimal implementation**

Teach the presenter to render from the resolved disclosure plan. Keep helper extraction small and local if the file starts growing again.

**Step 4: Run test to verify it passes**

Run the same command and confirm the standalone tests pass.

**Step 5: Commit**

```bash
git add src/AsterGraph.Avalonia/Presentation/DefaultGraphNodeVisualPresenter.cs src/AsterGraph.Avalonia/Presentation/GraphNodeVisualContext.cs tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs
git commit -m "feat(105): render adaptive node surface tiers"
```

### Task 5: Apply the contract to demo/test definitions

**Files:**
- Modify: `src/AsterGraph.Demo/DemoGraphFactory.cs`
- Modify: `src/AsterGraph.Demo/DemoProof.cs`
- Modify: `tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs`
- Modify: `tests/AsterGraph.Demo.Tests/DemoCapabilityShowcaseTests.cs`
- Test: `tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj`

**Step 1: Write the failing test**

Add demo-oriented tests proving at least one shipped/demo node:
- defaults to baseline chrome coverage
- reveals side-rail summaries and editors through width growth
- reveals defaultable inputs through height growth

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj -v minimal --no-restore`

Expected: FAIL until demo definitions adopt the new metadata and proof expectations.

**Step 3: Write minimal implementation**

Annotate demo/test node definitions with the required semantic facts and update proof expectations to match the measured disclosure model.

**Step 4: Run test to verify it passes**

Run the same command and confirm demo tests pass.

**Step 5: Commit**

```bash
git add src/AsterGraph.Demo/DemoGraphFactory.cs src/AsterGraph.Demo/DemoProof.cs tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs tests/AsterGraph.Demo.Tests/DemoCapabilityShowcaseTests.cs
git commit -m "feat(105): adopt adaptive node surface metadata in demo"
```

### Task 6: Full verification and cleanup

**Files:**
- Modify: `docs/zh-CN/node-presentation-guidelines.md`
- Modify: `docs/en/node-presentation-guidelines.md`
- Test: `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`
- Test: `tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj`
- Test: `tests/AsterGraph.ConsumerSample.Tests/AsterGraph.ConsumerSample.Tests.csproj`

**Step 1: Write the failing test**

If documentation claims need proof, add or tighten one last regression around measured disclosure ordering or default chrome coverage.

**Step 2: Run test to verify it fails**

Run the most focused failing test command first.

Expected: FAIL until the final cleanup is in place.

**Step 3: Write minimal implementation**

Update docs to describe:
- baseline default sizing
- height-based endpoint disclosure
- width-based summary/editor disclosure
- connected-input suppression of competing inline editors

**Step 4: Run test to verify it passes**

Run:
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -v minimal --no-restore`
- `dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj -v minimal --no-restore`
- `dotnet test tests/AsterGraph.ConsumerSample.Tests/AsterGraph.ConsumerSample.Tests.csproj -v minimal --no-restore`
- `dotnet build avalonia-node-map.sln -v minimal`

Expected: all pass, zero build errors.

**Step 5: Commit**

```bash
git add docs/zh-CN/node-presentation-guidelines.md docs/en/node-presentation-guidelines.md
git commit -m "docs(105): describe adaptive node surface disclosure"
```

Plan complete and saved to `docs/plans/2026-04-20-phase-105-definition-driven-bounds-and-side-rails.md`.
