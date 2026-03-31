---
phase: 06-demo
verified: 2026-03-27T09:35:35Z
status: human_needed
score: 8/8 must-haves verified
human_verification:
  - test: "Inspect the Demo window layout and emphasis"
    expected: "The three-column shell reads as left navigation, one dominant center editor, and right-side evidence cards without any secondary surface competing with the main editor."
    why_human: "Visual hierarchy and perceived page balance cannot be verified reliably from source and headless tests alone."
  - test: "Click each capability row in the Demo"
    expected: "The right rail updates to the selected capability while the center editor stays live and feels continuous rather than rebuilt."
    why_human: "The code preserves one retained editor session, but interactive continuity and perceived UX flow still need manual confirmation."
  - test: "Review Chinese-first shell copy in the running Demo"
    expected: "User-facing shell text reads naturally in Simplified Chinese while technical seam names such as AsterGraphCanvasViewFactory and IGraphEditorSession stay English where intended."
    why_human: "Natural-language quality and translation tone are subjective and not fully checkable programmatically."
---

# Phase 6: Demo Verification Report

**Phase Goal:** Rebuild the Demo into a Chinese-first three-column SDK showcase that visibly proves the existing architecture seams — full shell, standalone surfaces, replaceable presentation, and runtime diagnostics — over one retained editor session.
**Verified:** 2026-03-27T09:35:35Z
**Status:** human_needed
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
| --- | --- | --- | --- |
| 1 | The demo home page presents one dominant live GraphEditorView inside a fixed three-column overview instead of the old top-toolbar-plus-editor layout. | ✓ VERIFIED | `src/AsterGraph.Demo/Views/MainWindow.axaml:22-27`, `src/AsterGraph.Demo/Views/MainWindow.axaml:140-207`, `tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs:35-42` |
| 2 | Users can switch among 完整壳层, 独立表面, 可替换呈现, and 运行时与诊断 without navigating away or rebuilding a second session model. | ✓ VERIFIED | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:97-160`, `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:376-385`, `src/AsterGraph.Demo/Views/MainWindow.axaml:57-80` |
| 3 | The page visibly proves full-shell boundary, standalone surfaces, and replacement seams using real runtime-backed content rather than placeholder copy. | ✓ VERIFIED | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:204-227`, `src/AsterGraph.Demo/Views/MainWindow.axaml:191-205`, `src/AsterGraph.Demo/Views/MainWindow.axaml:350-388` |
| 4 | The stock full-shell experience now reads as Simplified Chinese for user-facing shell copy. | ✓ VERIFIED | `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml:72-125`, `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml:141-149`, `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml:221-339`, `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs:57-78` |
| 5 | Runtime-owned captions still flow through the existing localization seam instead of a Demo-only replacement path. | ✓ VERIFIED | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:519-529`, `src/AsterGraph.Editor/Localization/IGraphLocalizationProvider.cs:5-13`, `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs:80-93` |
| 6 | Technical API/type identifiers remain English where they teach the host-facing seam. | ✓ VERIFIED | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:119-121`, `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:133-139`, `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:287-293`, `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs:95-104` |
| 7 | The Demo right rail shows machine-readable diagnostics and inspection information from Editor.Session.Diagnostics instead of relying on StatusMessage alone. | ✓ VERIFIED | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:245-255`, `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:383-385`, `src/AsterGraph.Demo/Views/MainWindow.axaml:267-340`, `tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs:11-39` |
| 8 | The Demo explicitly teaches canonical host seams by naming IGraphEditorSession and Editor.Session.Diagnostics in the runtime evidence area. | ✓ VERIFIED | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:287-293`, `src/AsterGraph.Demo/Views/MainWindow.axaml:283-286`, `tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs:18-21` |

**Score:** 8/8 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
| --- | --- | --- | --- |
| `src/AsterGraph.Demo/Views/MainWindow.axaml` | Three-column showcase shell with navigation, one center editor, right-rail proof cards, and runtime diagnostics UI | ✓ VERIFIED | Exists, substantive, wired to `MainWindowViewModel`, and contains required named sections plus machine-readable diagnostics rows at `src/AsterGraph.Demo/Views/MainWindow.axaml:22-392` |
| `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` | Capability selection, proof metadata, runtime diagnostics projection, localization provider, and retained-session demo state | ✓ VERIFIED | Exists, substantive, wired to view bindings, and projects live session data via `CaptureInspectionSnapshot()` and `GetRecentDiagnostics(10)` at `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:245-255` and `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:383-385` |
| `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` | Chinese-first stock shell copy for toolbar, library, workspace, fragments, mini map, and shortcuts | ✓ VERIFIED | Exists, substantive, wired as the only `GraphEditorView` embedded in `MainWindow.axaml`, and carries translated user-facing literals at `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml:104-125` and `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml:221-339` |
| `tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs` | Focused regression coverage for three-column shell contract | ✓ VERIFIED | Confirms required section labels, one `GraphEditorView`, and locked column sizes at `tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs:17-43` |
| `tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs` | Focused coverage for runtime diagnostics projection and separation from status text | ✓ VERIFIED | Confirms canonical seam names, inspection projection, and helper text at `tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs:10-76` |
| `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs` | Focused coverage for Chinese shell literals, provider path, and preserved English seam names | ✓ VERIFIED | Confirms translated literals and `IGraphLocalizationProvider` path at `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs:57-104` |
| `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` | Demo-focused headless tests can target the real Demo window | ✓ VERIFIED | Targets `net9.0` and references `AsterGraph.Demo` at `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj:2-26` |

### Key Link Verification

| From | To | Via | Status | Details |
| --- | --- | --- | --- | --- |
| `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` | `src/AsterGraph.Demo/Views/MainWindow.axaml` | Selected capability metadata and card bindings | ✓ WIRED | View binds `Capabilities`, `SelectedCapability*`, `ChromeModeProofLines`, `StandaloneSurfaceLines`, `PresentationLines`, and runtime properties at `src/AsterGraph.Demo/Views/MainWindow.axaml:57-80`, `src/AsterGraph.Demo/Views/MainWindow.axaml:223-340`, `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:194-347` |
| `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` | `src/AsterGraph.Avalonia/Controls/GraphEditorViewChromeMode.cs` | Full-shell versus canvas-only proof strip | ✓ WIRED | Proof strings reference both enum values at `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:204-210`; enum exists at `src/AsterGraph.Avalonia/Controls/GraphEditorViewChromeMode.cs:5-16` |
| `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` | Avalonia standalone factories | Standalone surface proof metadata | ✓ WIRED | `AsterGraphCanvasViewFactory`, `AsterGraphInspectorViewFactory`, and `AsterGraphMiniMapViewFactory` are surfaced in proof lines at `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:212-219` and match canonical usage in `tools/AsterGraph.HostSample/Program.cs:204-236` |
| `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` | `src/AsterGraph.Editor/Diagnostics/IGraphEditorDiagnostics.cs` | Inspection snapshot and recent-diagnostics projection | ✓ WIRED | Calls `Editor.Session.Diagnostics.CaptureInspectionSnapshot()` and `GetRecentDiagnostics(10)` at `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:245-255` and `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:383-385`; contract defined at `src/AsterGraph.Editor/Diagnostics/IGraphEditorDiagnostics.cs:5-18` |
| `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` | `src/AsterGraph.Editor/Localization/IGraphLocalizationProvider.cs` | Localized stock shell plus runtime-owned captions | ✓ WIRED | Stock shell binds editor captions while runtime provider remains the source for keyed editor text; provider contract at `src/AsterGraph.Editor/Localization/IGraphLocalizationProvider.cs:5-13`, demo provider at `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:519-529`, regression at `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs:80-93` |

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
| --- | --- | --- | --- | --- |
| `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` | `RecentDiagnostics` | `Editor.Session.Diagnostics.GetRecentDiagnostics(10)` | Yes — diagnostics come from public session diagnostics, exercised by `SaveWorkspace()` in `tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs:47-58` and demonstrated in `tools/AsterGraph.HostSample/Program.cs:163-165` | ✓ FLOWING |
| `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` | `RuntimeDocumentTitle`, `RuntimeNodeCount`, `RuntimeViewportZoom`, `RuntimeHasPendingConnection` | `Editor.Session.Diagnostics.CaptureInspectionSnapshot()` | Yes — snapshot is built from live document/selection/viewport/capability state via `GraphEditorInspectionSnapshot` at `src/AsterGraph.Editor/Diagnostics/GraphEditorInspectionSnapshot.cs:22-89` | ✓ FLOWING |
| `src/AsterGraph.Demo/Views/MainWindow.axaml` | Right-rail runtime card bindings | `MainWindowViewModel` runtime properties | Yes — bindings consume live session-backed properties and tests assert rendered helper text and layout contract at `tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs:61-76` and `tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs:17-43` | ✓ FLOWING |
| `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` | User-facing shell literals and editor captions | Static Chinese literals plus editor-localized bound captions | Yes — translated literals exist in source and provider-backed captions remain in `IGraphLocalizationProvider` path | ✓ FLOWING |

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
| --- | --- | --- | --- |
| Phase 6 focused regressions pass | `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter "DemoMainWindowTests|DemoDiagnosticsProjectionTests|GraphEditorLocalizationTests" --nologo -v minimal` | `失败: 0，通过: 12，已跳过: 0，总计: 12` | ✓ PASS |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
| --- | --- | --- | --- | --- |
| EMBD-01 | 06-01 | Host can embed the full default editor shell as a convenience composition | ✓ SATISFIED | Phase 6 demo centers one `GraphEditorView` as the canonical full shell at `src/AsterGraph.Demo/Views/MainWindow.axaml:159-180` |
| EMBD-02 | 06-01 | Host can embed a standalone graph viewport or canvas surface without the full shell | ✓ SATISFIED | Demo proof lines name `AsterGraphCanvasViewFactory` at `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:212-219` and host sample verifies actual factory usage at `tools/AsterGraph.HostSample/Program.cs:204-213` |
| EMBD-03 | 06-01 | Host can embed the mini map independently of the full shell | ✓ SATISFIED | Demo proof lines and host sample both cover `AsterGraphMiniMapViewFactory` at `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:212-219` and `tools/AsterGraph.HostSample/Program.cs:218-236` |
| EMBD-04 | 06-01 | Host can embed the inspector independently of the full shell | ✓ SATISFIED | Demo proof lines and host sample both cover `AsterGraphInspectorViewFactory` at `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:212-219` and `tools/AsterGraph.HostSample/Program.cs:214-236` |
| EMBD-05 | 06-01 | Host can embed or omit default menu and chrome presenters without rebuilding editor state | ✓ SATISFIED | Demo teaches `GraphEditorViewChromeMode.Default` vs `.CanvasOnly` at `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:204-210`; host sample verifies chrome toggling keeps the canvas/runtime at `tools/AsterGraph.HostSample/Program.cs:167-181` |
| PRES-01 | 06-01 | Host can replace node visuals without reimplementing behavior | ✓ SATISFIED | Demo right rail teaches presenter replacement through `AsterGraphPresentationOptions` at `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:221-227`; host sample shows custom node presenter wiring at `tools/AsterGraph.HostSample/Program.cs:184-203` |
| PRES-02 | 06-01 | Host can replace context-menu presentation while reusing menu intent | ✓ SATISFIED | Same proof surface plus host sample custom menu presenter at `tools/AsterGraph.HostSample/Program.cs:184-203` and `tools/AsterGraph.HostSample/Program.cs:456-467` |
| PRES-03 | 06-01 | Host can replace inspector presentation while reusing editor data contracts | ✓ SATISFIED | Same proof surface plus host sample custom inspector presenter at `tools/AsterGraph.HostSample/Program.cs:184-203` and `tools/AsterGraph.HostSample/Program.cs:469-476` |
| PRES-04 | 06-01 | Host can replace mini-map presentation without forking runtime | ✓ SATISFIED | Same proof surface plus host sample custom mini-map presenter at `tools/AsterGraph.HostSample/Program.cs:184-203` and `tools/AsterGraph.HostSample/Program.cs:478-485` |
| DEMO-I18N-01 | 06-02 | Chinese-first demo shell and preserved English technical seam names | ✓ SATISFIED | `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml:104-339` and `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs:57-104` |
| DIAG-01 | 06-03 | Host can receive machine-readable diagnostics without parsing UI status text | ✓ SATISFIED | Recent diagnostics list binds `Code`, `Severity`, `Operation`, `Message` at `src/AsterGraph.Demo/Views/MainWindow.axaml:314-340`; data sourced from `Editor.Session.Diagnostics.GetRecentDiagnostics(10)` at `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:245-255` |
| DIAG-02 | 06-03 | Host can request inspection snapshots for troubleshooting | ✓ SATISFIED | Runtime card binds inspection snapshot properties sourced by `CaptureInspectionSnapshot()` at `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:297-337` and `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:383-385` |
| DIAG-03 | 06-03 | Host can attach public diagnostics sinks/tracing contracts | ✓ SATISFIED | Phase 6 does not implement the sink itself, but it correctly teaches the canonical diagnostics seam by exposing `IGraphEditorSession` and `Editor.Session.Diagnostics` at `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:287-293`; public sink/tracing usage remains demonstrated in `tools/AsterGraph.HostSample/Program.cs:62-73` and `tools/AsterGraph.HostSample/Program.cs:96-114` |

**Orphaned requirements:** None. `REQUIREMENTS.md` does not map any additional Phase 6-specific requirement IDs, and every requirement declared in phase 6 plan frontmatter was accounted for above.

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
| --- | --- | --- | --- | --- |
| None | — | No phase-6 TODO/FIXME/placeholder or empty-return stub patterns found in scanned Demo source files | ℹ️ Info | No blocker anti-patterns detected in the verified implementation |

### Human Verification Required

### 1. Demo shell visual hierarchy

**Test:** Run the Demo and inspect the initial window.
**Expected:** The center `GraphEditorView` is clearly the dominant artifact, with left navigation and right explanation cards reading as supporting rails.
**Why human:** Source and headless tests verify structure, not perceived visual weight.

### 2. Capability switching continuity

**Test:** Click each of the four capability rows in the left rail.
**Expected:** The right rail updates to the selected capability while the editor stays live and feels continuous, with no visual sign of session reconstruction.
**Why human:** The retained-session code path is verified, but experiential continuity is a UX judgement.

### 3. Chinese copy quality

**Test:** Read the running Demo shell, toolbar, workspace, fragment, mini map, shortcuts, and proof cards.
**Expected:** Chinese UI copy feels natural and technical seam names remain intentionally English only where they identify real APIs.
**Why human:** Fluency and tone are subjective and cannot be fully asserted by code search.

### Gaps Summary

No automated implementation gaps found. The phase code, bindings, data flow, and focused regressions all support the intended Phase 6 showcase outcome. Remaining work is human confirmation of visual hierarchy, interaction feel, and localization quality in the live window.

---

_Verified: 2026-03-27T09:35:35Z_
_Verifier: Claude (gsd-verifier)_
