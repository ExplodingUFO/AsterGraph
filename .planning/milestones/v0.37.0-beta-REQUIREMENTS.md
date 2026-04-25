# Requirements: AsterGraph v0.37.0-beta Authoring Surface Polish

**Defined:** 2026-04-25
**Core Value:** External hosts can embed AsterGraph and get a host-native, definition-driven authoring experience where commands, editing, inspection, trust decisions, and semantic workflows feel product-complete without rebuilding the toolkit.

## v1 Requirements

### Parameter / Metadata Contract

- [ ] **PARAM-01**: Hosts can define parameter metadata with `defaultValue`, `isAdvanced`, `helpText`, `placeholderText`, `constraints`, and `groupName` through one shared vocabulary.
- [ ] **PARAM-02**: The same parameter metadata contract drives the inspector, selected-node read/write seam, and multi-selection batch editing without host-side duplication.
- [ ] **PARAM-03**: Parameter validation messages and read-only reasons are surfaced consistently across inspector, node-side editors, badges, and status cues.
- [ ] **PARAM-04**: The support-bundle `parameterSnapshots` evidence reflects the exact parameter state, validation results, and metadata contract version at capture time.

### Custom Node / Port / Edge Authoring

- [ ] **AUTHOR-01**: Custom nodes support multiple input/output handles with host-declared port grouping and validation rules on the canonical session route.
- [ ] **AUTHOR-02**: Nodes expose a resize affordance (hover, drag, and live feedback) that stays within the defended latency budget on the canonical route.
- [ ] **AUTHOR-03**: Node and edge quick tools (toolbar, context actions) project from the shared command descriptor surface without adapter-specific presenter code.
- [ ] **AUTHOR-04**: Reconnect, temporary edge preview, and delete-on-drop gestures work on the canonical route with explicit command routing and undo support.
- [ ] **AUTHOR-05**: Port grouping and validation errors are visible in the default Avalonia host without custom discovery plumbing.

### Host Copyability and Recipes

- [ ] **RECIPE-01**: The hosted route ladder (`Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`) contains explicit "copy-from-here" paragraphs in public docs.
- [ ] **RECIPE-02**: A plugin-host recipe documents how a real host loads, trusts, and communicates with plugins on the canonical session route.
- [ ] **RECIPE-03**: A custom-node host recipe documents how a real host registers and styles custom nodes, ports, and edges without adapter-specific runtime API.
- [ ] **RECIPE-04**: A migration route comparison doc explains when to choose `CreateSession(...)` vs retained `GraphEditorViewModel` / `GraphEditorView`, with staging steps.
- [ ] **RECIPE-05**: The support-bundle format and issue template are actively used to collect and triage external adopter feedback; target is 3–5 real external reports.

### Performance Guardrails

- [x] **PERF-01**: Authoring-interaction latency budgets (inspector open, node resize, edge create, command palette) have defended proof markers that fail loudly on regression.
- [x] **PERF-02**: The 1000-node `large` ScaleSmoke tier remains a defended budget with an explicit pass/fail contract in release validation.
- [x] **PERF-03**: The 5000-node `stress` tier publishes p50/p95 telemetry in prerelease proof summaries but is NOT marketed as a defended public claim.

## v2 Requirements

Deferred to future milestones after real external feedback clusters.

### Wider Performance Claims

- **PERF-V2-01**: Defended performance budgets beyond 1000 nodes only after adopter demand and trustworthy telemetry exist.
- **PERF-V2-02**: Stress-tier upgrade to defended status only after reproducible external validation.

### Advanced Authoring Gestures

- **AUTHOR-V2-01**: Drag-to-reconnect previews with full animation fidelity.
- **AUTHOR-V2-02**: Drop-to-empty disconnect semantics.
- **AUTHOR-V2-03**: Pointer-only edge gestures without keyboard fallback.

## Out of Scope

| Feature | Reason |
|---------|--------|
| WPF parity as a headline | WPF remains validation-only/partial; this milestone is driven by the Avalonia canonical route. |
| Adapter-2 onboarding route | Adapter-2 stays validation-only; no new onboarding promises. |
| New capability waves (marketplace, sandboxing, SSR, multiplayer) | Explicit pre-stable non-goals; authoring surface polish comes first. |
| Version/release narrative cleanup as primary work | Noted and accepted as secondary cleanup, not a milestone deliverable. |
| Stable / GA / `1.0` positioning | The project remains on `0.xx` beta framing and still lacks enough real-world validation. |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| PARAM-01 | Phase 274 | Complete |
| PARAM-02 | Phase 274 | Complete |
| PARAM-03 | Phase 275 | Complete |
| PARAM-04 | Phase 275 | Complete |
| AUTHOR-01 | Phase 276 | Complete |
| AUTHOR-02 | Phase 276 | Complete |
| AUTHOR-03 | Phase 277 | Complete |
| AUTHOR-04 | Phase 277 | Complete |
| AUTHOR-05 | Phase 277 | Complete |
| RECIPE-01 | Phase 278 | Complete |
| RECIPE-02 | Phase 278 | Complete |
| RECIPE-03 | Phase 279 | Complete |
| RECIPE-04 | Phase 279 | Complete |
| RECIPE-05 | Phase 280 | Complete |
| PERF-01 | Phase 280 | Complete |
| PERF-02 | Phase 280 | Complete |
| PERF-03 | Phase 280 | Complete |

**Coverage:**
- v1 requirements: 15 total
- Mapped to phases: 15
- Unmapped: 0 ✓

---
*Requirements defined: 2026-04-25*
*Last updated: 2026-04-25 after initializing `v0.37.0-beta Authoring Surface Polish`*
