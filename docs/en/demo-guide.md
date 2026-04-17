# Demo Guide

`src/AsterGraph.Demo` is the graph-first showcase host for the SDK. It is not part of the supported package boundary.

## What the Demo Proves

- the main demo route now uses `AsterGraphEditorFactory.Create(...)`
- Avalonia surfaces are composed through the view factories
- plugin trust/discovery/load state is visible in the UI
- automation request/progress/result is visible in the UI
- standalone surfaces and presenter replacement are rendered as real controls
- the host shell can switch between Chinese and English without rebuilding the editor session

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

## How to Read It

- `Extensions` proves candidate discovery, trust decisions, and load snapshots.
- `Automation` proves typed execution and result projection.
- `Integration` points to `HostSample`, renders standalone surfaces, and shows presenter-replacement previews.
- `Proof` keeps host-owned shell state next to shared runtime evidence.

## Demo vs Other Entry Samples

- `HelloWorld` = smallest runtime-only first-run sample
- `HelloWorld.Avalonia` = smallest hosted-UI first-run sample
- `HostSample` = narrow proof harness for the canonical consumer routes
- `Demo` = full showcase host for product surface and host boundary

Use the demo when you need to inspect behavior visually. Use `HelloWorld` or `HelloWorld.Avalonia` for the quickest first run on a single route and `HostSample` for proof-oriented route validation.
