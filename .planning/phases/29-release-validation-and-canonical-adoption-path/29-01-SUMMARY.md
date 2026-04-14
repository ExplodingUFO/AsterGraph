## Summary

Plan 29-01 turned `eng/ci.ps1` into a real release-validation lane. The repo now packs the four publishable packages, runs `PackageSmoke` against packed packages, runs `ScaleSmoke`, collects checked-in coverage, and enforces SDK-integrated package validation from the same repo-local command path that CI uses.

## Changes

- extended [eng/ci.ps1](/F:/CodeProjects/DotnetCore/avalonia-node-map/eng/ci.ps1) with a `release` lane that restores project-by-project, packs publishable packages, runs `PackageSmoke`, runs `ScaleSmoke`, and collects coverage artifacts
- added [eng/coverage-report.ps1](/F:/CodeProjects/DotnetCore/avalonia-node-map/eng/coverage-report.ps1) to aggregate Cobertura outputs and fail if the publishable package surface is missing from the report
- enabled SDK package validation for packable packages in [Directory.Build.props](/F:/CodeProjects/DotnetCore/avalonia-node-map/Directory.Build.props)
- added checked-in coverage collector/config through [Directory.Packages.props](/F:/CodeProjects/DotnetCore/avalonia-node-map/Directory.Packages.props), [tests/coverage.runsettings](/F:/CodeProjects/DotnetCore/avalonia-node-map/tests/coverage.runsettings), [tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj](/F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj), [tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj](/F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj), and [tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj](/F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj)
- wired GitHub Actions to reuse the new release lane in [.github/workflows/ci.yml](/F:/CodeProjects/DotnetCore/avalonia-node-map/.github/workflows/ci.yml)

## Verification

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release`
- coverage summary written to `artifacts/coverage/release-summary.json` with publishable-package coverage present for `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Avalonia`
