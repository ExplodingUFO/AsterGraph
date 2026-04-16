# AsterGraph Alpha Status

## Current Alpha Target

- package baseline: `0.2.0-alpha.1`
- repo status: public alpha readiness
- public entry path: `README.md` plus the guides under `docs/en` and `docs/zh-CN`

`.planning` remains maintainer context, not the primary consumer guide.

## Included in the Alpha Surface

- the four publishable packages
- canonical factory/session composition
- default Avalonia hosted UI and standalone surfaces
- plugin discovery, trust policy, load inspection, and runtime loading
- automation execution through `IGraphEditorSession.Automation`
- official proof lanes and smoke tools

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

- public prerelease publication is still tightening around the tag-driven release flow
- CI still keeps the deepest package-validation lane on Windows
- retained compatibility APIs are still present during the migration window

## Recommended Entry Points

- [Quick Start](./quick-start.md)
- [Host Integration](./host-integration.md)
- [State Contracts](./state-contracts.md)
- [Extension Contracts](./extension-contracts.md)
- [Demo Guide](./demo-guide.md)
