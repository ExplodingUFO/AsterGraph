# Codebase Concerns

**Analysis Date:** 2026-04-14

## Primary Architecture Concerns

### Kernel-First Migration Is Not Finished

- `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs` is now the canonical runtime state owner for the session-first path.
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` still implements `IGraphEditorSessionHost`, so compatibility-facade obligations remain.
- `.planning/STATE.md` and `.planning/ROADMAP.md` now reflect v1.5 completion through Phase 28 with Phase 29 next.
- Risk: dual-path drift remains between kernel-first runtime and retained compatibility pathways until migration debt is removed.

### Public Surface Still Carries MVVM-Shaped Compatibility Debt

- `src/AsterGraph.Editor/Menus/CompatiblePortTarget.cs` still exposes `NodeViewModel` and `PortViewModel` references.
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs` still includes legacy compatibility-oriented members such as `GetCompatibleTargets(...)`.
- `[Obsolete(...)]` compatibility members are still present in extension and runtime seams.
- Risk: later capability normalization and API simplification remain deferred until these signatures are retired.

## Coordination Hotspots

### Large Orchestration Types Still Dominate Change Risk

- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs`, and `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` remain the main coordination hotspots.
- Even with recent extraction work, editor/session, MVVM projection, and Avalonia interaction logic remain concentrated in large files.
- Risk: a local change in one boundary can propagate across command, selection, history, diagnostics, and rendering behavior.

### Compatibility And Proof Logic Are Mixed Into Production Flow

- The repo intentionally keeps legacy composition paths while adding kernel/session composition and machine-checkable proofs.
- `src/AsterGraph.Demo` (sample host) and `tools/AsterGraph.PackageSmoke`/`tools/AsterGraph.ScaleSmoke` (proof tools) still validate overlapping behavior.
- Risk: maintenance overhead persists while two composition models remain behaviorally aligned.

## Quality And Tooling Concerns

### Quality Gates Are Improving, But Not Fully Mature

- `.editorconfig` is tracked, central package management exists (`Directory.Packages.props`), and checked-in CI exists (`.github/workflows/ci.yml`) invoking `eng/ci.ps1`.
- Release-grade quality remains incomplete: no coverage thresholds or additional policy checks are enforced from a single release gate.
- Risk: style and behavior drift can still appear in host-specific and adoption-path workflows that are not fully codified.

### Mixed Target Matrix Requires Ongoing Attention

- Publishable packages target `net8.0` and `net9.0`.
- `tests/AsterGraph.Serialization.Tests` and tools are on `net8.0`; `tests/AsterGraph.Editor.Tests` and `tests/AsterGraph.Demo.Tests` are on `net9.0`.
- Risk: target-specific behavior can still regress if lane coverage is not maintained across all relevant combinations.

## Product And Roadmap Concerns

### Plugin Loading And Automation Adoption

- Plugin loading and richer automation shipped in v1.4, so it is no longer an open-scope gap.
- Current risk is adoption-path clarity: host-facing sequencing between runtime-only, full-shell, and compatibility facade lanes.
- Risk: external consumers can still misapply capabilities if migration boundaries are not read as intended.

### Documentation Drift Is the Highest Operational Risk

- The repo moves quickly through release cleanup phases (`Phase 26` runtime-boundary cleanup, `Phase 28` proof-surface alignment, `Phase 29` next).
- `.planning/codebase` and related host/docs require periodic refresh to stay evidence-aligned.
- Risk: stale guidance can mis-route integration planning and review priorities.

## Suggested Next Watchpoints

- Track when `GraphEditorViewModel` stops implementing `IGraphEditorSessionHost`.
- Track when compatibility query and menu APIs stop relying on MVVM-oriented model shapes.
- Keep package smoke, scale smoke, and migration suites synchronized with host/session contract changes.

---

*Concerns analysis refreshed: 2026-04-14*
