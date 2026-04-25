# Requirements: AsterGraph v0.38.0-beta External Adopter Feedback Loop

**Defined:** 2026-04-25
**Core Value:** External hosts can embed AsterGraph and get a host-native, definition-driven authoring experience where commands, editing, inspection, trust decisions, and semantic workflows feel product-complete without rebuilding the toolkit.

## v1 Requirements

### Intake and Feedback Collection

- [ ] **INTAKE-01**: The support-bundle format captures enough context (environment, metrics, reproduction steps, parameter snapshots) that a first-line triage can classify the report without asking the adopter for more information.
- [ ] **INTAKE-02**: The GitHub issue template actively guides adopters to attach a support bundle and describe their host integration context (framework version, adapter choice, custom node usage).
- [ ] **INTAKE-03**: There is a defined intake criteria checklist (minimum info needed) and a triage workflow that routes reports to docs-fix, sample-fix, or capability-gap buckets.

### Feedback-Driven Polish

- [ ] **POLISH-01**: Within one feedback cycle, any friction reported by ≥2 adopters on the same docs page or sample route gets a prioritized fix with a proof marker that fails on regression.
- [ ] **POLISH-02**: The `Starter.Avalonia → HelloWorld.Avalonia → ConsumerSample.Avalonia` ladder is validated by at least one external attempt (simulated or real) and gaps are documented and fixed.
- [ ] **POLISH-03**: Common onboarding failures (missing package reference, wrong initialization order, overlooked compatibility service) are detected by the proof harness and surfaced as explicit error markers, not silent defaults.

### Trust and Telemetry Hardening

- [ ] **TELEMETRY-01**: The support bundle includes a schema version and a public tag that map unambiguously to a released package version, eliminating "which build was this?" ambiguity.
- [ ] **TELEMETRY-02**: ConsumerSample proof markers include a "host health" snapshot (startup time, plugin scan, command latency) that can be compared across adopter environments without exposing host-internal data.
- [ ] **TELEMETRY-03**: The support-bundle write path validates JSON schema before persisting and emits a `SUPPORT_BUNDLE_SCHEMA_OK` marker; malformed bundles are rejected with a clear reason.

## v2 Requirements

Deferred to future milestones after the feedback loop has generated enough signal.

### Advanced Intake

- **INTAKE-V2-01**: Automated telemetry collection (opt-in) from real hosts in the wild.
- **INTAKE-V2-02**: Public issue dashboard or changelog derived from adopter feedback clusters.

## Out of Scope

| Feature | Reason |
|---------|--------|
| New capability waves (marketplace, sandboxing, SSR, multiplayer) | Explicit pre-stable non-goals; feedback loop comes first. |
| Major API refactoring based on single-adopter feedback | Require ≥2 independent reports or strong internal evidence before breaking changes. |
| WPF parity expansion | WPF remains validation-only; no new onboarding promises. |
| Stable / GA / `1.0` positioning | The project remains on `0.xx` beta framing and still lacks enough real-world validation. |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| INTAKE-01 | Phase 281 | Pending |
| INTAKE-02 | Phase 281 | Pending |
| INTAKE-03 | Phase 282 | Pending |
| POLISH-01 | Phase 282 | Pending |
| POLISH-02 | Phase 283 | Pending |
| POLISH-03 | Phase 283 | Pending |
| TELEMETRY-01 | Phase 284 | Pending |
| TELEMETRY-02 | Phase 284 | Pending |
| TELEMETRY-03 | Phase 284 | Pending |

**Coverage:**
- v1 requirements: 9 total
- Mapped to phases: 9
- Unmapped: 0 ✓

---
*Requirements defined: 2026-04-25*
