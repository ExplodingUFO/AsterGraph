# Phase 27: Repo Quality Gates And Target Matrix - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-04-14
**Phase:** 27-repo-quality-gates-and-target-matrix
**Areas discussed:** quality baseline source of truth, CI host and command path, matrix scope, enforcement posture, phase scope boundary

---

## Quality baseline source of truth

| Option | Description | Selected |
|--------|-------------|----------|
| Repo-root baseline files | Add `.editorconfig` and `Directory.Packages.props`, keep `Directory.Build.props` as the shared MSBuild hub | ✓ |
| Style-only baseline | Add `.editorconfig` now, leave package versions distributed across individual project files | |
| Manual conventions | Keep the current per-project setup and rely on docs/manual contributor discipline | |

**User's choice:** [auto] Repo-root baseline files
**Notes:** Recommended because the repo already has a shared MSBuild center in `Directory.Build.props`, but package and style baselines are still missing as tracked root-level sources of truth.

---

## CI host and command path

| Option | Description | Selected |
|--------|-------------|----------|
| GitHub Actions + repo script | Check in GitHub Actions and have it call one repo-local validation entry point | ✓ |
| GitHub Actions only | Check in workflow YAML with inline commands only, no shared local script | |
| Local script only | Add local scripts but defer checked-in CI | |

**User's choice:** [auto] GitHub Actions + repo script
**Notes:** Recommended because `QUAL-02` explicitly calls for checked-in automation, and one shared script path prevents CI/local drift.

---

## Matrix scope

| Option | Description | Selected |
|--------|-------------|----------|
| Supported package boundary + focused lanes | Validate the four publishable packages across `net8.0`/`net9.0` plus current focused test/tool lanes | ✓ |
| Solution-wide single lane | Only build/test the solution in one broad lane | |
| Publishable packages only | Only build the four packages, no focused test/tool lanes | |

**User's choice:** [auto] Supported package boundary + focused lanes
**Notes:** Recommended because the repo already has real target splits across package, test, and tool projects that a single-lane solution build would hide.

---

## Enforcement posture

| Option | Description | Selected |
|--------|-------------|----------|
| Staged enforcement | Gate restore/build/test matrix failures now, defer `CS1591`, coverage thresholds, and public API/package compatibility to later phases | ✓ |
| Immediate hard enforcement | Turn on strict warning-as-error and doc/API gates immediately | |
| Observational CI | Add CI mostly for visibility, not for strong gating | |

**User's choice:** [auto] Staged enforcement
**Notes:** Recommended because Phase 27 only owns `QUAL-01` and `QUAL-02`; release-grade gates belong to Phase 29.

---

## Phase scope boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Keep scope narrow | Leave `HostSample`/solution/doc drift and lane cleanup to Phase 28 | ✓ |
| Fold drift cleanup into Phase 27 | Mix proof-surface and solution cleanup into the quality baseline phase | |
| Pull release gates forward | Expand this phase to include coverage/public API/package compatibility now | |

**User's choice:** [auto] Keep scope narrow
**Notes:** Recommended because the roadmap already gave proof-surface alignment and release validation their own later phases.

---

## the agent's Discretion

- Exact script location and naming for the shared validation entry point
- Exact GitHub Actions job split and matrix shape
- Whether `global.json` is necessary once the planned workflow is concrete

## Deferred Ideas

- Fix `HostSample` and solution/doc drift in Phase 28
- Separate demo/sample regression lanes from core SDK regressions in Phase 28
- Add package smoke gating, coverage thresholds, and public API/package compatibility gates in Phase 29
