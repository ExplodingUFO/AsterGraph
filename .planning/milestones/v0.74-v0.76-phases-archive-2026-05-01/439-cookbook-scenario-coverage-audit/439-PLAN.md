# Phase 439: Cookbook Scenario Coverage Audit - Plan

**Bead:** `avalonia-node-map-h3e.1`
**Requirement:** COOK-01
**Mode:** Audit and coverage matrix, no production code edits

## Goal

Create a narrow coverage map for the professional cookbook surface before changing behavior.

## Tasks

1. Split audit work into child beads and isolated worktrees.
2. Inspect cookbook catalog/model/projection and tests.
3. Inspect Demo UI/view-model projection and graph-above-detail tests.
4. Inspect cookbook docs, Demo proof markers, and proof/docs tests.
5. Synthesize a coverage matrix across active recipes.
6. Assign actionable gaps to later phases.
7. Verify focused cookbook tests and close the phase.

## Ownership Boundaries

| Area | Owner | Write Scope |
| --- | --- | --- |
| Catalog/model audit | `avalonia-node-map-h3e.1.1` | Read-only worktree audit |
| UI/projection audit | `avalonia-node-map-h3e.1.2` | Read-only worktree audit |
| Docs/proof audit | `avalonia-node-map-h3e.1.3` | Read-only worktree audit |
| Coverage matrix synthesis | Parent thread | `.planning/phases/439-*` only |
| Beads and cleanup | Parent thread | `.beads/issues.jsonl`, worktree cleanup |

## Validation

- `dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj -c Release -f net9.0 --no-restore --filter "FullyQualifiedName~DemoCookbook" -v minimal`
- `bd dep cycles`
- `bd ready --json`
- `git status --short --branch`
- `dolt status` in `.beads\dolt\avalonia_node_map`

## Stop Conditions

Stop and report if:

- A subagent finds a production behavior blocker that invalidates the roadmap.
- Focused cookbook tests fail in a way unrelated to Phase 439 audit work.
- Beads, Dolt, branch, or worktree cleanup cannot be made safe.
