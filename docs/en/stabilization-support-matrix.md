# Stabilization Support Matrix

This page freezes the consumer-facing support boundary on the path toward `v1.0.0`. It does not widen the public surface; it restates the defended boundary that the entry docs should rely on.

## Frozen Boundary

| Area | Stabilized rule | Notes |
| --- | --- | --- |
| Published SDK packages | `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, `AsterGraph.Avalonia` | These are the only supported published SDK packages. Sample and proof tools stay out of the package boundary. |
| Target frameworks | `net8.0`, `net9.0`, and `net10.0` | These are the supported published targets for the SDK surface. |
| Canonical runtime route | `CreateSession(...)` + `IGraphEditorSession` | This remains the supported runtime/custom-UI route. |
| Hosted route | `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | This remains the supported hosted-Avalonia route. |
| Hosted adapter | Avalonia | This is the supported hosted adapter path today. |
| Secondary adapter | `WPF` | Validation-only and partial-fallback per [Adapter Capability Matrix](./adapter-capability-matrix.md); it does not define a second runtime model. |
| Retained route | Migration-only | The retained route stays available for legacy hosts moving in batches. |

## Upgrade Guidance Toward `v1.0.0`

- Stay on the canonical runtime/session route or the shipped Avalonia hosted route.
- Use only the four published SDK packages when planning new consumer integrations.
- Treat `WPF` as validation-only unless the adapter matrix explicitly marks a row as `Supported`; otherwise keep the host on the canonical route and use the documented partial/fallback shape.
- Keep retained MVVM entry points as migration bridges, not as new primary host surfaces.
- Read `v1.0.0` as promotion of this same defended boundary to stable, not as a widening of package, framework, or adapter support.

## Release Readiness Proof

Phase 382 keeps release readiness, support boundary, and beta claim wording tied to this frozen matrix with `RELEASE_READINESS_GATE_OK:True`, `SUPPORT_BOUNDARY_GATE_OK:True`, and `BETA_CLAIM_ALIGNMENT_OK:True`. These markers do not add packages, runtime routes, WPF parity, marketplace, sandbox, execution-engine, GA, or `1.0` support claims.

## Related Docs

- [Beta Evaluation Path](./evaluation-path.md)
- [Versioning](./versioning.md)
- [Quick Start](./quick-start.md)
- [Project Status](./project-status.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
- [Host Integration](./host-integration.md)
