# AsterGraph

## What This Is

AsterGraph is a modular node-graph editor toolkit for .NET with a reusable editor state layer and an Avalonia UI shell. This project is now focused on turning that foundation into a more decoupled, host-friendly component library that external consumers can embed, replace, extend, and debug without forking the default implementation.

## Core Value

Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Requirements

### Validated

- ✓ Host can consume the four publishable AsterGraph packages through a documented SDK boundary and supported `net8.0` / `net9.0` target story — Phase 1
- ✓ Host can initialize the editor runtime and default Avalonia view through public factory/options APIs while the constructor-based path remains supported — Phase 1
- ✓ Host can migrate through a staged compatibility path backed by parity tests and smoke coverage across legacy and factory entry routes — Phase 1
- ✓ Host can embed a working Avalonia graph editor with node rendering, connection editing, zoom/pan, and viewport navigation — existing
- ✓ Host can save and load graph documents plus clipboard and fragment payloads through the current editor services — existing
- ✓ Host can customize context menus, node presentation, localization, style tokens, and command permissions through current host-facing seams — existing
- ✓ Host can use compile-time node-definition providers and compatibility services to compose custom node catalogs — existing
- ✓ Host can run demo and host-sample applications that exercise the current package boundary and extension model — existing

### Active

- [ ] Split the monolithic Avalonia shell into smaller reusable controls so hosts can embed only canvas, mini map, inspector, context menu, or other subcomponents as needed
- [ ] Expose richer public APIs and interfaces for host-driven commands, state queries, events, service replacement, and secondary development scenarios
- [ ] Refactor responsibilities so core editing behavior is less entangled with Avalonia control code and easier to reuse, test, and evolve
- [ ] Allow hosts to replace default visual pieces such as node views, menus, inspector panels, and other UI surfaces without reimplementing the full editor
- [ ] Provide explicit diagnostics and debugging interfaces so hosts can inspect editor state, subscribe to meaningful lifecycle signals, and troubleshoot integrations
- [ ] Deliver the above through a planned, incremental API reorganization suitable for publishing as a general-purpose component library

### Out of Scope

- Runtime algorithm execution engine — not part of the current editor-SDK refactor goal
- Runtime plugin loading — valuable later, but secondary to stabilizing extension seams and package boundaries first
- A non-Avalonia UI stack — this milestone is about decoupling and modularizing the existing Avalonia-based offering, not replacing the presentation technology
- Reworking graph-domain concepts unrelated to extensibility — avoid expanding scope beyond componentization, API exposure, and debugging support

## Context

The repository already contains a layered multi-project .NET solution with `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, `AsterGraph.Avalonia`, a demo app, tests, and a host sample. Current documentation and codebase mapping show that the editor already supports graph editing, persistence, menu augmentation, style options, and host-provided node presentation/localization, but key orchestration classes remain large and tightly coupled.

The user wants the package line to evolve from an internally useful editor into a publishable general-purpose component library. The highest priority is splitting Avalonia components, followed by exposing more host-facing APIs for secondary development, then clarifying `AsterGraph.Editor` versus `AsterGraph.Avalonia` boundaries, and finally adding better debugging and diagnostic support.

The current codebase map highlights concrete pressure points that align with this goal: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` centralizes too many behaviors, `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` mixes rendering and interaction concerns, and some reusable services still carry demo-oriented defaults. The refactor therefore needs to preserve working capabilities while creating smaller seams, clearer package responsibilities, and a more intentional public API surface.

Phase 1 is now complete. The repository has a documented four-package SDK boundary, public factory/options initialization APIs for both the editor runtime and default Avalonia view, and an explicit compatibility story that keeps `GraphEditorViewModel` plus `GraphEditorView` valid during migration.

## Constraints

- **Tech stack**: Keep the solution centered on .NET, C#, and Avalonia — the existing packages and host story already depend on that stack
- **Compatibility strategy**: API reorganization is allowed, but it should be phased and deliberate rather than a single uncontrolled break — the user accepts planned migration
- **Product positioning**: The result must be publishable as a general-purpose component library, not just a private host refactor — public API quality matters
- **Architecture**: Existing validated editing capabilities must remain available during the transition — the project is a refactor and SDK hardening effort, not a rewrite
- **Extensibility**: Hosts should be able to replace or embed subcomponents independently — package and API design must support partial adoption
- **Observability**: Debuggability is now part of the product requirement, not a local developer convenience — diagnostics need explicit public seams

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Treat the current codebase as the validated baseline | The repo already ships working editor capabilities that should inform future phases instead of being rediscovered | ✓ Good |
| Prioritize component splitting before broader API exposure work | Smaller UI and presentation seams make later host-facing APIs more coherent and easier to stabilize | — Pending |
| Position AsterGraph as a general-purpose SDK for external hosts | The target audience is broader than one internal host, so public surface design and replaceability are first-class concerns | ✓ Good |
| Accept a phased API reorganization | A clean extensibility model is more important than preserving every current shape, but migration still needs to be controlled | ✓ Good |
| Include diagnostics and debugging as part of the planned public API | Integration and secondary development are central goals, so observability cannot stay internal-only | — Pending |
| Use factory/options APIs as the canonical host initialization path while preserving constructor-based compatibility facades | Phase 1 needed a formal public entry surface without forcing a breaking rewrite on existing hosts | ✓ Good |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `$gsd-transition`):
1. Requirements invalidated? -> Move to Out of Scope with reason
2. Requirements validated? -> Move to Validated with phase reference
3. New requirements emerged? -> Add to Active
4. Decisions to log? -> Add to Key Decisions
5. "What This Is" still accurate? -> Update if drifted

**After each milestone** (via `$gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check -> still the right priority?
3. Audit Out of Scope -> reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-03-26 after Phase 1 completion*
