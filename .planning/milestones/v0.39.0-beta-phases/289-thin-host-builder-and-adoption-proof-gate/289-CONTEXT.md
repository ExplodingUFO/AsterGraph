# Phase 289: Thin Host Builder And Adoption Proof Gate - Context

**Gathered:** 2026-04-26
**Status:** Ready for planning
**Mode:** Autonomous context from roadmap, Phase 288 output, factory inspection, and initialization tests

<domain>
## Phase Boundary

Phase 289 owns a thin hosted builder/facade for common Avalonia host setup and the proof gates that defend the productized adoption path. It should reduce first-run composition friction without introducing a second runtime, compatibility shim, fallback path, or generated template.

</domain>

<decisions>
## Implementation Decisions

- Put the builder in `AsterGraph.Avalonia.Hosting` because it returns the shipped Avalonia view and composes the existing hosted route.
- Keep required inputs explicit: document, catalog, and compatibility are still required. `UseDefaultCompatibility()` is opt-in and delegates to `DefaultPortCompatibilityService`.
- Delegate editor creation to `AsterGraphEditorFactory.Create(...)` and view creation to `AsterGraphAvaloniaViewFactory.Create(...)`.
- Expose `BuildEditorOptions()` and `BuildViewOptions(...)` for tests and advanced hosts, but keep the default ergonomic endpoint as `BuildAvaloniaView()`.
- Update docs to show when to use the builder and when to drop down to canonical session/runtime factories.

</decisions>

<code_context>
## Existing Code Insights

- `AsterGraphEditorOptions` already contains all target inputs: document, node catalog, compatibility, plugin registrations, plugin trust policy, localization provider, diagnostics sink, platform services, style/behavior, services, and instrumentation.
- `AsterGraphEditorFactory.Create(...)` returns the retained hosted facade while keeping runtime authority on `Editor.Session`.
- `AsterGraphAvaloniaViewFactory.Create(...)` applies view chrome, context-menu, shortcut, and presentation options to the stock `GraphEditorView`.
- `GraphEditorInitializationTests` already defends factory forwarding and can host builder coverage without a new test project.
- Phase 288 added ConsumerSample onboarding markers and bilingual quick-start docs.

</code_context>

<specifics>
## Specific Ideas

- Add `AsterGraphHostBuilder.Create()`.
- Add fluent methods for `UseDocument`, `UseCatalog`, `UseCompatibility`, `UseDefaultCompatibility`, `UsePluginTrustPolicy`, `UsePluginRegistrations`, `UseLocalization`, `UseDiagnostics`, `UsePresentation`, and common view options.
- Add tests proving builder output carries the same session/view types and forwarded seams as the existing factories.
- Add docs tests for builder vocabulary plus README first-view, scenario launch, five-minute path, and ConsumerSample markers.

</specifics>

<deferred>
## Deferred Ideas

- `dotnet new astergraph-avalonia`.
- Public API analyzer and API baseline/diff gate.
- Runtime-only builder.
- Compatibility-only fallbacks or retained migration wrappers.

</deferred>
