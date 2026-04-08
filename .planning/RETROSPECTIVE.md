# Project Retrospective

*A living document updated after each milestone. Lessons feed forward into future planning.*

## Milestone: v1.2 — Kernel Extraction, Capability Contracts, and Plugin Readiness

**Shipped:** 2026-04-08
**Phases:** 6 | **Plans:** 18 | **Sessions:** Not tracked as a first-class metric

### What Was Built

- Extracted `GraphEditorKernel` and made the canonical runtime/session composition path kernel-first.
- Converted retained session/facade behavior into compatibility adapters over the shared runtime boundary.
- Shipped descriptor-based capability, command, and menu contracts plus thinner Avalonia routing/platform adapters.
- Locked migration and plugin/automation readiness into tests, `HostSample`, `PackageSmoke`, and `ScaleSmoke`.

### What Worked

- Tight phase scoping plus proof-ring closeout kept the architectural refactor from drifting.
- Sample and smoke markers made architectural claims machine-checkable instead of doc-only.

### What Was Inefficient

- No standalone milestone audit artifact was captured before closeout.
- The milestone archive tool did not infer accomplishments or task counts from the summaries, so manual cleanup was still required at archive time.

### Patterns Established

- Extract state ownership first, then normalize host contracts, then thin UI adapters, then lock proof.
- Keep canonical-vs-compatibility guidance synchronized across tests, samples, smoke tools, API remarks, and docs.

### Key Lessons

1. Architectural refactors stay controllable when every phase closes with runnable proof, not just unit tests.
2. Compatibility windows should be explicit adapters over the canonical runtime, not alternate owners of mutable state.
3. Descriptor-based contracts are a better long-term host surface than MVVM object shape when plugin and automation work is planned next.

### Cost Observations

- Model mix: Not tracked
- Sessions: Not tracked
- Notable: The milestone stayed manageable because each phase shipped proof artifacts alongside code, which reduced closeout rework and regression ambiguity.

---

## Cross-Milestone Trends

### Process Evolution

| Milestone | Sessions | Phases | Key Change |
|-----------|----------|--------|------------|
| v1.0 | Not tracked | 6 | Established the publishable SDK boundary and proof-oriented baseline |
| v1.1 | Not tracked | 6 | Hardened host boundaries, native Avalonia integration, and scaling hotspots |
| v1.2 | Not tracked | 6 | Moved the architecture to a kernel-first runtime with descriptor contracts and readiness proof |

### Cumulative Quality

| Milestone | Tests | Coverage | Zero-Dep Additions |
|-----------|-------|----------|-------------------|
| v1.0 | Proof ring established | Not tracked | Four-package SDK boundary |
| v1.1 | Host and scale regressions expanded | Not tracked | Native host integration and scale smoke hardening |
| v1.2 | Migration/readiness regressions plus sample/smoke/scale markers | Not tracked | Kernel runtime, descriptor contracts, and shared Avalonia adapters |

### Top Lessons (Verified Across Milestones)

1. Architectural claims need proof artifacts at the same time as the code that introduces them.
2. Host-facing surfaces stay healthier when the canonical contract is explicit and compatibility shims are clearly signposted.
