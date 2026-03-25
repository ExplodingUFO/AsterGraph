# External Integrations

**Analysis Date:** 2026-03-25

## APIs & External Services

**Package Distribution:**
- NuGet.org - External package source referenced by the sample restore configuration in `NuGet.config.sample`.
  - SDK/Client: NuGet restore through `dotnet`/MSBuild project restore from the `*.csproj` files.
  - Auth: Not configured in the repository. `NuGet.config.sample` contains only public feed definitions.
- Local package feed (`artifacts/packages`) - Local restore target used for package smoke testing and local package validation in `NuGet.config.sample`, `README.md`, and `tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj`.
  - SDK/Client: `dotnet pack` output plus NuGet restore sources.
  - Auth: None.

**Desktop Platform Services:**
- Avalonia system clipboard - Clipboard interop for selection copy/paste is implemented in `src/AsterGraph.Avalonia/Services/AvaloniaTextClipboardBridge.cs` and wired from `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`.
  - SDK/Client: `Avalonia.Input.Platform.IClipboard`.
  - Auth: None.

**Host Application Extension Surface:**
- Host-owned localization, presentation, and context-menu augmentation are injected through `src/AsterGraph.Editor/Localization/IGraphLocalizationProvider.cs`, `src/AsterGraph.Editor/Presentation/INodePresentationProvider.cs`, `src/AsterGraph.Editor/Menus/IGraphContextMenuAugmentor.cs`, and are demonstrated in `tools/AsterGraph.HostSample/Program.cs`.
  - SDK/Client: Public AsterGraph editor abstractions consumed by host applications.
  - Auth: Host-defined; no built-in authentication layer exists.
- Host service-provider propagation is exposed through `src/AsterGraph.Editor/Hosting/IGraphHostContext.cs` and adapted for Avalonia in `src/AsterGraph.Avalonia/Hosting/AvaloniaGraphHostContext.cs`.
  - SDK/Client: `IServiceProvider`.
  - Auth: Host-defined; none in repo code.

## Data Storage

**Databases:**
- None detected.
  - Connection: Not applicable.
  - Client: Not applicable.

**File Storage:**
- Local filesystem only.
- Graph document persistence uses JSON files through `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`.
- Default graph workspace path resolves under `%LocalAppData%\\AsterGraphDemo\\demo-graph.json` in `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`.
- Default fragment workspace path resolves under `%LocalAppData%\\AsterGraphDemo\\selection-fragment.json` in `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs`.
- Fragment template library files are stored under `%LocalAppData%\\AsterGraphDemo\\fragments` in `src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs`.
- The repository-local package output directory is `artifacts/packages`, referenced by `README.md`, `NuGet.config.sample`, and `tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj`.

**Caching:**
- None detected. The codebase uses direct in-memory state and file reads/writes instead of a cache service.

## Authentication & Identity

**Auth Provider:**
- None.
  - Implementation: The repository contains no login flow, token handling, OAuth client, API key config, or identity provider integration.

## Monitoring & Observability

**Error Tracking:**
- None detected. No Sentry, App Insights, Seq, or similar SDK references are present in `src/`, `tests/`, or `tools/`.

**Logs:**
- Basic console output only in sample and smoke tools, via `Console.WriteLine` in `tools/AsterGraph.HostSample/Program.cs` and `tools/AsterGraph.PackageSmoke/Program.cs`.
- No structured logging framework is configured in project files.

## CI/CD & Deployment

**Hosting:**
- Desktop application hosting via Avalonia in `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- Library distribution via NuGet packages from `src/AsterGraph.Abstractions`, `src/AsterGraph.Core`, `src/AsterGraph.Editor`, and `src/AsterGraph.Avalonia`.

**CI Pipeline:**
- None detected. No `.github/workflows/`, Azure Pipelines, GitLab CI, or Docker build files are present.
- `Directory.Build.props` contains an optional `ContinuousIntegrationBuild` toggle keyed off `CI=true`, but no pipeline definition is tracked in the repository.

## Environment Configuration

**Required env vars:**
- None required for local build or runtime based on tracked files.
- Optional: `CI=true` enables `ContinuousIntegrationBuild` in `Directory.Build.props`.

**Secrets location:**
- Not applicable. No tracked secret store, `.env` file, credential file, or key file is detected in the repository root.

## Webhooks & Callbacks

**Incoming:**
- None. The codebase defines no HTTP endpoints, webhook receivers, or background listeners.

**Outgoing:**
- None in application code.
- Package restore may contact `https://api.nuget.org/v3/index.json` when using the sample NuGet configuration in `NuGet.config.sample`.

---

*Integration audit: 2026-03-25*
