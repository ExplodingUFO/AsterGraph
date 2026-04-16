---
phase: 42
title: Clean Runner CI Parity
status: planned
last_updated: 2026-04-16
---

# Phase 42 Validation

## Required Checks

1. Focused test coverage for the new test-plugin artifact resolver:
   - `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~TestPluginArtifactPath"`
2. Focused plugin regression coverage in Release mode:
   - `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~GraphEditorPlugin"`
3. Repo-local proof gate:
   - `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release`
4. Net9 validation lane parity:
   - `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework net9.0 -Configuration Release`

## Acceptance

- No plugin test reaches into `tests/AsterGraph.TestPlugins/bin/Debug/net9.0`.
- Release-mode local validation uses the same test-plugin path assumptions as clean hosted runners.
- The existing `contract` and `all` gates stay on the same command path after the fix.
