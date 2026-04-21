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
dotnet restore .\AsterGraph.sln
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
- [`README.zh-CN.md`](./README.zh-CN.md)
- [`docs/en/quick-start.md`](./docs/en/quick-start.md)
- [`docs/en/consumer-sample.md`](./docs/en/consumer-sample.md)
- [`docs/en/host-integration.md`](./docs/en/host-integration.md)
- [`docs/en/state-contracts.md`](./docs/en/state-contracts.md)
- [`docs/en/extension-contracts.md`](./docs/en/extension-contracts.md)
- [`docs/en/alpha-status.md`](./docs/en/alpha-status.md)
- [`docs/en/versioning.md`](./docs/en/versioning.md)
- [`docs/en/project-status.md`](./docs/en/project-status.md)
- [`docs/en/public-launch-checklist.md`](./docs/en/public-launch-checklist.md)
- [`docs/en/demo-guide.md`](./docs/en/demo-guide.md)
- [`docs/en/adoption-feedback.md`](./docs/en/adoption-feedback.md)
- [`docs/zh-CN/quick-start.md`](./docs/zh-CN/quick-start.md)
- [`docs/zh-CN/consumer-sample.md`](./docs/zh-CN/consumer-sample.md)
- [`docs/zh-CN/host-integration.md`](./docs/zh-CN/host-integration.md)
- [`docs/zh-CN/state-contracts.md`](./docs/zh-CN/state-contracts.md)
- [`docs/zh-CN/extension-contracts.md`](./docs/zh-CN/extension-contracts.md)
- [`docs/zh-CN/alpha-status.md`](./docs/zh-CN/alpha-status.md)
- [`docs/zh-CN/versioning.md`](./docs/zh-CN/versioning.md)
- [`docs/zh-CN/project-status.md`](./docs/zh-CN/project-status.md)
- [`docs/zh-CN/public-launch-checklist.md`](./docs/zh-CN/public-launch-checklist.md)
- [`docs/zh-CN/demo-guide.md`](./docs/zh-CN/demo-guide.md)
- [`docs/zh-CN/adoption-feedback.md`](./docs/zh-CN/adoption-feedback.md)

Keep public onboarding understandable from `README.md`, `README.zh-CN.md`, and the canonical guides under `docs/en` and `docs/zh-CN`. Do not introduce public dependencies on local-only planning files or other ignored maintainer context.
