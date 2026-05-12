# Hosted Accessibility Recipe

Use this recipe when you want one copyable hosted path for keyboard, focus, and accessibility semantics on the canonical Avalonia route.
Pair it with `src/AsterGraph.Demo`, because that is the defended hosted proof route for the current beta evaluator ladder.

## What It Covers

- baseline automation names on the hosted editor shell, canvas, search surfaces, and shipped inspector chrome
- command-palette focus round-trip back to the host surface that opened it
- accessible names on projected command buttons and authoring tool buttons
- authoring-surface metadata plus edge text editors on the same hosted route
- one screen-reader-ready evaluation path that stays on `src/AsterGraph.Demo`, `release validation lane`, and the bounded support-bundle record

## Copy This Hosted Path

- Step 1: keep baseline automation names on `GraphEditorView`, `NodeCanvas`, `GraphInspectorView`, `PART_StencilSearchBox`, `PART_OpenCommandPaletteButton`, `PART_CommandPaletteSearchBox`, and `PART_ParameterSearchBox`.
- Step 2: keep command-palette keyboard flow on the shared hosted view route so `Control+Shift+P` opens the palette and closing it returns focus to the prior host surface.
- Step 3: project accessible names for header commands, palette commands, node quick tools, and edge quick tools from the same action descriptors that already feed the hosted command surface.
- Step 4: keep selected-node parameter metadata and connection text editors on the same hosted authoring route, then close the proof with `src/AsterGraph.Demo -- --proof`.

## Screen-Reader-Ready Evaluation Path

- Run `src/AsterGraph.Demo -- --proof` first and keep `HOSTED_ACCESSIBILITY_BASELINE_OK:True`, `HOSTED_ACCESSIBILITY_FOCUS_OK:True`, `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`, `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`, `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`, and `HOSTED_ACCESSIBILITY_OK:True` on the defended hosted route.
- Generate the local evidence attachment from `src/AsterGraph.Demo` with [Beta Support Bundle](./support-bundle.md); the support bundle remains the bounded intake attachment for this route.
- Run `release validation lane` after the realistic hosted proof and keep `HOST_SAMPLE_AUTOMATION_OK:True`, `HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True`, and `HOST_SAMPLE_ACCESSIBILITY_AUTOMATION_OK:True` on the same bounded intake record.
- If you do local screen-reader-ready checks with Narrator, NVDA, or VoiceOver, keep them on the same named hosted surfaces and controls: `GraphEditorView`, `NodeCanvas`, `GraphInspectorView`, `PART_CommandPaletteSearchBox`, `PART_ParameterSearchBox`, projected command buttons, and projected node or edge tools.
- This path is local evaluation evidence only. It does not widen support promises and it does not claim screen-reader certification.

## Manual Assistive-Technology Validation Checklist

`ACCESSIBILITY_MANUAL_AT_VALIDATION_PLAN` keeps manual assistive-technology validation separate from the existing headless automation proof. Start only after `HOSTED_ACCESSIBILITY_OK:True` and the release validation lane are green; that evidence proves automation names, focus routes, keyboard flow, and command surfaces, but it still leaves live announcements as unverified live screen-reader behavior.

- Narrator on Windows: open the hosted Demo route, tab through `GraphEditorView`, `NodeCanvas`, `GraphInspectorView`, `PART_CommandPaletteSearchBox`, and projected command buttons; record whether names, focus movement, command-palette close, validation focus buttons, and export/status text are announced as expected.
- NVDA on Windows: repeat the same named route and note whether browse/focus modes expose the same command labels, selected-node parameter metadata, edge text editor names, and validation target help text.
- VoiceOver on macOS or the nearest platform-equivalent check for the current host: repeat the same named surfaces and record any divergence in focus order, command labels, and status announcements.
- Attach the manual notes beside the Demo support bundle and keep failures as adopter evidence for a later implementation tracker.

This checklist is planning evidence only: no live-region/runtime behavior change, no UI change, no public API change, no retained API removal, and no broad screen-reader certification claim.

## Manual Assistive-Technology Evidence Package

`ACCESSIBILITY_MANUAL_AT_EVIDENCE_PACKAGE` is the Phase 516 / GitHub #152 / `avalonia-node-map-821` evidence record. It converts the Phase 505 plan into a bounded platform-equivalent package while keeping the headless automation proof separate from live assistive-technology observations.

| Evidence lane | Hosted route states exercised | Observed result | Boundary |
| --- | --- | --- | --- |
| Fresh Demo proof on 2026-05-12 with `dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --configuration Release --nologo -- --proof` | hosted Demo route, command surface, native interaction accessibility checks, and proof metrics | emitted `NATIVE_INTERACTION_A11Y_OK:True`, `COMMAND_SURFACE_OK:True`, `DEMO_OK:True`, and `HOST_NATIVE_METRIC:*` lines | platform-equivalent headless automation proof only; live screen-reader observations were not performed |
| Focused headless accessibility tests | `GraphEditorView`, `NodeCanvas`, `GraphInspectorView`, `PART_CommandPaletteSearchBox`, `PART_ParameterSearchBox`, projected command buttons, projected node or edge tools, validation focus buttons, and export/status text | accessible names, focusability, focus recovery, command-surface projection, and validation focus routes remain guarded by `GraphEditorViewTests`, `NodeCanvasStandaloneTests`, and `AccessibilityManualValidationDocsTests` | dynamic validation/export/status announcements remain not observed by a live screen reader |
| Live assistive-technology pass | Narrator, NVDA, and VoiceOver | not observed in Phase 516 | GitHub #156 / `avalonia-node-map-1pd` owns live screen-reader announcement validation before any broader claim |

The evidence package keeps no live-region/runtime behavior change, no UI change, no public API change, no retained API removal, and no broad screen-reader certification claim. Any later Narrator, NVDA, VoiceOver, or platform-equivalent notes should stay on this same hosted route and record the exact announcements and focus transitions rather than replacing the headless proof.

## Live Assistive-Technology Platform-Equivalent Evidence

`ACCESSIBILITY_LIVE_AT_UIA_EVIDENCE` is the Phase 517 / GitHub #156 / `avalonia-node-map-1pd` evidence record. It records a Windows UI Automation platform-equivalent check against the hosted Demo route without claiming live screen-reader speech output.

| Evidence lane | Hosted route states exercised | Observed result | Boundary |
| --- | --- | --- | --- |
| Windows UI Automation platform-equivalent check on 2026-05-12 with `src/AsterGraph.Demo/bin/Release/net9.0/AsterGraph.Demo.exe --scenario validation-prevent-cycle` | `GraphEditorView`, `NodeCanvas`, projected host command buttons, and visible node/port names in the `validation-prevent-cycle` scenario | `GraphEditorView`, `NodeCanvas`, projected host command buttons, and some node/port names were exposed through UIA | platform-equivalent evidence only; live screen-reader speech output was not observed |
| Gap observed in the same UIA run | `PART_CommandPaletteSearchBox`, `PART_ParameterSearchBox`, validation/export/status surfaces, and usable live-region metadata | those targets were not exposed in the observed initial validation scenario state | dynamic validation/export/status announcements remain unproven and are tracked by GitHub #158 / `avalonia-node-map-g0u` |

This Phase 517 evidence keeps no live-region/runtime behavior change, no UI change, no public API change, no retained API removal, and no broad screen-reader certification claim. GitHub #158 / `avalonia-node-map-g0u` owns the follow-up runtime proof for dynamic validation/export/status announcements.

## Dynamic Announcement Runtime Contract

`ACCESSIBILITY_DYNAMIC_ANNOUNCEMENT_CONTRACT` is the Phase 518 / GitHub #158 / `avalonia-node-map-g0u` runtime contract. It adds stable automation names, reviewed help text, and `AutomationProperties.LiveSetting` set to `Polite` on the existing hosted Demo validation/export/status text surfaces.

| Surface | Automation name | Live-region contract | Boundary |
| --- | --- | --- | --- |
| `PART_ValidationStatusText` | `Validation status` | `AutomationProperties.LiveSetting=Polite`; text updates when the graph validation snapshot changes | headless/runtime metadata proof only |
| `PART_StatusValidationText` | `Status bar validation` | `AutomationProperties.LiveSetting=Polite`; text updates when the graph validation snapshot changes | headless/runtime metadata proof only |
| `PART_ExportStatusText` | `Export status` | `AutomationProperties.LiveSetting=Polite`; text updates when scene export progress or completion changes | headless/runtime metadata proof only |
| `PART_CurrentStatusText` | `Current editor status` | `AutomationProperties.LiveSetting=Polite`; text updates when editor commands publish status messages | headless/runtime metadata proof only |

The focused guard is `DynamicStatusAnnouncementRegions_ExposeStableLiveRegionContract`, which verifies the named surfaces and dynamic text updates without requiring Narrator, NVDA, or VoiceOver in CI. This closes the previous validation/export/status live-region metadata gap, but live screen-reader speech output was not observed and this is not a broad screen-reader certification claim.

## Proof Contract

Validate the hosted route with:

```powershell
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -- --proof
```

Expected hosted-accessibility proof markers:

- `HOSTED_ACCESSIBILITY_BASELINE_OK:True`
- `HOSTED_ACCESSIBILITY_FOCUS_OK:True`
- `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`
- `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`
- `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`
- `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_OK:True`

## Related Docs

- [Demo Guide](./demo-guide.md)
- [Authoring Surface Recipe](./authoring-surface-recipe.md)
- [Beta Support Bundle](./support-bundle.md)
