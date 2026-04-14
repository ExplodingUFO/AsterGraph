# Phase 29 Research: Release Validation And Canonical Adoption Path

**Date:** 2026-04-14
**Phase:** 29-release-validation-and-canonical-adoption-path

## Research Questions

1. What tracked validation surface already exists that Phase 29 can extend without creating a second command path?
2. Which release-grade checks are still missing from the current repo baseline?
3. What maintained proof and doc surfaces already exist for the canonical host-adoption story?
4. How should Phase 29 stay disciplined so it closes the milestone instead of reopening earlier architecture work?

## Findings

### 1. The repo already has a reusable script-first validation entrypoint

`eng/ci.ps1` currently builds all four publishable packages across `net8.0` and `net9.0`, builds the smoke-tool projects, and runs the split regression lanes. `.github/workflows/ci.yml` already shells out to that same script.

Implication:

- Phase 29 should extend `eng/ci.ps1` rather than create a second release-validation command path.
- CI changes should stay thin and keep reusing the repo-local command surface.

### 2. Release-grade checks are still missing, not partially solved

The current repo has no checked-in coverage collector, no `runsettings`, and no public API or package-compatibility infrastructure. `PackageSmoke` and `ScaleSmoke` exist and are documented, but Phase 28 intentionally stopped at building them inside automation rather than executing them there.

Implication:

- Phase 29 should introduce fresh checked-in release-validation infrastructure rather than pretend there is an existing baseline to extend lightly.
- The release lane needs to make smoke execution, coverage/reporting, and API/package compatibility first-class machine-checked work.

### 3. The canonical host-adoption story already exists, but it is spread across multiple docs

`docs/quick-start.md` already contains the shortest current route guide:

- `CreateSession(...)` for runtime-only hosts
- `Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)` for the shipped UI path
- retained `GraphEditorViewModel` / `GraphEditorView` for staged migration

`README.md` and `docs/host-integration.md` also cover these paths, but in longer forms.

Implication:

- Phase 29 should make Quick Start the short canonical decision path and make other docs point back to it instead of carrying parallel route trees.
- A new host sample should not be the default answer if the current maintained docs and demo can express the path clearly.

### 4. The maintained proof surfaces are already good enough to reuse

The repo already has:

- `tools/AsterGraph.PackageSmoke` for packed-package proof
- `tools/AsterGraph.ScaleSmoke` for runtime/readiness proof
- `src/AsterGraph.Demo` as the runnable visual reference host
- split `AsterGraph.Editor.Tests`, `AsterGraph.Serialization.Tests`, and `AsterGraph.Demo.Tests`

Implication:

- Phase 29 should reuse these maintained surfaces rather than introduce new proof tools unless a real gap appears during implementation.

### 5. Scope discipline matters more than feature breadth here

The remaining milestone requirements are `QUAL-03` and `PROOF-03`. That means the phase is about:

- release validation becoming automatic and checked in
- host adoption becoming shorter and more synchronized

It is not about:

- changing the package boundary
- removing retained compatibility APIs
- inventing a broader documentation IA rewrite

Implication:

- Plan Phase 29 around release validation automation first, canonical adoption-path docs second, and final proof/doc sync third.

## Risks And Guardrails

- Do not let the release lane and README diverge. The repo should document the same command it actually runs.
- Do not collapse `PackageSmoke` and `ScaleSmoke` into one ambiguous step; each exists for a different proof reason.
- Do not create a second "canonical" adoption guide parallel to Quick Start.
- Do not widen the phase into new samples, trust/distribution work, or compatibility removal.

## Recommended Planning Posture

### Wave 1: Add a real release-validation lane

Extend `eng/ci.ps1` with a release lane that runs packed-package smoke, scale smoke, coverage/reporting, and API/package compatibility checks through checked-in config.

### Wave 2: Make Quick Start the short canonical host-adoption path

Shorten and synchronize the runtime-only, shipped-UI, and retained-migration route guidance around Quick Start while preserving deeper host-integration detail.

### Wave 3: Sync proof/docs/CI around the same release lane

Update README, Host Integration, and CI wiring so the repo, the docs, and the milestone verification all point at the same release-validation command and entry APIs.

## Recommendation

Plan Phase 29 around one narrow principle: the repo itself should execute and document the same release-validation and host-adoption paths that it asks external consumers to trust.

That closes v1.5 without reopening solved boundary work.

---

*Research complete: 2026-04-14*
