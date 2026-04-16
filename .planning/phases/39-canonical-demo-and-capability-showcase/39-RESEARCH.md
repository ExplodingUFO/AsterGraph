---
phase: 39
title: Canonical Demo And Capability Showcase
status: completed
last_updated: 2026-04-16
---

# Phase 39 Research

## Goal

Move the demo onto the canonical host path and expose the repo's current plugin, automation, consumer-path, and presenter-seam surface as visible UI.

## Findings

1. `MainWindowViewModel` still constructed `GraphEditorViewModel` directly, so the main demo was teaching the retained path.
2. The demo menu and drawer only exposed showcase/view/behavior/runtime/proof groups, leaving plugin trust and automation mostly in docs and tests.
3. Standalone-surface and presenter-seam capability existed, but the window only described them in text.
4. `AsterGraph.Demo.Tests` already covered the shell heavily, so the safest path was to extend those tests and then implement the new composition model.

## Execution Direction

- Build the retained facade through `AsterGraphEditorFactory.Create(...)`.
- Build the main `GraphEditorView` and preview surfaces through Avalonia factory helpers.
- Add plugin-candidate/load projections and automation projections to the drawer.
- Add real standalone-surface and presenter-replacement hosts into the demo shell.
