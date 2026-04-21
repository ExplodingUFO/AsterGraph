# Extension and Maintenance Contracts

This document publishes the contract around surface stability, compatibility retirement, extension precedence, and lane ownership.

## Stability Tiers

### Stable canonical surfaces

- `AsterGraphEditorFactory.CreateSession(...)`
- `IGraphEditorSession`
- DTO/snapshot queries such as `GetCompatiblePortTargets(...)`
- runtime-boundary diagnostics, automation, and plugin inspection

### Supported hosted-UI composition helper

- `AsterGraphEditorFactory.Create(...)`

### Retained migration surfaces

- `GraphEditorViewModel`
- `GraphEditorView`
- `GraphEditorViewModel.Session` as a bridge into the canonical runtime contract

### Compatibility-only shims

- `IGraphEditorQueries.GetCompatibleTargets(...)`
- `CompatiblePortTarget`
- older MVVM-shaped overloads where newer runtime-first alternatives already exist

## Extension Precedence

- plugin trust is host-owned and runs before activation
- plugin localization composes first; host localization runs last and wins final override
- plugin node presentation composes first; host presentation wins final override fields while merged adornments remain
- plugin commands register through the canonical session command descriptor pipeline; stock commands keep id authority when collisions exist
- runtime/session menus project stock descriptors today and will keep converging toward that shared command source
- retained `GraphEditorViewModel.BuildContextMenu(...)` remains the final host override point for compatibility hosts

## Lane Ownership

- `eng/ci.ps1 -Lane all` = framework-matrix build/test lane
- `eng/ci.ps1 -Lane contract` = focused consumer/state-contract gate
- `eng/ci.ps1 -Lane maintenance` = hotspot-refactor gate
- `eng/ci.ps1 -Lane release` = packed publish gate with smoke tools and coverage
- `tests/AsterGraph.Demo.Tests` = demo/sample-host lane

Classify failures by lane before changing code. That keeps refactor work, contract work, and release work from bleeding into each other without evidence.
