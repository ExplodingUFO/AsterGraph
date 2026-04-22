# AsterGraph Alpha Status

## Current Alpha Target

- package baseline: `0.2.0-alpha.3`
- repo status: public alpha
- public versioning guidance: [Versioning](./versioning.md)
- public entry path: `README.md` plus the guides under `docs/en` and `docs/zh-CN`
- public status page: [Project Status](./project-status.md)
- maintainer launch checklist: [Public Launch Checklist](./public-launch-checklist.md)

## Included in the Alpha Surface

- the four publishable packages
- canonical factory/session composition
- default Avalonia hosted UI and standalone surfaces
- runtime inspection surface for trusted, loaded, and blocked outcomes
- command/trust timeline and perf overlay in the showcase host
- `tools/AsterGraph.Starter.Avalonia` as the shipped Avalonia starter scaffold
- plugin discovery, trust policy, load inspection, and runtime loading
- automation execution through `IGraphEditorSession.Automation`
- official proof lanes and smoke tools
- one medium hosted-UI consumer sample between HelloWorld and Demo
- packed `HostSample` compatibility proof under `.NET 10` inside the release lane
- shipped definition-driven inspector metadata plus stock text/number/boolean/enum/list editors
- tiered node surfaces, hierarchy snapshots, composite scope navigation, edge notes, and routed edge geometry tooling

## Explicitly Not Included

- plugin marketplace or remote install/update workflows
- plugin unload lifecycle
- sandboxing or untrusted-code isolation guarantees
- arbitrary host-agnostic property editor frameworks beyond the shipped definition-driven inspector
- richer automation authoring UI or embedded scripting IDE workflows
- abrupt removal of retained compatibility APIs

## Stability Guidance

- stable canonical runtime surfaces: `CreateSession(...)`, `IGraphEditorSession`, DTO/snapshot queries, runtime diagnostics/automation/plugin inspection
- supported hosted-UI composition helper: `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
- retained migration surfaces: `GraphEditorViewModel`, `GraphEditorView`
- compatibility-only shims: older MVVM-shaped helpers described in [Extension Contracts](./extension-contracts.md)

## Known Limitations

- the latest public prerelease tag is `v0.2.0-alpha.3`, while `v1.9` remains only as a historical pre-launch milestone marker
- public prerelease publishing and release artifacts still depend on the maintainer release flow
- the deepest package-validation lane and the `.NET 10` packed-consumer proof still run on the Windows release-validation path
- retained compatibility APIs are still present during the migration window, but they are only a migration bridge
- the public alpha still has only one official UI adapter today: Avalonia
- `v0.9.0-beta` locks `WPF` as adapter 2, but parity is not implied until the [Adapter Capability Matrix](./adapter-capability-matrix.md) is filled with `Supported`, `Partial`, and `Fallback`
- advanced editing is now published as host-facing capability modules, but broader ecosystem and adapter validation work remains ahead

## Recommended Entry Points

- [Versioning](./versioning.md)
- [Project Status](./project-status.md)
- [Quick Start](./quick-start.md)
- [ScaleSmoke Baseline](./scale-baseline.md)
- [Advanced Editing Guide](./advanced-editing.md)
- [Authoring Inspector Recipe](./authoring-inspector-recipe.md)
- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Retained-To-Session Migration Recipe](./retained-migration-recipe.md)
- [Public Launch Checklist](./public-launch-checklist.md)
- [Host Integration](./host-integration.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
- [State Contracts](./state-contracts.md)
- [Extension Contracts](./extension-contracts.md)
- [Demo Guide](./demo-guide.md)
