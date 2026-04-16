---
phase: 40
title: Bilingual Docs And Localization Proof
status: in_progress
last_updated: 2026-04-16
---

# Phase 40 Validation

## Required Outcomes

1. Core public guides exist in paired English and `zh-CN` forms under a stable docs layout.
2. The demo can switch between Chinese and English without rebuilding the editor session.
3. `AsterGraph.Demo.Tests` prove the bilingual toggle and localization seam while preserving the canonical demo route.

## Checks

- `dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj -c Release --no-restore`

## Expected Signals

- demo tests pass after toggling between Chinese and English
- visible shell copy changes with the selected language
- runtime localization keys resolve through the host localization provider
- paired public docs exist under `docs/en` and `docs/zh-CN`
