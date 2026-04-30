# Phase 457 Plan

## Success Criteria

1. Public docs and examples explain supported v0.76 contracts without external-inspiration naming.
2. All v0.76 requirements map to phases and proof markers.
3. Required verification passes for contract, cookbook, editor, layout, hierarchy, and prohibited-name gates.
4. Beads, Dolt, Git branch, and workspace are clean and pushed.

## 457.1 — Contract Docs And Requirement Mapping Audit

Bead: `avalonia-node-map-y7i.7.1`

Scope:

- Audit public docs and examples against v0.76 requirements.
- Update docs only when a real source-backed contract or proof marker is missing.
- Write a compact mapping report in this phase directory.

Verification:

- Docs/proof marker grep checks.
- Prohibited external-inspiration-name scan across `docs`, `src`, and `.planning`.

## 457.2 — Release Proof And Verification Gate

Bead: `avalonia-node-map-y7i.7.2`

Scope:

- Run release-sensitive focused tests for cookbook, editor contracts, hierarchy, layout, and docs.
- Write a verification report in this phase directory.
- Do not change docs owned by 457.1 unless verification exposes a concrete blocker.

Verification:

- `dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj --filter "FullyQualifiedName~DemoCookbook" -m:1 --logger "console;verbosity=minimal"`
- `dotnet test tests\AsterGraph.Editor.Tests\AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorHierarchyStateContractsTests|FullyQualifiedName~GraphEditorLayoutProviderSeamTests|FullyQualifiedName~GraphEditorKernelCommandRouterTests|FullyQualifiedName~GraphEditorToolProviderContractTests|FullyQualifiedName~GraphEditorViewTests" -m:1 --logger "console;verbosity=minimal"`
- `git diff --check`

## 457.3 — Milestone Clean Closeout

Bead: `avalonia-node-map-y7i.7.3`

Scope:

- Close Phase 457 and the v0.76 epic if verification is green.
- Update roadmap/state/milestone artifacts.
- Ensure beads, Dolt, Git, and worktrees are clean and pushed.
