## Summary

Plan 28-02 split demo/sample regressions out of the core SDK test lane. `AsterGraph.Editor.Tests` no longer depends on `AsterGraph.Demo`, demo-coupled suites now live in a dedicated `AsterGraph.Demo.Tests` project, and the `net9.0` automation lane runs both lanes explicitly.

## Changes

- added [AsterGraph.Demo.Tests.csproj](/F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj)
- moved the demo-focused suites into [tests/AsterGraph.Demo.Tests](/F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Demo.Tests)
- extracted demo-specific localization assertions into [GraphEditorLocalizationDemoTests.cs](/F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Demo.Tests/GraphEditorLocalizationDemoTests.cs)
- removed the Demo project reference from [AsterGraph.Editor.Tests.csproj](/F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj)
- updated [GraphEditorLocalizationTests.cs](/F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs) so it stays on core/editor localization coverage only
- added `AsterGraph.Demo.Tests` to [avalonia-node-map.sln](/F:/CodeProjects/DotnetCore/avalonia-node-map/avalonia-node-map.sln)
- updated [eng/ci.ps1](/F:/CodeProjects/DotnetCore/avalonia-node-map/eng/ci.ps1) so the `net9.0` lane runs both `AsterGraph.Editor.Tests` and `AsterGraph.Demo.Tests`

## Verification

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework net9.0 -Configuration Release`
