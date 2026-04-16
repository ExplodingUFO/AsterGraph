---
phase: 40
title: Bilingual Docs And Localization Proof
status: in_progress
last_updated: 2026-04-16
---

# Phase 40 Research

## Goal

Publish the public guides in English and `zh-CN`, then make the demo's localization seam visible through a real language toggle with proof coverage.

## Findings

1. The public guides still lived primarily as single-language root docs even after the public-alpha framing landed.
2. `MainWindowViewModel` still used localized host-group titles as internal state, which would make any language toggle brittle.
3. The demo already had a runtime localization seam through `GraphEditorViewModel.SetLocalizationProvider(...)`, so language switching did not require rebuilding the editor session.
4. `AsterGraph.Demo.Tests` already covered the shell, surfaces, plugin pane, and automation pane; the missing proof was language switching and bilingual host/editor copy.

## Execution Direction

- Replace localized host-group state with stable internal keys.
- Add a demo language toggle that updates both host-shell copy and runtime localization through `SetLocalizationProvider(...)`.
- Publish paired English and `zh-CN` guides under a stable docs structure, then point the root README surfaces at them.
