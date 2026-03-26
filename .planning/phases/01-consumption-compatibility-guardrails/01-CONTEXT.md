# Phase 1: Consumption & Compatibility Guardrails - Context

**Gathered:** 2026-03-25
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase defines how external hosts are expected to consume AsterGraph safely: supported package boundary, public initialization entry points, and staged migration rules. It does not yet perform the deeper runtime API split or Avalonia surface decomposition from later phases.

</domain>

<decisions>
## Implementation Decisions

### Package Boundary
- **D-01:** Treat `AsterGraph.Editor` as a standard host-facing entry package for advanced or primary host integration scenarios, not only as an optional/internal-facing dependency. The package story should move beyond "Avalonia + Abstractions only" and explicitly document when `Editor` is part of the intended public consumption path.
- **D-02:** Keep `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Avalonia` as the publishable package set for this phase. `AsterGraph.Demo` remains non-consumable.

### Initialization Entry Points
- **D-03:** Add formal public initialization entry points in Phase 1, such as `AddAsterGraphEditor(...)`, `AddAsterGraphAvalonia(...)`, options objects, factories, or equivalent public registration/construction helpers.
- **D-04:** Do not remove the current `new GraphEditorViewModel(...)` plus `GraphEditorView` path in Phase 1. Keep that path supported as a compatibility route while the new initialization surface is introduced and documented.

### Migration Compatibility
- **D-05:** `GraphEditorViewModel` and `GraphEditorView` remain the compatibility facade in this phase. Prefer additive public APIs, migration shims, and deprecation guidance over immediate hard breaks.
- **D-06:** Phase 1 must produce an explicit staged migration story for existing hosts. The goal is "planned migration with guardrails," not "one-shot rewrite."

### the agent's Discretion
- Exact naming and placement of the new public registration helpers between `AsterGraph.Editor` and `AsterGraph.Avalonia`
- Whether the public entry surface is pure DI registration, factory-based, or a hybrid
- Exact obsolete/deprecation mechanics and migration-note format, as long as the staged compatibility strategy is preserved

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Package Consumption And Host Entry Story
- `README.md` — current package boundary, host-facing API narrative, and pack/run commands that Phase 1 must not contradict
- `docs/host-integration.md` — current documented host composition flow, direct-constructor entry path, and package-choice guidance that Phase 1 will formalize and evolve
- `tools/AsterGraph.HostSample/Program.cs` — current real host composition example using `GraphEditorViewModel`, style/behavior options, augmentors, events, and `GraphEditorView`
- `tools/AsterGraph.PackageSmoke/Program.cs` — current minimal package smoke surface proving which public types are already treated as consumable

### Packaging And Compatibility Guardrails
- `docs/plans/2026-03-23-astergraph-nuget-readiness-checklist.md` — current packaging release gate, target-framework guidance, and host dependency policy discussion
- `docs/plans/2026-03-23-astergraph-nuget-readiness-implementation-plan.md` — concrete packaging/readme/smoke verification tasks already aligned with Phase 1 goals
- `docs/plans/2026-03-23-astergraph-upgrade-path-and-editor-regression.md` — prior decision to keep the current public API stable while compatibility helpers and regression tests are introduced
- `docs/plans/2026-03-23-astergraph-host-sample-and-api-comments.md` — prior intent that `GraphEditorViewModel` remains a real host-facing entry point during hardening

### Package Metadata And Current Public Surface
- `Directory.Build.props` — current package metadata baseline and versioning values
- `src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj` — current public package metadata and target frameworks
- `src/AsterGraph.Editor/AsterGraph.Editor.csproj` — current public package metadata and target frameworks
- `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj` — current Avalonia package metadata, dependencies, and packable status

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `tools/AsterGraph.HostSample/Program.cs`: already demonstrates realistic host composition and can serve as the baseline before introducing formal registration/factory APIs
- `tools/AsterGraph.PackageSmoke/Program.cs`: already acts as a compile-time public-surface smoke test and should evolve with the official package boundary
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`: current host entry object and the compatibility facade to preserve during Phase 1
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`: current default view entry point and the other compatibility facade to preserve during Phase 1

### Established Patterns
- Public host integration is currently constructor-driven and code-first rather than DI-first
- The repo already publishes four packable libraries with separate `csproj` metadata and README files, so Phase 1 should harden rather than reinvent package structure
- Existing design docs repeatedly treat `GraphEditorViewModel` and `GraphEditorView` as stable host entry points while internals evolve behind them

### Integration Points
- Packaging and metadata changes flow through `Directory.Build.props` and the four packable library `csproj` files
- The host-consumption story is anchored in `README.md`, `docs/host-integration.md`, `tools/AsterGraph.HostSample/Program.cs`, and `tools/AsterGraph.PackageSmoke/Program.cs`
- Migration compatibility and regression coverage should connect to existing tests under `tests/AsterGraph.Editor.Tests` and future package smoke validation

</code_context>

<specifics>
## Specific Ideas

- The default host story may become more ambitious than the current "Avalonia + Abstractions" recommendation, with `AsterGraph.Editor` explicitly treated as part of the standard public integration path for hosts that need formal runtime control.
- New public initialization APIs should be introduced in Phase 1, but they must coexist with the current constructor-based composition path instead of forcing an immediate cutover.
- Compatibility messaging should be intentional and staged: formal entry points are added now, while hard breaks are deferred behind migration shims and deprecation guidance.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 01-consumption-compatibility-guardrails*
*Context gathered: 2026-03-25*
