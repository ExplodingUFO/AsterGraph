# Phase 29 Discussion Log

**Date:** 2026-04-14
**Phase:** 29-release-validation-and-canonical-adoption-path
**Mode:** auto

## Topic 1: Where release validation should live

| Option | Notes | Selected |
|--------|-------|----------|
| Add a brand-new standalone release script unrelated to the repo gate | Creates a second command path and repeats the drift problem Phase 27 tried to remove. | |
| Extend `eng/ci.ps1` with a dedicated release-validation lane | Keeps one repo-local command story for contributors and CI. | X |

**Auto-choice:** Keep `eng/ci.ps1` as the canonical command surface and add a release-validation lane there.

## Topic 2: How smoke proof should behave in release automation

| Option | Notes | Selected |
|--------|-------|----------|
| Continue building `PackageSmoke` / `ScaleSmoke` only | Leaves release proof dependent on manual README commands. | |
| Execute both smoke tools inside the release-validation lane | Makes release proof machine-checkable inside the tracked command path. | X |

**Auto-choice:** Execute both smoke tools explicitly in release validation while preserving their separate roles.

## Topic 3: How to expose the canonical host adoption path

| Option | Notes | Selected |
|--------|-------|----------|
| Keep route guidance duplicated across README, Quick Start, and Host Integration | Keeps the current documentation surface broader than it needs to be. | |
| Make Quick Start the short canonical decision path and let other docs point back to it | Keeps one concise entrypoint and reduces route-tree drift. | X |

**Auto-choice:** Keep `docs/quick-start.md` as the short canonical host-adoption entrypoint.

## Topic 4: How broadly to expand scope

| Option | Notes | Selected |
|--------|-------|----------|
| Add a new host sample and broader doc rewrite in the same phase | Increases churn and reopens solved proof-surface questions. | |
| Close validation and adoption-path gaps around the current maintained surfaces only | Stays aligned with the v1.5 roadmap and Phase 28 baseline. | X |

**Auto-choice:** Use the current maintained surfaces (`eng/ci.ps1`, `PackageSmoke`, `ScaleSmoke`, `AsterGraph.Demo`, Quick Start, Host Integration) rather than adding a new sample.

## Locked Outcomes

- Release validation stays on one repo-local command path rooted in `eng/ci.ps1`.
- `PackageSmoke` and `ScaleSmoke` are executed, not just built, inside the release-validation path.
- Quick Start remains the short canonical adoption entrypoint.
- Phase 29 closes validation and host-adoption gaps without reopening package-boundary or compatibility-retirement scope.
