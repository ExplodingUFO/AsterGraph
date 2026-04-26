# Phase 289: Thin Host Builder And Adoption Proof Gate - Verification

**Status:** Passed
**Verified:** 2026-04-26

## Commands

```powershell
dotnet test tests\AsterGraph.Editor.Tests\AsterGraph.Editor.Tests.csproj -c Release --nologo -v minimal --filter "FullyQualifiedName~GraphEditorInitializationTests"
```

Result: passed, 18 tests.

```powershell
dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj -c Release --nologo -v minimal --filter "FullyQualifiedName~DemoProofReleaseSurfaceTests|FullyQualifiedName~StarterRecipeHardeningDocsTests|FullyQualifiedName~RetainedRouteComparisonClosureDocsTests"
```

Result: passed, 40 tests.

```powershell
dotnet build src\AsterGraph.Avalonia\AsterGraph.Avalonia.csproj -c Release --nologo -v minimal
```

Result: passed for net8.0 and net9.0. The build emitted the pre-existing `GraphPort` XML documentation warnings.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File eng\validate-public-versioning.ps1 -RepoRoot . -PublicTag v0.11.0-beta
```

Result: passed with `PUBLIC_VERSIONING_OK:0.11.0-beta:v0.11.0-beta`.

## Residual Risk

- The builder is source-documented and test-backed, but not yet included in a `dotnet new` template.
- Public API analyzer and API baseline/diff gates remain deferred to v2 API governance.
