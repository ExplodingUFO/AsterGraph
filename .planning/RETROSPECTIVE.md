# Project Retrospective

*A living document updated after each milestone. Lessons feed forward into future planning.*

## Milestone: v1.6 — Facade Convergence and Proof Guardrails

**Shipped:** 2026-04-16
**Phases:** 4 | **Plans:** 12 | **Sessions:** Not tracked as a first-class metric

### What Was Built

- Reconstructed the missing `v1.4` milestone archive and aligned live planning/docs around the same proof story.
- Added a dedicated `maintenance` lane and then used it to close the carried retained history/save mismatch with focused proof.
- Moved more retained bootstrap, descriptor, compatibility-menu, and fragment orchestration out of `GraphEditorViewModel`.
- Split the next `GraphEditorKernel` and `NodeCanvas` hotspots behind dedicated helpers while tightening the package-boundary XML-doc guardrail.

### What Worked

- Treating v1.6 as a contraction milestone kept the work centered on semantic risk and maintenance cost instead of chasing new feature surface.
- The maintenance lane plus `ScaleSmoke` gave every hotspot move one repeatable proof path.

### What Was Inefficient

- `PROJECT.md` drifted behind live phase state and had to be corrected during the milestone audit.
- The archive tooling still needed manual closeout work for ROADMAP/PROJECT/RETROSPECTIVE shape, and `audit-open` crashed in the local GSD toolchain.

### Patterns Established

- Archive/history cleanup, semantic closure, hotspot contraction, and guardrail tightening make a workable sequence for SDK-hardening milestones.
- XML-doc debt should live at the owning project boundary, not at repo scope.

### Key Lessons

1. A carried proof mismatch should be treated as milestone work, not as a tolerated baseline footnote.
2. Hotspot reduction stays controlled when each extracted seam immediately joins the same maintenance gate used for other refactor-sensitive paths.
3. Milestone-close docs need the same rigor as code changes, or the integration story regresses even when tests stay green.

### Cost Observations

- Model mix: Not tracked
- Sessions: Not tracked
- Notable: The milestone moved quickly because each phase stayed narrow, but the final closeout still depended on manual planning/doc synthesis.

---

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
| v1.6 | Not tracked | 4 | Used a contraction milestone to close archive/history drift, shrink hotspots, and tighten guardrails without public API churn |

### Cumulative Quality

| Milestone | Tests | Coverage | Zero-Dep Additions |
|-----------|-------|----------|-------------------|
| v1.0 | Proof ring established | Not tracked | Four-package SDK boundary |
| v1.1 | Host and scale regressions expanded | Not tracked | Native host integration and scale smoke hardening |
| v1.2 | Migration/readiness regressions plus sample/smoke/scale markers | Not tracked | Kernel runtime, descriptor contracts, and shared Avalonia adapters |
| v1.6 | Maintenance lane, focused retained semantic suites, and hotspot seam tests | Not tracked | Archive closure, semantic proof, narrower facade/kernel/canvas seams, and scoped XML-doc debt |

### Top Lessons (Verified Across Milestones)

1. Architectural claims need proof artifacts at the same time as the code that introduces them.
2. Host-facing surfaces stay healthier when the canonical contract is explicit and compatibility shims are clearly signposted.
3. Milestone-close planning docs need the same integration discipline as runtime code, or the shipped story drifts even when proof commands stay green.
