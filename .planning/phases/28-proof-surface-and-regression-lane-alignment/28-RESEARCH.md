# Phase 28 Research: Proof Surface And Regression Lane Alignment

**Date:** 2026-04-14
**Phase:** 28-proof-surface-and-regression-lane-alignment

## Research Questions

1. Which proof-surface claims are stale relative to the current checked-in tree?
2. What tracked entry points actually define the live verification surface today?
3. What is the cleanest way to separate demo/sample regressions from the core SDK lane without changing the supported package boundary?
4. Which updates belong in Phase 28, and which should remain Phase 29 release-validation work?

## Findings

### 1. `HostSample` is a stale claim today, not a live proof tool

The current tree contains `tools/AsterGraph.HostSample/bin` and `obj` residue only. There is no checked-in `tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj` or `Program.cs`.

However, the repo still describes `HostSample` as live in several current planning/codebase artifacts, including:

- `.planning/codebase/STRUCTURE.md`
- `.planning/codebase/TESTING.md`
- `.planning/codebase/STACK.md`

Implication:

- Phase 28 should remove stale `HostSample` claims from current contributor-facing planning/codebase documents unless the phase intentionally reintroduces a real maintained replacement.
- Historical phase artifacts can remain historical, but current source-of-truth documents should stop pretending the project exists.

### 2. `ScaleSmoke` is a real proof tool, but tracked entry points do not all agree on it

`tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj` and `Program.cs` are live.

Public docs already treat `ScaleSmoke` as real:

- `README.md`
- `docs/quick-start.md`
- `docs/host-integration.md`

Phase 27 automation also treats it as real:

- `eng/ci.ps1` builds `ScaleSmoke` in the `net8.0` lane

But `avalonia-node-map.sln` currently includes `AsterGraph.PackageSmoke` and does not include `AsterGraph.ScaleSmoke`.

Fresh-worktree evidence:

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release` fails in a fresh worktree because solution restore does not produce `tools/AsterGraph.ScaleSmoke/obj/project.assets.json` before the script reaches `ScaleSmoke`.

Implication:

- Phase 28 should align solution membership with the real proof-tool surface or otherwise make `ScaleSmoke` explicitly out of band.
- Since docs and automation already treat it as first-class proof, the simplest fix is to add it to the solution and keep that status explicit.

### 3. The core SDK regression lane is currently mixed with demo/sample coverage

`tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` still references `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.

The test project contains multiple demo-focused suites, including files such as:

- `DemoDiagnosticsProjectionTests.cs`
- `DemoHostMenuControlTests.cs`
- `DemoMainWindowTests.cs`
- `GraphEditorDemoShellTests.cs`

Implication:

- The repo cannot currently answer “did the SDK regress, or did the sample host regress?” with one glance at the test lane.
- Phase 28 should extract demo-coupled tests into a dedicated test project or otherwise create an equally explicit sample lane.

### 4. The cleanest lane split keeps the package boundary fixed and moves only test ownership

The four-package boundary remains:

- `AsterGraph.Abstractions`
- `AsterGraph.Core`
- `AsterGraph.Editor`
- `AsterGraph.Avalonia`

`AsterGraph.Demo` remains sample-only.

Implication:

- Phase 28 should not add demo dependencies back into the supported package story.
- The correct cut is to keep core regressions in `AsterGraph.Editor.Tests` and move demo-specific suites to a dedicated `AsterGraph.Demo.Tests` lane that can still run under `net9.0` and Avalonia headless test support.

### 5. Phase 28 should refresh current docs and tracked verification entry points, not absorb release gates

Phase 29 still owns:

- release-grade package smoke enforcement
- coverage/reporting expectations
- public API/package compatibility checks
- the final short canonical adoption path

Implication:

- Phase 28 should focus on truthfulness and lane separation:
  - fix tracked proof-tool entry points
  - split demo/sample regressions from core SDK regressions
  - refresh public docs and current planning/codebase maps
- It should not expand into release gating or a broader documentation rewrite.

## Risks And Guardrails

- Do not update historical milestone artifacts just to erase every mention of `HostSample`; only current source-of-truth artifacts need to match the live tree.
- Do not leave `ScaleSmoke` half-in and half-out of tracked entry points after the phase. Either keep it first-class everywhere relevant or document it as intentionally out of band.
- Do not keep demo-specific tests in the core SDK lane after introducing a new demo lane; the separation needs to be real.
- Do not let public docs continue pointing to old verification assumptions after the lane split lands.
- Do not turn this phase into package-compatibility or coverage-threshold work.

## Recommended Planning Posture

### Wave 1: Align tracked proof entry points

Add `ScaleSmoke` to the solution, remove stale `HostSample` claims from current planning/codebase maps, and get the fresh-worktree `net8.0` automation lane green.

### Wave 2: Split the demo regression lane

Create a dedicated demo test project, move demo-specific suites there, remove the demo reference from `AsterGraph.Editor.Tests`, and update `eng/ci.ps1` so the `net9.0` validation lane runs both the core SDK lane and the demo/sample lane explicitly.

### Wave 3: Refresh public docs

Update `README.md`, `docs/quick-start.md`, and `docs/host-integration.md` so they describe the same proof tools and the same core-vs-demo regression split now encoded in the tree and automation.

## Recommendation

Plan Phase 28 around one narrow principle: the repo should describe only proof surfaces and regression lanes that actually exist and are actually exercised.

That keeps the phase disciplined:

- tracked automation stops failing on a fresh worktree for avoidable surface-drift reasons
- contributors can tell whether a failure belongs to the SDK boundary or the sample host
- Phase 29 can build release gates on top of a proof surface that is finally trustworthy

---

*Research complete: 2026-04-14*
