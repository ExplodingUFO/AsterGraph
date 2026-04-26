# Phase 289: Thin Host Builder And Adoption Proof Gate - Plan

## Goal

Add a thin hosted builder/facade over the canonical route and defend the productized adoption path with tests/proof markers.

## Success Criteria

1. Avalonia hosts can compose common hosted setup through a thin builder/facade accepting document, catalog, compatibility, plugin trust, localization, and diagnostics inputs.
2. Tests prove the builder/facade delegates to the existing canonical factories and does not introduce a parallel runtime path.
3. CI or docs tests defend README first-viewport claims, five-minute quick start, scenario demo launch, and ConsumerSample scenario markers.

## Tasks

1. Add hosted builder.
   - Implement `AsterGraphHostBuilder` in `AsterGraph.Avalonia.Hosting`.
   - Forward host inputs into `AsterGraphEditorOptions`.
   - Forward view inputs into `AsterGraphAvaloniaViewOptions`.
   - Keep `BuildAvaloniaView()` as composition over existing factories.

2. Add focused tests.
   - Validate required input behavior.
   - Verify editor/view/session types come from existing factory route.
   - Verify plugin trust, localization, diagnostics, presentation, shortcut policy, and view chrome are forwarded.

3. Update docs.
   - Show the five-minute builder path in README and Quick Start.
   - Explain builder versus canonical session/runtime route in Host Integration.
   - Add docs tests for builder vocabulary and adoption proof markers.

4. Verify and close.
   - Run focused initialization and docs tests.
   - Run versioning validation so public release wording stays aligned.

## Verification Commands

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --nologo -v minimal --filter "FullyQualifiedName~GraphEditorInitializationTests"
dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj -c Release --nologo -v minimal --filter "FullyQualifiedName~DemoProofReleaseSurfaceTests|FullyQualifiedName~StarterRecipeHardeningDocsTests|FullyQualifiedName~RetainedRouteComparisonClosureDocsTests"
pwsh -NoProfile -ExecutionPolicy Bypass -File eng/validate-public-versioning.ps1 -RepoRoot . -PublicTag v0.11.0-beta
```
