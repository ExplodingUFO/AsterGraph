# Phase 470 Handoff

## Next Phase

Phase 471: `v0.78 Release Proof And API Governance`

Bead: `avalonia-node-map-v78.6`

## What To Verify Next

Phase 471 should close the milestone by checking release-facing contracts rather than adding more cookbook behavior.

Recommended beads:

1. Public API inventory and baseline classification
   - Responsibility: compare v0.78 public API changes against intended component-platform contracts.
   - Scope: API baseline files, package metadata, release notes inputs.
   - Verification: existing API baseline tests plus a focused diff review.

2. Docs and examples release gate
   - Responsibility: verify supported v0.78 contracts are documented without external inspiration names or unsupported adapter/runtime claims.
   - Scope: `docs/`, `README.md`, samples, templates, cookbook docs.
   - Verification: documentation scans and targeted docs tests.

3. CI-sensitive .NET 8/9/10 proof
   - Responsibility: confirm local release gates still exercise the intended target frameworks, especially .NET 10.
   - Scope: workflow files, test projects, release scripts.
   - Verification: focused local commands that match CI where possible.

4. Milestone closeout synthesis
   - Responsibility: map INT-01, RENDER-01, CUSTOM-01, SPACE-01, COOK-02, and REL-02 to phase evidence and close beads/Dolt/Git cleanly.
   - Scope: `.planning`, beads, branch/worktree cleanup.
   - Verification: `bd ready`, `bd status`, `bd dolt status`, `git status`, and push confirmation.

## Phase 470 Evidence To Reuse

- Rendering/viewport cookbook route: `v078-rendering-viewport-route`
- Customization cookbook route: `v078-customization-route`
- Spatial authoring cookbook route: `v078-spatial-authoring-route`
- Focused verification command: `dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj --filter Cookbook -m:1 --logger "console;verbosity=minimal"`

## Boundaries

- Do not add fallback layers, compatibility shims, or generated runnable cookbook code.
- Do not make Demo the supported package boundary.
- Do not name external inspiration projects in docs or planning artifacts.
- Keep Phase 471 as a release proof/governance phase unless a release gate exposes a real defect.
