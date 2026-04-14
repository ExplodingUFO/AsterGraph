# Phase 27 Research: Repo Quality Gates And Target Matrix

**Date:** 2026-04-14
**Phase:** 27-repo-quality-gates-and-target-matrix

## Research Questions

1. Which repo-level quality artifacts are missing today and fit the real scope of Phase 27?
2. How should centralized package version management be introduced without turning the phase into a dependency-upgrade campaign?
3. What is the actual supported target-framework and validation topology that CI must encode?
4. How should local and CI validation share one command path so README commands stop being the only source of truth?
5. Where should Phase 27 stop so it lands a trustworthy baseline without swallowing Phase 28 or Phase 29 work?

## Findings

### 1. The repo already has a shared MSBuild baseline, but it is still missing tracked editor/package/CI guardrails

`Directory.Build.props` already centralizes nullable, implicit usings, package metadata, XML-doc generation, the `CI=true` switch, and the `artifacts/**` exclusion.

What is still missing is the rest of the repo baseline:

- no tracked `.editorconfig`
- no tracked `Directory.Packages.props`
- no checked-in `.github/workflows/`
- no checked-in shared validation script under a conventional repo path such as `eng/`
- no tracked `global.json`

Implication:

- Phase 27 should extend the existing root-baseline posture instead of trying to replace it.
- The highest-value additions are `.editorconfig`, `Directory.Packages.props`, a repo-local validation script, and a GitHub Actions workflow.

### 2. Central package management is a fit for this repo because version sprawl is real but still small enough to migrate cleanly

Current versions are repeated directly in project files:

- `Avalonia`, `Avalonia.Desktop`, `Avalonia.Diagnostics`, `Avalonia.Fonts.Inter`, `Avalonia.Headless`, `Avalonia.Headless.XUnit`, `Avalonia.Markup.Xaml.Loader`, and `Avalonia.Themes.Fluent` on `11.3.10`
- `CommunityToolkit.Mvvm` on `8.2.1`
- `Microsoft.NET.Test.Sdk` on `17.11.1`
- `Microsoft.Extensions.Logging.Abstractions` on `9.0.0`
- `NuGet.Packaging` on `7.3.0`
- `xunit` on `2.9.2`
- `xunit.runner.visualstudio` on `2.8.2`

`tools/AsterGraph.PackageSmoke` also pins local package-consumption references to `$(Version)`, which means the central-package pass must account for repo-self package references and not only third-party dependencies.

Implication:

- `Directory.Packages.props` should be introduced as a version-only normalization pass.
- Phase 27 should not upgrade package versions; it should move existing versions to one tracked file and keep project intent otherwise unchanged.

### 3. The repo supports a split validation topology today, so CI cannot pretend every project exercises both target frameworks equally

The four publishable packages target `net8.0;net9.0`:

- `src/AsterGraph.Abstractions`
- `src/AsterGraph.Core`
- `src/AsterGraph.Editor`
- `src/AsterGraph.Avalonia`

But the validation and host surface is intentionally split:

- `tests/AsterGraph.Serialization.Tests` targets `net8.0`
- `tools/AsterGraph.PackageSmoke` targets `net8.0`
- `tools/AsterGraph.ScaleSmoke` targets `net8.0`
- `tests/AsterGraph.Editor.Tests` targets `net9.0`
- `tests/AsterGraph.TestPlugins` targets `net9.0`
- `src/AsterGraph.Demo` targets `net9.0`

Implication:

- A trustworthy Phase 27 baseline must validate the four publishable packages on both frameworks while also preserving the current test/tool split.
- The CI command path should make the framework split explicit instead of relying only on `dotnet build avalonia-node-map.sln`.

### 4. The repo needs one checked-in validation entry point that both humans and CI can run

`README.md` and `docs/host-integration.md` already document multiple manual commands for build, pack, tests, and package-smoke flows.

That is useful context, but it is still manual-memory driven. The repo has no checked-in local automation layer that CI can call directly.

Implication:

- Phase 27 should introduce a repo-local PowerShell entry point, most naturally `eng/ci.ps1`.
- GitHub Actions should call that script instead of duplicating matrix logic inline in YAML.
- This keeps local and CI behavior aligned and gives later phases one stable place to extend with package smoke, coverage, and public API checks.

### 5. The correct scope cut is a baseline gate, not the final release gate

The milestone requirements and roadmap already defer:

- package smoke as a release gate
- coverage reporting or thresholds
- public API/package compatibility automation
- proof-surface drift cleanup
- sample-vs-core lane separation

The current tree also confirms that `tools/AsterGraph.HostSample` is stale tree residue rather than a live project: the directory exists only as build artifacts and does not contain a checked-in `.csproj`.

Implication:

- Phase 27 should acknowledge this topology when designing the matrix, but it should not absorb the doc/solution drift cleanup that belongs to Phase 28.
- Coverage, package smoke gating, and public API/package compatibility should remain explicit Phase 29 work even if Phase 27 lays the command-path groundwork.

## Risks And Guardrails

- Do not turn central package management into a version-upgrade sweep.
- Do not introduce strict warning enforcement that blocks the repo on existing XML-doc debt in the same phase that adds the baseline files.
- Do not hide the supported matrix inside opaque workflow YAML; the repo-local script should remain the readable source of truth.
- Do not claim the stale `HostSample` tree is solved by merely ignoring it in CI; Phase 28 still needs to reconcile docs, solution membership, and proof tooling.
- Do not let the Phase 27 script become a release-validation kitchen sink. Package smoke gating, coverage, and public API/package compatibility remain later gates.

## Recommended Planning Posture

### Wave 1: Root baseline normalization

Add `.editorconfig` and `Directory.Packages.props`, then migrate the existing package-version declarations without changing supported frameworks or dependency versions.

### Wave 2: Shared local validation entry point

Add a checked-in `pwsh` script that explicitly encodes the supported package and framework topology for local use and CI reuse.

### Wave 3: Checked-in GitHub Actions CI

Add a workflow that runs the shared script against explicit `net8.0` and `net9.0` lanes on a Windows runner, keeping the command path visible and reproducible.

## Recommendation

Plan Phase 27 around one narrow principle: make repo quality gates real, tracked, and repeatable without pretending that the repo is already at final release-validation maturity.

That keeps the phase disciplined:

- contributors get one visible baseline for editor rules and package versions
- CI gains an explicit command path for both supported framework lanes
- later phases can extend the same lane with proof-surface cleanup and release-grade validation instead of starting over

---

*Research complete: 2026-04-14*
