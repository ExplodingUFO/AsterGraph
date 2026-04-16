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

- `Showcase`
- `View`
- `Behavior`
- `Runtime`
- `Extensions`
- `Automation`
- `Integration`
- `Proof`

The top menu also exposes the visible language switch.

## How to Read It

- `Extensions` proves candidate discovery, trust decisions, and load snapshots.
- `Automation` proves typed execution and result projection.
- `Integration` points to `HostSample`, renders standalone surfaces, and shows presenter-replacement previews.
- `Proof` keeps host-owned shell state next to shared runtime evidence.

## Demo vs HostSample

- `HostSample` = smallest canonical consumer path
- `Demo` = full showcase host for product surface and host boundary

Use the demo when you need to inspect behavior visually. Use `HostSample` when you need the shortest adoption proof.
