# Adapter Capability Matrix

This page locks the adapter-capability contract for `v0.9.0-beta Second Adapter Validation`.

`WPF` is adapter 2. The milestone exists to validate one second official adapter on top of the existing canonical session/runtime route; it does not introduce adapter-specific runtime APIs or a second host-facing runtime model.

## Locked Target

- adapter 1 remains Avalonia
- adapter 2 is `WPF`
- the canonical host/runtime root remains `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession` from `AsterGraph.Editor`
- `Create(...)` remains a hosted-adapter composition helper layered on the same runtime owner

Avalonia is still the only shipped hosted adapter today. `WPF` is the locked validation target for the next milestone, not a parity promise before the matrix is filled in.

## Matrix Vocabulary

Use exactly these labels when the public docs describe Avalonia/WPF support:

| Label | Meaning |
| --- | --- |
| `Supported` | the documented stock adapter surface works on that adapter through the primary supported route |
| `Partial` | the capability stays on the same canonical route, but the adapter has an explicit scope limit or a missing stock projection |
| `Fallback` | the host stays on the same canonical session/runtime seam and uses the lower-level documented path, sample, or proof harness instead of a stock adapter surface |

Retained migration is not `Fallback`. It remains a compatibility bridge for legacy hosts.

## Matrix Categories

The public matrix should use these rows:

| Matrix row | What it covers |
| --- | --- |
| Canonical runtime/session route | `CreateSession(...)`, `IGraphEditorSession`, DTO/snapshot queries, diagnostics, automation, and plugin inspection |
| Hosted full editor shell | stock adapter-hosted editor composition layered on `Create(...)` |
| Standalone surfaces | stock canvas, inspector, and mini map surfaces published by an adapter package |
| Command and tool projection | menus, toolbars, shortcuts, palettes, and tools projected from shared descriptors |
| Authoring presentation | node, edge, group, inspector, parameter-editor, and geometry chrome supplied by the adapter |
| Platform integration | clipboard, focus, pointer, wheel, theme, and host-context glue that stay inside the adapter package |
| Proof and sample coverage | starter/sample/proof tools that demonstrate the adapter surface without changing the runtime route |

## Fallback Rule

A `Fallback` row does not mean "use retained MVVM," and it does not allow a `WPF`-specific runtime API. It means the host stays on the same canonical seam and uses the lower-level documented path already proven by samples or proof harnesses.

## Phase 157 Matrix

| Matrix row | Avalonia | WPF | Evidence anchors | WPF fallback rule |
| --- | --- | --- | --- | --- |
| Canonical runtime/session route | `Supported` | `Supported` | `CreateSession(...)`, `HOST_SAMPLE_OK`, `CONSUMER_SAMPLE_OK`, `DEMO_OK`, `HELLOWORLD_WPF_OK` | None. Canonical seam is unchanged; keep runtime/session contracts intact for custom shell work. |
| Hosted full editor shell | `Supported` | `Partial` | `tools/AsterGraph.Starter.Avalonia`, `tools/AsterGraph.HelloWorld.Avalonia`, `tools/AsterGraph.Starter.Wpf`, `tools/AsterGraph.HelloWorld.Wpf` | Compose the top-level shell in host UI and treat `AsterGraph.Wpf.Controls.GraphEditorView` (from `AsterGraphWpfViewFactory`) as embedded content when full stock shell parity is not yet present. |
| Standalone surfaces | `Supported` | `Partial` | Avalonia has dedicated canvas/inspector/minimap factories; WPF currently proves only `GraphEditorView` in `tools/AsterGraph.Starter.Wpf` and `tools/AsterGraph.HelloWorld.Wpf` | Build canvas/inspector/minimap-like surfaces via `IGraphEditorSession` snapshots and host-owned WPF rendering. |
| Command and tool projection | `Supported` | `Partial` | `COMMAND_SURFACE_OK`, `CONSUMER_SAMPLE_OK`, and `HELLOWORLD_WPF_OK` (WPF proof emits `COMMAND_SURFACE_OK`) | Use shared command descriptors through the canonical session route (`GetCommandDescriptors`) and project host actions from those descriptors. |
| Authoring presentation | `Supported` | `Partial` | `DEMO_OK` (full authoring proofs) and current WPF shell summary-only inspector output | Render node/edge/inspector/parameter chrome in host-owned WPF views backed by session/query snapshots until stock presenters are shipped. |
| Platform integration | `Supported` | `Partial` | `HOST_SAMPLE_OK` runtime-seam checks plus `AsterGraph.Wpf` platform services for clipboard/host-context binding | Keep focus/clipboard/pointer/wheel/theme glue in host layer and bind only through compatibility seams, no adapter-only runtime semantics. |
| Proof and sample coverage | `Supported` | `Supported` | `HOST_SAMPLE_OK`, `CONSUMER_SAMPLE_OK`, `DEMO_OK`, `COMMAND_SURFACE_OK`, `HELLOWORLD_WPF_OK` | None. |

Use [Host Integration](./host-integration.md) for the route and capability map, [Architecture](./architecture.md) for the adapter boundary, and [Quick Start](./quick-start.md) for the current Avalonia-first onboarding path.
