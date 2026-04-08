---
phase: 16-avalonia-adapter-boundary-cleanup
plan: 02
subsystem: platform-seam-binder
completed: 2026-04-08
---

# Phase 16 Plan 02 Summary

Narrowed Avalonia's platform seam attachment by introducing a shared binder for clipboard and host-context wiring, while keeping those seams Avalonia-owned and preserving the retained facade migration path.

Key changes:

- Added `GraphEditorPlatformSeamBinder` under `src/AsterGraph.Avalonia/Controls/Internal/` and switched `GraphEditorView` to use it instead of directly calling `SetTextClipboardBridge(...)` and `SetHostContext(...)`.
- Added standalone `NodeCanvas` platform seam attachment on visual-tree attach/detach through the same binder, so supported standalone canvas hosts now receive Avalonia clipboard and host-context wiring without relying on the full shell.
- Added an internal `AttachPlatformSeams` toggle on `NodeCanvas`, and set the embedded shell canvas to `false` from `GraphEditorView` so full-shell ownership remains on the shell view instead of being stolen by the child canvas.
- Clarified compatibility posture in `IGraphInspectorPresenter`, `IGraphMiniMapPresenter`, and `GraphNodeVisualContext`: these presenter seams remain retained-facade-oriented during migration even though the platform seam binding around them is thinner.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorInitializationTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal`
  - exit 0
  - `26` tests passed
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorInitializationTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~AvaloniaGraphHostContextTests|FullyQualifiedName~GraphInspectorStandaloneTests|FullyQualifiedName~GraphMiniMapStandaloneTests|FullyQualifiedName~GraphEditorViewTests" -v minimal`
  - exit 0
  - `39` tests passed

Phase 16 status after this plan:

- `ADAPT-02` is materially closed: clipboard and host-context seams remain Avalonia-owned, but now flow through a shared platform binder instead of ad hoc retained-facade wiring.
- Supported standalone canvas composition now receives the same Avalonia platform seam wiring previously only applied by the full shell.
- `16-03` remains next: lock the thinner adapter boundary into proof tests, sample/smoke markers, and package/docs guidance.
