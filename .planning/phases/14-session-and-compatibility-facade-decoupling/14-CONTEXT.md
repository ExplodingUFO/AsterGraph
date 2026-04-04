# Phase 14: Session And Compatibility Facade Decoupling - Context

**Gathered:** 2026-04-04  
**Status:** Ready for planning  
**Source:** v1.2 roadmap + Phase 13 implementation review

<domain>
## Phase Boundary

Phase 14 picks up where Phase 13 stopped: the canonical `CreateSession(...)` path is now kernel-first, but the product still has two runtime shapes.

This phase is about:

- making `GraphEditorSession` a kernel-facing runtime surface instead of a thin wrapper over two different host implementations
- reducing `GraphEditorViewModel` from primary state owner to compatibility adapter over the extracted kernel
- closing the most important mutable-state leaks so host-facing runtime state is read-only or snapshot-based by default

This phase is not yet about:

- full capability/menu/descriptor redesign beyond what is needed to remove facade coupling
- thinning the Avalonia layer itself
- final migration lock/proof or plugin-readiness proof ring

</domain>

<decisions>
## Implementation Decisions

### Locked

- Phase 14 must satisfy `KERN-03` and `CAP-03`; `CAP-01` and `CAP-02` stay in Phase 15.
- The kernel-first runtime path introduced in Phase 13 is now canonical and must remain the center of gravity.
- `GraphEditorViewModel` must remain available for staged migration, but it should become an adapter over kernel-owned state instead of maintaining a parallel implementation path.
- Public host-facing state should default to detached snapshots or read-only shapes; mutable live collections are now considered architecture debt, not convenience.
- Phase 14 should reduce dual-path drift between `AsterGraphEditorFactory.CreateSession(...)` and `GraphEditorViewModel.Session`, not increase it.
- Avalonia dependencies must stay out of the editor kernel/runtime contracts.

### the agent's Discretion

- How far to push delegation in one phase versus leaving some compatibility shims for Phase 15.
- Whether the adapter boundary is best expressed through richer kernel services, narrower host interfaces, or VM-side projection helpers.
- Which public collection exposures can be converted immediately without causing unnecessary migration churn.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope
- `.planning/PROJECT.md` — milestone constraints, host-facing quality bar, and migration expectations
- `.planning/REQUIREMENTS.md` — `KERN-03`, `CAP-03`, and the later-phase boundaries that must stay deferred
- `.planning/ROADMAP.md` — Phase 14 goal and the dependency chain into phases 15-18
- `.planning/STATE.md` — carry-forward milestone state and architecture concerns
- `.planning/phases/13-editor-kernel-state-owner-extraction/13-CONTEXT.md` — previous phase scope and explicit deferrals
- `.planning/phases/13-editor-kernel-state-owner-extraction/13-01-SUMMARY.md`
- `.planning/phases/13-editor-kernel-state-owner-extraction/13-02-SUMMARY.md`
- `.planning/phases/13-editor-kernel-state-owner-extraction/13-03-SUMMARY.md`

### Current runtime composition and compatibility boundary
- `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs` — extracted kernel state owner introduced in Phase 13
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs` — public runtime session surface now pointed at `IGraphEditorSessionHost`
- `src/AsterGraph.Editor/Runtime/IGraphEditorSessionHost.cs` — internal host abstraction currently implemented by both kernel and compatibility facade
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` — still large compatibility facade and current adapter candidate
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` — canonical `CreateSession(...)` vs retained `Create(...)` composition paths

### Public contracts that should become more kernel-shaped
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs`
- `src/AsterGraph.Editor/Runtime/GraphEditorCapabilitySnapshot.cs`
- `src/AsterGraph.Editor/Menus/CompatiblePortTarget.cs`

### Regression and proof surfaces that must stay aligned
- `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorProofRingTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsInspectionTests.cs`
- `tools/AsterGraph.HostSample/Program.cs`
- `tools/AsterGraph.PackageSmoke/Program.cs`

</canonical_refs>

<specifics>
## Specific Ideas

- Phase 13 left an intentional dual-path reality:
  1. kernel-backed canonical `CreateSession(...)`
  2. compatibility `GraphEditorViewModel` path implementing `IGraphEditorSessionHost`
- `GraphEditorViewModel` now exposes a large explicit `IGraphEditorSessionHost` implementation block around lines 3421-3557, which is a strong signal that adapter extraction is the next job.
- `CompatiblePortTarget` still exposes `NodeViewModel` / `PortViewModel`; this is retained compatibility surface and likely one of the main remaining CAP-03 / KERN-03 pressure points.
- A fresh Phase 13 hardening fix also showed that snapshot detachment matters in practice; Phase 14 should keep eliminating mutable-state escape hatches instead of treating them as secondary cleanup.

</specifics>

<deferred>
## Deferred Ideas

- Full capability descriptor rollout (`CAP-01`)
- Full menu/command descriptor normalization (`CAP-02`)
- Avalonia adapter thinning (`ADAPT-01`, `ADAPT-02`)
- Migration proof lock (`MIG-01`, `MIG-02`)
- Final plugin/automation readiness proof ring (`PLUG-READY-01`)

</deferred>

---

*Phase: 14-session-and-compatibility-facade-decoupling*  
*Context gathered: 2026-04-04*
