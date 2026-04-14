# Phase 29: Release Validation And Canonical Adoption Path - Context

**Gathered:** 2026-04-14
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 29 closes v1.5 by turning the current build/test baseline into a release-grade validation surface and by reducing host onboarding to one short canonical decision path.

This phase is about:

- extending the tracked repo-local validation path so release automation executes the same smoke and compatibility checks the milestone now depends on
- adding checked-in coverage/reporting and public API or package-compatibility expectations for the four publishable packages
- collapsing runtime-only, shipped-UI, and retained-migration host guidance into one short canonical entry path
- synchronizing README and host-facing docs so they point at the same release-validation command and the same adoption decision path

This phase is not about:

- reopening the four-package SDK boundary
- replacing `AsterGraph.Demo` with a new maintained host sample unless the current maintained surfaces prove insufficient
- removing retained compatibility APIs in one shot
- adding new end-user graph-editing features outside release validation and host-adoption clarity

</domain>

<decisions>
## Implementation Decisions

### Release Validation Surface

- **D-01:** Keep one repo-local validation entrypoint anchored in `eng/ci.ps1`. Phase 29 should extend that script with a release-validation lane instead of inventing a separate README-only or CI-only command path.
- **D-02:** The Phase 29 release lane must execute proof steps, not just build them. `PackageSmoke` and `ScaleSmoke` should run explicitly inside release validation, with their roles kept distinct.

### Coverage And Compatibility Gates

- **D-03:** Coverage and reporting expectations should be checked in as deterministic repo configuration, not left as local instructions in README prose.
- **D-04:** Public API or package-compatibility checks should cover the four publishable packages, with `AsterGraph.Abstractions` treated as the minimum non-negotiable contract layer.

### Canonical Adoption Path

- **D-05:** One short host-adoption entry path should become the source of truth. `docs/quick-start.md` should stay that short decision entrypoint, while deeper docs link out from it instead of duplicating route trees.
- **D-06:** The canonical decision path must stay three-way: runtime-only (`CreateSession(...)`), shipped UI (`Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)`), and retained migration (`GraphEditorViewModel` / `GraphEditorView`).

### Scope Control

- **D-07:** Prefer synchronizing docs and validation around the currently maintained surfaces (`PackageSmoke`, `ScaleSmoke`, `AsterGraph.Demo`, `eng/ci.ps1`) over adding a new host sample in this phase.
- **D-08:** Do not reopen package boundaries or compatibility-retirement scope here. Phase 29 closes validation and adoption-path gaps on top of the already-shipped v1.5 baseline.

### the agent's Discretion

- The exact checked-in coverage format, threshold, and artifact layout once the release lane is in place
- The exact storage location and naming of API baseline or compatibility artifacts under the repo
- Whether the short adoption path lives as a dedicated section inside `docs/quick-start.md` or as a nearby doc linked from Quick Start, as long as Quick Start remains the canonical short entry

</decisions>

<specifics>
## Specific Ideas

- Keep the repo command story simple: contributors and CI should both run the same release-validation entrypoint rather than memorize a separate command bundle from the README.
- Keep `PackageSmoke` focused on packed-package consumption and keep `ScaleSmoke` focused on runtime/readiness proof rather than merging them into one catch-all tool.
- Make the short adoption path explicitly answer three questions for a host:
  - "I only need the runtime."
  - "I want the shipped Avalonia shell."
  - "I am migrating from the retained facade."

</specifics>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope and current state
- `.planning/PROJECT.md` - current v1.5 framing after Phase 28 completion
- `.planning/REQUIREMENTS.md` - `QUAL-03` and `PROOF-03` requirements for this phase
- `.planning/ROADMAP.md` - Phase 29 goal, dependencies, and success criteria
- `.planning/STATE.md` - current phase position and next execution target

### Live validation surface
- `eng/ci.ps1` - current repo-local validation entrypoint to extend
- `.github/workflows/ci.yml` - checked-in CI reuse of the repo-local validation command
- `tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj` - packed-package proof tool
- `tools/AsterGraph.PackageSmoke/Program.cs` - packed-package proof behavior
- `tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj` - runtime/readiness proof tool
- `tools/AsterGraph.ScaleSmoke/Program.cs` - runtime/readiness proof behavior

### Current host-adoption docs
- `README.md` - public proof and package story
- `docs/quick-start.md` - current short route guide and likely canonical adoption entry
- `docs/host-integration.md` - detailed runtime-only, shipped-UI, and migration guidance

### Canonical entry APIs
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` - runtime-only and hosted-UI creation entrypoints
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs` - shipped Avalonia shell entrypoint
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` - retained compatibility facade that still needs staged migration guidance

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets

- `eng/ci.ps1` already centralizes per-framework package builds and split regression lanes, so Phase 29 can extend a real repo-local command path instead of inventing a second one.
- `.github/workflows/ci.yml` already shells out to `eng/ci.ps1`, so later CI changes can stay thin if the release lane lives there too.
- `tools/AsterGraph.PackageSmoke` already supports packed-package consumption through `UsePackedAsterGraphPackages=true`.
- `tools/AsterGraph.ScaleSmoke` already emits stable runtime/readiness proof markers for larger-graph scenarios.
- `docs/quick-start.md` already contains the three-way route guide for runtime-only, shipped-UI, and retained-migration entrypoints.

### Established Patterns

- The repo prefers script-first local validation that CI reuses instead of duplicating logic in YAML.
- Public host documentation is already layered: README for package/proof overview, Quick Start for the short path, and Host Integration for deeper implementation detail.
- Core-vs-demo regression lanes are already split, so release validation can build on that separation instead of re-litigating it.

### Integration Points

- Release validation work will connect primarily to `eng/ci.ps1`, `.github/workflows/ci.yml`, smoke-tool projects, and any new checked-in coverage or API-compat config.
- Canonical adoption-path work will connect primarily to `docs/quick-start.md`, `README.md`, and `docs/host-integration.md`.
- Compatibility guidance must stay anchored in `AsterGraphEditorFactory.CreateSession(...)`, `AsterGraphEditorFactory.Create(...)`, `AsterGraphAvaloniaViewFactory.Create(...)`, and the retained `GraphEditorViewModel` bridge.

</code_context>

<deferred>
## Deferred Ideas

- Reintroducing a brand-new minimal host sample if the current demo and docs still prove insufficient after Phase 29
- Marketplace, trust, signing, or plugin distribution work beyond the current milestone
- A one-shot removal of retained compatibility APIs

</deferred>

---

*Phase: 29-release-validation-and-canonical-adoption-path*
*Context gathered: 2026-04-14*
