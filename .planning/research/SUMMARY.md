# Project Research Summary

**Project:** AsterGraph
**Domain:** Publishable .NET graph-editor SDK with modular Avalonia UI
**Researched:** 2026-03-25
**Confidence:** HIGH

## Executive Summary

AsterGraph should evolve into a host-extensible graph-editor SDK, not a prettier monolithic editor shell. The research is consistent across stack, features, architecture, and pitfalls: keep the existing four-package shape, move editor intent/state/contracts into framework-neutral runtime layers, and make Avalonia a thin presentation/adaptation package with reusable controls and themes.

The recommended approach is compatibility-first. Stabilize package/API guardrails, extract narrow editor services behind the current `GraphEditorViewModel`, then split `NodeCanvas` and `GraphEditorView` into independently embeddable surfaces such as viewport, mini map, inspector, menus, and toolbar presenters. Commands, queries, events, and service seams need to become the primary integration contract before more UI is exposed.

The main risks are avoidable but serious: breaking existing hosts during refactor, exposing unstable internals as public API, splitting the UI without removing hidden coupling, and letting diagnostics or package moves become accidental breaking changes. Mitigation is clear: add package validation and migration shims early, prove standalone embedding with focused samples/tests, and keep package responsibilities strict.

## Key Findings

### Recommended Stack

Keep the four publishable packages and harden their boundaries rather than adding more packages now. Move packable targets to `net8.0;net10.0`, update Avalonia to `11.3.12`, keep `CommunityToolkit.Mvvm` internal to runtime/sample layers, and add `Microsoft.Extensions.Logging.Abstractions` plus `Microsoft.SourceLink.GitHub`. Central Package Management, Source Link, and package validation are part of the publishing baseline, not optional cleanup.

**Core technologies:**
- .NET SDK `10.0.201+` with `net8.0;net10.0` targets: current packaging/tooling baseline with a durable LTS-to-LTS support story.
- Avalonia `11.3.12`: keep the current UI stack while using `TemplatedControl`, `ControlTheme`, and lookless control patterns for reusable surfaces.
- `CommunityToolkit.Mvvm` `8.4.0`: acceptable for internal state/commands, but do not let toolkit types become public contracts.
- `Microsoft.Extensions.Logging.Abstractions` `10.0.4` and `ActivitySource`: standard diagnostics surface for hosts, tests, and tooling.
- `Avalonia.Headless.XUnit` `11.3.12`: required for extracted control coverage and embed-ability verification.

### Expected Features

The market expectation is clear: hosts want a reusable editor kernel plus replaceable UI pieces, not a black-box editor view. The MVP should prioritize embeddable surfaces, typed commands/queries/events, replaceable rendering/presenter seams, and host-supplied services.

**Must have (table stakes):**
- Composable surfaces: full shell plus independently embeddable canvas, mini map, inspector, and menu presenters.
- Stable commands, queries, and typed events for selection, viewport, document mutation, lifecycle, and failures.
- Replaceable visuals for nodes, edges, ports, and mini-map rendering without reimplementing editor behavior.
- Replaceable service seams for storage, clipboard, serialization, validation, localization, presentation, and diagnostics.
- Accessibility/localization/style-token contracts that do not force hosts into visual-tree hacks.

**Should have (competitive):**
- A first-class replacement kit for chrome surfaces such as menus, inspector sections, badges, and toolbars.
- Public diagnostics and observability contracts: snapshots, journals, warning/error streams, tracing hooks.
- Transaction and automation APIs for batch edits, imports, macros, and scripted editor workflows.
- Versioned capability contracts for optional host features.

**Defer (v2+):**
- Full diagnostics workbench UI.
- New end-user editing features unrelated to extensibility.
- Runtime plugin loading or a non-Avalonia presentation stack.

### Architecture Approach

The target architecture is a framework-neutral editor runtime with a thin Avalonia presentation package. `AsterGraph.Editor` should own session/state, commands, queries, policies, descriptors, diagnostics, and host seams. `AsterGraph.Avalonia` should own gesture translation, rendering, themes, presenters, and platform adapters. `GraphEditorViewModel` stays public initially as a compatibility facade while logic moves into narrower services.

**Major components:**
1. `AsterGraph.Abstractions` and `AsterGraph.Core` — durable contracts, IDs, tokens, immutable document model, serialization, compatibility rules.
2. `AsterGraph.Editor` runtime — mutable state, use-case services, commands/queries/events, diagnostics, and host-replaceable services.
3. `AsterGraph.Avalonia` presentation — viewport/canvas, mini map, shell chrome, themes, clipboard/menu/style adapters, and default composition controls.

### Critical Pitfalls

1. **Breaking existing consumers during extraction** — keep `GraphEditorViewModel` as the compatibility boundary at first, add package validation, and prefer additive shims plus migration notes.
2. **Promoting refactor helpers into permanent API** — only expose intent-based contracts tied to real host scenarios; keep internal helpers internal.
3. **Splitting controls without removing coupling** — define narrow component contracts and prove canvas-only, mini-map-only, and inspector-only hosting paths.
4. **Diagnostics becoming unstable implementation leaks** — publish stable codes/payloads and keep localized status text separate from machine-readable signals.
5. **Performance and test regressions during modularization** — add contract tests, headless UI tests, package smoke tests, build hygiene fixes, and performance budgets before deeper decomposition.

## Implications for Roadmap

Based on the research, the roadmap should follow dependency order rather than UI-first enthusiasm.

### Phase 1: Package and API Guardrails
**Rationale:** Public refactoring is unsafe until compatibility and packaging baselines exist.
**Delivers:** Central package management, `net8.0;net10.0`, Avalonia/tooling updates, Source Link, package validation, baseline checks, explicit options/registration extensions.
**Addresses:** Stable host consumption and migration safety.
**Avoids:** Breaking consumer upgrades, package-boundary drift, build-validation blind spots.

### Phase 2: Runtime Extraction and Public Contracts
**Rationale:** Commands, queries, events, diagnostics, and service seams are prerequisites for reusable surfaces.
**Delivers:** `IGraphEditorSession`, read-only state/query surfaces, command registry/policy gates, structured events, diagnostics contracts, host-owned service interfaces.
**Addresses:** Table-stakes integration API, automation, and host observability.
**Avoids:** Exposing unstable helpers, status-text-as-API, Avalonia leakage into runtime contracts.

### Phase 3: Viewport and Surface Decomposition
**Rationale:** `NodeCanvas` and the shell can only be split safely after runtime seams are stable.
**Delivers:** `GraphViewportControl`, separated connection/overlay layers, templated node visuals, embeddable mini map/inspector/menu/toolbar components, `GraphEditorView` as a convenience shell.
**Addresses:** Composable surfaces and replaceable visuals.
**Avoids:** Hidden coupling, shell-only embedding, performance regressions from uncontrolled subscriptions.

### Phase 4: Host Replacement Kit and Diagnostics Hardening
**Rationale:** Differentiators should be built on top of stable contracts, not invented in parallel.
**Delivers:** Presenter replacement kit, richer diagnostics/tracing, transaction/automation surface, migration documentation, package smoke matrix.
**Addresses:** Competitive extensibility, troubleshooting, and secondary development.
**Avoids:** One-off replacement hacks, opaque failures, and unverified public upgrade paths.

### Phase Ordering Rationale

- Packaging and compatibility come first because every later phase touches public surface area.
- Runtime extraction precedes UI splitting because menus, inspector, diagnostics, and automation all depend on stable commands/queries/events.
- UI decomposition comes before richer chrome replacement so hosts can embed real components instead of wrapper shells.
- Diagnostics hardening follows public runtime contracts so it can describe stable operations rather than transient internals.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 2:** Public contract shaping for commands/queries/events/diagnostics needs careful API design and migration review.
- **Phase 3:** Large-graph rendering, retained visuals, and subscription strategy need repository-specific performance validation.
- **Phase 4:** Transaction/automation semantics and diagnostics scope need tighter product decisions before implementation.

Phases with standard patterns (skip research-phase):
- **Phase 1:** Central package management, Source Link, package validation, DI registration, and options patterns are well-documented.
- **Core control-type choices:** Avalonia guidance for `UserControl` vs `TemplatedControl` vs custom-drawn `Control` is already strong enough to proceed.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | Based on current official .NET, NuGet, and Avalonia guidance with concrete version recommendations. |
| Features | HIGH | Strong convergence across mature graph-editor SDK comparables. |
| Architecture | HIGH | Package/control guidance is well-supported; migration sequencing is slightly more inferential but still solid. |
| Pitfalls | HIGH | Backed by both repository evidence and official .NET library compatibility guidance. |

**Overall confidence:** HIGH

### Gaps to Address

- Large-graph performance targets are not yet quantified; define budgets before component extraction.
- The compatibility window and exact migration shims for current hosts still need explicit planning.
- Diagnostics scope is directionally clear, but the stable event/code taxonomy still needs design work.

## Sources

### Primary
- `.planning/research/STACK.md`
- `.planning/research/FEATURES.md`
- `.planning/research/ARCHITECTURE.md`
- `.planning/research/PITFALLS.md`
- `.planning/PROJECT.md`

### Supporting official references reflected in the source docs
- Avalonia documentation on custom controls, templated controls, control themes, templates, and DI.
- Microsoft Learn guidance on .NET support policy, library packaging, package validation, Generic Host, options, logging, and distributed tracing.
- React Flow, tldraw, Rete.js, JointJS, and yFiles documentation for feature expectations and extension patterns.

---
*Research completed: 2026-03-25*
*Ready for roadmap: yes*
