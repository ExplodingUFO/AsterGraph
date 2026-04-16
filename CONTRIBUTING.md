# Contributing to AsterGraph

Thanks for contributing.

## Scope

AsterGraph is developed as a reusable SDK, not only as a demo application. Prefer changes that preserve:

- the four-package public boundary
- the canonical factory/session runtime path
- machine-checkable proof through the maintained CI lanes

## Prerequisites

- .NET SDK from [`global.json`](./global.json)
- PowerShell 7 for the maintained repo scripts

Bootstrap:

```powershell
dotnet restore .\avalonia-node-map.sln
```

## Branches and PRs

- Keep PRs focused. Split unrelated refactors from behavior changes.
- Open PRs against `master`.
- Do not mix milestone-planning churn with unrelated product changes.
- If a change affects public behavior, docs, or proof markers, update the relevant docs in the same PR.

Commit messages do not need a rigid format, but the repo convention is short, scoped messages such as:

- `feat(demo): ...`
- `docs(38): ...`
- `build(ci): ...`

## Before Opening a PR

Pick the narrowest lane that proves your change, then escalate when needed.

```powershell
# framework build/test matrix
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release

# focused consumer/state contract gate
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release

# hotspot refactor gate
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release

# full publish gate
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

Use these expectations:

- docs/config-only PRs: run the narrowest lane that covers the touched surface
- demo or consumer-surface PRs: at minimum run `contract`
- CI, packaging, or release-surface PRs: run `release`

If you add or change tests directly, you can also run the project-level suites:

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --nologo -v minimal
dotnet test tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj --nologo -v minimal
dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj --nologo -v minimal
```

## Coding Expectations

- Keep new host-facing behavior on canonical runtime/session contracts when possible.
- Treat `GraphEditorViewModel` and `GraphEditorView` as retained compatibility surfaces, not as the default place to add new product direction.
- Add focused regression coverage for behavior changes before or with the implementation.
- Preserve existing proof markers emitted by smoke tools unless the contract intentionally changes.

## Documentation Expectations

For externally visible changes, update the matching public docs:

- [`README.md`](./README.md)
- [`docs/quick-start.md`](./docs/quick-start.md)
- [`docs/host-integration.md`](./docs/host-integration.md)
- [`docs/state-contracts.md`](./docs/state-contracts.md)
- [`docs/extension-contracts.md`](./docs/extension-contracts.md)
- [`docs/alpha-status.md`](./docs/alpha-status.md)

Planning files under [`.planning`](./.planning/) are maintainer-facing context. Public onboarding should remain understandable from `README` plus `docs/`.
