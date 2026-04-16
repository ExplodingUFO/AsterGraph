---
phase: 39
title: Canonical Demo And Capability Showcase
status: completed
last_updated: 2026-04-16
---

# Phase 39 Validation

## Required Outcomes

1. The main demo host uses `Create(...)` plus the Avalonia view factories instead of direct `new GraphEditorViewModel(...)`.
2. Plugin trust/discovery/loading and automation execution are visible in the demo UI.
3. Standalone surfaces and presenter replacement are rendered as actual controls, and the consumer path points to `HostSample`.

## Checks

- `dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj -c Release --no-restore`

## Expected Signals

- demo tests pass
- new demo menu groups render
- main graph surface and preview hosts are created from the live editor session
