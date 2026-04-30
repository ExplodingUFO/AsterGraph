# Phase 459 Handoff: Unified Command Registry And Keybinding Surface

## Status

Phase 459 is complete. Runtime command discovery now exposes registry entries with stable command descriptors and stock placement metadata, hosted shortcut conflicts are detectable, and stock Avalonia command surfaces consume registry-backed workbench placements.

## Beads

- `avalonia-node-map-48w.2.1` - Runtime command registry contracts: complete.
- `avalonia-node-map-48w.2.2` - Effective keybinding conflict detection: complete.
- `avalonia-node-map-48w.2.3` - Avalonia registry-backed command projection: complete.
- `avalonia-node-map-48w.2.4` - Command registry proof and closeout: complete.

## Changed Areas

- Runtime registry contracts:
  - `GraphEditorCommandSurfaceKind`
  - `GraphEditorCommandPlacementSnapshot`
  - `GraphEditorCommandRegistryEntrySnapshot`
  - `IGraphEditorQueries.GetCommandRegistry()`
  - `GraphEditorCommandRegistry`
- Hosted shortcut conflicts:
  - `AsterGraphHostedActionShortcutConflictDetector`
- Avalonia command projection:
  - `GraphEditorView` header, command palette, shortcut help, and composite workflow surfaces now read workbench placement metadata from command registry entries.

## Verification

- `dotnet test tests\AsterGraph.Editor.Tests\AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~CommandRegistry|FullyQualifiedName~GraphEditorKernelCommandRouter|FullyQualifiedName~GraphEditorSession" -m:1 --logger "console;verbosity=minimal"`: passed, 104 tests after one retry. The first attempt was blocked by a transient local antivirus file lock on an obj output.
- `dotnet test tests\AsterGraph.Editor.Tests\AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~Shortcut|FullyQualifiedName~Keybinding|FullyQualifiedName~HostedAction" -m:1 --logger "console;verbosity=minimal"`: passed, 19 tests.
- `dotnet test tests\AsterGraph.Editor.Tests\AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~CommandRegistry|FullyQualifiedName~HeaderCommandSurface|FullyQualifiedName~CommandPalette|FullyQualifiedName~ShortcutHelp|FullyQualifiedName~AuthoringToolsChrome" -m:1 --logger "console;verbosity=minimal"`: passed, 15 tests.
- `dotnet build src\AsterGraph.Avalonia\AsterGraph.Avalonia.csproj -m:1 --nologo`: passed for net8.0, net9.0, and net10.0.
- `.\eng\validate-public-api-surface.ps1 -Configuration Release -Framework net9.0`: passed with `PUBLIC_API_SURFACE_OK:3838:net9.0`, `PUBLIC_API_DIFF_GATE_OK:True`, and `PUBLIC_API_STABILITY_SCOPE_OK:True`.
- Prohibited external-name scan across `.planning`, `docs`, and `src`: no matches.
- `git diff --check`: clean except normal Windows line-ending warnings.

## Next Phase

Phase 459 unblocks Phases 460-463. Recommended next execution is Phase 460 first, then parallelize 461-463 where dependencies permit:

- Phase 460 should use the command registry as the supported command-discovery surface for semantic editing operations and clipboard payloads.
- Phase 461 should use registry placement metadata for template/palette actions instead of adding Demo-owned command lists.
- Phase 462 should keep pointer gestures in typed Avalonia coordinators, but expose semantic transform commands through registry descriptors.
- Phase 463 should expose navigation/search workflows as source-backed commands and query snapshots, not a background query service.

## Remaining Risks

- Shortcut conflict equivalence is text-based after trimming and case-insensitive grouping. It does not canonicalize reordered chord text.
- Registry workbench placements are static for stock surfaces. Dynamic leaf items such as node-definition menu entries are represented through stable parent command IDs.
- Demo/cookbook proof for the new command platform is deferred to Phase 464.
