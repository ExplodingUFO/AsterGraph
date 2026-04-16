# AsterGraph Alpha Status

## Current Alpha Target

- package baseline: `0.2.0-alpha.1`
- repo status: public alpha ready, pending visibility and first public prerelease operations
- public entry path: `README.md` plus the guides under `docs/en` and `docs/zh-CN`
- maintainer launch checklist: [Public Launch Checklist](./public-launch-checklist.md)

`.planning` remains maintainer context, not the primary consumer guide.

## Included in the Alpha Surface

- the four publishable packages
- canonical factory/session composition
- default Avalonia hosted UI and standalone surfaces
- plugin discovery, trust policy, load inspection, and runtime loading
- automation execution through `IGraphEditorSession.Automation`
- official proof lanes and smoke tools
- packed `HostSample` compatibility proof under `.NET 10` inside the release lane

## Explicitly Not Included

- plugin marketplace or remote install/update workflows
- plugin unload lifecycle
- sandboxing or untrusted-code isolation guarantees
- richer automation authoring UI or embedded scripting IDE workflows
- abrupt removal of retained compatibility APIs

## Stability Guidance

- stable canonical surfaces: factories, `IGraphEditorSession`, DTO/snapshot queries, runtime diagnostics/automation/plugin inspection
- retained migration surfaces: `GraphEditorViewModel`, `GraphEditorView`
- compatibility-only shims: older MVVM-shaped helpers described in [Extension Contracts](./extension-contracts.md)

## Known Limitations

- repository visibility, branch protection, and the first public prerelease tag are still maintainer-run operational steps
- the deepest package-validation lane and the `.NET 10` packed-consumer proof still run on the Windows release-validation path
- retained compatibility APIs are still present during the migration window

## Recommended Entry Points

- [Quick Start](./quick-start.md)
- [Public Launch Checklist](./public-launch-checklist.md)
- [Host Integration](./host-integration.md)
- [State Contracts](./state-contracts.md)
- [Extension Contracts](./extension-contracts.md)
- [Demo Guide](./demo-guide.md)
