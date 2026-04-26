# Requirements: AsterGraph v0.39.0-beta Productized SDK Adoption Path

**Defined:** 2026-04-26
**Core Value:** External hosts can embed AsterGraph and get a host-native, definition-driven authoring experience where commands, editing, inspection, trust decisions, and semantic workflows feel product-complete without rebuilding the toolkit.

## v1 Requirements

### Public Entry and Demo Story

- [ ] **DEMO-01**: External evaluator can understand AsterGraph from the README first viewport through one concrete visual that shows drag, connect, parameter editing, automation, and export in a real graph scenario.
- [ ] **DEMO-02**: External evaluator can launch a prebuilt scenario demo from the Demo host without starting from a blank canvas.
- [ ] **DEMO-03**: The scenario demo exercises custom nodes, parameter editing, connection validation, plugin/trust visibility, save/load, automation/proof output, and export from one coherent product story.
- [ ] **DEMO-04**: The Demo host exposes a guided tour or equivalent scenario flow that walks through creating nodes, connecting them, editing parameters, loading plugin content, and exporting output.

### Version and Release Clarity

- [ ] **REL-01**: Consumer-facing entry points clearly state the current installable package version, matching public tag/release expectation, package boundaries, and historical `v1.x` tag caveat without contradiction.
- [ ] **REL-02**: English and Chinese versioning docs distinguish NuGet/package SemVer, GitHub prerelease tags, GitHub Releases, and local planning-only milestone labels.
- [ ] **REL-03**: Release validation fails when `Directory.Build.props`, README/versioning docs, public tag, or generated release-note header disagree about the public package version.
- [ ] **REL-04**: Issue templates and public launch/release docs route "which version should I use?" questions to the same versioning source of truth.

### Onboarding and Samples

- [ ] **ONB-01**: New Avalonia hosts can follow a five-minute quick start from package install or local starter run to first graph load/save and first custom node definition.
- [ ] **ONB-02**: README and quick-start docs explain when to copy `Starter.Avalonia`, `HelloWorld`, `HelloWorld.Avalonia`, `ConsumerSample.Avalonia`, or the full Demo.
- [ ] **ONB-03**: `ConsumerSample.Avalonia` opens as a realistic host integration with a scenario graph, host-owned actions, parameter editing, trusted plugin flow, and support-bundle proof.
- [ ] **ONB-04**: ConsumerSample proof emits stable markers for scenario graph load, plugin trust state, parameter editing, export/support bundle readiness, and onboarding health.

### Thin API Access

- [ ] **API-01**: Avalonia hosts can compose the common hosted setup through a thin builder/facade that accepts document, catalog, compatibility, plugin trust, localization, and diagnostics inputs.
- [ ] **API-02**: The builder/facade delegates to the existing canonical `AsterGraphEditorFactory.CreateSession(...)` / `Create(...)` and Avalonia view factories instead of creating a parallel runtime path.
- [ ] **API-03**: Host integration docs show when to use the builder/facade, when to drop down to the canonical session/runtime route, and when retained `GraphEditorViewModel` migration remains appropriate.

### Proof and Scope Guardrails

- [ ] **PROOF-01**: CI or docs tests defend the README first-viewport claims, five-minute quick start, scenario demo launch path, and ConsumerSample scenario markers.
- [ ] **PROOF-02**: Public docs continue to describe the 1000-node tier as defended and the 5000-node tier as telemetry until a later milestone explicitly promotes that budget.
- [ ] **PROOF-03**: Plugin trust wording remains explicit that AsterGraph supports trusted in-process extension but does not provide sandboxing or untrusted-code isolation.

## v2 Requirements

Deferred to future milestones after the productized adoption path is coherent.

### Performance

- **PERF-V2-01**: Promote the 5000-node `stress` tier from informational telemetry to a defended release budget with clear p50/p95 thresholds.
- **PERF-V2-02**: Add viewport culling, incremental edge geometry updates, minimap downsampling, inspector projection caching, or export pipeline improvements only where profiling evidence justifies them.

### Plugin Ecosystem

- **PLUGIN-V2-01**: Provide a `dotnet new astergraph-plugin` template with manifest, node provider, parameter editor, and README.
- **PLUGIN-V2-02**: Provide a trusted plugin sample and trust-policy cookbook covering local-dev allow, hash allowlist, publisher/signature allow, block unknown source, and enterprise fixed directory patterns.
- **PLUGIN-V2-03**: Provide a manifest validator CLI that reports manifest, target framework, capability summary, and hash/signature status.

### API Governance

- **API-V2-01**: Add public API analyzer warnings for retained or compatibility-only surfaces.
- **API-V2-02**: Add public API baseline/diff gating for stable, retained, obsolete, and compatibility-only API categories.
- **TEMPLATE-V2-01**: Add `dotnet new astergraph-avalonia` once the five-minute hosted path and thin builder/facade stabilize.

## Out of Scope

| Feature | Reason |
|---------|--------|
| Runtime architecture rewrite | Current session-first/runtime-first architecture is already the defended direction; this milestone packages it for adoption. |
| WPF parity expansion | WPF remains validation-only until the target audience requires a second full adapter. |
| Plugin marketplace, remote install/update, unload lifecycle, or sandboxing | Trusted in-process plugins are enough for this milestone; untrusted plugin execution needs a separate security design. |
| Defended 5000-node performance budget | Valuable later, but this milestone is about first-run adoption and public trust clarity. |
| Stable / GA / `1.0` positioning | The project remains prerelease until the public adoption path and evidence loop are coherent. |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| REL-01 | Phase 285 | Pending |
| REL-02 | Phase 285 | Pending |
| REL-03 | Phase 285 | Pending |
| REL-04 | Phase 285 | Pending |
| PROOF-02 | Phase 285 | Pending |
| PROOF-03 | Phase 285 | Pending |
| DEMO-01 | Phase 286 | Pending |
| DEMO-02 | Phase 286 | Pending |
| DEMO-03 | Phase 287 | Pending |
| DEMO-04 | Phase 287 | Pending |
| ONB-01 | Phase 288 | Pending |
| ONB-02 | Phase 288 | Pending |
| ONB-03 | Phase 288 | Pending |
| ONB-04 | Phase 288 | Pending |
| API-01 | Phase 289 | Pending |
| API-02 | Phase 289 | Pending |
| API-03 | Phase 289 | Pending |
| PROOF-01 | Phase 289 | Pending |

**Coverage:**
- v1 requirements: 18 total
- Mapped to phases: 18
- Unmapped: 0

---
*Requirements defined: 2026-04-26*
