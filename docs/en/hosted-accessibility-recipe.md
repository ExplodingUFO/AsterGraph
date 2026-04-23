# Hosted Accessibility Recipe

Use this recipe when you want one copyable hosted path for keyboard, focus, and accessibility semantics on the canonical Avalonia route.
Pair it with `ConsumerSample.Avalonia`, because that is the defended hosted proof route for the current beta evaluator ladder.

## What It Covers

- baseline automation names on the hosted editor shell, canvas, search surfaces, and shipped inspector chrome
- command-palette focus round-trip back to the host surface that opened it
- accessible names on projected command buttons and authoring tool buttons
- authoring-surface metadata plus edge text editors on the same hosted route
- one screen-reader-ready evaluation path that stays on `ConsumerSample.Avalonia`, `HostSample`, and the bounded support-bundle record

## Copy This Hosted Path

- Step 1: keep baseline automation names on `GraphEditorView`, `NodeCanvas`, `GraphInspectorView`, `PART_StencilSearchBox`, `PART_OpenCommandPaletteButton`, `PART_CommandPaletteSearchBox`, and `PART_ParameterSearchBox`.
- Step 2: keep command-palette keyboard flow on the shared hosted view route so `Control+Shift+P` opens the palette and closing it returns focus to the prior host surface.
- Step 3: project accessible names for header commands, palette commands, node quick tools, and edge quick tools from the same action descriptors that already feed the hosted command surface.
- Step 4: keep selected-node parameter metadata and connection text editors on the same hosted authoring route, then close the proof with `AsterGraph.ConsumerSample.Avalonia -- --proof`.

## Screen-Reader-Ready Evaluation Path

- Run `AsterGraph.ConsumerSample.Avalonia -- --proof` first and keep `HOSTED_ACCESSIBILITY_BASELINE_OK:True`, `HOSTED_ACCESSIBILITY_FOCUS_OK:True`, `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`, `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`, and `HOSTED_ACCESSIBILITY_OK:True` on the defended hosted route.
- Generate the local evidence attachment from `ConsumerSample.Avalonia` with [Beta Support Bundle](./support-bundle.md); the support bundle remains the bounded intake attachment for this route.
- Run `AsterGraph.HostSample` after the realistic hosted proof and keep `HOST_SAMPLE_AUTOMATION_OK:True` plus `HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True` on the same bounded intake record.
- If you do local screen-reader-ready checks with Narrator, NVDA, or VoiceOver, keep them on the same named hosted surfaces and controls: `GraphEditorView`, `NodeCanvas`, `GraphInspectorView`, `PART_CommandPaletteSearchBox`, `PART_ParameterSearchBox`, projected command buttons, and projected node or edge tools.
- This path is local evaluation evidence only. It does not widen support promises and it does not claim screen-reader certification.

## Proof Contract

Validate the hosted route with:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

Expected hosted-accessibility proof markers:

- `HOSTED_ACCESSIBILITY_BASELINE_OK:True`
- `HOSTED_ACCESSIBILITY_FOCUS_OK:True`
- `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`
- `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`
- `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_OK:True`

## Related Docs

- [Consumer Sample](./consumer-sample.md)
- [Authoring Surface Recipe](./authoring-surface-recipe.md)
- [Beta Support Bundle](./support-bundle.md)
