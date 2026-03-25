---
phase: 01-consumption-compatibility-guardrails
plan: 04
subsystem: documentation
tags: [packages, docs, migration, verification, nuget, avalonia]
requires:
  - phase: 01-02
    provides: factory-first editor and Avalonia composition APIs
  - phase: 01-03
    provides: compatibility-facade guidance and migration smoke markers
provides:
  - Unified package-boundary and target-framework docs across root and package READMEs
  - Factory-first host integration guidance with explicit constructor-based migration staging
  - Phase 1 verification evidence covering pack output, host sample behavior, and current workspace blockers
affects: [phase-02, host-guidance, package-consumption, verification]
tech-stack:
  added: []
  patterns: [shared package matrix docs, factory-first migration guidance, verification-first closeout]
key-files:
  created:
    - .planning/phases/01-consumption-compatibility-guardrails/01-04-SUMMARY.md
  modified:
    - README.md
    - docs/host-integration.md
    - src/AsterGraph.Abstractions/README.md
    - src/AsterGraph.Core/README.md
    - src/AsterGraph.Editor/README.md
    - src/AsterGraph.Avalonia/README.md
    - .planning/STATE.md
    - .planning/ROADMAP.md
key-decisions:
  - "Document the supported SDK boundary around the four publishable packages and treat AsterGraph.Editor as a standard host-facing runtime package."
  - "Make the factory/options path canonical in docs while keeping GraphEditorViewModel and GraphEditorView documented as supported migration facades."
  - "Treat packed-package failures in this workspace as restore/cache environment blockers only after confirming the freshly packed nupkgs still expose the new public APIs."
patterns-established:
  - "Consumer docs must share one package matrix, target-framework story, and migration framing across the root README, host guide, and package READMEs."
  - "Verification closeout should distinguish source-surface blockers from packed-package restore/cache blockers instead of collapsing them into one failure bucket."
requirements-completed: [PKG-01, PKG-02, PKG-03]
duration: 14min
completed: 2026-03-26
---

# Phase 01 Plan 04: Consumption & Compatibility Guardrails Summary

**Aligned the external AsterGraph package story around a four-package SDK boundary, factory-first host initialization, and explicit migration guidance**

## Performance

- **Duration:** 14 min
- **Started:** 2026-03-26T00:47:00+08:00
- **Completed:** 2026-03-26T01:01:10+08:00
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments

- Rewrote the root README and package READMEs so they agree on the supported package set, `net8.0`/`net9.0` support story, and the non-consumable status of `AsterGraph.Demo`.
- Reworked the host integration guide to make `AsterGraphEditorFactory` and `AsterGraphAvaloniaViewFactory` the canonical path while keeping the constructor/view route documented as a staged migration path.
- Ran the Phase 1 verification ring far enough to confirm all four packages pack, the host sample still works, and the remaining failures are workspace test/restore blockers rather than silent doc drift.

## Task Commits

Each task was committed atomically:

1. **Task 1: Align the package boundary and migration story across all consumer docs** - `fe3cbae` (chore)
2. **Task 2: Run the full Phase 1 verification ring against source and packed packages** - `e29a276` (chore)

**Plan metadata:** pending

## Files Created/Modified

- `README.md` - Added the supported package matrix, target-framework support notes, and factory-first migration framing.
- `docs/host-integration.md` - Reframed host setup around the canonical factory/options path plus the retained compatibility path.
- `src/AsterGraph.Abstractions/README.md` - Positioned the contracts package inside the supported four-package SDK set.
- `src/AsterGraph.Core/README.md` - Positioned the model/serialization package inside the supported four-package SDK set.
- `src/AsterGraph.Editor/README.md` - Documented the editor package as the standard host-facing runtime package.
- `src/AsterGraph.Avalonia/README.md` - Documented the Avalonia package as the canonical UI companion to `AsterGraph.Editor`.
- `.planning/phases/01-consumption-compatibility-guardrails/01-04-SUMMARY.md` - Recorded execution evidence, decisions, and blockers for the plan.
- `.planning/STATE.md` - Updated plan position, metrics, and accumulated context after execution.
- `.planning/ROADMAP.md` - Updated Phase 1 plan progress.

## Decisions Made

- Documented `AsterGraph.Editor` as a standard direct dependency for hosted editor runtime composition instead of framing it as only an optional deep-integration package.
- Kept the docs additive: new hosts are pointed to the factories first, while existing hosts still get explicit support for `new GraphEditorViewModel(...)` and `new GraphEditorView { Editor = editor }`.
- Used nupkg inspection to separate real package-surface regressions from stale-cache or restore-source failures during packed verification.

## Deviations from Plan

None - plan execution followed the specified doc and verification work, but verification exposed pre-existing workspace blockers that were recorded instead of patched in out-of-scope files.

## Issues Encountered

- The targeted editor regression command failed in `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` with `CS0246` because the out-of-scope untracked file still cannot resolve `GraphEditorViewTestsAppBuilder`.
- The full solution test run exited with the same test-project compilation blocker, so the source-test portion of the ring remains red in this workspace.
- All four publishable packages packed successfully, but the first packed-package smoke run resolved stale package artifacts and failed on missing factory/`ChromeMode` members even though the freshly packed `AsterGraph.Editor` and `AsterGraph.Avalonia` nupkgs contain those APIs.
- After forcing package-cache refresh attempts, the packed-package path hit environment-level restore problems: missing third-party sources without a real `NuGet.config`, then locked files in the global NuGet cache (`Avalonia.BuildServices.dll` / `Avalonia.Build.Tasks.dll`) during reruns.
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj` passed and printed the expected factory, host-context, presentation, and `ChromeMode` markers.

## Verification Evidence

- `rg -n "AsterGraph.Abstractions|AsterGraph.Core|AsterGraph.Editor|AsterGraph.Avalonia|factory|migration|net8.0|net9.0|AsterGraph.Demo" README.md docs/host-integration.md src/AsterGraph.Abstractions/README.md src/AsterGraph.Core/README.md src/AsterGraph.Editor/README.md src/AsterGraph.Avalonia/README.md` ✅
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorInitializationTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal` ❌ blocked by `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` (`CS0246: GraphEditorViewTestsAppBuilder`)
- `dotnet test avalonia-node-map.sln -v minimal` ❌ blocked during the same test-project compile path
- `dotnet pack src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj -c Release -o artifacts/packages` ✅
- `dotnet pack src/AsterGraph.Core/AsterGraph.Core.csproj -c Release -o artifacts/packages` ✅
- `dotnet pack src/AsterGraph.Editor/AsterGraph.Editor.csproj -c Release -o artifacts/packages` ✅
- `dotnet pack src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj -c Release -o artifacts/packages` ✅
- nupkg inspection for `artifacts/packages/AsterGraph.Editor.0.1.0-preview.7.nupkg` and `artifacts/packages/AsterGraph.Avalonia.0.1.0-preview.7.nupkg` ✅ found `AsterGraphEditorFactory` and `GraphEditorViewChromeMode` in the packaged XML docs
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj -p:UsePackedAsterGraphPackages=true` ❌ first hit stale package resolution, then restore-source/cache-lock issues after cache refresh attempts
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj` ✅
  - `Host sample view type: AsterGraph.Avalonia.Controls.GraphEditorView`
  - `Host preview menu item exists: True`
  - `ChromeMode switched to: CanvasOnly`
  - `ChromeMode canvas-only keeps canvas: True`

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Phase 1 documentation is now aligned around one package boundary and migration story.
- The remaining workspace blockers are operational rather than doc drift: the out-of-scope `GraphEditorViewTests.cs` compile failure and the packed-package restore/cache issues need cleanup before this workspace can produce a fully green verification ring.

## Self-Check

PASSED

- FOUND: `.planning/phases/01-consumption-compatibility-guardrails/01-04-SUMMARY.md`
- FOUND: `fe3cbae`
- FOUND: `e29a276`

---
*Phase: 01-consumption-compatibility-guardrails*
*Completed: 2026-03-26*
