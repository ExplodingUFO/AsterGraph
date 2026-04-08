---
phase: 21-in-context-proof-and-narrative-polish
plan: 02
subsystem: runtime-proof-sections
completed: 2026-04-08
---

# Phase 21 Plan 02 Summary

Replaced the summary-heavy runtime and proof groups with dedicated live sections for configuration, runtime signals, and ownership proof.

Key changes:

- Added `CurrentConfigurationLines`, `RuntimeSignalLines`, and `OwnershipProofLines` in `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`, all projected directly from the existing host booleans and canonical `Editor.Session` inspection path.
- Reworked the runtime drawer content in `src/AsterGraph.Demo/Views/MainWindow.axaml` into `PART_RuntimeConfigurationSection`, `PART_RuntimeSignalsSection`, and the existing diagnostics section.
- Reworked the proof drawer content into `PART_ProofConfigurationSection`, `PART_ProofOwnershipSection`, and `PART_ProofRuntimeSignalsSection` instead of one generic text list.
- Added focused regressions that prove the new configuration/runtime rows stay aligned with the current live session.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~DemoMainWindowTests|FullyQualifiedName~DemoHostMenuControlTests|FullyQualifiedName~DemoDiagnosticsProjectionTests" -v minimal`
  - exit 0
  - `16` tests passed

Plan result:

- `PROOF-01` and `PROOF-02` are now satisfied inside the drawer surface itself.
- Runtime and proof surfaces now expose live state through dedicated rows rather than generic narrative cards.
