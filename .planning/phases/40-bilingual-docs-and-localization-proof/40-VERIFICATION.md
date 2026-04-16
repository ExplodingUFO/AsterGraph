---
phase: 40
title: Bilingual Docs And Localization Proof
status: completed
last_updated: 2026-04-16
---

# Phase 40 Verification

## Commands

- `dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj -c Release --no-restore`
- directory check for `docs/en` and `docs/zh-CN`
- README link check for bilingual public entry paths

## Results

- demo tests passed: `35/35`
- bilingual guide pairs exist for quick start, host integration, state contracts, extension contracts, alpha status, and demo guide
- root README surfaces now link the English and `zh-CN` guide trees plus `README.zh-CN.md`
