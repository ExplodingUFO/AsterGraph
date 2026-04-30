# Phase 456 Plan: Designer Workbench Authoring UX

## Success Criteria

1. Navigator/outline projection is source-backed by existing graph, group, scope, and selection snapshots.
2. Route/group/layout/recovery affordances appear through existing workbench surfaces with coherent enabled/disabled state.
3. Demo cookbook documents and proves the designer workbench route without overstating package contracts.
4. Focused tests pass for new projections and existing authoring/command/cookbook behavior.
5. Beads, Git, Dolt, branches, and `.worktrees` are clean after each bead closes.

## Execution

### 456.1 — Navigator and Outline Projection

Bead: `avalonia-node-map-y7i.6.1`

Scope:

- Add a small source-backed navigator/outline projection over existing graph, group, scope, and selection snapshots.
- Prefer a projection type and focused tests over broad UI rewrites.
- If UI chrome is required, keep it minimal and do not overlap with 456.2 affordance controls.

Candidate files:

- `src/AsterGraph.Demo/Cookbook/DemoCookbookWorkspaceProjection.cs`
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.CookbookState.cs`
- `tests/AsterGraph.Demo.Tests/DemoCookbookWorkspaceProjectionTests.cs`
- or a small editor/runtime projection type if explorer evidence shows it is the cleaner source boundary.

Verification:

- `dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj --filter "FullyQualifiedName~DemoCookbookWorkspaceProjection" -m:1 --logger "console;verbosity=minimal"`

### 456.2 — Route Group Layout Recovery Affordances

Bead: `avalonia-node-map-y7i.6.2`

Scope:

- Project route-aware group/layout/recovery actions through existing workbench surfaces.
- Use session descriptors and command enabled/disabled state as the source of truth.
- Keep command execution on existing command IDs; do not add compatibility adapters.

Candidate files:

- `src/AsterGraph.Avalonia/Controls/GraphEditorView.AuthoringTools.cs`
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`
- `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`
- descriptor builders only if an affordance is missing from the existing source.

Verification:

- `dotnet test tests\AsterGraph.Editor.Tests\AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~AuthoringToolsChrome|FullyQualifiedName~CommandPalette_ProjectsContextualAuthoringActions|FullyQualifiedName~ProblemsPanel" -m:1 --logger "console;verbosity=minimal"`

### 456.3 — Designer Cookbook Proof and Docs

Bead: `avalonia-node-map-y7i.6.3`

Scope:

- Add cookbook recipe/proof/docs markers for the designer workbench UX.
- Update roadmap/state and close Phase 456.
- Keep docs source-backed and avoid unsupported claims.

Candidate files:

- `src/AsterGraph.Demo/Cookbook/DemoCookbookCatalog.Recipes.cs`
- `src/AsterGraph.Demo/Cookbook/DemoCookbookProof.cs`
- `docs/en/demo-cookbook.md`
- `docs/zh-CN/demo-cookbook.md`
- `tests/AsterGraph.Demo.Tests/DemoCookbookDocsTests.cs`
- `tests/AsterGraph.Demo.Tests/DemoCookbookProofClosureTests.cs`
- `.planning/ROADMAP.md`
- `.planning/STATE.md`

Verification:

- `dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj --filter "FullyQualifiedName~DemoCookbook" -m:1 --logger "console;verbosity=minimal"`
- Prohibited external-inspiration-name scan across `docs`, `src`, and `.planning` must return no matches.

## Worktree Rules

- Implementation worktrees must be under `.worktrees/phase456-*`.
- Each worktree owns exactly one child bead.
- Each child bead closes only after focused tests pass, implementation is merged to `master`, bead state is exported/committed, direct Dolt push succeeds, Git push succeeds, and the worktree/branch is removed.
