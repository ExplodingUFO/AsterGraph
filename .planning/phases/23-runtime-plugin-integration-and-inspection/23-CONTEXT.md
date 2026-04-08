# Phase 23: Runtime Plugin Integration And Inspection - Context

**Gathered:** 2026-04-08
**Status:** Ready for planning
**Source:** v1.4 roadmap + Phase 22 closeout + direct plan-phase continuation

<domain>
## Phase Boundary

Phase 23 starts after Phase 22 published the first public plugin registration/loading path and proved that the canonical factory/options boundary can load plugins recoverably.

This phase is about:

- taking the plugin contribution contracts already published in `AsterGraph.Editor.Plugins` and making the supported ones affect live runtime behavior
- exposing loaded-plugin state, descriptors, contribution shape, and failures through canonical runtime inspection instead of only recent diagnostics text
- preserving parity between `AsterGraphEditorFactory.CreateSession(...)` and `AsterGraphEditorFactory.Create(...)` by reusing one composition result
- keeping plugin integration additive and rooted in the shipped session/descriptor/query boundary rather than inventing a second host runtime

This phase is not yet about:

- introducing an arbitrary service-container or dependency-injection plugin surface beyond the explicit contribution contracts already published in Phase 22
- adding plugin marketplace workflows, trust policy, signing, sandboxing, or unload/reload lifecycle guarantees
- implementing the automation runner planned for Phase 24
- broadening proof into `HostSample`, `PackageSmoke`, `ScaleSmoke`, and end-user docs; that full proof ring is Phase 25 scope

</domain>

<decisions>
## Implementation Decisions

### Contribution posture

- **D-01:** Limit live integration to the explicit plugin contribution types already published in Phase 22: node definition providers, context-menu augmentors, node presentation providers, and localization providers.
- **D-02:** Do not widen Phase 23 into arbitrary plugin-owned service injection. If a later milestone needs a broader runtime service model, it should be designed explicitly rather than smuggled in through this phase.
- **D-03:** Compose plugin node definitions additively without mutating the host-supplied `INodeCatalog` instance in place.

### Inspection posture

- **D-04:** `plugin.load.succeeded` / `plugin.load.failed` diagnostics remain important, but they are not sufficient for `PLUG-03` because they are history-oriented text events rather than a stable inspection read model.
- **D-05:** Add a stable plugin-load inspection/query surface to the canonical runtime boundary so hosts can inspect current plugin state without scraping UI state or diagnostic messages.
- **D-06:** Extend `GraphEditorInspectionSnapshot` with plugin-load state so the existing inspection story stays one-stop and session-first.

### Compatibility and precedence

- **D-07:** Runtime-first composition still flows through `AsterGraphEditorFactory.Resolve(...)`, `CreateSession(...)`, and `Create(...)`. Do not introduce a retained-only plugin integration path.
- **D-08:** Explicit host seams (`ContextMenuAugmentor`, `NodePresentationProvider`, `LocalizationProvider`) remain valid and keep final authority. Plugin contributions compose underneath or before those host-owned overrides.
- **D-09:** Plugin menu contribution should happen in the canonical `IGraphEditorQueries.BuildContextMenuDescriptors(...)` path first; retained compatibility `BuildContextMenu(...)` should inherit that result instead of re-implementing plugin behavior separately.

### Scope control

- **D-10:** Phase 23 may refine the internal plugin load result shape so it carries per-registration reports, contribution summaries, and failure data. It should not reopen the loader activation strategy chosen in Phase 22.
- **D-11:** Broader sample/smoke/docs proof remains Phase 25 work even if this phase adds new focused regressions or fixture behavior.

### the agent's Discretion

- The exact public names/shapes of the plugin inspection DTOs.
- Whether inspection discoverability should be expressed only through a new query or through both a query and an explicit feature descriptor.
- The exact merge order for plugin localization and presentation providers, as long as host-supplied providers keep the final override position.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope and active requirements
- `.planning/PROJECT.md` - v1.4 milestone framing and carry-forward constraints
- `.planning/REQUIREMENTS.md` - `PLUG-02`, `PLUG-03`
- `.planning/ROADMAP.md` - Phase 23 goal, dependency, and success criteria
- `.planning/STATE.md` - active project state and carry-forward concerns

### Carry-forward plugin baseline
- `.planning/phases/22-plugin-composition-contracts/22-CONTEXT.md` - public plugin contract and loader scope boundaries
- `.planning/phases/22-plugin-composition-contracts/22-RESEARCH.md` - loader and shared-contract guidance
- `.planning/phases/22-plugin-composition-contracts/22-VERIFICATION.md` - verified loader baseline and known warning posture
- `.planning/phases/18-plugin-and-automation-readiness-proof-ring/18-CONTEXT.md` - descriptor-first readiness posture this phase must preserve

### Canonical runtime and composition code
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs` - public host composition inputs
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` - canonical `Create(...)` / `CreateSession(...)` composition flow
- `src/AsterGraph.Editor/Plugins/GraphEditorPluginBuilder.cs` - explicit published contribution contracts from Phase 22
- `src/AsterGraph.Editor/Plugins/Internal/AsterGraphPluginLoader.cs` - current loader aggregation and diagnostic behavior
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs` - canonical query surface that needs plugin inspection support
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs` - canonical menu, feature, and inspection projection boundary
- `src/AsterGraph.Editor/Runtime/GraphEditorSessionDescriptorSupport.cs` - descriptor/inspection support object that can carry composed plugin state
- `src/AsterGraph.Editor/Diagnostics/GraphEditorInspectionSnapshot.cs` - canonical inspection DTO that currently lacks plugin load state
- `src/AsterGraph.Editor/Menus/GraphContextMenuAugmentationContext.cs` - retained host menu augmentation seam and compatibility posture
- `src/AsterGraph.Editor/Presentation/NodePresentationContext.cs` - retained node-presentation seam and compatibility posture
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` - retained compatibility surface that currently owns singular menu/presentation/localization providers

### Existing proof surfaces
- `tests/AsterGraph.Editor.Tests/GraphEditorPluginContractsTests.cs` - public contract baseline from Phase 22
- `tests/AsterGraph.Editor.Tests/GraphEditorPluginLoadingTests.cs` - current loader baseline and “not applied yet” assertions
- `tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsInspectionTests.cs` - inspection snapshot coverage precedent
- `tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs` - seam continuity coverage
- `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs` - canonical session query/descriptor coverage
- `tests/AsterGraph.Editor.Tests/GraphEditorProofRingTests.cs` - retained/runtime parity and proof-ring coverage
- `tests/AsterGraph.TestPlugins/SamplePlugin.cs` - fixture plugin that already contributes all currently published plugin seam types

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable assets

- `AsterGraphPluginLoader.Load(...)` already returns one aggregate contribution set plus loader diagnostics, so Phase 23 does not need to invent plugin discovery or activation from scratch.
- `GraphEditorPluginBuilder` already exposes explicit additive contribution types for node definitions, context-menu augmentation, node presentation, and localization.
- `GraphEditorSession` already owns canonical feature descriptors, framework-neutral context-menu descriptors, and `GraphEditorInspectionSnapshot`, making it the right place for plugin inspection and menu contribution integration.
- `GraphEditorViewModel.BuildContextMenu(...)` already begins from `Session.Queries.BuildContextMenuDescriptors(...)`, so retained compatibility can inherit canonical plugin menu results if composition happens in the session.

### Current gaps

- There is no canonical query or DTO that returns loaded-plugin state, descriptors, or failures; hosts only get `integration.plugin-loader` plus recent diagnostics.
- Plugin node definition providers are loaded but not applied to the active catalog.
- Plugin context-menu augmentors are loaded but not applied to the canonical `BuildContextMenuDescriptors(...)` path.
- Plugin localization and node-presentation providers are loaded but not composed into the runtime/retained provider chain.
- The internal loader result keeps successful descriptors and diagnostics, but it does not currently preserve a stable per-registration report suitable for inspection.

### Integration points

- `AsterGraphEditorFactory.Resolve(...)` remains the single place where plugin loading and composition should be normalized before either runtime or retained routes are built.
- `GraphEditorSessionDescriptorSupport` is the natural carrier for composed catalogs, localization delegates, plugin augmentors, and plugin load snapshots.
- `GraphEditorViewModel.ApplyNodePresentation(...)` and `LocalizeText(...)` are the retained-path touch points that need composite providers instead of singular host-only providers.
- The sample plugin fixture already contributes node definitions, menu augmentation, node presentation, and localization, so Phase 23 can prove real integration without inventing more fixture surface.

</code_context>

<specifics>
## Specific Ideas

- Add a new public plugin inspection DTO such as `GraphEditorPluginLoadSnapshot` plus a compact contribution summary DTO so hosts can inspect source, descriptor, outcome, and contribution shape.
- Expose plugin inspection through `IGraphEditorQueries` and carry the same data into `GraphEditorInspectionSnapshot`.
- Add a stable discoverability marker such as `query.plugin-load-snapshots` so hosts can detect the new inspection read intentionally.
- Use composed/wrapper implementations for catalog, localization, and node presentation rather than mutating host objects or creating retained-only plugin code paths.
- Fold plugin menu augmentors into the canonical session descriptor pipeline, then keep the existing compatibility adapter on top of that canonical result.

</specifics>

<deferred>
## Deferred Ideas

- Arbitrary plugin-owned runtime services or DI composition
- Plugin-defined automation, macros, or command handlers
- Plugin trust, signing, sandboxing, or marketplace distribution
- Collectible unload/reload lifecycle promises
- Full sample/smoke/doc proof ring refresh

</deferred>

---

*Phase: 23-runtime-plugin-integration-and-inspection*
*Context gathered: 2026-04-08*
