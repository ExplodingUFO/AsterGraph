# Phase 444: Desktop Graph Library Capability Audit - Plan

status: ready_for_execution
bead: avalonia-node-map-mqm.1
goal: Map current defended capabilities and library-grade gaps before implementation.

## Success Criteria

1. Rendering, viewport, interaction, customization, packaging, examples, and proof gaps are classified.
2. Current defended capabilities are separated from aspirational claims.
3. Implementation targets are small enough for later phases.
4. No source code changes are made in the audit phase.

## Task Breakdown

### Task 1 - Evidence Collection

Owner: main orchestrator plus read-only explorer agents.

Read source, tests, docs, and proof surfaces for:
- Rendering and viewport.
- Interaction and connection.
- Customization, host packaging, examples, and proof.

Validation:
- Explorer handoffs cite exact paths.
- No files are edited during exploration.

### Task 2 - Capability Matrix

Owner: main orchestrator.

Create `444-CAPABILITY-AUDIT.md` with:
- Current defended evidence.
- Gaps or weak claims.
- Narrow implementation target.
- Owning future phase and bead.
- First files to read.

Validation:
- Every active v0.75 requirement has an audit row.
- Gap wording avoids new implementation promises.

### Task 3 - Handoff And Verification

Owner: main orchestrator.

Create `444-SUMMARY.md` and `444-VERIFICATION.md`; update ROADMAP and STATE; close `avalonia-node-map-mqm.1`.

Validation:
- `rg` confirms planning/docs do not contain prohibited external project names.
- `bd ready` shows 445, 446, and 447 ready after closing 444.
- Git/Dolt/beads are clean and pushed at phase close.

## Dependency And Parallelization Notes

- Phase 444 is a serial dependency for 445, 446, and 447.
- It is read-only except for local GSD artifacts and bead status.
- After Phase 444 closes, Phases 445, 446, and 447 can run in parallel because their primary write sets are disjoint:
  - 445: rendering/viewport/performance surface.
  - 446: interaction/connection/command surface.
  - 447: customization/extension/docs/proof surface.

## Verification Commands

```powershell
bd ready --json
bd dep cycles --json
rg -n "<prohibited external inspiration names>" .planning docs README.md AGENTS.md
git status --short --branch
dolt status
```
