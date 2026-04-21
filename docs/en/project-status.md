# AsterGraph Project Status

## Current Status

- package baseline: `0.2.0-alpha.3`
- latest semver-aligned public prerelease tag: `v0.2.0-alpha.3`
- latest legacy repository milestone tag: `v1.9`
- repo posture: public alpha
- public versioning guidance: [Versioning](./versioning.md)
- supported published packages:
  - `AsterGraph.Abstractions`
  - `AsterGraph.Core`
  - `AsterGraph.Editor`
  - `AsterGraph.Avalonia`
- sample and proof tools:
  - `tools/AsterGraph.HelloWorld` for the quickest runtime-only first run
  - `tools/AsterGraph.HelloWorld.Avalonia` for the quickest hosted-UI first run
  - `tools/AsterGraph.ConsumerSample.Avalonia` for one realistic hosted integration before the full Demo shell
  - `tools/AsterGraph.HostSample` for the minimal consumer proof harness
  - `tools/AsterGraph.PackageSmoke` for packed-package proof
  - `tools/AsterGraph.ScaleSmoke` for the public scale baseline and state-continuity proof
- canonical adoption path:
  - runtime-only hosts use `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`
  - Avalonia UI hosts use `AsterGraphEditorFactory.Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)`

## What Is Already Stable Enough To Evaluate

- the four-package SDK boundary
- kernel/session-first runtime ownership
- default Avalonia shell plus standalone surfaces
- v1.23 graph-surface usability proof markers:
  - `COMMAND_SURFACE_OK:True`
  - `TIERED_NODE_SURFACE_OK:True`
  - `FIXED_GROUP_FRAME_OK:True`
  - `NON_OBSCURING_EDITING_OK:True`
  - `VISUAL_SEMANTICS_OK:True`
- v1.21 semantic graph composition proof markers:
  - `COMPOSITE_SCOPE_OK:True`
  - `EDGE_NOTE_OK:True`
  - `DISCONNECT_FLOW_OK:True`
- plugin discovery, trust policy, loading, and inspection
- automation execution through `IGraphEditorSession.Automation`
- contract, maintenance, and release proof lanes
- packed `HostSample` compatibility proof under `.NET 10` in the release lane

## Current Priorities

The current public-repo priority is not new runtime capability. It is keeping the repository surface clean and contributor-friendly:

- public docs stay under `README.md`, `README.zh-CN.md`, `docs/en`, and `docs/zh-CN`
- source, tests, samples, proof tools, workflows, and governance files remain visible
- internal workflow traces and local-only files do not remain part of the tracked public repo surface

## Near-Term Roadmap

- `v0.3.0-alpha Platform Skeleton Freeze` — freeze the canonical runtime surface and establish scene/input/platform seams that outlive Avalonia-owned implementation
- keep public alpha documentation and proof guidance easy to follow while that skeleton solidifies
- maintain hosted CI parity across the supported proof lanes
- continue the retained compatibility migration window without abrupt public breaks

## Public Entry Matrix

- `tools/AsterGraph.HelloWorld` = first-run runtime-only sample
- `tools/AsterGraph.HelloWorld.Avalonia` = first-run hosted-UI sample
- `tools/AsterGraph.ConsumerSample.Avalonia` = realistic hosted integration sample
- `tools/AsterGraph.HostSample` = minimal canonical adoption proof
- `tools/AsterGraph.PackageSmoke` = packed-package consumption proof
- `tools/AsterGraph.ScaleSmoke` = larger-graph baseline plus history/state-continuity proof
- `src/AsterGraph.Demo` = showcase host for visual/manual inspection

## Public Entry Points

- [Versioning](./versioning.md)
- [Quick Start](./quick-start.md)
- [Consumer Sample](./consumer-sample.md)
- [ScaleSmoke Baseline](./scale-baseline.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Retained-To-Session Migration Recipe](./retained-migration-recipe.md)
- [Alpha Status](./alpha-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
- [Host Integration](./host-integration.md)
- [Demo Guide](./demo-guide.md)
