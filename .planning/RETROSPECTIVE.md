# Project Retrospective

*A living document updated after each milestone. Lessons feed forward into future planning.*

## Milestone: v0.39.0-beta — Productized SDK Adoption Path

**Shipped:** 2026-04-26
**Phases:** 5 | **Plans:** 5

### What Was Built

- Public version, tag, release, NuGet, and local-planning wording now share one defended explanation and release validation gate.
- README and Demo now lead with a concrete AI pipeline scenario that can be launched with `--scenario ai-pipeline`.
- Demo includes a guided scenario tour covering custom nodes, typed connections, parameters, plugin trust, automation, save/load, and export.
- ConsumerSample now acts as a realistic five-minute hosted integration path with scenario graph, host actions, proof markers, and support-bundle readiness.
- `AsterGraphHostBuilder` provides a thin Avalonia hosted facade over the existing editor and view factories.

### What Worked

- Keeping each phase narrow made the adoption path easier to verify: public wording, demo scenario, tour, ConsumerSample, and builder proof each had separate tests.
- Proof markers and docs tests caught the user-facing claims without requiring manual GUI inspection.
- Worktree-per-phase execution kept feature implementation and closeout artifacts isolated until each phase was ready to merge.

### What Was Inefficient

- The GSD milestone completion helper did not accept the current `v0.39.0-beta` argument shape, so archive steps were completed manually.
- Phase-local `VALIDATION.md` artifacts were not produced; the milestone relied on `VERIFICATION.md`, focused regression tests, and proof output instead.

### Patterns Established

- Prefer scenario-led onboarding evidence over broad capability checklists.
- Treat local planning versions as internal labels and keep public package/tag wording anchored on the installable package version.
- Add thin facades only when they shorten host setup while delegating to the canonical runtime/session factories.

### Key Lessons

1. External adoption improvements should be defended by docs tests and proof markers, not only prose.
2. A short hosted builder can improve onboarding without changing runtime architecture when required inputs remain explicit.
3. Milestone closeout should avoid local Git tags when local planning labels could be confused with public release tags.

### Cost Observations

- Sessions: 1 closeout continuation after phase execution.
- Notable: manual archive was faster and safer than trying to force the current `gsd-sdk milestone.complete` helper through an incompatible argument path.

---

## Cross-Milestone Trends

### Process Evolution

| Milestone | Sessions | Phases | Key Change |
|-----------|----------|--------|------------|
| v0.39.0-beta | 1 | 5 | Adoption-path work was split into public wording, demo story, onboarding sample, and thin API proof. |

### Cumulative Quality

| Milestone | Tests | Coverage | Zero-Dep Additions |
|-----------|-------|----------|-------------------|
| v0.39.0-beta | Focused phase suites plus release/version validation | Requirements 18/18 | Thin builder added without new runtime dependency. |

### Top Lessons

1. Keep public consumption wording separate from local planning labels.
2. Use scenario proof and docs assertions to defend first-run adoption claims.
