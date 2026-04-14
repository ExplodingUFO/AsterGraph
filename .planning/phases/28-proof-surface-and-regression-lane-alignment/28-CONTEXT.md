# Phase 28: Proof Surface And Regression Lane Alignment - Context

**Gathered:** 2026-04-14
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 28 is the verification-surface alignment phase for v1.5.

This phase is about:

- making README, quick-start guidance, planning/codebase maps, solution membership, and proof-tool references describe the same live verification surface
- removing or replacing stale `HostSample` claims so contributors can tell which proof tools are real today
- separating core SDK regression coverage from demo/sample integration coverage so failures identify the right layer

This phase is not about:

- adding final release gates such as coverage thresholds, public API/package compatibility, or packed-package smoke enforcement
- reopening the four-package SDK boundary, the canonical runtime/session path, or the sample-only status of `AsterGraph.Demo`
- inventing a broader new host-product narrative beyond aligning the existing proof and regression surface

</domain>

<decisions>
## Implementation Decisions

### Source Of Truth

- **D-01:** Treat the checked-in tree itself as the source of truth for proof-surface claims. Docs and planning artifacts must follow the real solution/tools/tests surface instead of carrying forward old phase-history references.
- **D-02:** Treat `avalonia-node-map.sln` and `eng/ci.ps1` together as the operational verification surface. If a tool is described as first-class proof, it should either be represented in those tracked entry points or be explicitly documented as intentionally out of band.

### Stale Tool Claims

- **D-03:** Do not preserve stale `HostSample` references by inertia. The current tree contains only `tools/AsterGraph.HostSample/bin` and `obj` residue, not a live project file, so Phase 28 should remove stale claims unless it intentionally creates a real maintained replacement.
- **D-04:** Prefer removing stale `HostSample` claims in this phase over reintroducing a new maintained sample unless a replacement is clearly necessary to satisfy a proof/documentation contract. A new minimal host sample can remain later optional work if the docs no longer depend on it.
- **D-05:** `PackageSmoke` and `ScaleSmoke` remain legitimate proof tools. If `ScaleSmoke` is kept as a first-class proof surface, Phase 28 should align solution/doc references with that reality instead of leaving it in a half-listed state.

### Regression Lane Separation

- **D-06:** Split demo-coupled tests from the core SDK regression lane. `tests/AsterGraph.Editor.Tests` currently references `src/AsterGraph.Demo` and contains demo-specific suites, which makes SDK failures harder to classify.
- **D-07:** Keep core SDK regression tests dependent on the publishable packages and test fixtures only. Demo-specific coverage should move to a dedicated demo/sample lane with its own project or clearly separated execution path.
- **D-08:** Keep `AsterGraph.Demo` sample-only. Test-lane separation should reduce coupling without turning the demo into a supported package boundary.

### Scope Control

- **D-09:** Keep this phase focused on proof-surface alignment and regression-lane clarity. Release validation gates, package compatibility checks, and coverage/reporting automation remain Phase 29 work.
- **D-10:** Prefer synchronized command/documentation cleanup over broad prose rewrites. The phase should change only the docs and planning/codebase artifacts needed to make the live verification surface trustworthy.

### the agent's Discretion

- Whether the demo-separated regression lane becomes a new `tests/AsterGraph.Demo.Tests` project or another equally explicit structure.
- Whether `ScaleSmoke` is added to the existing `tools` solution folder or another tracked verification entry point, as long as docs and automation tell the same story.
- Which planning/codebase reference artifacts under `.planning/codebase/` should be refreshed directly in this phase versus regenerated from source in a later cleanup pass.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope and current state
- `.planning/PROJECT.md` - current v1.5 framing after Phase 27
- `.planning/REQUIREMENTS.md` - `PROOF-01` and `PROOF-02` requirements for this phase
- `.planning/ROADMAP.md` - Phase 28 goal, dependencies, and success criteria
- `.planning/STATE.md` - current phase position and carry-forward concerns

### Live proof and integration docs
- `README.md` - current public proof/run/build guidance
- `docs/quick-start.md` - canonical host entry guidance and current proof-tool links
- `docs/host-integration.md` - package/smoke/manual validation guidance that should match the live tree

### Live tree and verification entry points
- `avalonia-node-map.sln` - current solution membership
- `eng/ci.ps1` - current automated validation surface from Phase 27
- `tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj` - live package proof tool
- `tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj` - live scale/readiness proof tool
- `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` - current mixed core/demo regression lane
- `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj` - current core serialization lane
- `tests/AsterGraph.TestPlugins/AsterGraph.TestPlugins.csproj` - current plugin fixture project
- `src/AsterGraph.Demo/AsterGraph.Demo.csproj` - current sample host boundary that should remain sample-only

### Drift evidence inside planning artifacts
- `.planning/codebase/STRUCTURE.md` - still lists `tools/AsterGraph.HostSample` as live structure
- `.planning/codebase/TESTING.md` - still treats `HostSample` as a practical proof tool and notes `AsterGraph.Editor.Tests` references `AsterGraph.Demo`
- `.planning/codebase/STACK.md` - still lists `tools/AsterGraph.HostSample` in stack/project summaries

</canonical_refs>

<code_context>
## Existing Code Insights

### Current Mismatches

- `avalonia-node-map.sln` includes `AsterGraph.PackageSmoke`, but not `AsterGraph.ScaleSmoke` and not any live `HostSample` project.
- `eng/ci.ps1` already validates `PackageSmoke` and `ScaleSmoke`, so automation and solution membership do not yet describe the same proof surface.
- `tools/AsterGraph.HostSample` exists only as `bin/` and `obj/` residue; there is no checked-in `.csproj` or source file in the current tree.
- `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` still references `src/AsterGraph.Demo/AsterGraph.Demo.csproj`, and the project contains multiple demo-focused suites.

### Stable Proof Surface

- `PackageSmoke` and `ScaleSmoke` are real runnable tools in the current tree.
- `README.md`, `docs/quick-start.md`, and `docs/host-integration.md` already point hosts to `PackageSmoke`, `ScaleSmoke`, and `AsterGraph.Demo` as current proof/reference surfaces.
- Phase 27 introduced `eng/ci.ps1` and `.github/workflows/ci.yml`, giving the repo a stable automation anchor that Phase 28 should align docs and solution membership around rather than replace.

### Planning Drift

- `.planning/codebase/STRUCTURE.md`, `.planning/codebase/TESTING.md`, and `.planning/codebase/STACK.md` still describe `HostSample` as if it were live.
- Some planning artifacts still describe older roadmap positions or verification surfaces from earlier milestones, so Phase 28 should refresh only the parts that materially guide contributors today.

</code_context>

<specifics>
## Specific Ideas

- Remove stale `HostSample` references from docs/planning artifacts unless a real replacement is intentionally added.
- Add `AsterGraph.ScaleSmoke` to the solution or otherwise make its first-class status explicit in one tracked verification surface.
- Split demo-specific tests into a dedicated regression lane so the core SDK lane can depend only on the publishable packages plus test fixtures.
- Update public and planning-facing docs so they point to the same live proof commands and project list now exercised by `eng/ci.ps1`.

</specifics>

<deferred>
## Deferred Ideas

- Add release-grade package smoke enforcement, coverage/reporting thresholds, or public API/package compatibility automation in Phase 29.
- Introduce a brand-new minimal host sample if stale `HostSample` references can be removed cleanly without blocking the proof/documentation story in Phase 28.
- Build a shorter canonical adoption decision tree in docs; that remains Phase 29 alongside release validation.

</deferred>

---

*Phase: 28-proof-surface-and-regression-lane-alignment*
*Context gathered: 2026-04-14*
