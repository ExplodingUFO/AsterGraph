# Codebase Concerns

**Analysis Date:** 2026-04-08

## Primary Architecture Concerns

### Kernel-First Migration Is Not Finished

- `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs` now exists as the canonical runtime state owner for the session-first path.
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` still implements `IGraphEditorSessionHost`, which means the compatibility facade still owns part of the runtime contract surface.
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs`, `.planning/STATE.md`, and `.planning/ROADMAP.md` all point at Phase 14 as the next decoupling step.
- Risk: dual-path drift between the kernel/session path and the retained facade path.

### Public Surface Still Carries MVVM-Shaped Compatibility Debt

- `src/AsterGraph.Editor/Menus/CompatiblePortTarget.cs` still exposes `NodeViewModel` and `PortViewModel` references.
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs` retains both DTO-style `GetCompatiblePortTargets(...)` and compatibility-oriented `GetCompatibleTargets(...)`.
- Several extension interfaces and runtime members still carry `[Obsolete(...)]` compatibility overloads instead of being fully retired.
- Risk: later capability normalization work remains harder until hosts no longer depend on view-model-shaped data.

## Coordination Hotspots

### Large Orchestration Types Still Dominate Change Risk

- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs`, and `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` remain the main coordination hotspots.
- Even with recent extraction work, editor/session, MVVM projection, and Avalonia interaction logic are still concentrated in a few large files.
- Risk: regressions can cross command, selection, history, diagnostics, and rendering boundaries when one hotspot changes.

### Compatibility And Proof Logic Are Mixed Into Production Flow

- The repo intentionally keeps legacy constructor/view paths alive while adding new factory/session paths.
- `tools/AsterGraph.HostSample/Program.cs` and `tools/AsterGraph.PackageSmoke/Program.cs` verify both stories at once.
- Risk: this is useful proof coverage, but it also increases maintenance cost because two composition models must stay behaviorally aligned until migration is complete.

## Quality And Tooling Concerns

### No Strong Repo-Level Quality Gates

- No `.editorconfig`, analyzer ruleset, or central package-management file is tracked.
- No coverage config or checked-in CI workflow was detected.
- The main quality gates are `dotnet build`, `dotnet test`, and the proof tools.
- Risk: style drift, unnoticed target-matrix regressions, and documentation drift are easier to accumulate.

### Mixed Target Matrix Requires Ongoing Attention

- Publishable packages target `net8.0` and `net9.0`.
- `tests/AsterGraph.Editor.Tests` runs on `net9.0`, while `tests/AsterGraph.Serialization.Tests` and the tool projects run on `net8.0`.
- Risk: behavior that only reproduces on one target framework may escape if verification is not run across the right combination of projects.

## Product And Roadmap Concerns

### Plugin And Automation Readiness Is Still Deferred

- `.planning/REQUIREMENTS.md` keeps plugin loading and richer automation explicitly out of scope for the current milestone.
- The architecture is being hardened for those later goals, but the runtime is not yet at the final explicit-capability boundary.
- Risk: consumers may infer more runtime extensibility than the current public API actually stabilizes today.

### Documentation Can Drift Quickly During Phase Work

- The codebase moved materially after the previous map on 2026-04-03: Phase 13 landed, `GraphEditorKernel` was introduced, Phase 14 planning artifacts were added, and `tools/AsterGraph.ScaleSmoke` became part of the proof surface.
- Generated planning/reference docs therefore need periodic refresh to remain trustworthy.
- Risk: stale architecture or testing docs can push later planning and review work in the wrong direction.

## What Looks Better Than Before

- `Directory.Build.props` now excludes `artifacts/**` from default compile globs, reducing an earlier class of audit-output build risk.
- `tools/AsterGraph.ScaleSmoke/Program.cs` gives the repo a more explicit large-graph proof surface than before.
- Diagnostics, inspection snapshots, and instrumentation seams are now formalized in `src/AsterGraph.Editor/Diagnostics/*.cs`.

## Suggested Next Watchpoints

- Track when `GraphEditorViewModel` stops implementing `IGraphEditorSessionHost`.
- Track when `CompatiblePortTarget` and related compatibility APIs stop exposing MVVM objects as canonical runtime data.
- Keep the package smoke, scale smoke, and migration compatibility suites in sync whenever runtime/session contracts change.

---

*Concerns analysis refreshed: 2026-04-08*
