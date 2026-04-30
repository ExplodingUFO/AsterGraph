# Phase 463 Handoff: Viewport Navigation Search And Focus Workflows

## Status

Phase 463 is complete.

Closed beads:
- `avalonia-node-map-48w.6.1` — Navigation search and focus workflow audit
- `avalonia-node-map-48w.6.2` — Source-backed graph item search projection
- `avalonia-node-map-48w.6.3` — Bookmark breadcrumb and focus command workflows
- `avalonia-node-map-48w.6.4` — Phase 463 proof and closeout

## Delivered Contracts

- `IGraphEditorQueries.SearchGraphItems(...)` returns stable source-backed search results for scopes, nodes, groups, connections, and validation issues.
- `IGraphEditorQueries.GetViewportBookmarks()` returns current session viewport bookmarks with scope and viewport coordinates.
- `IGraphEditorCommands.TryFocusGraphNode(...)` navigates to a scope, selects a node, and centers the viewport.
- `IGraphEditorCommands.TryFocusGraphItemSearchResult(...)` focuses node, issue, connection, group, and scope search results through supported session commands.
- `IGraphEditorCommands.TryAddViewportBookmark(...)`, `TryRemoveViewportBookmark(...)`, and `TryActivateViewportBookmark(...)` expose bookmark workflows.
- Session command routes now include `viewport.focus-node`, `viewport.focus-issue`, `viewport.focus-search-result`, `viewport.bookmark.add`, `viewport.bookmark.remove`, and `viewport.bookmark.activate`.

## Verification

- `dotnet test tests\AsterGraph.Editor.Tests\AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~Navigation|FullyQualifiedName~Search|FullyQualifiedName~Viewport|FullyQualifiedName~Focus|FullyQualifiedName~Bookmark|FullyQualifiedName~Navigator" -m:1 --logger "console;verbosity=minimal"` — passed 56/56.
- `dotnet build src\AsterGraph.Editor\AsterGraph.Editor.csproj -m:1 --nologo` — passed net8.0, net9.0, net10.0.
- `dotnet build src\AsterGraph.Avalonia\AsterGraph.Avalonia.csproj -m:1 --nologo` — passed net8.0, net9.0, net10.0 after rerunning serially. The first parallel attempt hit a transient local file lock from Huorong on `AsterGraph.Core.dll`, not a code failure.
- `.\eng\validate-public-api-surface.ps1 -Configuration Release -Framework net9.0` — passed.
- `git diff --check` — passed.

## Next Phase

Phase 464 should consume these contracts in cookbook authoring flows. Keep the cookbook as code plus live proof, not a generated runnable workflow system. Reuse the session command/query contracts above instead of adding UI-local navigation state.
