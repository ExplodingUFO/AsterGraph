# Phase 5: Diagnostics & Integration Inspection - Context

**Gathered:** 2026-03-26
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase makes diagnostics and integration inspection first-class public contracts in `AsterGraph.Editor`. It is about machine-readable diagnostics, pull-based inspection snapshots, and host-standard logging or tracing hooks that let hosts troubleshoot editor integrations without parsing Avalonia UI text. It is not about adding a dedicated diagnostics workbench UI, not about moving diagnostics into `AsterGraph.Avalonia`, not about replacing the current runtime/session surface, and not about instrumenting every pointer gesture or frame-level render detail.

</domain>

<decisions>
## Implementation Decisions

### Diagnostics Contract Evolution
- **D-01:** Phase 5 should build on the existing public diagnostics baseline from Phase 2 instead of replacing it. `GraphEditorDiagnostic`, `IGraphEditorDiagnosticsSink`, and `GraphEditorRecoverableFailureEventArgs` remain valid foundations and should be extended into a fuller host-facing diagnostics story.
- **D-02:** `StatusMessage` remains a compatibility UX surface only. Hosts must be able to receive stable machine-readable diagnostics without scraping or parsing UI text.
- **D-03:** Diagnostics stay rooted in `AsterGraph.Editor`, not in `AsterGraph.Avalonia`. The diagnostics story should work for runtime-only hosts, stock Avalonia hosts, and retained compatibility hosts alike.

### Inspection Snapshot Scope
- **D-04:** Phase 5 should expose medium-grain inspection data, not raw mutable internals. Hosts need immutable inspection snapshots or equivalent read models for current document/session state, not direct access to live implementation objects.
- **D-05:** Inspection should aggregate the runtime state that integration authors actually need to troubleshoot: document/session identity, selection, viewport, pending connection state, capability or permission surfaces, current status, and recent diagnostics context where appropriate.
- **D-06:** Existing runtime/session query snapshots (`CreateDocumentSnapshot`, selection, viewport, capabilities, node positions) should be reused as building blocks rather than duplicated behind a second unrelated inspection model.

### Logging And Tracing Integration
- **D-07:** Logging and tracing should be opt-in and vendor-neutral. The public diagnostics story should align with host-standard .NET tooling instead of inventing a new AsterGraph-only logging framework.
- **D-08:** Logging or tracing instrumentation belongs in the editor runtime layer and host extension seams, not in Avalonia control code. Save/load, workspace/fragment operations, host-provider failures, and similar runtime events are in scope; frame-by-frame UI trace spam is not.

### Compatibility And Adoption
- **D-09:** Diagnostics and inspection should be reachable from both the canonical runtime/session path and the retained compatibility facade. Existing hosts should gain observability through planned migration, not a mandatory rewrite.
- **D-10:** Phase 5 should preserve the split established so far: `AsterGraph.Editor` owns diagnostics contracts and state inspection, while `AsterGraph.Avalonia` may later host a workbench UI on top of those contracts in a future phase.

### the agent's Discretion
- Exact contract names, record shapes, and namespace placement inside `src/AsterGraph.Editor`
- Whether inspection is one aggregate snapshot or a few focused snapshot records
- Whether host-standard logging and tracing integration uses direct .NET abstractions, adapter interfaces, or options-driven wiring

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase Scope And Locked Upstream Decisions
- `.planning/ROADMAP.md` — Phase 5 goal, requirements, and dependency on completed presenter and runtime seams
- `.planning/REQUIREMENTS.md` — `DIAG-01`, `DIAG-02`, `DIAG-03`
- `.planning/PROJECT.md` — package-library positioning, observability requirement, and migration constraints
- `.planning/STATE.md` — current milestone position and next-step anchor
- `.planning/phases/02-runtime-contracts-service-seams/02-03-SUMMARY.md` — current machine-readable diagnostics sink baseline
- `.planning/phases/02-runtime-contracts-service-seams/02-04-SUMMARY.md` — recoverable-failure publication and service seam wiring
- `.planning/phases/02-runtime-contracts-service-seams/02-05-SUMMARY.md` — host story proof around diagnostics and replaceable services
- `.planning/phases/04-replaceable-presentation-kit/04-04-SUMMARY.md` — Phase 4 proof ring that Phase 5 diagnostics must layer on top of without destabilizing host replacement

### Existing Diagnostics And Runtime Sources
- `src/AsterGraph.Editor/Diagnostics/GraphEditorDiagnostic.cs` — current machine-readable diagnostics record and severity model
- `src/AsterGraph.Editor/Diagnostics/IGraphEditorDiagnosticsSink.cs` — current host-replaceable diagnostics publication seam
- `src/AsterGraph.Editor/Events/GraphEditorRecoverableFailureEventArgs.cs` — current recoverable-failure event shape
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs` — current diagnostics publication path and runtime event batching
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs` — canonical runtime root that diagnostics and inspection should integrate with
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` — current status text, query helpers, and state needed for future inspection snapshots
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` — canonical host composition path through which diagnostics and inspection options may need to flow
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs` — current options surface for diagnostics sink and related runtime configuration

### Prior Architecture Research And Consumer Proof Points
- `.planning/research/ARCHITECTURE.md` — earlier diagnostics recommendations including `ILogger` + `ActivitySource` guidance and the anti-pattern of status-text-only diagnostics
- `tools/AsterGraph.HostSample/Program.cs` — current host proof output for runtime diagnostics and host-facing session queries
- `tools/AsterGraph.PackageSmoke/Program.cs` — current machine-checkable diagnostics sink markers and compatibility/runtime proof ring

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `GraphEditorSession.PublishRecoverableFailure(...)` already converts failures into `GraphEditorDiagnostic` and publishes them through `IGraphEditorDiagnosticsSink`, so Phase 5 can deepen the diagnostics taxonomy from this existing path instead of rebuilding publication from scratch.
- `GraphEditorViewModel` already exposes runtime-readable state such as document snapshots, node-position snapshots, pending-connection state, capability flags, permissions, viewport, selection, and current `StatusMessage`; these can feed a new inspection surface without inventing a parallel editor model.
- The current runtime query snapshots (`GraphEditorSelectionSnapshot`, `GraphEditorViewportSnapshot`, `GraphEditorCapabilitySnapshot`) already establish the project’s pattern for immutable, host-readable state records.
- Host sample and package smoke already prove diagnostics sink wiring and runtime-session events, so Phase 5 can extend those proof points rather than creating a brand-new validation story.

### Established Patterns
- Public host contracts live in `AsterGraph.Editor`; Avalonia is a consumer of those contracts, not the owner of them.
- New host-facing APIs use immutable records, query/read-model contracts, and options objects instead of exposing live implementation classes.
- Existing compatibility facades remain supported while canonical host setup flows through factory/options APIs and `IGraphEditorSession`.
- Machine-readable diagnostics should coexist with `StatusMessage`, not replace every existing UX string immediately.

### Integration Points
- New diagnostics or inspection contracts should land under `src/AsterGraph.Editor`, most likely alongside the existing `Diagnostics`, `Runtime`, `Events`, or `Models` folders.
- `AsterGraphEditorOptions` and `AsterGraphEditorFactory` are the canonical places to thread new diagnostics configuration or host-standard instrumentation hooks.
- `GraphEditorSession` is the clearest current boundary for emitting structured diagnostics and exposing inspection snapshots without Avalonia dependencies.
- `GraphEditorViewModel.Session` should remain a compatibility bridge so retained hosts can adopt the new diagnostics surfaces incrementally.

</code_context>

<specifics>
## Specific Ideas

- A host debugging integration problems should be able to answer “what is the editor trying to do right now?” and “what state is it currently in?” without opening a UI shell or scraping strings.
- The current diagnostics baseline is recoverable-failure-heavy; Phase 5 should broaden that into a more intentional taxonomy that also covers important non-failure runtime operations and host seam observations where justified.
- Inspection should feel like a stable support contract: enough information to troubleshoot current selection, viewport, pending operations, permissions, and recent failures, but not so much internal surface that it freezes today’s implementation details forever.
- Logging or tracing should fit naturally into .NET hosts and remain optional. The product requirement is observability, not a mandatory telemetry stack.

</specifics>

<deferred>
## Deferred Ideas

- A dedicated diagnostics workbench UI or overlay inside `AsterGraph.Avalonia`
- Interaction replay, gesture timelines, or frame-level render diagnostics
- Non-.NET logging ecosystems or any alternate presentation stack for diagnostics visualization

</deferred>

---

*Phase: 05-diagnostics-integration-inspection*
*Context gathered: 2026-03-26*
