# Phase 17: Compatibility Lock And Migration Proof - Context

**Gathered:** 2026-04-08  
**Status:** Ready for planning  
**Source:** v1.2 roadmap + Phase 16 closeout review + auto-discuss decisions

<domain>
## Phase Boundary

Phase 17 starts after Phase 16 made the Avalonia layer consume the canonical descriptor/session boundary and proved that thinner adapter path in tests, sample output, smoke markers, and docs.

This phase is about:

- locking the retained `GraphEditorViewModel` / `GraphEditorView` path as an explicitly supported migration window rather than an implicit leftover
- proving, in a focused and repeatable way, that the retained compatibility path stays behaviorally aligned with the canonical kernel-first/runtime-descriptor path
- making the canonical composition route unmistakable in API remarks, quick-start docs, HostSample, PackageSmoke, and focused regressions

This phase is not yet about:

- removing `GraphEditorViewModel` or `GraphEditorView`
- broad public API removals or forced obsoletions on the retained compatibility surface
- runtime plugin loading, plugin contracts, or broader automation readiness proof beyond migration alignment

</domain>

<decisions>
## Implementation Decisions

### Migration posture

- **D-01:** Keep `new GraphEditorViewModel(...)` and `new GraphEditorView { Editor = ... }` supported through this phase; Phase 17 clarifies migration posture but does not force a rewrite.
- **D-02:** Treat `AsterGraphEditorFactory.CreateSession(...)` as the canonical runtime entry, and treat `AsterGraphEditorFactory.Create(...)` plus Avalonia factories as the canonical default hosted-UI entry. The direct VM/view constructors remain retained compatibility paths.
- **D-03:** Prefer stronger wording, proof markers, and parity tests over aggressive deprecation. If a narrower compatibility-only shim needs annotation, keep it targeted and additive.

### Proof strategy

- **D-04:** Migration proof must compare legacy constructor, factory facade, canonical session, direct `GraphEditorView`, factory full shell, and standalone factory surfaces wherever those paths are supposed to remain aligned.
- **D-05:** Host proof needs both human-readable and machine-checkable outputs. `HostSample` should explain the migration story; `PackageSmoke` should emit stable markers that fail loudly if the story drifts.
- **D-06:** Focused regressions should cover the behaviors most likely to drift now that the canonical path is clearer: selection/state signatures, descriptor/menu parity, diagnostics/inspection, presentation hooks, host/platform seams, and default opt-out behavior.

### Scope control

- **D-07:** Known transaction-history/save-semantics baseline failures stay out of scope unless they directly block migration proof; do not let this phase reopen unrelated history work.
- **D-08:** Phase 17 should not introduce new runtime/editor features unless a small additive surface is required to make migration proof explicit and stable.

### the agent's Discretion

- Whether to capture parity through one consolidated helper/signature model in tests or a few narrowly scoped assertion helpers.
- Which public docs and XML comments need the strongest canonical-vs-compatibility wording first.
- Whether any compatibility-only annotations should land in this phase beyond wording and comments already present on narrow shims.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope and current state
- `.planning/PROJECT.md` - milestone constraints, staged-migration quality bar, and publish-surface expectations
- `.planning/REQUIREMENTS.md` - `MIG-01`, `MIG-02`, and the deferred plugin-readiness boundary
- `.planning/ROADMAP.md` - Phase 17 goal, dependencies, and success criteria
- `.planning/STATE.md` - project state after Phase 16 closeout
- `.planning/codebase/ARCHITECTURE.md` - refreshed layer map showing canonical runtime vs retained facade vs Avalonia adapters
- `.planning/codebase/CONCERNS.md` - current drift and hotspot risks

### Carry-forward phase context
- `.planning/phases/14-session-and-compatibility-facade-decoupling/14-CONTEXT.md`
- `.planning/phases/14-session-and-compatibility-facade-decoupling/14-03-SUMMARY.md`
- `.planning/phases/15-capability-and-descriptor-contract-normalization/15-CONTEXT.md`
- `.planning/phases/15-capability-and-descriptor-contract-normalization/15-03-SUMMARY.md`
- `.planning/phases/16-avalonia-adapter-boundary-cleanup/16-CONTEXT.md`
- `.planning/phases/16-avalonia-adapter-boundary-cleanup/16-01-SUMMARY.md`
- `.planning/phases/16-avalonia-adapter-boundary-cleanup/16-02-SUMMARY.md`
- `.planning/phases/16-avalonia-adapter-boundary-cleanup/16-03-SUMMARY.md`

### Canonical runtime and retained compatibility surfaces
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` - canonical composition entrypoints and migration wording
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs` - canonical runtime root
- `src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs` - canonical command surface
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs` - canonical descriptor/read surface
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` - retained compatibility facade
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` - retained compatibility Avalonia shell entry
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs` - canonical full-shell factory
- `src/AsterGraph.Avalonia/Hosting/AsterGraphCanvasViewFactory.cs` - canonical standalone canvas factory

### Host-facing guidance and proof surfaces
- `docs/quick-start.md` - shortest supported composition path
- `docs/host-integration.md` - detailed host guidance
- `src/AsterGraph.Editor/README.md` - editor package guidance
- `src/AsterGraph.Avalonia/README.md` - Avalonia package guidance
- `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs` - retained/factory parity coverage
- `tests/AsterGraph.Editor.Tests/GraphEditorProofRingTests.cs` - milestone-level proof ring
- `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs` - factory and composition seams
- `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` - direct `GraphEditorView` compatibility surface
- `tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs` - standalone surface defaults and routing
- `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs` - canonical runtime contract checks
- `tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs` - host seam continuity
- `tools/AsterGraph.HostSample/Program.cs` - human-readable host proof path
- `tools/AsterGraph.PackageSmoke/Program.cs` - machine-checkable proof markers

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets

- `GraphEditorMigrationCompatibilityTests` already compares many legacy and factory paths; Phase 17 can extend this into a clearer migration-lock matrix instead of starting from scratch.
- `GraphEditorProofRingTests` already acts as the milestone-level proof surface and now includes runtime, retained adapter, and Avalonia adapter-boundary coverage.
- `HostSample` and `PackageSmoke` already emit descriptor and Phase 16 boundary markers, so Phase 17 can extend a known proof vocabulary rather than create a second proof system.
- `AsterGraphEditorFactory`, `src/AsterGraph.Editor/README.md`, and `src/AsterGraph.Avalonia/README.md` already describe canonical-vs-compatibility posture in parts of the surface.

### Established Patterns

- Recent phases have favored additive canonical signaling plus compatibility shims rather than one-shot breaks.
- The strongest phase-close evidence in this repo comes from three layers together: focused tests, human-readable HostSample output, and machine-checkable PackageSmoke markers.
- Migration wording is strongest when the same story is repeated consistently in XML remarks, README/quick-start docs, and proof outputs.

### Integration Points

- `GraphEditorViewModel` and `GraphEditorView` remarks still carry older "Phase 1" migration wording, which is now stale relative to the current Phase 17 state.
- `docs/quick-start.md` shows the canonical full-shell factory path, but it does not yet fully explain how that path relates to the retained compatibility constructors/views.
- `GraphEditorMigrationCompatibilityTests`, `GraphEditorViewTests`, and `GraphEditorProofRingTests` are the most direct places to lock behavior before PackageSmoke and docs repeat the same migration story.

</code_context>

<specifics>
## Specific Ideas

- `AsterGraphEditorFactory.CreateSession(...)` is already documented as kernel-first, but the retained `GraphEditorViewModel` constructor and `GraphEditorView` remarks still use older migration-phase language.
- `HostSample` prints descriptor and Phase 16 markers, but it does not yet emit a Phase 17 summary that explicitly says which route is canonical and whether the retained path still matches behaviorally.
- `PackageSmoke` already proves retained adapter backing, descriptor parity, and Phase 16 adapter-boundary behavior; Phase 17 should extend that into migration-lock markers rather than invent unrelated smoke outputs.
- `GraphEditorMigrationCompatibilityTests` already cover legacy/factory parity across editor/session/view/surface seams, but the assertions are still distributed rather than presented as a locked migration-proof narrative.

</specifics>

<deferred>
## Deferred Ideas

- Removing `GraphEditorViewModel` / `GraphEditorView` from the supported migration window
- Plugin loading, automation APIs, or broader readiness proof beyond migration alignment
- New end-user editing features unrelated to migration proof
- Non-Avalonia presentation stacks or visual redesign work

</deferred>

---

*Phase: 17-compatibility-lock-and-migration-proof*  
*Context gathered: 2026-04-08*
