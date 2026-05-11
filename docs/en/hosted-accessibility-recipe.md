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
