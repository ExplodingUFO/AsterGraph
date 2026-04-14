# Integrations

**Analysis Date:** 2026-04-14

## External Services

- No HTTP API client, database client, auth provider SDK, webhook receiver, or message broker integration is present in tracked source under `src/`, `tests/`, or `tools/`.
- Package restore uses NuGet sources from `NuGet.config` (`nuget.org`) with local package-source guidance in `NuGet.config.sample`.
- The product is currently a desktop/library SDK, not a networked service.

## Host Composition Seams

- `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs` is the main host integration contract.
- Hosts can inject `INodeCatalog`, `IPortCompatibilityService`, `IGraphWorkspaceService`, `IGraphFragmentWorkspaceService`, `IGraphFragmentLibraryService`, `IGraphClipboardPayloadSerializer`, `IGraphContextMenuAugmentor`, `INodePresentationProvider`, `IGraphLocalizationProvider`, `IGraphEditorDiagnosticsSink`, and `GraphEditorInstrumentationOptions`.
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` exposes both `CreateSession(...)` for runtime-first composition and `Create(...)` for the retained `GraphEditorViewModel` path.
- `src/AsterGraph.Demo` is the runnable visual integration sample for these seams.

## Filesystem And Workspace Integration

- Whole-document persistence is abstracted by `src/AsterGraph.Editor/Services/IGraphWorkspaceService.cs` and implemented by `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`.
- Fragment persistence is abstracted by `src/AsterGraph.Editor/Services/IGraphFragmentWorkspaceService.cs` and implemented by `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs`.
- Fragment template/library storage is abstracted by `src/AsterGraph.Editor/Services/IGraphFragmentLibraryService.cs` and implemented by `src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs`.
- Default storage-root logic lives in `src/AsterGraph.Editor/Services/GraphEditorStorageDefaults.cs`.
- Serialization for these flows is centered on `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs` and `src/AsterGraph.Editor/Services/GraphClipboardPayloadSerializer.cs`.

## Clipboard And Desktop Integration

- The editor-side clipboard seam is `src/AsterGraph.Editor/Services/IGraphTextClipboardBridge.cs`.
- Avalonia adapts the platform clipboard through `src/AsterGraph.Avalonia/Services/AvaloniaTextClipboardBridge.cs`.
- Selection-copy behavior is coordinated by `src/AsterGraph.Editor/Services/GraphSelectionClipboard.cs`.
- The shipped UI controls route clipboard and host-context concerns through `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` and `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`.
- Host object propagation is normalized through `src/AsterGraph.Editor/Hosting/IGraphHostContext.cs`, `src/AsterGraph.Editor/Hosting/GraphHostContextExtensions.cs`, and `src/AsterGraph.Avalonia/Hosting/AvaloniaGraphHostContext.cs`.

## Diagnostics And Instrumentation

- Runtime diagnostics are exposed through `src/AsterGraph.Editor/Diagnostics/IGraphEditorDiagnostics.cs`, `src/AsterGraph.Editor/Diagnostics/IGraphEditorDiagnosticsSink.cs`, and `src/AsterGraph.Editor/Diagnostics/GraphEditorInspectionSnapshot.cs`.
- Optional host logging and tracing are wired through `src/AsterGraph.Editor/Diagnostics/GraphEditorInstrumentationOptions.cs`, which expects host-supplied `ILoggerFactory` and `ActivitySource`.
- `src/AsterGraph.Demo` exercises host object propagation and UI composition paths in the runnable shell.
- `tools/AsterGraph.PackageSmoke/Program.cs` and `tools/AsterGraph.ScaleSmoke/Program.cs` exercise machine-checkable diagnostics and inspection proof behaviors.
- The diagnostics surface is intentionally editor-owned rather than Avalonia-owned.

## Avalonia Surface Integration

- Full-shell composition is exposed through `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs`.
- Standalone surface composition is exposed through `src/AsterGraph.Avalonia/Hosting/AsterGraphCanvasViewFactory.cs`, `src/AsterGraph.Avalonia/Hosting/AsterGraphInspectorViewFactory.cs`, and `src/AsterGraph.Avalonia/Hosting/AsterGraphMiniMapViewFactory.cs`.
- Presentation replacement seams exist for nodes, menus, inspector, and minimap under `src/AsterGraph.Avalonia/Presentation/`.
- These adapters keep Avalonia types out of `src/AsterGraph.Abstractions` and `src/AsterGraph.Core`.

## Packaging And Release Integration

- Shared package metadata and repository URLs are defined in `Directory.Build.props`.
- `README.md` documents `dotnet pack` output to `artifacts/packages`.
- `tools/AsterGraph.PackageSmoke/Program.cs` is the main integration proof for packed-package consumption.
- `tools/AsterGraph.ScaleSmoke/Program.cs` is a runtime proof tool for selection, compatibility, history, viewport, diagnostics, and workspace continuity under larger graph sizes.

## Environment, Secrets, And Identity

- No required environment variables are defined in tracked files.
- The only optional build flag observed is `CI=true` in `Directory.Build.props`.
- No OAuth config, API key handling, secret store, or local `.env` strategy is present in the repo.
- No identity or permission system exists beyond host-provided command-permission options such as `src/AsterGraph.Editor/Configuration/GraphEditorCommandPermissions.cs`.

## Notable Non-Integrations

- No web server or HTTP endpoint exists.
- No database schema or migration tool exists.
- No queue, cron, or background worker exists.
- No cloud SDKs, containers, or deployment manifests are tracked.

---

*Integration analysis refreshed: 2026-04-14*
