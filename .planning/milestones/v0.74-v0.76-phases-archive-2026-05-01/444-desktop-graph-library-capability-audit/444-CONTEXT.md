# Phase 444: Desktop Graph Library Capability Audit - Context

**Gathered:** 2026-04-30
**Status:** Ready for planning
**Mode:** Autonomous smart discuss with recommended defaults accepted

<domain>
## Phase Boundary

Phase 444 is a read-only capability audit. It defines the library-grade target for AsterGraph as a cross-platform desktop node graph library and maps current defended evidence against the v0.75 requirements. It does not change source code, public APIs, docs, templates, tests, runtime behavior, compatibility paths, or fallback behavior.

</domain>

<decisions>
## Implementation Decisions

### Audit Shape
- Treat the audit as the dependency gate for Phases 445, 446, and 447.
- Separate current defended capability from aspirational target language.
- Prefer specific file/test/doc evidence over broad product claims.
- Convert gaps into narrow later-phase implementation targets instead of designing a new architecture.

### Scope Control
- Do not implement rendering, interaction, customization, packaging, or documentation changes in Phase 444.
- Do not add compatibility layers, fallback rendering, scripting, macro systems, unsupported adapters, or new graph-size claims.
- Keep WPF described as validation-only unless later phases produce defended evidence for more.
- Keep Demo as proof/example surface; supported contracts must remain in packages, public APIs, samples, docs, and proof markers.

### Evidence Categories
- Rendering and viewport: scene projection, pan/zoom, fit-view, minimap, controls, large-graph budgets, and performance policy.
- Interaction and connection: selection, drag, hover, connection lifecycle, reconnect, keyboard commands, state events, and query APIs.
- Customization and host surface: custom nodes, edges, overlays, handles, markers, inspector, runtime decorations, templates, examples, and release gates.
- Proof and docs: ConsumerSample, HelloWorld, Demo cookbook, ScaleSmoke, docs, CI-sensitive gates, and public API inventory.

### The Agent's Discretion
- Use read-only parallel exploration for evidence coverage, then write one integrated audit artifact.
- If a gap is already partly defended, phrase the next target as "tighten/prove/document" rather than "build from scratch".
- If evidence is runtime-only, avoid calling it visible rendering proof until an Avalonia visual/projection gate exists.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- Runtime/session contracts: `IGraphEditorSession`, `IGraphEditorCommands`, `IGraphEditorQueries`, `IGraphEditorEvents`, `GraphEditorSessionCommands`, and `GraphEditorSessionQueries`.
- Viewport/runtime primitives: `GraphEditorKernelViewportCoordinator`, `ViewportMath`, `ViewportState`, `NodeCanvas`, `GraphMiniMap`, and `AsterGraphWorkbenchPerformancePolicy`.
- Interaction primitives: `NodeCanvasPointerInteractionCoordinator`, `NodeCanvasNodeDragCoordinator`, `NodeCanvasInteractionSession`, command descriptors, and selection/connection mutation coordinators.
- Customization primitives: `IGraphNodeVisualPresenter`, `GraphNodeVisual`, `GraphNodeVisualContext`, `AsterGraphPresentationOptions`, `IGraphRuntimeOverlayProvider`, inspector presenters, and parameter editor registries.
- Proof surfaces: ConsumerSample proof markers, Demo cookbook projection tests, HelloWorld starter tests, WPF validation tests, ScaleSmoke budget tests, and docs assertions.

### Established Patterns
- Session-first runtime ownership remains canonical; retained view-model APIs are migration support, not the main product boundary.
- Avalonia is the shipped hosted adapter. WPF is validation-only and partial.
- Proof markers are explicit string outputs asserted by tests.
- Performance claims are acceptable only when tied to repeatable tests or proof output.
- Docs and planning artifacts describe target capabilities directly and avoid external project naming.

### Integration Points
- Phase 445 should start with `NodeCanvas`, `NodeCanvasSceneHost`, `NodeCanvasConnectionSceneRenderer`, `GraphMiniMap`, viewport coordinator, ScaleSmoke, and performance-policy tests.
- Phase 446 should start with session commands/queries/events, command router, connection mutation coordinator, pointer interaction coordinator, and shortcut docs/tests.
- Phase 447 should start with presentation options, node visual presenter, runtime overlay provider, ConsumerSample visual/overlay examples, inspector metadata, and public API inventory.
- Phase 448 should start with templates, HelloWorld, ConsumerSample, WPF validation, package smoke, CI matrix, and host docs.
- Phase 449 should start with Demo cookbook catalog/projection, docs recipe ladder, ConsumerSample docs assertions, and proof markers.
- Phase 450 should start with requirements traceability, proof marker clustering, test gates, beads/Dolt/Git handoff, and release closure docs.

</code_context>

<specifics>
## Specific Ideas

- Use this audit as the handoff document for later parallel beads.
- Keep later implementation beads small and file-bounded.
- Build evidence before increasing "high-performance" wording.
- Use package-owned APIs and copyable samples as the contract boundary.

</specifics>

<deferred>
## Deferred Ideas

- Visible-scene rendering budgets, viewport controls, and minimap cadence belong to Phase 445.
- Adapter-neutral interaction snapshots, direct wire hit selection, bend-handle route editing, and shortcut doc repair belong to Phase 446.
- Extension surface docs/proof for custom node, edge overlay, runtime decoration, handles, markers, and inspector belong to Phase 447.
- Template/package/OS route proof belongs to Phase 448.
- Professional example suite consolidation belongs to Phase 449.
- Release proof closure belongs to Phase 450.

</deferred>
