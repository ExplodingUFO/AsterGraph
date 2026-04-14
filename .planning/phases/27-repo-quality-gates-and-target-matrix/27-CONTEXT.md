# Phase 27: Repo Quality Gates And Target Matrix - Context

**Gathered:** 2026-04-14
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 27 is the repo-baseline hardening phase for v1.5.

This phase is about:

- adding tracked repo-level quality baseline files instead of relying on scattered per-project defaults
- establishing a checked-in CI path that validates the supported SDK/package surface across `net8.0` and `net9.0`
- centralizing the command path for core validation so local contributors and CI run the same build/test matrix

This phase is not about:

- resolving proof-surface drift, solution membership drift, or sample-vs-core lane cleanup beyond what the CI baseline needs to know
- turning coverage thresholds, package smoke gating, or public API/package compatibility into the final release gate
- reopening the four-package boundary, canonical runtime path, or staged compatibility story that earlier phases already locked

</domain>

<decisions>
## Implementation Decisions

### Quality Baseline

- **D-01:** Add tracked repo-root quality baseline files in this phase, specifically `.editorconfig` and `Directory.Packages.props`, instead of continuing with style and package versions scattered across individual project files.
- **D-02:** Keep `Directory.Build.props` as the shared MSBuild behavior hub. New baseline files should complement it, not duplicate or replace its current role for nullable, implicit usings, XML docs, and CI build flags.
- **D-03:** Keep `global.json` discretionary for this phase. Add it only if CI or local reproducibility work proves that SDK pinning is necessary; do not make SDK pinning the main objective.

### CI Host And Command Path

- **D-04:** Use checked-in GitHub Actions as the canonical CI host for Phase 27.
- **D-05:** Prefer a repo-local scripted validation entry point that CI calls, instead of burying all matrix logic directly inside workflow YAML. The same command path should be runnable locally.
- **D-06:** Use PowerShell as the first-class scripting path for the repo-local entry point so it fits the existing Windows-heavy contributor environment while remaining usable from GitHub Actions via `pwsh`.

### Matrix Scope

- **D-07:** The CI matrix should cover the four publishable packages (`AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, `AsterGraph.Avalonia`) across both `net8.0` and `net9.0`.
- **D-08:** The matrix should also exercise the current focused test/tool lanes that materially validate the SDK boundary, rather than only building the solution or only building the publishable projects.
- **D-09:** Keep the demo and proof tooling visible to CI design, but do not let this phase collapse into proof-surface cleanup. The immediate target is a trustworthy baseline matrix, not a final release workflow.

### Enforcement Posture

- **D-10:** Use staged enforcement in this phase: restore/build/test/package-matrix failures should gate CI immediately, but `CS1591` retirement, public API compatibility checks, and coverage thresholds should remain staged follow-on gates.
- **D-11:** Do not convert this phase into a warnings-as-errors sweep for existing public XML-doc debt. The baseline should make that debt visible and easier to retire later without blocking the entire repo today.
- **D-12:** Keep package smoke, coverage thresholding, and public API/package compatibility as Phase 29 work even if Phase 27 lays groundwork that makes them easy to add later.

### Scope Control

- **D-13:** Keep `HostSample`, `ScaleSmoke`, solution-membership drift, and demo-lane/test-lane separation as explicit Phase 28 concerns unless a Phase 27 baseline command cannot be defined without noting them.

### the agent's Discretion

- Whether the shared validation entry point lives under a new `eng/` folder, a repo-root script, or another narrow conventional location.
- Whether the CI workflow uses one matrix job or a small number of purpose-specific jobs, as long as the supported package boundary and target-framework matrix stay explicit.
- Whether a minimal `global.json` belongs in Phase 27 once the actual workflow shape is planned and validated.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope and carry-forward constraints
- `.planning/PROJECT.md` - v1.5 milestone framing, fixed package boundary, and post-Phase-26 current state
- `.planning/REQUIREMENTS.md` - `QUAL-01` and `QUAL-02` plus the explicit deferral of `QUAL-03`
- `.planning/ROADMAP.md` - Phase 27 goal, dependency chain, and success criteria
- `.planning/STATE.md` - current milestone position and carry-forward blockers

### Current manual validation and public command surface
- `README.md` - current build/test/pack/manual validation commands that CI should eventually replace or call through one shared path
- `docs/host-integration.md` - current packaged validation and rebuild guidance used for host-facing verification

### Current build/package topology
- `Directory.Build.props` - existing repo-wide MSBuild defaults, XML docs, and CI flag
- `avalonia-node-map.sln` - current solution membership and project grouping
- `NuGet.config.sample` - current package-feed guidance that may influence CI restore shape

### Current SDK/package and validation projects
- `src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj` - publishable package targeting `net8.0;net9.0`
- `src/AsterGraph.Core/AsterGraph.Core.csproj` - publishable package targeting `net8.0;net9.0`
- `src/AsterGraph.Editor/AsterGraph.Editor.csproj` - publishable package targeting `net8.0;net9.0`
- `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj` - publishable package targeting `net8.0;net9.0`
- `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` - current editor regression lane targeting `net9.0`
- `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj` - current serialization regression lane targeting `net8.0`
- `tests/AsterGraph.TestPlugins/AsterGraph.TestPlugins.csproj` - plugin fixture project used by focused tests
- `tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj` - current package-consumption smoke surface targeting `net8.0`
- `tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj` - current large-graph proof surface targeting `net8.0`
- `tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj` - current host sample project present in the tree but not yet the focus of this phase

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets

- `Directory.Build.props` already centralizes nullable, implicit usings, XML docs, package metadata, and the `CI=true` switch. Phase 27 should extend this shared-build posture rather than invent a second configuration center.
- The four publishable package projects already share the same `net8.0;net9.0` target-framework story, so the matrix can be centered on an existing supported boundary rather than a hypothetical one.
- `tools/AsterGraph.PackageSmoke` and `tools/AsterGraph.ScaleSmoke` already exist as explicit proof executables, which means Phase 27 can design CI with real boundary-checking tools in mind even if final release gating waits until Phase 29.

### Established Patterns

- The repo currently prefers shared root-level MSBuild configuration over per-project divergence.
- Validation commands are currently documented in `README.md` and `docs/host-integration.md`, but not yet centralized in a checked-in script or workflow.
- The test and tool landscape is intentionally split by responsibility today: editor regressions are mostly `net9.0`, while serialization and smoke tooling are mostly `net8.0`.

### Integration Points

- Repo-root baseline files such as `.editorconfig`, `Directory.Packages.props`, and possibly `global.json` are the natural integration points for tracked quality configuration.
- A new checked-in CI workflow should live under `.github/workflows/`.
- A shared validation script should become the one place that encodes the matrix decisions used by both contributors and CI.
- Existing project files under `src/`, `tests/`, and `tools/` will need package-reference updates if central package management is adopted.

</code_context>

<specifics>
## Specific Ideas

- Add `.editorconfig` to lock formatting, whitespace, and baseline C# conventions at the repo root.
- Add `Directory.Packages.props` and move current `PackageReference Version=...` values there for the shared dependency graph.
- Introduce a checked-in GitHub Actions workflow that uses `pwsh` and the local validation script to restore/build/test the supported package boundary across `net8.0` and `net9.0`.
- Keep the first CI matrix focused on reproducible build/test correctness, not on final release qualification. Release-grade package smoke, coverage thresholds, and public API/package compatibility checks should remain a later milestone step.

</specifics>

<deferred>
## Deferred Ideas

- Resolve `HostSample` vs solution/doc drift in Phase 28 rather than folding that cleanup into this quality-baseline phase.
- Split demo-coupled regressions from core SDK regressions in Phase 28.
- Add package smoke gating, coverage thresholds, and public API/package compatibility baselines in Phase 29.

</deferred>

---

*Phase: 27-repo-quality-gates-and-target-matrix*
*Context gathered: 2026-04-14*
