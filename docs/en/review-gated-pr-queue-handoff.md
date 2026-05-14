# Review-Gated PR Queue Handoff

Last checked: 2026-05-14.

This handoff records the `master` PR queue that is already implementation-complete from the agent side but blocked by the required review gate. Do not admin-bypass or merge any review-gated PR without explicit user authorization.

## Current `master` Queue

| PR | Branch | Scope | Current blocker |
| --- | --- | --- | --- |
| #200 | `feature/phase-538-eraser-feasibility` | Phase 538 eraser feasibility boundary | Required review only; reported GitHub checks are green. |
| #261 | `docs/issue-260-serialization-schema-6` | Serialization contract docs aligned with schema version 6 | Required review only; reported GitHub checks are green. |
| #263 | `docs/issue-262-zh-status-support-boundary` | Chinese project status support-boundary summary | Required review only; reported GitHub checks are green. |
| #265 | `docs/issue-264-pr-template-traceability` | Pull request template traceability | Required review only; reported GitHub checks are green. |
| #267 | `docs/issue-266-feature-request-template` | Feature request template traceability | Required review only; reported GitHub checks are green. |
| #269 | `docs/issue-268-bug-report-template` | Bug report template traceability | Required review only; reported GitHub checks are green. |

## Operating Policy

- Treat `bd ready` returning no open issues as authoritative for this repository: there is no unblocked implementation task to start from the existing stack.
- Do not start later whiteboard or annotation phases while their beads are blocked by stacked PR dependencies.
- Do not merge, rebase, or clean up review-gated branches unless the user explicitly authorizes that action.
- Safe independent work may be created only when it has a new GitHub issue, a new bead, an isolated worktree, and a write set that does not overlap current open PR files.

## Verification Snapshot

The queue above was derived from `gh pr list --state open --base master` and the matching bead notes. At the time of this handoff, each listed PR reported `reviewDecision: REVIEW_REQUIRED`; the remaining blocker was review rather than failing or pending checks.
