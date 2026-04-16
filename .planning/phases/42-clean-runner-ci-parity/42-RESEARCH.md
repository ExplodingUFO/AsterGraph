---
phase: 42
title: Clean Runner CI Parity
status: planned
last_updated: 2026-04-16
---

# Phase 42 Research

## Goal

Remove hidden local-output assumptions from plugin proof tests and make the current `ci.yml` validation lanes representative of clean GitHub-hosted runners.

## Findings

1. The current GitHub Actions failures are reproducible from the hosted logs without changing product code. The failing suites are plugin discovery, loading, inspection, package staging, and proof-ring tests under the existing `contract` and `all` lanes.
2. Several editor test files derive the sample plugin path by walking from `AppContext.BaseDirectory` into `tests/AsterGraph.TestPlugins/bin/Debug/net9.0/AsterGraph.TestPlugins.dll`. That path exists on a warmed local workspace but not on a clean Release-only CI build.
3. `eng/ci.ps1` already builds `tests/AsterGraph.TestPlugins/AsterGraph.TestPlugins.csproj` in Release before the failing test runs. The hidden assumption is therefore in test path resolution, not in the lane order.
4. The safest fix is to centralize test-plugin artifact resolution behind one helper that derives the active configuration and framework from the current test output, then switch the plugin suites to that helper and lock it with focused tests.

## Execution Direction

- Add one focused test helper for resolving `AsterGraph.TestPlugins` artifacts from the active test run configuration.
- Add failing tests that prove the helper targets the current configuration/framework instead of the old `Debug/net9.0` convention.
- Refactor the existing plugin suites to use the helper and rerun the affected plugin-heavy proof lanes in Release mode.
