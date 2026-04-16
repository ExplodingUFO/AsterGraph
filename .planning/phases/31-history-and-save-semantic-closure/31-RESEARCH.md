# Phase 31 Research: History And Save Semantic Closure

**Date:** 2026-04-16
**Phase:** 31-history-and-save-semantic-closure

## Research Questions

1. What exactly is the carried `STATE_HISTORY_OK` mismatch, and is it still reproducible in the current tree?
2. Which code paths currently own retained history/save behavior, and where do they diverge from kernel-owned runtime state?
3. How should Phase 31 stay narrow so it closes the semantic contract without widening into the larger facade-convergence phase?
4. Which proof surfaces should change so contributors can trust the maintained maintenance lane after the semantic fix lands?

## Findings

### 1. The mismatch is real, reproducible, and narrower than the full facade-convergence problem

Current repo evidence shows a clean contradiction:

- focused retained history/save tests in `GraphEditorTransactionTests.cs` pass,
- but `tools/AsterGraph.ScaleSmoke` still emits `SCALE_HISTORY_OK:True:False:True:True`.

That means the carried concern is not documentation drift alone. The repo currently has two different observed answers for the same history/save story depending on which surface executes it.

Implication:

- Phase 31 should treat `STATE_HISTORY_OK` as a live proof mismatch, not a wording problem.
- The phase should close that mismatch first, then let Phase 32 resume broader facade slimming.

### 2. Retained history interaction still records state locally, while undo/redo can prefer kernel history

The retained facade still owns a local history stack and local pending interaction state:

- `GraphEditorViewModel.BeginHistoryInteraction()` captures a retained snapshot through `_pendingInteractionState`.
- `GraphEditorViewModel.CompleteHistoryInteraction(...)` compares retained snapshots and pushes them through `_historyService`.
- `GraphEditorHistoryStateCoordinator.MarkDirty(...)` also captures retained view-model snapshots and pushes local history.

But the same facade's command surface resolves undo/redo against kernel capability first:

- `GraphEditorViewModel.CanUndo` / `CanRedo` are `kernel OR local-history`.
- `GraphEditorViewModel.Undo()` / `Redo()` call `_kernel.Undo()` / `_kernel.Redo()` whenever the kernel reports capability, only falling back to local history when kernel history is empty.

Implication:

- Once any earlier runtime/session command has populated kernel history, later retained-only mutations can commit to one history owner while undo/redo execute against another.
- This is the semantic gap Phase 31 needs to close.

### 3. `ScaleSmoke` reproduces the mixed-owner path that the existing focused tests mostly avoid

`ScaleSmoke` performs kernel-owned session mutations before the retained drag/save sequence:

- selection and connection mutations run through `session.Commands`,
- then drag uses retained `BeginHistoryInteraction()` / `ApplyDragOffset()` / `CompleteHistoryInteraction()`,
- then save/undo/redo are observed from the retained facade.

The focused transaction tests mostly start from a clean editor and only exercise retained drag/save flows, so they do not strongly pressure the "kernel already has history, retained drag commits locally" path.

Implication:

- Phase 31 needs one focused regression that seeds kernel history first, then performs retained drag/save/undo/redo.
- That mixed-path regression should become the semantic contract anchor for the phase.

### 4. The local retained mutation path is broader than drag alone

The same retained/local history machinery is also used by other compatibility-only mutation finishers:

- `GraphEditorHistoryStateCoordinator.MarkDirty(...)` backs parameter edits,
- fragment paste/import uses `MarkDirty(...)`,
- layout helpers can still finish through retained dirty/history signaling after local projection changes.

Implication:

- Phase 31 should not hardcode a drag-only fix if the real contract is "retained local mutations must not fork history/save semantics from kernel-owned undo/redo."
- A shared retained-mutation commit path is the right level of change for this phase.

### 5. `GraphEditorTransactionTests` is carrying two unrelated responsibilities today

`GraphEditorTransactionTests.cs` currently mixes:

- runtime session mutation batching and kernel-owned event coverage,
- retained history interaction and save-boundary semantics.

That makes the maintenance lane rely on one broad file for two separate failure domains.

Implication:

- Phase 31 should split retained history/save regressions into smaller dedicated suites.
- The maintenance lane should point at those suites directly instead of a large catch-all transaction file.

### 6. The proof marker itself needs to become explicit, not just "different booleans"

`ScaleSmoke` currently prints raw dirty booleans:

- after move
- after save
- after undo
- after redo

That is useful for debugging, but it is not a stable pass/fail contract once the repo has already decided those states should mean something specific.

Implication:

- Phase 31 should turn the history proof into an explicit semantic assertion, not just a raw tuple that humans must interpret.
- If details remain useful, keep them as supplemental fields after an explicit pass/fail marker.

## Risks And Guardrails

- Do not change public factory/session/view-model entry points. This phase is about semantic closure, not public API churn.
- Do not widen into general `GraphEditorViewModel` extraction work. If the change mainly moves orchestration around without closing the history/save contract, it belongs to Phase 32.
- Do not break live drag feedback. Retained node movement still needs to feel immediate while the interaction is in progress.
- Do not leave the maintenance gate pointing at stale test bundles after the focused suites are introduced.

## Recommended Planning Posture

### Wave 1: Converge retained local mutation completion onto one history/save contract

Add one mixed-path regression, then make retained interaction completion and shared retained `MarkDirty(...)` flows commit back through the same history/save authority that undo/redo actually executes.

### Wave 2: Split the retained history/save regressions into focused suites and update the maintenance lane

Move retained history interaction, save-boundary, and drag-related regressions out of the broad transaction file so failures localize cleanly and the maintenance lane stays deliberate.

### Wave 3: Replace the carried `STATE_HISTORY_OK` mismatch with aligned proof output

Update `ScaleSmoke` and proof-ring coverage so the machine-checkable history marker expresses the same semantic contract enforced by the focused tests.

## Recommendation

Plan Phase 31 as a semantic-closure phase, not a generic refactor phase:

1. fix the mixed kernel-vs-retained history authority,
2. split the broad history/save regression surface into smaller suites,
3. align `ScaleSmoke` and the proof ring to the new contract.

That closes the carried mismatch without stealing the larger facade-convergence work queued for Phase 32.

---

*Research complete: 2026-04-16*
