# AsterGraph Project Status

## Current Status

- package baseline: `0.11.0-beta`
- matching public prerelease tag for this package line: `v0.11.0-beta`
- historical legacy repository milestone tag series: `v1.x`-style pre-launch checkpoints
- repo posture: public beta
- public versioning guidance: [Versioning](./versioning.md)
- supported published packages:
  - `AsterGraph.Abstractions`
  - `AsterGraph.Core`
  - `AsterGraph.Editor`
  - `AsterGraph.Avalonia`
- frozen support boundary: [Stabilization Support Matrix](./stabilization-support-matrix.md)
- sample and proof tools:
  - `tools/AsterGraph.HelloWorld` for the quickest runtime-only first run
  - `tools/AsterGraph.Starter.Avalonia` for the shipped Avalonia starter scaffold
  - `tools/AsterGraph.HelloWorld.Avalonia` for the quickest hosted-UI first run after the starter scaffold
  - `tools/AsterGraph.ConsumerSample.Avalonia` for one realistic hosted integration before the full Demo shell
  - `tools/AsterGraph.HostSample` for the post-ladder proof harness on the canonical adoption route
  - `tools/AsterGraph.PackageSmoke` for packed-package proof
  - `tools/AsterGraph.ScaleSmoke` for the public scale baseline and state-continuity proof
- canonical adoption path:
  - runtime-only hosts use `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`
  - Avalonia UI hosts use `AsterGraphEditorFactory.Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)`
- active adapter validation target: `WPF` as adapter 2 under the [Adapter Capability Matrix](./adapter-capability-matrix.md)

## What Is Already Stable Enough To Evaluate

- the four-package SDK boundary
- kernel/session-first runtime ownership
- default Avalonia shell plus standalone surfaces
- runtime inspection surface for trusted, loaded, and blocked outcomes
- command/trust timeline and perf overlay in the showcase surface
- graph-surface usability proof markers:
  - `COMMAND_SURFACE_OK:True`
  - `TIERED_NODE_SURFACE_OK:True`
  - `FIXED_GROUP_FRAME_OK:True`
  - `NON_OBSCURING_EDITING_OK:True`
  - `VISUAL_SEMANTICS_OK:True`
- advanced-editing closure markers:
  - `HIERARCHY_SEMANTICS_OK:True`
  - `COMPOSITE_SCOPE_OK:True`
  - `EDGE_NOTE_OK:True`
  - `EDGE_GEOMETRY_OK:True`
  - `DISCONNECT_FLOW_OK:True`
- plugin discovery, trust policy, loading, and inspection
- automation execution through `IGraphEditorSession.Automation`
- contract, maintenance, and release proof lanes
- packed `HostSample` compatibility proof under `.NET 10` in the release lane

## Current Priorities

The current public-repo priority is turning the public beta into a coherent SDK surface rather than a pile of disconnected feature slices:

- public docs stay under `README.md`, `README.zh-CN.md`, `docs/en`, and `docs/zh-CN`
- advanced editing is described as canonical capability modules, not retained-only behavior
- source, tests, samples, proof tools, workflows, and governance files remain visible
- internal workflow traces and local-only files do not remain part of the tracked public repo surface

## Near-Term Roadmap

- keep the canonical runtime/session surface stable while broadening official capability modules and proof guidance
- keep public beta documentation and proof guidance easy to follow as advanced editing closes
- maintain hosted CI parity across the supported proof lanes
- continue the retained compatibility migration window without abrupt public breaks
- keep the shipped starter scaffold, runtime inspection surface, command/trust timeline, and perf overlay aligned with the canonical session-first route
- validate `WPF` as adapter 2 on the same canonical route and publish Avalonia/WPF status using `Supported`, `Partial`, and `Fallback`; do not read `Partial` or `Fallback` as parity

## External Capability Readiness Gate

Use this section as the single external capability readiness gate for release notes, maintainer replies, and beta intake. Every public claim below stays tied to route-level evidence instead of parity aspirations or internal confidence.

### Externally proven now

| Claim | Route-level evidence |
| --- | --- |
| Canonical runtime/session route and the maintained evaluator ladder are externally proven on the defended beta line. | `tools/AsterGraph.HelloWorld`, `tools/AsterGraph.Starter.Avalonia`, `tools/AsterGraph.HelloWorld.Avalonia`, `tools/AsterGraph.ConsumerSample.Avalonia`, `tools/AsterGraph.HostSample`, `HOST_SAMPLE_OK`, `CONSUMER_SAMPLE_OK` |
| The showcase authoring surface is externally proven as a bounded beta host experience. | `src/AsterGraph.Demo`, `DEMO_OK`, `COMMAND_SURFACE_OK`, `COMPOSITE_SCOPE_OK`, `EDGE_NOTE_OK`, `EDGE_GEOMETRY_OK`, `DISCONNECT_FLOW_OK` |
| Packaged consumer proof is externally proven without widening the SDK boundary. | `tools/AsterGraph.PackageSmoke`, `PACKAGE_SMOKE_OK`, `HOST_SAMPLE_NET10_OK` |
| Scale proof is externally proven only at the defended beta tiers. | `tools/AsterGraph.ScaleSmoke`, `SCALE_PERFORMANCE_BUDGET_OK:baseline:True`, `SCALE_PERFORMANCE_BUDGET_OK:large:True`, `SCALE_PERF_SUMMARY:stress` |

### Validation-only or bounded claims

| Claim | Current public stance | Route-level evidence |
| --- | --- | --- |
| `WPF` as adapter 2 | Validation-only and not Avalonia parity or public WPF support. | `HELLOWORLD_WPF_OK`, `ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`, `ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`, [Adapter Capability Matrix](./adapter-capability-matrix.md) |
| Retained route | Migration-only bridge, not a new primary host path. | [Retained-To-Session Migration Recipe](./retained-migration-recipe.md), [Stabilization Support Matrix](./stabilization-support-matrix.md) |
| Stress-scale telemetry | Informational only, not a defended budget claim. | `SCALE_PERF_SUMMARY:stress`, [ScaleSmoke Baseline](./scale-baseline.md) |

### Deferred until more adopter evidence

- larger defended performance commitments beyond the current `baseline` and `large` tiers
- new hosted adapters or widened adapter claims beyond Avalonia plus the current `WPF` validation lane
- marketplace, remote install/update, unload lifecycle, or sandboxed plugin stories
- stable / GA / `1.0` support guarantees
- the next 0.xx alpha/beta line stays on copyable host-owned parameter/metadata polish first; only widen toward defended large-tier performance or broader parameter/metadata editing when 3-5 real external entries cluster on that bounded risk
- if a new report does not fit one of the proven or bounded rows above, route it through the [Adoption Feedback Loop](./adoption-feedback.md) and the [Beta Support Bundle](./support-bundle.md) instead of widening the claim ad hoc

## Public Entry Matrix

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`; `HostSample` comes after that ladder as the proof harness.

- `tools/AsterGraph.HelloWorld` = first-run runtime-only sample
- `tools/AsterGraph.Starter.Avalonia` = shipped Avalonia starter scaffold
- `tools/AsterGraph.HelloWorld.Avalonia` = first-run hosted-UI sample after the starter scaffold
- `tools/AsterGraph.ConsumerSample.Avalonia` = realistic hosted integration sample
- `tools/AsterGraph.HostSample` = post-ladder canonical adoption proof
- `tools/AsterGraph.PackageSmoke` = packed-package consumption proof
- `tools/AsterGraph.ScaleSmoke` = larger-graph baseline plus history/state-continuity proof
- `src/AsterGraph.Demo` = showcase host for visual/manual inspection

## Public Entry Points

- [Versioning](./versioning.md)
- [Beta Evaluation Path](./evaluation-path.md) = single route ladder from first install to realistic hosted proof
- [Quick Start](./quick-start.md)
- [Consumer Sample](./consumer-sample.md)
- [ScaleSmoke Baseline](./scale-baseline.md)
- [Advanced Editing Guide](./advanced-editing.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Retained-To-Session Migration Recipe](./retained-migration-recipe.md)
- [Alpha Status](./alpha-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
- [Host Integration](./host-integration.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
- [Stabilization Support Matrix](./stabilization-support-matrix.md)
- [Demo Guide](./demo-guide.md)
