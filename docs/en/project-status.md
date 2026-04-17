# AsterGraph Project Status

## Current Status

- package baseline: `0.2.0-alpha.1`
- repo posture: public alpha
- public versioning guidance: [Versioning](./versioning.md)
- supported published packages:
  - `AsterGraph.Abstractions`
  - `AsterGraph.Core`
  - `AsterGraph.Editor`
  - `AsterGraph.Avalonia`
- sample and proof tools:
  - `tools/AsterGraph.HelloWorld` for the quickest runtime-only first run
  - `tools/AsterGraph.HostSample` for the minimal consumer path
  - `tools/AsterGraph.PackageSmoke` for packed-package proof
  - `tools/AsterGraph.ScaleSmoke` for scale and state-continuity proof
- canonical adoption path:
  - runtime-only hosts use `AsterGraphEditorFactory.CreateSession(...)`
  - Avalonia UI hosts use `AsterGraphEditorFactory.Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)`

## What Is Already Stable Enough To Evaluate

- the four-package SDK boundary
- kernel/session-first runtime ownership
- default Avalonia shell plus standalone surfaces
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

- keep public alpha documentation and proof guidance easy to follow
- maintain hosted CI parity across the supported proof lanes
- continue the retained compatibility migration window without abrupt public breaks
- use external alpha feedback to decide what should become the next product-facing milestone

## Public Entry Matrix

- `tools/AsterGraph.HelloWorld` = first-run runtime-only sample
- `tools/AsterGraph.HostSample` = minimal canonical adoption proof
- `tools/AsterGraph.PackageSmoke` = packed-package consumption proof
- `tools/AsterGraph.ScaleSmoke` = larger-graph, history, and state-continuity proof
- `src/AsterGraph.Demo` = showcase host for visual/manual inspection

## Public Entry Points

- [Versioning](./versioning.md)
- [Quick Start](./quick-start.md)
- [Alpha Status](./alpha-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
- [Host Integration](./host-integration.md)
- [Demo Guide](./demo-guide.md)
