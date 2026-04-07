# Phase 15: Capability And Descriptor Contract Normalization - Context

**Gathered:** 2026-04-08  
**Status:** Ready for planning  
**Source:** v1.2 roadmap + Phase 14 closeout review + auto-discuss decisions

<domain>
## Phase Boundary

Phase 15 starts after the retained `GraphEditorViewModel.Session` path became adapter-backed in Phase 14.

This phase is about:

- making capability and optional-service discovery explicit on the canonical runtime path instead of forcing hosts to infer support from VM/object shape
- replacing MVVM-shaped command and menu control-plane contracts with stable descriptor- and ID-driven contracts
- keeping compatibility adapters available so existing `GraphEditorViewModel`, `MenuItemDescriptor`, and augmentor integrations can migrate in stages

This phase is not yet about:

- thinning Avalonia shell/canvas/input adapters
- final migration lock and long-horizon proof ownership
- runtime plugin loading or automation APIs beyond the descriptor seams they will later consume

</domain>

<decisions>
## Implementation Decisions

### Capability discovery

- **D-01:** The canonical runtime path should expose immutable capability/service descriptors, not just boolean `Can*` flags, so hosts can discover optional surfaces without inspecting VM properties or nullable service objects.
- **D-02:** `GraphEditorCapabilitySnapshot` remains as the lightweight compatibility snapshot for existing callers, but richer descriptor discovery should live beside it on the runtime/session path rather than replacing it abruptly.
- **D-03:** Explicit discovery must cover both command capability and optional host-facing services/features already modeled in composition, such as workspace persistence, fragment services, diagnostics, localization, presentation, menu augmentation, and instrumentation support.

### Command and menu contracts

- **D-04:** Canonical editor-layer command/menu contracts should be data-first and stable-ID-first; `RelayCommand`, `ICommand`, `ObservableCollection`, and `NodeTemplateViewModel` are compatibility implementation details, not the long-term public control plane.
- **D-05:** Menu construction should pivot to descriptor snapshots that reference stable command IDs and argument payloads, with compatibility adapters translating those descriptors back into existing `MenuItemDescriptor` / `ICommand` flows where needed.
- **D-06:** Command invocation should stay runtime-owned and kernel-shaped; descriptor rollout should not push execution semantics back into Avalonia or `GraphEditorViewModel`.

### Migration posture

- **D-07:** Existing augmentor and retained-facade paths stay available during this phase, but they should become clearly compatibility-oriented and consume canonical descriptor/runtime data whenever possible.
- **D-08:** Phase 15 should prefer additive canonical contracts plus compatibility shims over a one-shot public break, with stronger migration/proof locking still deferred to Phase 17.

### the agent's Discretion

- Exact naming of the new descriptor records/interfaces and whether they group capability, service, and command/menu metadata into one snapshot or a small cluster of related snapshots.
- Whether command execution is best keyed by stable command IDs plus payload objects, or by a small typed request record layer, as long as canonical contracts stop requiring MVVM command objects.
- Which compatibility shims should be marked as compatibility-only in this phase versus left as soft adapters until Phase 17.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope
- `.planning/PROJECT.md` - milestone constraints, publish-surface quality bar, and staged-migration expectations
- `.planning/REQUIREMENTS.md` - `CAP-01`, `CAP-02`, and the later-phase boundaries that must stay deferred
- `.planning/ROADMAP.md` - Phase 15 goal plus the dependency chain into phases 16-18
- `.planning/STATE.md` - current milestone state after Phase 14 closeout
- `.planning/phases/13-editor-kernel-state-owner-extraction/13-CONTEXT.md` - kernel extraction scope and deferrals
- `.planning/phases/14-session-and-compatibility-facade-decoupling/14-CONTEXT.md` - retained-facade and snapshot decisions Phase 15 builds on
- `.planning/phases/14-session-and-compatibility-facade-decoupling/14-01-SUMMARY.md`
- `.planning/phases/14-session-and-compatibility-facade-decoupling/14-02-SUMMARY.md`
- `.planning/phases/14-session-and-compatibility-facade-decoupling/14-03-SUMMARY.md`

### Current canonical runtime contracts
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs` - current session root and control-plane entrypoints
- `src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs` - current canonical command surface
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs` - current snapshot/read path plus compatibility query shim
- `src/AsterGraph.Editor/Runtime/GraphEditorCapabilitySnapshot.cs` - current boolean capability model and compatibility baggage
- `src/AsterGraph.Editor/Diagnostics/GraphEditorInspectionSnapshot.cs` - inspection surface that already reuses snapshot contracts
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs` - current composition-time optional service inputs
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` - canonical session path versus retained facade path

### MVVM-shaped command and menu seams to normalize
- `src/AsterGraph.Editor/Menus/MenuItemDescriptor.cs` - current menu contract still carries `ICommand`
- `src/AsterGraph.Editor/Menus/IGraphContextMenuHost.cs` - current internal menu host exposes `ICommand`, `NodeViewModel`, and `NodeTemplateViewModel`
- `src/AsterGraph.Editor/Menus/GraphContextMenuBuilder.cs` - current builder creates ad hoc `RelayCommand` wrappers and consumes VM objects directly
- `src/AsterGraph.Editor/Menus/GraphContextMenuAugmentationContext.cs` - current augmentor context already includes session/runtime data but still carries `CompatibilityEditor`
- `src/AsterGraph.Editor/Menus/ContextMenuContext.cs` - stable hit-test/menu context record that can remain canonical
- `src/AsterGraph.Editor/ViewModels/NodeTemplateViewModel.cs` - current template projection leaking into menu construction
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` - retained facade still exposes many `ICommand` properties and menu-facing VM seams
- `src/AsterGraph.Editor/Services/GraphEditorCommandStateNotifier.cs` - current `RelayCommand` fan-out utility that indicates command state still lives in MVVM command objects

### Existing validation surfaces
- `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs` - runtime contract and compatibility-shim assertions
- `tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsContractsTests.cs` - public diagnostics/inspection contract validation
- `tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs` - host-supplied services and augmentor continuity
- `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs` - retained/factory parity coverage
- `tests/AsterGraph.Editor.Tests/GraphEditorProofRingTests.cs` - proof-ring coverage
- `tests/AsterGraph.Editor.Tests/GraphEditorCommandStateNotifierTests.cs` - current MVVM command-state helper behavior
- `tools/AsterGraph.HostSample/Program.cs` - human-readable host proof path
- `tools/AsterGraph.PackageSmoke/Program.cs` - machine-checkable proof markers

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets

- `GraphEditorCapabilitySnapshot` already gives a lightweight immutable capability shape; it is the obvious compatibility layer for richer descriptor work rather than something to delete immediately.
- `ContextMenuContext` is already a stable, framework-neutral menu hit-test contract and should remain a canonical input to descriptor-driven menu building.
- `AsterGraphEditorOptions` already centralizes optional host service composition; Phase 15 can project those optional seams into runtime-visible descriptors instead of inventing a second source of truth.
- `GraphEditorInspectionSnapshot` already proves the project can reuse immutable runtime records to surface richer host-facing state safely.

### Established Patterns

- Recent phases favored additive canonical contracts plus compatibility shims marked by docs or `Obsolete` attributes instead of one-shot public breaks.
- The canonical runtime path now flows through `GraphEditorKernel` and `GraphEditorSession`; retained `GraphEditorViewModel` paths are expected to consume that runtime rather than define it.
- Snapshot/record-based reads are the preferred host-facing shape when a choice exists between live mutable objects and detached data.

### Integration Points

- `IGraphEditorQueries` is the natural place for capability/service discovery reads, because hosts already treat it as the read-only runtime surface.
- `GraphContextMenuBuilder` and `GraphContextMenuAugmentationContext` are the main place where command/menu descriptors can become canonical while leaving adapters behind for current shell code.
- `GraphEditorViewModel` and existing augmentor overloads are the compatibility boundary where descriptor-backed adapters will need to bridge to `ICommand` and other MVVM objects during migration.

</code_context>

<specifics>
## Specific Ideas

- `GraphContextMenuBuilder` currently manufactures many `RelayCommand` instances inline, which is a strong signal that the current menu contract is UI-command-shaped instead of runtime-command-shaped.
- `IGraphContextMenuHost` currently depends on `NodeTemplateViewModel`, `NodeViewModel`, and many `ICommand` properties; this is the densest CAP-02 seam in the editor layer.
- `GraphContextMenuAugmentationContext` already contains both `IGraphEditorSession` and `CompatibilityEditor`, which suggests the project is ready to make session/runtime data canonical and relegate the facade reference to a migration shim.
- `GraphEditorCapabilitySnapshot` currently exposes booleans such as undo/save/load support, but there is still no explicit contract for optional service discovery or for command/menu descriptor availability.

</specifics>

<deferred>
## Deferred Ideas

- Avalonia adapter thinning and shell/canvas command-routing cleanup stay in Phase 16.
- Final compatibility lock, longer-horizon obsoletion posture, and full drift-proof ownership stay in Phase 17.
- Plugin and automation readiness proof beyond descriptor seam creation stays in Phase 18.
- Actual runtime plugin loading and richer macro/automation APIs remain deferred milestone follow-ons.

</deferred>

---

*Phase: 15-capability-and-descriptor-contract-normalization*  
*Context gathered: 2026-04-08*
