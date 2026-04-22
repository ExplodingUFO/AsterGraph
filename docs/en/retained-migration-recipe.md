# Retained-To-Session Migration Recipe

Choose retained only when you are migrating an existing host in batches. Use this recipe when an existing host still constructs `GraphEditorViewModel` directly but wants to move toward the canonical runtime/session path without rewriting everything at once. The retained route is a bridge, not the destination.

This is the single bounded retained recipe set. If an existing host still constructs `GraphEditorViewModel` or `GraphEditorView`, use this doc; otherwise start on `CreateSession(...)` or `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`.

## Target State

New code should land on:

- `AsterGraphEditorFactory.CreateSession(...)` for runtime-only or custom UI hosts
- `AsterGraphEditorFactory.Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)` for the shipped Avalonia UI path
- `IGraphEditorSession.Commands`, `Queries`, `Events`, and diagnostics snapshots

If you are starting new work, begin with [Quick Start](./quick-start.md) or [Host Integration](./host-integration.md) instead of the retained bridge.

## Step 1: Centralize Editor Options

Move document, catalog, compatibility, storage, plugin, localization, and diagnostics setup into one `AsterGraphEditorOptions` factory in the host.

## Step 2: Stop Routing New Logic Through ViewModel-Specific Helpers

When adding new host features:

- prefer `editor.Session.Commands` over retained helper methods
- prefer DTO/snapshot queries over retained view-model projections
- keep `GraphEditorViewModel` only as a UI bridge while the host is still migrating

### Step 2 Note: Compatibility Bridges

Canonical connection control should use runtime session commands (`connections.disconnect-*`, especially `connections.disconnect-all`) before adding retained-only variants. The existing retained compatibility shape for compatibility target discovery is still available while migrating:

- compatibility query: `GetCompatibleTargets(...)` and `CompatiblePortTarget`
- canonical replacement: `GetCompatiblePortTargets(...)` and `GraphEditorCompatiblePortTargetSnapshot`

For node-group UI parity during migration, retain compatibility helpers only when required:

- `TrySetNodeExpansionState(...)`
- `TrySetNodeGroupExtraPadding(...)`

## Step 3: Switch Runtime-Only Callers First

If part of the host does not need Avalonia controls, convert that slice to:

```csharp
var session = AsterGraphEditorFactory.CreateSession(options);
```

That usually gives the quickest reduction in retained-surface coupling.

## Step 4: Keep The Shipped UI On The Factory Route

If the host still wants the default Avalonia shell, move from:

```csharp
var editor = new GraphEditorViewModel(...);
var view = new GraphEditorView { Editor = editor };
```

to:

```csharp
var editor = AsterGraphEditorFactory.Create(options);
var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
});
```

This keeps the retained UI bridge, but the runtime ownership stays on the shared kernel/session path.

## Step 5: Retire Compatibility-Only Queries Last

Keep `GetCompatibleTargets(...)` and `CompatiblePortTarget` only while the host still needs MVVM-shaped results. New work should already be on DTO/snapshot APIs such as `GetCompatiblePortTargets(...)`.

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
