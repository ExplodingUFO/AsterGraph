# Phase 18: Plugin And Automation Readiness Proof Ring - Context

**Gathered:** 2026-04-08  
**Status:** Ready for planning  
**Source:** v1.2 roadmap + Phase 17 closeout review + inline readiness research

<domain>
## Phase Boundary

Phase 18 starts after Phase 17 locked the migration story into focused regressions, HostSample, PackageSmoke, and host-facing docs.

This phase is about:

- proving that the kernel-first runtime and descriptor-first control plane are strong enough for later plugin and automation work
- making the remaining replaceable services and integration seams explicit enough that future plugin/automation work does not have to rediscover them from implementation details
- closing the milestone with a final proof ring that spans focused tests, HostSample, PackageSmoke, and large-graph smoke coverage

This phase is not yet about:

- implementing runtime plugin loading, discovery, manifests, or trust policy
- adding a new automation or macro public API beyond what the existing runtime/session contract already supports
- reopening migration-path wording or broad compatibility-surface redesign after Phase 17
- fixing pre-existing history baseline behavior unless it directly blocks readiness proof

</domain>

<decisions>
## Implementation Decisions

### Readiness posture

- **D-01:** Treat Phase 18 as a readiness-proof phase, not a feature-delivery phase. Small additive surfaces are acceptable only when they make readiness explicit and stable.
- **D-02:** Keep `IGraphEditorSession` as the canonical automation root. Any proof of future automation should start from descriptor/query/event/diagnostic surfaces instead of `GraphEditorViewModel` methods.
- **D-03:** Keep plugin-oriented extension seams additive and session-first. Existing compatibility properties on augmentation/presentation contexts can remain, but proof should emphasize the stable session and snapshot members.

### Explicit seam signaling

- **D-04:** The first likely gap is seam discoverability, not seam existence. `AsterGraphEditorOptions` already exposes many replaceable services and providers, but current feature descriptors do not yet advertise enough of that surface.
- **D-05:** Phase 18 should prefer explicit readiness descriptors for optional services/integrations such as fragment workspace, fragment library, clipboard payload serialization, localization, node presentation, context-menu augmentation, diagnostics sink, and instrumentation.
- **D-06:** Keep readiness descriptors data-only and additive. Do not introduce plugin-loader-specific contract types in this phase.

### Proof strategy

- **D-07:** Use three proof layers together:
  - focused regression/contract tests for descriptor and seam correctness
  - `tools/AsterGraph.HostSample` for human-readable readiness explanation
  - `tools/AsterGraph.PackageSmoke` and `tools/AsterGraph.ScaleSmoke` for machine-checkable readiness markers
- **D-08:** `ScaleSmoke` should be part of the readiness story because future automation must remain credible under larger-graph session-driven workflows, not just tiny sample graphs.

### Scope control

- **D-09:** Avoid turning this phase into a partial plugin system. If a planned change starts looking like loader/runtime-discovery infrastructure, it belongs after the milestone closes.
- **D-10:** Preserve the Phase 17 migration markers as baseline proof rather than replacing them. Phase 18 should extend the proof ring, not reset it.

### the agent's Discretion

- Whether readiness descriptors should enumerate every optional seam individually or group some of them by category when that still keeps proof explicit.
- Whether the best automation proof helper belongs in `GraphEditorProofRingTests`, `GraphEditorServiceSeamsTests`, or a new focused readiness test file.
- How much of the final readiness story should live in package READMEs versus `docs/host-integration.md`, as long as one canonical explanation exists and sample/smoke outputs line up with it.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope and current state
- `.planning/PROJECT.md`
- `.planning/REQUIREMENTS.md`
- `.planning/ROADMAP.md`
- `.planning/STATE.md`
- `.planning/codebase/ARCHITECTURE.md`
- `.planning/codebase/CONCERNS.md`

### Carry-forward phase context
- `.planning/phases/15-capability-and-descriptor-contract-normalization/15-03-SUMMARY.md`
- `.planning/phases/16-avalonia-adapter-boundary-cleanup/16-03-SUMMARY.md`
- `.planning/phases/17-compatibility-lock-and-migration-proof/17-01-SUMMARY.md`
- `.planning/phases/17-compatibility-lock-and-migration-proof/17-02-SUMMARY.md`
- `.planning/phases/17-compatibility-lock-and-migration-proof/17-03-SUMMARY.md`

### Canonical runtime and seam contracts
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs`
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs`
- `src/AsterGraph.Editor/Runtime/GraphEditorFeatureDescriptorSnapshot.cs`
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs`
- `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs`
- `src/AsterGraph.Editor/Menus/GraphContextMenuAugmentationContext.cs`
- `src/AsterGraph.Editor/Presentation/NodePresentationContext.cs`
- `src/AsterGraph.Editor/Diagnostics/GraphEditorInstrumentationOptions.cs`

### Existing proof surfaces
- `tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsContractsTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorProofRingTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs`
- `tools/AsterGraph.HostSample/Program.cs`
- `tools/AsterGraph.PackageSmoke/Program.cs`
- `tools/AsterGraph.ScaleSmoke/Program.cs`
- `docs/host-integration.md`
- `docs/quick-start.md`
- `src/AsterGraph.Editor/README.md`
- `src/AsterGraph.Avalonia/README.md`

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets

- `AsterGraphEditorOptions` already exposes the replaceable seams Phase 18 cares about: workspace, fragment workspace, fragment library, clipboard payload serializer, context-menu augmentation, node presentation, localization, diagnostics sink, and instrumentation.
- `GraphContextMenuAugmentationContext` and `NodePresentationContext` already have a stable `Session` plus stable context data, with compatibility editor/node properties explicitly marked as migration-only.
- `GraphEditorDiagnosticsContractsTests`, `GraphEditorServiceSeamsTests`, and `GraphEditorProofRingTests` already validate parts of the contract/proof surface; Phase 18 can extend them instead of creating a second testing style.
- `HostSample`, `PackageSmoke`, and `ScaleSmoke` already cover human-readable host composition, machine-readable package consumption, and large-graph runtime driving.

### Established Patterns

- Recent phases succeed when the same story appears in three places: focused tests, sample output, and smoke markers.
- Runtime discoverability has been moving toward explicit descriptors rather than inferred MVVM shape.
- Compatibility-only properties are already being retained as additive shims with clear remarks rather than as canonical inputs.

### Integration Gaps

- `GraphEditorKernel.GetFeatureDescriptors()` currently advertises capabilities, workspace, diagnostics, and instrumentation, but it does not yet surface several optional seams already available through `AsterGraphEditorOptions`.
- `PackageSmoke` and `HostSample` currently prove migration and service continuity, but not an explicit plugin/automation readiness baseline.
- `ScaleSmoke` proves large-graph runtime behavior, but it is still positioned as v1.1 scaling evidence rather than Phase 18 readiness proof.

</code_context>

<specifics>
## Specific Ideas

- Add explicit readiness descriptors for the optional service/integration seams that later plugin/automation work would need to detect without poking internal objects.
- Extend contract/proof tests so inspection snapshots and feature descriptors jointly prove those seams are visible from the canonical runtime boundary.
- Add Phase 18 proof markers to:
  - `HostSample` for human-readable readiness reporting
  - `PackageSmoke` for machine-checkable descriptor/seam coverage
  - `ScaleSmoke` for machine-checkable automation-at-scale coverage
- Update host/package docs so the readiness story points to the same proof surfaces rather than relying on milestone memory.

</specifics>

<deferred>
## Deferred Ideas

- Runtime plugin loading and discovery
- Plugin packaging/version negotiation
- Dedicated automation DSL or macro surface
- New non-Avalonia presentation stacks
- Reopening compatibility-facade removal or history semantics

</deferred>

---

*Phase: 18-plugin-and-automation-readiness-proof-ring*  
*Context gathered: 2026-04-08*
