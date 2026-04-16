# AsterGraph Alpha Status

## Current Alpha Target

- package version baseline: `0.2.0-alpha.1`
- repo status: public alpha
- current source of truth for consumer onboarding: [`README.md`](../README.md) plus the docs in this folder
- public status page: [`en/project-status.md`](./en/project-status.md)
- launch checklist: [`en/public-launch-checklist.md`](./en/public-launch-checklist.md)

## Included In The Alpha Surface

The intended public alpha surface is:

- the four publishable packages:
  - `AsterGraph.Abstractions`
  - `AsterGraph.Core`
  - `AsterGraph.Editor`
  - `AsterGraph.Avalonia`
- canonical runtime/session composition:
  - `AsterGraphEditorFactory.CreateSession(...)`
  - `AsterGraphEditorFactory.Create(...)`
  - `AsterGraphAvaloniaViewFactory.Create(...)`
- plugin discovery, trust policy, inspection, and runtime loading
- automation execution through `IGraphEditorSession.Automation`
- official proof lanes and smoke tools

## Explicitly Not Included

The current alpha does not promise:

- plugin marketplace or remote install/update workflows
- plugin unload lifecycle
- sandboxing or untrusted-code isolation guarantees
- richer automation authoring UI or embedded scripting IDE workflows
- abrupt removal of retained compatibility APIs

## Stability Guidance

Treat these as the main stability tiers:

- stable canonical surfaces:
  - factory/session entry points
  - descriptor/snapshot queries
  - diagnostics, automation, and plugin inspection on the runtime boundary
- retained migration surfaces:
  - `GraphEditorViewModel`
  - `GraphEditorView`
- compatibility-only shims:
  - older MVVM-shaped query or target helpers documented in [`extension-contracts.md`](./extension-contracts.md)

See:

- [`state-contracts.md`](./state-contracts.md)
- [`extension-contracts.md`](./extension-contracts.md)

## Known Limitations

- public prerelease publishing and release artifacts still depend on the maintainer release flow
- CI coverage is still expanding beyond the current Windows-first release path

## Recommended Entry Points

- fastest package/route guide: [`quick-start.md`](./quick-start.md)
- current public posture: [`en/project-status.md`](./en/project-status.md)
- launch checklist: [`en/public-launch-checklist.md`](./en/public-launch-checklist.md)
- longer host walkthrough: [`host-integration.md`](./host-integration.md)
- state semantics: [`state-contracts.md`](./state-contracts.md)
- extension precedence and retirement: [`extension-contracts.md`](./extension-contracts.md)

## Feedback

- bugs and feature requests: repository issues
- security issues: follow [`SECURITY.md`](../SECURITY.md)
