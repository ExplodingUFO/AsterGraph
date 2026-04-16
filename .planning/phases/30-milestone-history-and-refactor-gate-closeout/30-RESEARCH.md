# Phase 30 Research: Milestone History And Refactor Gate Closeout

**Date:** 2026-04-16
**Phase:** 30-milestone-history-and-refactor-gate-closeout

## Research Questions

1. What is actually missing from the current milestone history, and how reliable is the original `v1.4` planning snapshot?
2. Which checked-in command path should own the new hotspot-sensitive maintenance gate?
3. Where do the current repo docs already describe the live proof ring and carried concerns?
4. How should Phase 30 stay narrow so it prepares the next refactor phases instead of reopening them?

## Findings

### 1. The `v1.4` archive gap is real and still visible in current planning

The current tree has archived milestone files for `v1.2`, `v1.3`, and `v1.5`, but not for `v1.4`. `ROADMAP.md` still lists `v1.4` as complete-but-waiting-for-archive, and `STATE.md` still carries that as active maintenance debt.

Implication:

- Phase 30 should create checked-in `v1.4` archive files under `.planning/milestones/`.
- Phase 30 should update the top-level milestone ledger so `v1.4` is no longer the one missing historical link in an otherwise archived sequence.

### 2. The `v1.4` archive must be retrospective, not a naive copy of current repo state

The original `v1.4` milestone framing exists in git history:

- requirements at `7b99800`
- roadmap at `5622eb7`

But the shipped work between `7b99800` and `ec8d566` did not stop exactly at the original four planned phases. That range includes the expected plugin-loading, automation, and proof-ring commits, plus later trust/discovery/staging hardening and follow-up refactor work before `v1.5` started.

Implication:

- The archived `v1.4-ROADMAP.md` and `v1.4-REQUIREMENTS.md` should be reconstructed from the historical snapshots, not rewritten from the current tree.
- The milestone ledger entry should explicitly say the archive is retrospective and that the delivered surface extended beyond the original four-phase framing before `v1.5` initialization.
- Phase 30 should prefer honest notes over fake precision when summarizing shipped scope and git range.

### 3. The new refactor guardrail should extend `eng/ci.ps1`, not create a second command story

The repo already has one checked-in script-first validation entrypoint in `eng/ci.ps1`, and CI already reuses it. Today it exposes `restore`, `build`, `test`, `release`, and `all`, but nothing focused on hotspot-sensitive maintenance work during refactors.

Relevant current assets:

- `release` already handles package validation, `PackageSmoke`, `ScaleSmoke`, and coverage.
- `all` already gives a faster build-and-test path.
- focused hotspot test suites already exist across `GraphEditorMutationCompatibilityTests`, `GraphEditorFacadeMutationParityTests`, `GraphEditorSessionTests`, `GraphEditorTransactionTests`, `GraphEditorMigrationCompatibilityTests`, `GraphEditorFacadeRefactorTests`, `GraphEditorViewModelProjectionTests`, and `NodeCanvasPointerInteractionCoordinatorTests`.
- `docs/plans/2026-04-13-god-code-split-foundation.md` already identifies this family of suites as the right guardrail surface for hotspot splitting.

Implication:

- Phase 30 should add one new targeted lane to `eng/ci.ps1` rather than document a loose command bundle.
- That lane should be materially narrower than `release`, centered on hotspot refactor feedback, and still reuse maintained proof surfaces such as `ScaleSmoke`.

### 4. The live proof ring is already present; the gap is consistency and discoverability

The repo already documents the maintained proof ring in `README.md`, `docs/quick-start.md`, `docs/host-integration.md`, and `.planning/PROJECT.md`:

- `tools/AsterGraph.PackageSmoke`
- `tools/AsterGraph.ScaleSmoke`
- core SDK regression lanes
- demo/sample regression lane

The problem is no longer whether the proof ring exists. The problem is that the current milestone history is partially normalized and there is still no single checked-in maintenance gate for hotspot refactors.

Implication:

- Phase 30 should not introduce new proof tools.
- Phase 30 should sync top-level planning/docs around the archive and the new maintenance lane so contributors do not need stale phase directories to orient themselves.

### 5. The carried concern for the next phase is already explicit

`STATE.md` still marks the `STATE_HISTORY_OK` mismatch as carried maintenance debt, and `GraphEditorTransactionTests.cs` already contains the two high-signal history/save semantic tests that Phase 31 intends to tighten further.

Implication:

- Phase 30 should describe the concern clearly and leave semantic closure to Phase 31.
- Do not widen Phase 30 into fixing history/save behavior directly.

## Risks And Guardrails

- Do not rewrite `v1.4` history as if it had been archived cleanly at the time; treat archive reconstruction as retrospective work backed by explicit git evidence.
- Do not add a second maintenance script, README-only command bundle, or workflow YAML-only logic; keep `eng/ci.ps1` as the single command surface.
- Do not let the new maintenance lane masquerade as a release gate; `release` must remain the publish-facing full proof path.
- Do not consume Phase 30 on `GraphEditorViewModel`, `GraphEditorKernel`, or `NodeCanvas` structural changes. Those belong to later phases.

## Recommended Planning Posture

### Wave 1: Reconstruct and archive `v1.4`

Recover the original `v1.4` roadmap and requirements from git history, write the archive files into `.planning/milestones/`, and add a retrospective milestone ledger entry that explains the shipped outcome honestly.

### Wave 2: Add one script-first maintenance/refactor lane

Extend `eng/ci.ps1` with a targeted maintenance lane that exercises the hotspot-sensitive regression surface and `ScaleSmoke` without requiring contributors to hand-assemble filters.

### Wave 3: Sync current planning/docs around the archive and new lane

Update the live roadmap/state/project and contributor-facing verification docs so the current proof ring, carried concerns, and next action all line up with the new archive and maintenance command.

## Recommendation

Plan Phase 30 as a narrow normalization phase:

1. archive the missing `v1.4` history from explicit git evidence,
2. add one checked-in maintenance/refactor gate through `eng/ci.ps1`,
3. synchronize the top-level planning/docs to that same reality.

That closes historical drift without stealing execution time from the history/save proof work queued for Phase 31.

---

*Research complete: 2026-04-16*
