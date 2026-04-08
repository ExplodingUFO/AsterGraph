---
phase: 20-host-menu-control-consolidation
verified: 2026-04-08T08:01:56.3774364Z
status: passed
score: 3/3 must-haves verified
---

# Phase 20: Host Menu Control Consolidation Verification Report

**Phase Goal:** Consolidate view, behavior, and runtime-facing demo controls into compact host-level menu groups that act on the same live graph session.
**Verified:** 2026-04-08T08:01:56.3774364Z
**Status:** passed
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
| --- | --- | --- | --- |
| 1 | Shell and chrome visibility controls are now exposed as host-menu toggles plus drawer controls that act on the same `MainGraphEditorView`. | ✓ VERIFIED | `src/AsterGraph.Demo/Views/MainWindow.axaml:47`, `src/AsterGraph.Demo/Views/MainWindow.axaml:60`, `src/AsterGraph.Demo/Views/MainWindow.axaml:193`, `tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs:37`, `tests/AsterGraph.Editor.Tests/DemoHostMenuControlTests.cs:65` |
| 2 | Editing behavior controls remain menu-first but still flow through the existing `ApplyHostOptions()` and `BuildCommandPermissions()` path on the current editor instance. | ✓ VERIFIED | `src/AsterGraph.Demo/Views/MainWindow.axaml:66`, `src/AsterGraph.Demo/Views/MainWindow.axaml:85`, `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:452`, `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:513`, `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:531`, `tests/AsterGraph.Editor.Tests/DemoHostMenuControlTests.cs:42` |
| 3 | Runtime-facing controls and readouts now live under the same host menu structure, keep using the canonical session diagnostics path, and do not replace the current editor/session. | ✓ VERIFIED | `src/AsterGraph.Demo/Views/MainWindow.axaml:91`, `src/AsterGraph.Demo/Views/MainWindow.axaml:259`, `src/AsterGraph.Demo/Views/MainWindow.axaml:290`, `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:324`, `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:400`, `tests/AsterGraph.Editor.Tests/GraphEditorDemoShellTests.cs:32`, `tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs:12`, `tests/AsterGraph.Editor.Tests/DemoHostMenuControlTests.cs:13` |

**Score:** 3/3 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
| --- | --- | --- | --- |
| `src/AsterGraph.Demo/Views/MainWindow.axaml` | Host menu contains direct view/behavior controls, runtime entries, and dedicated drawer sections for control groups | ✓ VERIFIED | Menu entries and drawer sections exist at the expected named parts, including `PART_ViewDrawerControls`, `PART_BehaviorDrawerControls`, `PART_RuntimeSummarySection`, and `PART_RuntimeDiagnosticsSection`. |
| `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` | Existing booleans remain the single source of truth and runtime metrics stay projected from the current session path | ✓ VERIFIED | Group visibility, runtime metric lines, `RuntimeConnectionCount`, `ApplyHostOptions()`, and `BuildCommandPermissions()` all remain in the one host view-model. |
| `tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs` | Structural regression coverage for Phase 20 menu entries and drawer sections | ✓ VERIFIED | Covers exact menu labels and named drawer sections introduced in this phase. |
| `tests/AsterGraph.Editor.Tests/GraphEditorDemoShellTests.cs` | Session continuity coverage across operational host groups | ✓ VERIFIED | Proves `视图`, `行为`, and `运行时` navigation keep the same `MainWindowViewModel.Editor`. |
| `tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs` | Canonical runtime projection and diagnostics-path coverage | ✓ VERIFIED | Continues to prove the runtime readouts come from inspection snapshots and `Editor.Session.Diagnostics`. |
| `tests/AsterGraph.Editor.Tests/DemoHostMenuControlTests.cs` | Focused Phase 20 coverage for runtime summary rows, behavior propagation, and chrome binding reach | ✓ VERIFIED | Covers runtime connection-count lines, behavior-to-editor propagation, and live `GraphEditorView` chrome binding. |

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
| --- | --- | --- | --- |
| Phase 20 focused demo-shell regressions pass | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~DemoMainWindowTests|FullyQualifiedName~GraphEditorDemoShellTests|FullyQualifiedName~DemoDiagnosticsProjectionTests|FullyQualifiedName~DemoHostMenuControlTests" -v minimal` | Exit code `0`; `16` tests passed | ✓ PASS |
| Demo project still builds cleanly after shell/control consolidation | `dotnet build src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -v minimal` | Exit code `0`; `0` warnings, `0` errors | ✓ PASS |

### Requirements Coverage

| Requirement | Description | Status | Evidence |
| --- | --- | --- | --- |
| CTRL-01 | User can adjust shell and chrome visibility from a host-level menu while staying on the same live editor session | ✓ SATISFIED | View menu checkboxes and drawer controls are bound to the chrome booleans, and live-view binding coverage passes. |
| CTRL-02 | User can adjust editing behavior toggles from a host-level menu and see the effect immediately on the current graph | ✓ SATISFIED | Behavior menu and drawer both bind the existing booleans, and tests prove those booleans still update editor behavior/permissions. |
| CTRL-03 | User can access runtime-facing controls or readouts from the same host-level menu structure without rebuilding the editor session | ✓ SATISFIED | Runtime menu entries route into the existing drawer, runtime summary includes the required metrics, diagnostics stay on the canonical path, and session-continuity tests pass. |

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
| --- | --- | --- | --- | --- |
| None | — | No new duplicate editor/session creation path or alternate behavior/runtime state model was introduced during Phase 20 execution. | ℹ️ Info | The phase stayed within its planned consolidation boundary. |

### Gaps Summary

No implementation gaps were found for the Phase 20 goal. Remaining demo narrative and proof-polish work is intentionally deferred to Phase 21.

---

_Verified: 2026-04-08T08:01:56.3774364Z_
_Verifier: Codex inline execution_
