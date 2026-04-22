# Retained-To-Session Migration Recipe

Choose retained only when you are migrating an existing host in batches. This recipe is migration-only, not the preferred long-term extension path. Use it when an existing host still constructs `GraphEditorViewModel` directly but wants to move toward the canonical runtime/session seams without rewriting everything at once.

This is the single bounded retained recipe set. If an existing host still constructs `GraphEditorViewModel` or `GraphEditorView`, use this doc; otherwise start on `CreateSession(...)` or `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`.

## Target State

New code should land on:

- `AsterGraphEditorFactory.CreateSession(...)` for runtime-only or custom UI hosts
- `AsterGraphEditorFactory.Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)` for the shipped Avalonia UI path
- `IGraphEditorSession.Commands`, `Queries`, `Events`, and diagnostics snapshots

If you are starting new work, begin with [Quick Start](./quick-start.md) or [Host Integration](./host-integration.md) instead of the retained bridge.

## Stage 0: Keep the retained bridge

Keep temporarily:

- `GraphEditorViewModel`
- `GraphEditorView`
- host-owned `AsterGraphEditorOptions` composition

Replace with canonical seams:

- create runtime-only slices with `AsterGraphEditorFactory.CreateSession(...)`
- route new commands and queries through `IGraphEditorSession`

Ownership after this stage:

- the host still owns shell composition and options wiring
- the session owns commands, queries, events, diagnostics, and automation
- the retained bridge only adapts the UI surface

## Stage 1: Move command and query ownership into the session

Keep temporarily:

- the retained view-model bridge for untouched host slices
- the shipped Avalonia shell if the host still needs it

Replace with canonical seams:

- use `editor.Session.Commands` for new actions
- use DTO/snapshot queries instead of retained projections
- prefer `CreateSession(...)` for runtime-only callers

Compatibility bridges that can stay only during this stage:

- retained connection discovery: `GetCompatibleTargets(...)` and `CompatiblePortTarget`
- canonical replacement: `GetCompatiblePortTargets(...)` and `GraphEditorCompatiblePortTargetSnapshot`
- temporary node-group helpers: `TrySetNodeExpansionState(...)` and `TrySetNodeGroupExtraPadding(...)`
- canonical connection control: `connections.disconnect-*`, especially `connections.disconnect-all`

Ownership after this stage:

- commands and queries are session-owned
- host code owns orchestration and composition only
- compatibility shims are legacy-only and do not get new behavior

## Stage 2: Replace retained-only helpers and retire bridge-only behavior

Keep temporarily:

- `GraphEditorViewModel` and `GraphEditorView` only as migration scaffolding for untouched legacy code

Replace with canonical seams:

- `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` for shipped Avalonia UI
- `GetCompatiblePortTargets(...)` and snapshot APIs for compatibility projection
- the session/runtime route for new work

Ownership after this stage:

- UI composition is factory-owned
- commands and queries are session-owned
- retained helpers are legacy-only and stop being the extension path

## Proof and Evidence Handoff

For retained migration evidence, use the same defended hosted beta route as the rest of the public docs:

- run `AsterGraph.ConsumerSample.Avalonia -- --proof`
- if you need a local artifact, attach `artifacts/consumer-support-bundle.json` using [Beta Support Bundle](./support-bundle.md)
- review the same public proof markers: `CONSUMER_SAMPLE_OK:True`, `COMMAND_SURFACE_OK:True`, the `HOST_NATIVE_METRIC:*` lines, and the support-bundle markers `SUPPORT_BUNDLE_OK:True` and `SUPPORT_BUNDLE_PATH:...`

The retained bridge does not add a separate support boundary or a retained-only evidence lane, so maintainers can review evidence with the same public proof and bundle docs.

## Success Criteria

A host is effectively off the migration-critical path once:

- new commands and queries land on `IGraphEditorSession`
- UI composition uses the factory route instead of direct retained construction
- compatibility-only shims are only serving untouched legacy host code
- new features start from the canonical session route or the shipped Avalonia route, not from retained constructors

See also:

- [Quick Start](./quick-start.md)
- [Host Integration](./host-integration.md)
- [Extension Contracts](./extension-contracts.md)
