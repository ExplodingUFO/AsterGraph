---
phase: 17-compatibility-lock-and-migration-proof
plan: 02
subsystem: canonical-vs-compatibility-guidance
completed: 2026-04-08
---

# Phase 17 Plan 02 Summary

Aligned API remarks and host-facing guidance so the canonical composition route is explicit and the retained constructor/view route is described consistently as a supported compatibility path.

Key changes:

- Updated `AsterGraphEditorFactory` remarks so the route split is explicit:
  - `CreateSession(...)` is the canonical runtime-first entry
  - `Create(...)` is the canonical hosted-UI entry
  - direct `GraphEditorViewModel` construction remains a retained compatibility path
- Updated `GraphEditorViewModel` and `GraphEditorView` remarks to remove stale "Phase 1" wording and replace it with current migration-window guidance.
- Updated `src/AsterGraph.Editor/README.md`, `src/AsterGraph.Avalonia/README.md`, `docs/quick-start.md`, and `docs/host-integration.md` so they repeat the same route guidance instead of mixing older milestone language with the current architecture.
- Updated `AsterGraphEditorOptions` remarks so the options record now reads as the current canonical composition contract instead of an earlier-phase artifact.

Verification run:

- `dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj --nologo -v minimal`
  - exit 0
  - build passed with existing nullable warnings in `GraphEditorViewModelKernelAdapter.cs` and `GraphEditorKernel.cs`
- `dotnet build src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj --nologo -v minimal`
  - exit 0
  - build passed with the same existing nullable warnings from `AsterGraph.Editor`
- `rg -n "Phase 1|Phase 2|canonical hosted-UI|canonical runtime|retained compatibility|CreateSession|new GraphEditorViewModel|new GraphEditorView" src/AsterGraph.Editor src/AsterGraph.Avalonia docs`
  - exit 0
  - remaining hits now reflect intentional route guidance, compatibility examples, or historical plan artifacts rather than stale canonical-path wording

Phase 17 status after this plan:

- `MIG-01` is materially advanced: the public migration story is now explicit in API comments and host-facing docs instead of being inferred from phase history.
- `17-03` is next: lock the same migration story into HostSample, PackageSmoke, and final phase-close proof artifacts.
