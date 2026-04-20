# Demo Guide

`src/AsterGraph.Demo` is the graph-first showcase host for the SDK. It is not part of the supported package boundary.

## What the Demo Proves

- the main demo route now uses `AsterGraphEditorFactory.Create(...)`
- Avalonia surfaces are composed through the view factories
- definition-driven inspector groups, shipped editors, and validation cues are visible on real demo node definitions
- plugin trust/discovery/load state is visible in the UI, including version, target framework, fingerprint, reason strings, and allowlist import or export
- automation request/progress/result is visible in the UI
- standalone surfaces and presenter replacement are rendered as real controls
- the host shell can switch between Chinese and English without rebuilding the editor session
- recent files, autosave recovery, dirty-exit protection, drag-and-drop open, and persisted layout are exposed as host-owned shell workflows

## Host Menu Groups

When the demo runs in English:

- `Showcase`
- `View`
- `Behavior`
- `Runtime`
- `Extensions`
- `Automation`
- `Integration`
- `Proof`

When the demo runs in Chinese:

- `展示`
- `视图`
- `行为`
- `运行时`
- `扩展`
- `自动化`
- `集成`
- `证明`

The top menu also exposes the visible language switch.

## Proof Mode

```powershell
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -- --proof
```

Expected proof markers:

- `DEMO_TRUST_OK:True`
- `DEMO_SHELL_OK:True`
- `COMMAND_SURFACE_OK:True`
- `TIERED_NODE_SURFACE_OK:True`
- `FIXED_GROUP_FRAME_OK:True`
- `NON_OBSCURING_EDITING_OK:True`
- `VISUAL_SEMANTICS_OK:True`
- `COMPOSITE_SCOPE_OK:True`
- `EDGE_NOTE_OK:True`
- `DISCONNECT_FLOW_OK:True`
- `HOST_NATIVE_METRIC:startup_ms=...`
- `HOST_NATIVE_METRIC:inspector_projection_ms=...`
- `HOST_NATIVE_METRIC:plugin_scan_ms=...`
- `HOST_NATIVE_METRIC:command_latency_ms=...`
- `DEMO_OK:True`

## How to Read It

- `Extensions` proves candidate discovery, trust decisions, load snapshots, and persisted allowlist decisions.
- `Automation` proves typed execution and result projection.
- `Integration` points to `HostSample`, renders standalone surfaces, and shows presenter-replacement previews.
- `Runtime` and `Proof` keep host-owned shell state, recent workspaces, autosave cues, threshold-driven side rails, and shared runtime evidence in one place.
- `Proof` also confirms non-obscuring parameter editing (`NON_OBSCURING_EDITING_OK`) and the cleaned graph-surface visual semantics (`VISUAL_SEMANTICS_OK`).
- `View and Proof` can verify composite scope navigation (`COMPOSITE_SCOPE_OK`), editable edge notes (`EDGE_NOTE_OK`), and disconnect recovery (`DISCONNECT_FLOW_OK`).

## Demo vs Other Entry Samples

- `HelloWorld` = smallest runtime-only first-run sample
- `HelloWorld.Avalonia` = smallest hosted-UI first-run sample
- `ConsumerSample.Avalonia` = realistic hosted-UI consumer sample with one host action rail, parameter editing, and one trusted plugin
- `HostSample` = narrow proof harness for the canonical consumer routes
- `Demo` = full showcase host for product surface and host boundary

Use the demo when you need to inspect behavior visually. Use `HelloWorld` or `HelloWorld.Avalonia` for the quickest first run on a single route, `ConsumerSample.Avalonia` for one realistic hosted integration, and `HostSample` for proof-oriented route validation.
