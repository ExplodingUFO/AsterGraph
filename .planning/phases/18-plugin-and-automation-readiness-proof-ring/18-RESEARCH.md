# Phase 18 Research: Plugin And Automation Readiness Proof Ring

**Date:** 2026-04-08  
**Phase:** 18-plugin-and-automation-readiness-proof-ring

## Research Questions

1. Which seams already exist that later plugin/automation work would depend on?
2. Which of those seams are explicit at the canonical runtime boundary versus only discoverable by reading implementation code?
3. What is the smallest proof-ring expansion that can credibly close the milestone without accidentally building a plugin system?

## Findings

### 1. The replaceable seam surface already exists

`AsterGraphEditorOptions` already exposes the important host-supplied seams:

- `IGraphWorkspaceService`
- `IGraphFragmentWorkspaceService`
- `IGraphFragmentLibraryService`
- `IGraphClipboardPayloadSerializer`
- `IGraphContextMenuAugmentor`
- `INodePresentationProvider`
- `IGraphLocalizationProvider`
- `IGraphEditorDiagnosticsSink`
- `GraphEditorInstrumentationOptions`

That means Phase 18 does not need to invent a new configuration model for plugin/automation readiness. The main job is to make this seam surface explicitly discoverable and verifiable.

### 2. Session-first extension contexts are already in place

`GraphContextMenuAugmentationContext` and `NodePresentationContext` already expose:

- `IGraphEditorSession`
- stable hit/context/snapshot data
- compatibility editor/node fallbacks that are clearly marked as migration-only

This is strong evidence that the architecture is close to plugin/automation readiness already. Future extension code can be written against the session and stable snapshots without depending on live MVVM objects.

### 3. Feature descriptors under-report readiness today

`GraphEditorSession.GetFeatureDescriptors()` currently merges:

- runtime capabilities
- `service.workspace`
- `service.diagnostics`
- instrumentation sink/source availability

That is useful, but incomplete relative to the actual host seam surface. Important replaceable services and providers are still not explicitly signaled through descriptors, even though hosts can already provide them.

This is the most concrete readiness gap surfaced by the research.

### 4. The proof ring is close, but not yet readiness-oriented

Current proof assets already cover most of the raw behavior:

- `GraphEditorDiagnosticsContractsTests` validates contract shape
- `GraphEditorServiceSeamsTests` validates host-supplied services/providers/diagnostics continuity
- `GraphEditorProofRingTests` validates milestone-level runtime/retained/Avalonia proof
- `HostSample` and `PackageSmoke` expose human-readable and machine-checkable phase markers
- `ScaleSmoke` proves session-driven large-graph behavior

What is missing is one explicit story that says: these seams are now enough for future plugin and automation work, and here is the proof.

### 5. Scale coverage should be part of readiness, not just performance history

Future automation work will likely batch commands, inspect diagnostics, and reason over snapshots on non-trivial graphs. `ScaleSmoke` already exercises:

- session commands
- selection and connection workflows
- workspace persistence
- diagnostics inspection
- large-graph state mutation

It should be promoted from “historical scale smoke” to “readiness proof at scale.”

## Risks

- If Phase 18 starts designing loader/discovery/runtime plugin infrastructure, it will exceed the milestone and create uncontrolled new public surface.
- If readiness stays only in docs without explicit descriptors and proof markers, later plugin/automation work will still have to reverse-engineer the intended seam model.
- If the phase ignores `ScaleSmoke`, the proof ring will stay biased toward tiny sample graphs and understate automation risk.
- The known `STATE_HISTORY_OK` baseline issue in `PackageSmoke` should remain isolated; it is not a Phase 18 readiness blocker unless it prevents the new proof markers from remaining stable.

## Recommended Planning Split

### Wave 1: Explicit readiness descriptors and contract proof

Make the seam surface explicit at the runtime boundary, then lock it with focused tests.

Recommended target areas:

- `GraphEditorKernel.GetFeatureDescriptors()`
- retained adapter delegation if needed
- contract/proof tests:
  - `GraphEditorDiagnosticsContractsTests`
  - `GraphEditorServiceSeamsTests`
  - `GraphEditorProofRingTests`
  - possibly `GraphEditorSessionTests`

### Wave 2: Sample, package, and scale readiness markers

Extend the proof ring outputs so hosts and CI can verify the same readiness story through real runnable tools.

Recommended target areas:

- `tools/AsterGraph.HostSample`
- `tools/AsterGraph.PackageSmoke`
- `tools/AsterGraph.ScaleSmoke`

### Wave 3: Docs and closeout

Update host/package guidance to point to the new readiness proof surfaces and close the phase so the milestone can move to completion rather than stay implicitly open.

Recommended target areas:

- `docs/host-integration.md`
- `docs/quick-start.md`
- package READMEs if wording needs alignment
- `.planning/STATE.md`
- `.planning/ROADMAP.md`
- `.planning/REQUIREMENTS.md`

## Recommendation

Plan Phase 18 around explicit seam discoverability first, then runnable proof markers, then docs/closeout.

This keeps the phase honest:

- it proves the architecture is ready for future plugin/automation work
- it does not overclaim by shipping an actual plugin system
- it finishes the milestone on explicit evidence rather than architectural optimism

---

*Research complete: 2026-04-08*
