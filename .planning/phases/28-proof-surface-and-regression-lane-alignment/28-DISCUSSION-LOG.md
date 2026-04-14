# Phase 28 Discussion Log

**Date:** 2026-04-14
**Phase:** 28-proof-surface-and-regression-lane-alignment
**Mode:** auto

## Topic 1: How to handle stale `HostSample` references

| Option | Notes | Selected |
|--------|-------|----------|
| Recreate a new `HostSample` immediately | Keeps old narrative shape, but adds new implementation scope before proof/doc drift is even cleaned up. | |
| Remove stale `HostSample` references unless a real replacement is required | Makes the live tree the source of truth and keeps the phase focused on alignment. | X |

**Auto-choice:** Remove stale `HostSample` claims first. A new minimal host sample can be added later only if docs still need one after the proof surface is aligned.

## Topic 2: What to do with `ScaleSmoke`

| Option | Notes | Selected |
|--------|-------|----------|
| Keep `ScaleSmoke` out of the solution but keep mentioning it everywhere | Leaves the current mismatch between docs, automation, and solution membership in place. | |
| Treat `ScaleSmoke` as a first-class proof tool and align tracked entry points around it | Matches the existing docs and the Phase 27 `eng/ci.ps1` validation surface. | X |

**Auto-choice:** Keep `ScaleSmoke` as first-class proof and align tracked entry points to match that reality.

## Topic 3: How to separate regression lanes

| Option | Notes | Selected |
|--------|-------|----------|
| Keep demo tests mixed into `AsterGraph.Editor.Tests` | Leaves failures ambiguous between core SDK regressions and sample/demo regressions. | |
| Split demo-coupled tests into a dedicated lane/project | Makes proof ownership and regression triage clearer without changing the publishable package boundary. | X |

**Auto-choice:** Split demo-coupled tests into a dedicated lane so the core SDK regression lane reflects the supported package boundary more clearly.

## Topic 4: How broadly to edit docs

| Option | Notes | Selected |
|--------|-------|----------|
| Rewrite all docs around a new narrative | High churn and likely to mix Phase 28 alignment with Phase 29 adoption-path work. | |
| Update only the docs and planning artifacts that materially describe the live verification surface | Keeps the phase narrowly tied to proof-surface truthfulness. | X |

**Auto-choice:** Restrict Phase 28 to the docs, planning/codebase maps, solution membership, and test-lane changes needed to make the current verification surface trustworthy.

## Locked Outcomes

- Remove stale `HostSample` claims unless a real maintained replacement proves necessary during implementation.
- Align `ScaleSmoke` with tracked verification entry points if it remains part of the first-class proof story.
- Separate demo-specific regressions from the core SDK test lane.
- Keep Phase 28 focused on verification-surface truthfulness, not release-validation gates or a broader adoption-doc rewrite.
