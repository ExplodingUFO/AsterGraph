# Extension And Maintenance Contracts

This document publishes the contract around surface stability, compatibility retirement, extension precedence, and verification-lane ownership.

For current alpha scope and known limitations, see [`alpha-status.md`](./alpha-status.md).

## Stability Tiers

### Stable canonical surfaces

Treat these as the long-lived host-facing runtime boundary:

- `AsterGraphEditorFactory.CreateSession(...)`
- `AsterGraphEditorFactory.Create(...)`
- `IGraphEditorSession`
- DTO/snapshot queries such as `GetCompatiblePortTargets(...)`
- descriptor-based diagnostics, feature discovery, automation, and plugin inspection

New host code should prefer these surfaces.

### Retained compatibility surfaces

These remain supported during migration, but they are not the canonical runtime owner:

- `GraphEditorViewModel`
- `GraphEditorView`
- `GraphEditorViewModel.Session` as the bridge into the canonical runtime contract

The retained facade stays supported so existing hosts can migrate in stages. It should not be treated as the source of future runtime-contract design.

### Compatibility-only shims

These are transitional bridges for older MVVM-rooted integrations:

- `IGraphEditorQueries.GetCompatibleTargets(...)`
- `CompatiblePortTarget`
- older MVVM-rooted extension overloads where newer context objects already exist

Retirement posture:

- current releases keep them available with migration guidance
- later minor releases may add stronger warnings
- a future major release may remove them

When a stable context or DTO/snapshot alternative exists, prefer it now instead of building new code on the shim.

## Extension Precedence

The precedence rules below describe the actual runtime composition behavior.

### Plugin trust is host-owned and runs before activation

- `AsterGraphEditorOptions.PluginTrustPolicy` is the host gate.
- A blocked plugin does not become an active contribution surface.

### Localization: plugins compose first, host wins final override

- plugin localization providers run in sequence first
- the host `IGraphLocalizationProvider` runs last
- the host can therefore override the final localized string

### Node presentation: plugin state merges, host wins final override fields

- plugin presentation providers contribute first
- the host `INodePresentationProvider` runs last
- host values can override subtitle, description, and status-bar output
- accumulated adornments such as badges continue to merge

### Context menus: runtime and retained paths have different final override points

- runtime/session path:
  - stock descriptors are built first
  - plugin context-menu augmentors then compose over those descriptors
- retained compatibility path:
  - the retained facade adapts the runtime/plugin-composed menu first
  - host `IGraphContextMenuAugmentor` then receives the final override point

For new runtime-first hosts, prefer descriptor-based menu composition over building new logic directly on the retained augmentor seam.

## Lane Ownership

- `eng/ci.ps1 -Lane all`
  - framework-matrix build/test lane
- `eng/ci.ps1 -Lane contract`
  - focused consumer/state-contract gate
  - runtime/session contracts
  - plugin trust/discovery/loading
  - automation contracts
  - hosted-surface proof
  - history/save/dirty contract
- `eng/ci.ps1 -Lane maintenance`
  - hotspot-refactor gate
  - compatibility facade drift
  - kernel/canvas hotspot regressions
  - scale/history readiness check
- `eng/ci.ps1 -Lane release`
  - publish gate
  - focused contract proof
  - package pack + package validation
  - packed `HostSample`
  - packed `PackageSmoke`
  - `ScaleSmoke`
  - coverage collection
- `tests/AsterGraph.Demo.Tests`
  - demo/sample-host lane

When a failure is under investigation, classify it by lane before changing code. That keeps hotspot work from spilling into the release or consumer-contract story without evidence.
