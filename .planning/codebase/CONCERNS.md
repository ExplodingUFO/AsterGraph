# Codebase Concerns

**Analysis Date:** 2026-03-25

## Tech Debt

**Editor orchestration monolith:**
- Issue: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` is a 2400+ line type that combines command wiring, selection, connection editing, clipboard flows, fragment/template workflows, workspace persistence, dirty tracking, history, localization, host context, and inspector projection.
- Files: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/Services/GraphEditorHistoryService.cs`, `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs`, `src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs`
- Impact: changes in one subsystem are likely to regress unrelated behavior because most editor state transitions share a single class and a single command surface.
- Fix approach: extract focused services for workspace/fragment I/O, history/dirty tracking, and selection/command orchestration so `GraphEditorViewModel` becomes a coordinator instead of the implementation for every concern.

**Manual scene composition in the canvas control:**
- Issue: `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` is a 1100+ line control that manually builds node visuals, owns pointer interaction state, renders connections, manages context menus, and wires per-node/per-port event handlers.
- Files: `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`, `src/AsterGraph.Avalonia/Controls/Internal/NodeCanvasInteractionSession.cs`, `src/AsterGraph.Avalonia/Controls/Internal/NodeCanvasDragAssistCalculator.cs`
- Impact: rendering and interaction bugs are hard to isolate because visual creation, scene invalidation, and pointer logic are tightly coupled.
- Fix approach: split the control into a visual factory, a rendering layer for connections, and a dedicated interaction coordinator with narrower test seams.

**Demo-branded storage defaults are baked into reusable packages:**
- Issue: default workspace, fragment, and template-library paths all point to `LocalApplicationData/AsterGraphDemo`, even when callers use the publishable editor packages directly.
- Files: `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`, `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs`, `src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs`
- Impact: hosts that do not override these services will share the same persisted files and template folder, which risks cross-app collisions and demo-specific file naming leaking into production hosts.
- Fix approach: require an explicit host storage root for packaged usage or derive a default path from host identity instead of the demo application name.

## Known Bugs

**Solution build/test is currently broken by nested audit outputs:**
- Symptoms: `dotnet test avalonia-node-map.sln --no-restore` currently fails with duplicate assembly attribute errors coming from both normal `obj` output and generated files under `src/AsterGraph.Abstractions/artifacts/audit/editor_obj/Release/net8.0/` and related audit directories.
- Files: `src/AsterGraph.Abstractions/artifacts/audit/editor_obj/`, `src/AsterGraph.Core/artifacts/audit/editor_obj/`, `src/AsterGraph.Editor/artifacts/audit/editor_obj/`, `src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj`, `.gitignore`
- Trigger: run a solution build or test while `src/*/artifacts/audit/*_obj` exists under project directories.
- Workaround: remove or relocate audit output directories before building, or explicitly exclude `artifacts/**` from compile items and repo hygiene.

**Mini-map repaint path is incomplete:**
- Symptoms: `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs` renders from live `GraphEditorViewModel` data but does not subscribe to editor, node, or collection changes and does not call `InvalidateVisual()` when the graph changes.
- Files: `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs`, `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`
- Trigger: add, move, or reselect nodes after initial layout; the control has no internal repaint signal for those mutations.
- Workaround: force redraws from parent code after graph changes until the control owns its own invalidation lifecycle.

## Security Considerations

**Host-supplied file paths are used directly for read/write/delete:**
- Risk: export/import/template operations call `File.WriteAllText`, `File.ReadAllText`, and `File.Delete` against caller-provided paths with no root restriction beyond trimming whitespace.
- Files: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs`, `src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs`, `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`
- Current mitigation: command-permission flags can disable some flows, and default paths exist for the demo path.
- Recommendations: treat path-based APIs as a host trust boundary, add optional safe-root enforcement, and avoid exposing delete/write helpers that can operate outside the configured storage area.

**Render code trusts externally supplied color strings:**
- Risk: `BrushFactory.Solid` calls `Color.Parse` directly, and many rendering paths use it with style values and node accent data that come from host configuration or serialized graph content.
- Files: `src/AsterGraph.Avalonia/Styling/BrushFactory.cs`, `src/AsterGraph.Avalonia/Styling/GraphEditorStyleAdapter.cs`, `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs`, `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`, `src/AsterGraph.Avalonia/Controls/GridBackground.cs`, `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs`
- Current mitigation: `BrushFactory.SolidSafe` exists, but only some badge/status paths use it.
- Recommendations: validate style/color inputs when constructing host-facing options and switch direct render-time parsing to safe parsing with fallback colors plus diagnostics.

## Performance Bottlenecks

**Connection rendering redraws the whole layer repeatedly:**
- Problem: `RenderConnections()` clears and rebuilds every connection visual, reparses geometry strings, and is called on connection changes, node moves, selection changes, zoom/pan, and pointer movement during pending connections.
- Files: `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- Cause: the canvas has no retained geometry cache or incremental invalidation strategy.
- Improvement path: cache connection visuals by connection id, update only affected edges, and throttle preview rendering during pointer move.

**Dirty tracking and history rely on whole-document serialization:**
- Problem: `CreateDocumentSignature()` serializes the entire graph for dirty checks, while `CaptureHistoryState()` stores full `GraphDocument` snapshots and `GraphEditorHistoryService` keeps them in an unbounded list.
- Files: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/Services/GraphEditorHistoryService.cs`, `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`, `src/AsterGraph.Editor/Configuration/HistoryBehaviorOptions.cs`
- Cause: undo/redo and dirty detection are snapshot-based rather than operation-based.
- Improvement path: add a configurable history cap, use cheaper dirty markers for common edits, and consider diff-based or coarser history checkpoints for large graphs.

**Template refresh does synchronous full-file reads:**
- Problem: `EnumerateTemplates()` reads every `*.json` file and deserializes enough payload to count nodes/connections each time templates are refreshed.
- Files: `src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs`, `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Cause: metadata is derived from full template files instead of cached or sidecar data.
- Improvement path: persist lightweight template metadata, lazy-load counts, or move template scanning off the UI command path.

## Fragile Areas

**Canvas event subscription lifecycle:**
- Files: `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- Why fragile: `AttachViewModel(...)` manually subscribes and unsubscribes editor, collection, and node handlers, while node and port visuals also capture lambdas during scene construction.
- Safe modification: preserve attach/detach symmetry and add control-level regression tests before changing scene rebuild or pointer-flow code.
- Test coverage: `tests/AsterGraph.Editor.Tests/NodeCanvasInteractionSessionTests.cs` covers the extracted interaction-session helper, but not the full `NodeCanvas` subscription and rebuild lifecycle.

**Workspace/fragment/template workflows on the UI command path:**
- Files: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`, `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs`, `src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs`
- Why fragile: command handlers mix status reporting, event emission, document mutation, and synchronous file I/O; failures mostly degrade to status text instead of explicit error contracts.
- Safe modification: separate storage primitives from command handlers and add direct tests for malformed files, locked files, and permission failures before changing these flows.
- Test coverage: `tests/AsterGraph.Editor.Tests/EditorClipboardAndFragmentCompatibilityTests.cs` covers payload compatibility, but not workspace save/load, template-library enumeration/deletion, or failure cases.

## Scaling Limits

**Undo/redo memory usage grows with edit count and graph size:**
- Current capacity: effectively unbounded because `src/AsterGraph.Editor/Services/GraphEditorHistoryService.cs` stores every pushed `GraphEditorHistoryState` in a `List<GraphEditorHistoryState>`.
- Limit: larger graphs and longer edit sessions will increase both heap usage and time spent serializing signatures/snapshots.
- Scaling path: cap history, compress snapshots, or move toward command-based undo entries.

**Canvas rendering cost scales with total node and edge count:**
- Current capacity: the current approach is adequate for demo-sized graphs, where `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` can rebuild nodes and redraw every connection without obvious contention.
- Limit: larger graphs will pay O(nodes + edges) work on drag, zoom, pan, selection changes, and scene rebuilds.
- Scaling path: introduce virtualization for off-screen nodes, retained visuals for edges, and more selective invalidation.

## Dependencies at Risk

**Not detected:**
- Risk: no dependency-specific deprecation or abandonment issue is directly visible from the current repository state.
- Impact: not applicable.
- Migration plan: not applicable.

## Missing Critical Features

**Repository CI/build gate:**
- Problem: there is no checked-in CI workflow under `.github/workflows/`, so solution-wide build/test regressions are not automatically blocked.
- Blocks: reliable package publishing and early detection of issues like the current duplicate-assembly-attribute build failure.

## Test Coverage Gaps

**Mini-map redraw and navigation behavior:**
- What's not tested: `GraphMiniMap` repaint behavior, graph-change invalidation, and drag-to-recenter integration.
- Files: `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs`
- Risk: stale minimap visuals or viewport-navigation regressions can ship unnoticed.
- Priority: High

**Workspace and template storage flows:**
- What's not tested: `SaveWorkspace()`, `LoadWorkspace()`, template enumeration/deletion, corrupted files, and filesystem failure paths.
- Files: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`, `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs`, `src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs`
- Risk: late discovery of data-loss or file-I/O regressions in host apps.
- Priority: High

**History-service behavior under long sessions:**
- What's not tested: `GraphEditorHistoryService` capacity growth, undo/redo behavior under large snapshots, and the performance impact of repeated `CreateDocumentSignature()` calls.
- Files: `src/AsterGraph.Editor/Services/GraphEditorHistoryService.cs`, `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Risk: memory/performance regressions remain invisible until graph size or session duration increases.
- Priority: Medium

---

*Concerns audit: 2026-03-25*
