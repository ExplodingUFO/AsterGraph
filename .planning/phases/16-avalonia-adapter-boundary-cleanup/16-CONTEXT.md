# Phase 16: Avalonia Adapter Boundary Cleanup - Context

**Gathered:** 2026-04-08  
**Status:** Ready for planning  
**Source:** v1.2 roadmap + Phase 15 closeout review + auto-discuss decisions

<domain>
## Phase Boundary

Phase 16 starts after Phase 15 made the editor control plane descriptor-first on the runtime path.

This phase is about:

- making `AsterGraph.Avalonia` consume the descriptor- and session-first runtime surface instead of treating `GraphEditorViewModel` command/menu shape as the stock source of truth
- removing duplicated default input and command-routing policy between `GraphEditorView` and `NodeCanvas`
- keeping clipboard, host-context, and other platform seams Avalonia-owned while reducing how much direct retained-facade internals those seams need to touch

This phase is not yet about:

- removing the retained `GraphEditorViewModel` / `GraphEditorView` compatibility path
- redesigning the shipped shell visuals or presenter system
- final compatibility lock, migration proof ownership, or plugin-readiness proof ring

</domain>

<decisions>
## Implementation Decisions

### Input, command, and menu routing

- **D-01:** Default Avalonia keyboard shortcut routing and stock context-menu opening should be shared adapter logic over `IGraphEditorSession` command/query seams, not duplicated `SaveCommand` / `UndoCommand` / `BuildContextMenu(...)` branches in both `GraphEditorView` and `NodeCanvas`.
- **D-02:** The shipped Avalonia menu presenter path should move toward canonical descriptor input first, with compatibility `MenuItemDescriptor` support preserved only where migration still requires it.
- **D-03:** Phase 16 should stop short of moving editor policy into Avalonia-specific code; the UI layer may coordinate gestures and presentation, but command meaning and menu composition remain editor/runtime-owned.

### Platform seam ownership

- **D-04:** Clipboard, host-context, context-menu presentation, and input routing remain Avalonia-owned adapters, but their editor-facing touchpoints should be consolidated behind thinner helper/adaptation seams instead of being scattered across control code.
- **D-05:** `GraphEditorView` may remain the compatibility shell entry point, but it should stop being the place where multiple platform seams and default behavior policies are hand-wired independently.

### Migration posture

- **D-06:** Public Avalonia APIs should prefer additive canonical overloads or thinner internal adapters over one-shot breaking changes; `new GraphEditorView { Editor = ... }` and current standalone factories remain supported during this phase.
- **D-07:** Phase 16 should only tighten public presenter/context contracts where that is required to make Avalonia consume the thinner runtime boundary. Broader presenter-surface redesign can stay deferred if the same outcome is achievable with compatibility shims.

### the agent's Discretion

- Whether the shared Avalonia routing layer is best expressed as one internal adapter or a small cluster of focused helpers for shortcuts, context menus, and platform seam attachment.
- Whether the stock menu presenter should gain a canonical descriptor overload directly or consume an internal descriptor-to-presentation adapter.
- Which Avalonia presenter/factory contracts need additive canonical overloads now versus clearer compatibility annotations only.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope and carry-forward decisions
- `.planning/PROJECT.md` - milestone constraints, staged-migration bar, and publish-surface expectations
- `.planning/REQUIREMENTS.md` - `ADAPT-01`, `ADAPT-02`, and later-phase boundaries that must stay deferred
- `.planning/ROADMAP.md` - Phase 16 goal and dependencies into phases 17-18
- `.planning/STATE.md` - current milestone state after Phase 15 closeout
- `.planning/codebase/ARCHITECTURE.md` - refreshed layer map showing Avalonia as the adapter layer over the kernel-first runtime
- `.planning/codebase/CONCERNS.md` - current hotspot and drift risks, especially `NodeCanvas.axaml.cs`
- `.planning/phases/15-capability-and-descriptor-contract-normalization/15-CONTEXT.md`
- `.planning/phases/15-capability-and-descriptor-contract-normalization/15-01-SUMMARY.md`
- `.planning/phases/15-capability-and-descriptor-contract-normalization/15-02-SUMMARY.md`
- `.planning/phases/15-capability-and-descriptor-contract-normalization/15-03-SUMMARY.md`

### Current canonical editor/runtime seams Avalonia should consume
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs` - canonical runtime root
- `src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs` - stable command execution surface
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs` - descriptor-first read path and menu descriptor generation
- `src/AsterGraph.Editor/Runtime/GraphEditorCommandDescriptorSnapshot.cs` - canonical command discovery shape
- `src/AsterGraph.Editor/Runtime/GraphEditorCommandInvocationSnapshot.cs` - canonical command execution payload
- `src/AsterGraph.Editor/Menus/GraphEditorMenuItemDescriptorSnapshot.cs` - canonical menu descriptor tree
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` - retained compatibility facade Avalonia still binds to today

### Avalonia seams under Phase 16 review
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` - shell-level clipboard, host-context, and duplicated shortcut routing
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` - canvas interaction hotspot and duplicated shortcut/context-menu flow
- `src/AsterGraph.Avalonia/Controls/Internal/NodeCanvasContextMenuContextFactory.cs` - reusable context snapshot factory already separating hit-test capture from menu building
- `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs` - stock menu renderer still consuming compatibility descriptors
- `src/AsterGraph.Avalonia/Presentation/IGraphContextMenuPresenter.cs` - public menu presenter seam
- `src/AsterGraph.Avalonia/Services/AvaloniaTextClipboardBridge.cs` - Avalonia-owned clipboard bridge
- `src/AsterGraph.Avalonia/Hosting/AvaloniaGraphHostContext.cs` - Avalonia-owned host-context adapter
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs` - full-shell composition root
- `src/AsterGraph.Avalonia/Hosting/AsterGraphCanvasViewFactory.cs` - standalone canvas composition root
- `src/AsterGraph.Avalonia/Controls/GraphInspectorView.axaml.cs` - standalone inspector surface
- `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs` - standalone minimap surface
- `src/AsterGraph.Avalonia/Presentation/GraphNodeVisualContext.cs` - node visual presenter context that still carries compatibility-shaped state
- `src/AsterGraph.Avalonia/Presentation/IGraphInspectorPresenter.cs`
- `src/AsterGraph.Avalonia/Presentation/IGraphMiniMapPresenter.cs`

### Validation and proof surfaces
- `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs` - factory/full-shell/standalone surface composition coverage
- `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` - shell behavior and host-context continuity
- `tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs` - canvas defaults and interaction coverage
- `tests/AsterGraph.Editor.Tests/GraphContextMenuPresenterTests.cs` - stock menu presenter contract coverage
- `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs` - retained/factory parity checks
- `tests/AsterGraph.Editor.Tests/AvaloniaGraphHostContextTests.cs` - host-context adapter tests
- `tests/AsterGraph.Editor.Tests/GraphInspectorStandaloneTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphMiniMapStandaloneTests.cs`
- `tools/AsterGraph.HostSample/Program.cs` - human-readable host proof path
- `tools/AsterGraph.PackageSmoke/Program.cs` - machine-checkable proof markers
- `docs/host-integration.md` - host-facing integration story
- `src/AsterGraph.Avalonia/README.md` - package-level Avalonia composition guidance

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets

- `IGraphEditorSession.Queries.BuildContextMenuDescriptors(...)` and `IGraphEditorCommands.TryExecuteCommand(...)` already provide the descriptor-first runtime seam Avalonia should lean on.
- `NodeCanvasContextMenuContextFactory` already isolates context capture from menu composition and should stay reusable.
- `AvaloniaTextClipboardBridge` and `AvaloniaGraphHostContext` already prove clipboard and host-context adaptation can remain Avalonia-owned.
- `AsterGraphAvaloniaViewFactory` and the standalone surface factories already partition the full-shell and per-surface composition story cleanly.

### Established Patterns

- Recent phases favored additive canonical contracts plus compatibility adapters rather than one-shot breaks.
- Canonical runtime ownership now lives in `GraphEditorSession` / `GraphEditorKernel`; Avalonia should consume that runtime rather than re-express editor policy.
- Proof work uses targeted tests plus `HostSample` and `PackageSmoke` to keep canonical and compatibility stories aligned.

### Integration Points

- `GraphEditorView` and `NodeCanvas` are the two primary places where duplicated shortcut and menu-opening policy can be collapsed.
- `IGraphContextMenuPresenter` and `GraphContextMenuPresenter` are the main stock rendering seam for moving Avalonia from compatibility menu input to canonical descriptors.
- `GraphEditorInitializationTests`, `GraphEditorViewTests`, and `NodeCanvasStandaloneTests` are the most direct regression harnesses for Phase 16 behavior changes.

</code_context>

<specifics>
## Specific Ideas

- `GraphEditorView.axaml.cs` and `NodeCanvas.axaml.cs` both implement near-identical default shortcut chains for save/load/undo/redo/copy/paste/delete.
- `NodeCanvas` still opens stock menus through `ViewModel.BuildContextMenu(context)` even though Phase 15 made session menu descriptors canonical.
- `GraphEditorView` still injects clipboard and host context by calling retained-facade setter methods directly.
- `IGraphContextMenuPresenter`, `IGraphInspectorPresenter`, and `IGraphMiniMapPresenter` still accept compatibility-shaped inputs; Phase 16 should only tighten these where doing so directly supports the adapter-boundary goal.

</specifics>

<deferred>
## Deferred Ideas

- Removing `GraphEditorViewModel` from every Avalonia public presenter/factory contract in one phase
- Replacing Avalonia with another UI stack
- Visual redesign or shell layout changes unrelated to adapter thinning
- Final migration lock/proof ownership beyond what Phase 16 needs to preserve compatibility

</deferred>

---

*Phase: 16-avalonia-adapter-boundary-cleanup*  
*Context gathered: 2026-04-08*
