# Codebase Concerns

**Analysis Date:** 2026-04-16

## Primary Product Concerns

### Truth Alignment Still Needs Active Maintenance

- The active milestone is now v1.7 consumer closure and release hardening.
- The main operational risk is not missing capability surface. It is drift between README, host docs, planning artifacts, codebase maps, and proof entry points.
- A repo that ships runtime plugins, automation, smoke tools, and release gates still becomes hard to adopt if those surfaces are described inconsistently.

### The Proof Ring Must Stay Discoverable

- `eng/ci.ps1 -Lane release` is the official scripted verification entry point.
- `eng/ci.ps1 -Lane maintenance` is the hotspot refactor gate.
- `tools/AsterGraph.HostSample`, `tools/AsterGraph.PackageSmoke`, and `tools/AsterGraph.ScaleSmoke` now form the maintained runnable proof-tool layer.
- Risk: if solution membership, docs, or scripts drift apart again, external consumers and contributors will not know which entry points are authoritative.

## Architecture Concerns

### Compatibility Debt Is Deliberate, But Still Real

- `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs` is the canonical mutable runtime state owner.
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` is now a retained compatibility facade rather than the runtime owner, but it is still a large integration hotspot.
- `src/AsterGraph.Editor/Menus/CompatiblePortTarget.cs` and `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs::GetCompatibleTargets(...)` remain public compatibility shims.
- Risk: maintainers still need clear retirement guidance so future simplification does not turn into accidental breaking change.

### Hotspots Are Narrower, Not Gone

- `GraphEditorViewModel`, `GraphEditorKernel`, and `NodeCanvas.axaml.cs` remain the main concentrated change surfaces.
- Recent extractions reduced inline ownership, but these files still coordinate many editor/runtime/UI concerns.
- Risk: local refactors can still have wider blast radius than the public API suggests if focused proof is not kept aligned.

## Quality And Release Concerns

### The Release Gate Exists, But Matrix Clarity Still Matters

- CI already runs `eng/ci.ps1 -Lane all` across `net8.0` / `net9.0` plus a `release` job.
- Publishable packages target both frameworks; proof tools target `net8.0`; editor/demo regressions split across `net9.0` and `net8.0`.
- Risk: framework-specific regressions can still hide if the repo gate and CI jobs stop making those responsibilities explicit.

### History/Save Semantics Are Fixed In Code But Still Need Consumer-Level Publication

- Focused regressions and `ScaleSmoke` now enforce one retained history/save contract.
- Current remaining risk is documentation and onboarding: consumers still need that behavior described as a product contract, not only as passing tests.

## Suggested Next Watchpoints

- Keep `HostSample`, `PackageSmoke`, `ScaleSmoke`, and the scripted release gate aligned as one proof system.
- Keep compatibility-retirement wording explicit anywhere `GetCompatibleTargets(...)` and `CompatiblePortTarget` are still exposed.
- Keep the maintenance lane focused on hotspot seams rather than allowing it to become a second broad release lane.

---

*Concerns analysis refreshed: 2026-04-16*
