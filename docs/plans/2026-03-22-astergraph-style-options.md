# AsterGraph Style Options Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a unified, maintainable style configuration entry point to AsterGraph that supports both overall theming and component-level visual customization.

**Architecture:** `AsterGraph.Abstractions` will define framework-neutral style option records, `AsterGraph.Editor` will hold the selected style configuration, and `AsterGraph.Avalonia` will adapt those options into Avalonia resources and control visuals.

**Tech Stack:** .NET 9, Avalonia 11, C#

---

## Implementation Note

This repository is currently **not a git repository**, so commit steps are intentionally omitted.

The design target is maintainability first, not maximal visual flexibility in a single pass.

### Task 1: Define public style option contracts in `AsterGraph.Abstractions`

**Files:**
- Create: `src/AsterGraph.Abstractions/Styling/GraphEditorStyleOptions.cs`
- Create: `src/AsterGraph.Abstractions/Styling/ShellStyleOptions.cs`
- Create: `src/AsterGraph.Abstractions/Styling/CanvasStyleOptions.cs`
- Create: `src/AsterGraph.Abstractions/Styling/NodeCardStyleOptions.cs`
- Create: `src/AsterGraph.Abstractions/Styling/PortStyleOptions.cs`
- Create: `src/AsterGraph.Abstractions/Styling/ConnectionStyleOptions.cs`
- Create: `src/AsterGraph.Abstractions/Styling/InspectorStyleOptions.cs`
- Create: `src/AsterGraph.Abstractions/Styling/ContextMenuStyleOptions.cs`
- Optionally create: targeted override record types

**Step 1: Create the root style entry point**

Add `GraphEditorStyleOptions` with typed sub-sections.

**Step 2: Keep contracts framework-neutral**

Use strings/numbers/booleans only. Do not expose Avalonia types.

**Step 3: Verify**

Run:

```powershell
dotnet build src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj
```

### Task 2: Add editor-layer style state support

**Files:**
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Optionally create: `src/AsterGraph.Editor/Styling/GraphEditorStyleState.cs`

**Step 1: Add style options to editor state**

Allow `GraphEditorViewModel` to carry or expose the active `GraphEditorStyleOptions`.

**Step 2: Keep editor layer framework-neutral**

The editor should pass style options through, not convert them into Avalonia resources.

### Task 3: Build Avalonia style adaptation layer

**Files:**
- Create: `src/AsterGraph.Avalonia/Styling/AvaloniaStylePalette.cs`
- Create: `src/AsterGraph.Avalonia/Styling/GraphEditorStyleAdapter.cs`
- Modify: `src/AsterGraph.Avalonia/Themes/AsterGraphTheme.axaml`

**Step 1: Translate style options into Avalonia-ready values**

Convert string colors and numeric tokens into:

- brushes
- radii
- spacing values

**Step 2: Apply adapted values to the existing theme and controls**

Ensure the current UI can render from style options instead of only hard-coded defaults.

### Task 4: Apply style options across core visual surfaces

**Files:**
- Modify: `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`
- Modify: `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- Modify: `src/AsterGraph.Avalonia/Controls/GridBackground.cs`

**Step 1: Shell and panel styling**

Route shell/panel visuals through the style adapter.

**Step 2: Canvas/grid styling**

Use `CanvasStyleOptions` for background and grid rendering.

**Step 3: Node/port/connection styling**

Use dedicated style sections for node cards, ports, and edges rather than inline magic values where practical.

### Task 5: Add optional override hooks

**Files:**
- Create or modify files under `src/AsterGraph.Abstractions/Styling`
- Modify: style adapter and visual application logic as needed

**Step 1: Add node-level override support**

Support overrides keyed by `NodeDefinitionId`.

**Step 2: Add port/connection override support**

Support overrides keyed by `PortTypeId` and `ConversionId` where useful.

**Step 3: Keep fallback behavior simple**

Use the global style when no override exists.

### Task 6: Update documentation

**Files:**
- Modify: `README.md`
- Optionally modify: `src/AsterGraph.Abstractions/README.md`
- Optionally modify: `src/AsterGraph.Avalonia/README.md`

**Step 1: Document the style entry point**

Explain:

- where style options live
- how a host overrides them
- which categories are stable

**Step 2: Document maintainability boundaries**

Explain the rule:

- public style model stays framework-neutral
- Avalonia details stay internal

### Task 7: Final verification

**Files:**
- Modify: any files required for final polish after verification

**Step 1: Run full build**

Run:

```powershell
dotnet build avalonia-node-map.sln
```

**Step 2: Smoke-run the demo**

Run:

```powershell
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj
```

**Step 3: Verify style flow**

Check:

- the demo still renders
- default style values still apply
- host-supplied overrides can change visible output
- node/port/connection visuals respond to style configuration

**Step 4: Record residual gaps**

Document anything deferred, such as:

- richer per-node visual templating
- plugin-supplied visual style injection
- advanced animation/motion tokens
