# AsterGraph Alpha Status

## Current Alpha Target

- package version baseline: `0.2.0-alpha.1`
- repo status: public alpha readiness in progress
- current source of truth for consumer onboarding: [`README.md`](../README.md) plus the docs in this folder

Use [`.planning`](../.planning/) as maintainer context, not as the first-stop consumer guide.

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

- the main demo is still being moved onto the canonical host path
- bilingual public docs are still being filled in
- public prerelease publishing and release artifacts are still being tightened around the release workflow
- CI coverage is still expanding beyond the current Windows-first release path

## Recommended Entry Points

- fastest package/route guide: [`quick-start.md`](./quick-start.md)
- longer host walkthrough: [`host-integration.md`](./host-integration.md)
- state semantics: [`state-contracts.md`](./state-contracts.md)
- extension precedence and retirement: [`extension-contracts.md`](./extension-contracts.md)

## Feedback

- bugs and feature requests: repository issues
- security issues: follow [`SECURITY.md`](../SECURITY.md)
