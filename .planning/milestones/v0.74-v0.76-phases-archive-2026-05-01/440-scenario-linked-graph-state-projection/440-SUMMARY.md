# Phase 440 Summary

## Beads

- Parent: `avalonia-node-map-h3e.2`
- Children:
  - `avalonia-node-map-h3e.2.3` — scenario cue projection model
  - `avalonia-node-map-h3e.2.1` — scenario selection ViewModel state
  - `avalonia-node-map-h3e.2.2` — scenario cue UI binding and proof

## Changes

- Added stable cookbook scenario keys plus graph cue labels, evidence-owned graph cue targets, and content cues in `DemoCookbookWorkspaceProjection`.
- Added selected cookbook scenario state to the Demo ViewModel and kept scenario-only refresh separate from full workspace reprojection.
- Added `PART_CookbookWorkspaceScenarioCueList` to the cookbook content panel so scenario selection updates graph and content cue lines while the live graph host stays above the details area.
- Added focused projection, ViewModel, headless UI, visual baseline, evidence-ownership, and narrow-ownership coverage.
- Fixed review feedback that identified fabricated path/evidence cue targets when scenario kind did not match the owning anchor category.

## Constraints

- No script execution was added.
- No generated runnable code path was added.
- No compatibility, fallback, polling, or degradation route was added.
- Cookbook changes stayed in narrow cookbook-owned files and tests.

## Verification

```powershell
dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj -c Release -f net9.0 --no-restore --filter "FullyQualifiedName~DemoCookbook" -v minimal
```

Result: passed 42/42.
