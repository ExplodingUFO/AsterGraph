# Domain Pitfalls

**Domain:** AsterGraph SDK refactor for modular Avalonia components and a harder public package boundary  
**Researched:** 2026-03-25  
**Repository confidence:** HIGH  

## Critical Pitfalls

### Pitfall 1: Breaking Existing API Consumers While Extracting the Monolith
**What goes wrong:** The refactor moves or reshapes the host-facing surface that external consumers already use directly: `GraphEditorViewModel`, its constructor, command properties, layout helpers, and event subscriptions.

**Why it happens:** The current host story is intentionally direct. `README.md`, `docs/host-integration.md`, and `tools/AsterGraph.HostSample/Program.cs` all tell hosts to construct `GraphEditorViewModel` themselves, pass providers/options into the constructor, and subscribe to `DocumentChanged`, `SelectionChanged`, `ViewportChanged`, `FragmentExported`, and `FragmentImported`. At the same time, `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` is the 2400+ line orchestration center, so any internal decomposition creates pressure to change constructor shape and public members.

**Consequences:** Upgrades turn into source and binary breaks for hosts, sample code drifts from published guidance, and package consumers lose trust in the SDK line before the modular story is complete.

**Warning signs:**
- A refactor requires changing both the host sample and the README quick-start at the same time.
- Constructor parameters multiply, reorder, or move to different packages without a compatibility layer.
- Event payloads or command names change because internals moved, not because host scenarios changed.
- A release requires consumers to update namespaces, package references, and call sites in one jump.
- The repo still has no API compatibility guardrails: `rg` found no `EnablePackageValidation`, `PackageValidationBaselineVersion`, or `ApiCompat` settings.

**Prevention strategy:**
- Freeze a deliberate host-facing facade before pulling logic apart. If `GraphEditorViewModel` stays public, treat it as the compatibility boundary, not as a free refactor target.
- Add package/API compatibility checks before the first public-surface move. For packable .NET libraries, Microsoft’s current guidance is `EnablePackageValidation` plus baseline validation against the previous released package.
- Prefer additive changes first: new overloads, new option records, new events. Keep old members alive with `[Obsolete]` messages long enough for one migration cycle.
- Put every host-breaking change behind an explicit migration note tied to the package version, not just a code diff.

**Address in:** Public API stabilization phase, then migration phase

### Pitfall 2: Over-Exposing Unstable Internals Just to Make Refactoring Easier
**What goes wrong:** Internal helpers get promoted to public API because they make the component split easier in the short term, even though they encode current implementation details rather than durable extension seams.

**Why it happens:** The codebase already has clear internal extraction targets such as `GraphEditorHistoryService`, `GraphEditorInspectorProjection`, `GraphContextMenuBuilder`, `NodeCanvasInteractionSession`, `NodeCanvasDragAssistCalculator`, and `GraphContextMenuPresenter`. The temptation will be to expose those classes directly instead of designing narrower contracts around host intent.

**Consequences:** The SDK inherits long-term support obligations for types that were only meant to help break up `GraphEditorViewModel` and `NodeCanvas`. Later cleanup becomes another breaking-change cycle.

**Warning signs:**
- New public types appear under `Internal`-style responsibilities or mirror existing private helper names.
- Public APIs expose raw `ObservableCollection<T>`, mutable view models, or service classes that currently exist only to support one control.
- Hosts start referencing `AsterGraph.Avalonia` for behavior/orchestration seams that belong in `AsterGraph.Editor` or `AsterGraph.Abstractions`.
- A proposed public type has no documented host scenario beyond “we need this during the refactor.”

**Prevention strategy:**
- Promote only intent-based contracts: interfaces, immutable records, and event payloads that map to real host customization scenarios.
- Keep helper algorithms and orchestration services `internal` unless a host truly needs to replace them.
- Require a host-sample or documentation example for every new public type before exposing it.
- Prefer extension points owned by `AsterGraph.Editor` or `AsterGraph.Abstractions`; keep Avalonia implementation plumbing private.

**Address in:** Public seam design phase

### Pitfall 3: Splitting the UI but Keeping the Same Hidden Coupling
**What goes wrong:** The repo ends up with more controls, but each one still requires the full shell view model and the same implicit platform wiring, so hosts cannot actually embed only the canvas, mini map, inspector, or menu surfaces independently.

**Why it happens:** `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` currently injects clipboard and host context into `GraphEditorViewModel`, while `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` binds directly to the full `GraphEditorViewModel`, subscribes to node/connection collections, builds context menus, and owns interaction state. `GraphEditorView` and `NodeCanvas` are also `UserControl`s today; Avalonia’s current docs position `UserControl` as a high-level composition tool, while reusable library controls are better served by templated/custom control patterns.

**Consequences:** Hosts still cannot compose subcomponents independently, UI work remains coupled to editor internals, and the refactor misses the main product goal even if files get smaller.

**Warning signs:**
- New reusable controls still take `GraphEditorViewModel` instead of a narrow sub-view contract.
- `GraphMiniMap`, inspector, or context menu extraction still depends on shell-only state such as `ChromeMode`, top-level clipboard access, or layout chrome parts.
- Reusing one control requires creating the whole `GraphEditorView`.
- Component APIs expose Avalonia visual concerns and editor orchestration concerns together.

**Prevention strategy:**
- Define sub-view models or read-only interfaces per component: canvas scene, viewport, selection, inspector, diagnostics.
- Keep platform bridges such as clipboard and top-level host context in the Avalonia shell layer, not in general component contracts.
- Use lookless/templated control patterns for controls that are meant to be restyled and embedded broadly.
- Prove embed-ability with focused host samples: canvas-only, mini-map-only, and inspector-only integration paths.

**Address in:** UI component decomposition phase

### Pitfall 4: Diagnostics That Leak Implementation Details Instead of Providing Stable Signals
**What goes wrong:** Diagnostics become a dump of internal state transitions, file-system details, or transient implementation names rather than a durable host-facing observability surface.

**Why it happens:** The current observable surface is thin and mixed-purpose. `StatusMessage` is UI-facing text. `GraphEditorDocumentChangedEventArgs` includes a `StatusMessage`. `GraphEditorFragmentEventArgs` exposes the full fragment path. The refactor goal adds “explicit diagnostics and debugging interfaces,” which creates pressure to publish whatever internal state is easiest to expose first.

**Consequences:** Hosts couple to message strings, local file paths, current command routing, or private implementation concepts. Later internal cleanup becomes a breaking change because host tooling started parsing incidental diagnostics.

**Warning signs:**
- New diagnostics reuse `StatusMessage` text as machine-readable data.
- Event payloads expose concrete service types, internal enum values, raw file paths, or exception messages from storage/rendering internals.
- Diagnostics fire on every pointer move or scene redraw because those are the easiest places to instrument.
- The only way to identify a condition is by comparing localized strings.

**Prevention strategy:**
- Separate user status text from diagnostics. Diagnostics should use stable codes, severity, timestamps, and typed payloads.
- Treat file paths and host objects as sensitive integration details; expose them only when explicitly intended and document the trust boundary.
- Publish a small diagnostic contract first: lifecycle events, load/save outcomes, compatibility warnings, rendering/perf counters.
- Keep implementation-specific traces internal or behind a debug-only/private hook.

**Address in:** Diagnostics and observability phase

### Pitfall 5: Migration Pain from a Package Reorganization That Happens All at Once
**What goes wrong:** Types move between assemblies or packages in a single step, forcing consumers to rewrite imports, package references, and upgrade logic together.

**Why it happens:** The roadmap explicitly calls for clearer `AsterGraph.Editor` versus `AsterGraph.Avalonia` boundaries. The current README says common hosts should directly reference `AsterGraph.Avalonia` and `AsterGraph.Abstractions`, with `AsterGraph.Editor` and `AsterGraph.Core` as optional deeper references. That boundary only works if type moves are staged carefully.

**Consequences:** Host upgrades turn into multi-package surgery, especially for consumers that already followed the current docs. Samples and migration docs lag the code, and preview users get stranded on old packages.

**Warning signs:**
- A type is moved to a different assembly with no forwarder, shim, or bridge package.
- Release notes say “API reorganized” without a before/after import map.
- The host sample only shows the new arrangement and no longer demonstrates compatibility with the prior consumption path.
- Consumers must add all four packages to keep working.

**Prevention strategy:**
- Stage package moves. Keep old assemblies forwarding moved public types when feasible; Microsoft’s CLR guidance explicitly supports type forwarding for this scenario.
- Publish a migration matrix: old namespace/package -> new namespace/package -> replacement pattern.
- Keep one supported compatibility path per release instead of requiring consumers to infer the intended dependency graph.
- Treat package-boundary changes as their own milestone, not as incidental fallout from component splitting.

**Address in:** Package-boundary realignment phase, then migration phase

### Pitfall 6: Test Gaps Hiding Regressions Until Hosts Upgrade
**What goes wrong:** The refactor appears clean in unit tests but breaks real host behavior because the current coverage is strongest around helpers and much thinner around public package contracts, UI lifecycle seams, and storage failure paths.

**Why it happens:** The repo already documents the biggest gaps: no minimap repaint coverage, no end-to-end workspace/template failure-path tests, limited `NodeCanvas` lifecycle coverage, no coverage thresholds, and no CI workflow. The current test suite also has no package-compatibility baseline or “compile an external host against the last package” check.

**Consequences:** Breaking changes and behavior regressions are discovered late by host apps instead of in the SDK repo. That is especially risky for a refactor whose main goal is safe external embedding.

**Warning signs:**
- A refactor is considered safe because the demo still runs.
- New APIs ship without host-sample coverage or public-event assertions.
- Refactors add tests for internal helpers only, while public contracts remain untested.
- Build/test hygiene stays weak; for example, the current nested `artifacts/audit/*_obj` outputs can break solution builds.

**Prevention strategy:**
- Add contract tests around the supported public surface: constructor paths, event payloads, context-menu augmentation, serialization, and component embedding.
- Add headless Avalonia tests for extracted controls, not just their helper classes.
- Add package compatibility validation and a smoke host that consumes built packages rather than project references.
- Fix build hygiene first so package validation and CI can be trusted.

**Address in:** Test-hardening phase before and during every public refactor phase

### Pitfall 7: Performance Regressions Caused by More Components and More Signals
**What goes wrong:** Modularity improves code organization but makes runtime behavior slower because more components subscribe to the same mutable editor state, redraw from the same changes, or recompute expensive snapshots more often.

**Why it happens:** The current hotspots are already known: `NodeCanvas.RenderConnections()` rebuilds the whole connection layer repeatedly, dirty tracking and history rely on whole-document serialization/snapshots, and template refresh reads every fragment file synchronously. Adding mini map, diagnostics, inspector, or host-facing event surfaces on top of that without changing the underlying signal model will amplify the cost.

**Consequences:** Dragging, zooming, pending-connection previews, and large-graph sessions get slower after the refactor even if the API story improves.

**Warning signs:**
- Each extracted component listens to `PropertyChanged` or collection changes independently and immediately rerenders.
- Diagnostics/logging hooks are emitted inside pointer-move or render loops.
- Undo/redo and dirty tracking still serialize full documents while more components react to those updates.
- Template scanning remains synchronous on UI-triggered paths.

**Prevention strategy:**
- Set performance budgets before extraction: drag latency, zoom latency, and acceptable graph sizes.
- Coalesce high-frequency signals and keep pointer-preview instrumentation lightweight.
- Move toward retained visuals/incremental connection updates instead of whole-layer rebuilds.
- Cap or redesign history storage before adding more consumers of document snapshots.

**Address in:** Component decomposition phase and performance-hardening phase

### Pitfall 8: Package-Boundary Mistakes That Recreate the Monolith in Public
**What goes wrong:** The package split looks cleaner on paper, but public dependencies pull consumers back into the full stack, or framework-neutral packages start depending on Avalonia-specific concepts.

**Why it happens:** The repository already communicates a package-consumption story: `AsterGraph.Abstractions` for stable contracts, `AsterGraph.Core` for models/serialization, `AsterGraph.Editor` for state/host seams, and `AsterGraph.Avalonia` for UI. But the current control layer depends directly on editor types, and the host sample reaches across multiple layers. A sloppy refactor could push view-specific types into `Abstractions`, move storage/services into `Avalonia`, or make `AsterGraph.Editor` impossible to use without UI assumptions.

**Consequences:** Consumers take unnecessary dependencies, package versioning becomes harder, and future non-shell or partial-host scenarios stay blocked.

**Warning signs:**
- `AsterGraph.Abstractions` or `AsterGraph.Core` gains Avalonia types, colors, controls, or visual-state concepts.
- `AsterGraph.Avalonia` exposes APIs that require direct file-storage services or low-level graph serialization to use visual components.
- A “canvas-only” consumer still has to reference shell-centric controls or shell-only options.
- Moving a type across package boundaries also changes its responsibility.

**Prevention strategy:**
- Enforce package rules explicitly:
  - `AsterGraph.Abstractions`: stable contracts and identifiers only.
  - `AsterGraph.Core`: pure models/serialization/compatibility only.
  - `AsterGraph.Editor`: framework-neutral editor state and host extension seams.
  - `AsterGraph.Avalonia`: UI adaptation only.
- Add a package smoke matrix that validates minimal supported combinations from the README.
- When a public type must move, keep responsibility constant and use forwarding/shims where feasible.

**Address in:** Package-boundary realignment phase

## Moderate Pitfalls

### Pitfall 1: Demo Defaults Leaking into the Published SDK
**What goes wrong:** Hosts inherit demo-branded storage paths and sample-oriented defaults from reusable packages.

**Why it happens:** `GraphWorkspaceService`, `GraphFragmentWorkspaceService`, and `GraphFragmentLibraryService` all default to `LocalApplicationData/AsterGraphDemo/...`.

**Consequences:** Cross-app collisions, surprising file locations, and production hosts accidentally depending on demo behavior.

**Warning signs:**
- Packaged usage works without configuring a host identity or storage root.
- Diagnostics, docs, or sample paths still mention `AsterGraphDemo`.
- Tests only cover the demo default path, not host-provided storage roots.

**Prevention strategy:**
- Make host identity/storage root explicit for publishable usage, or derive defaults from host-owned metadata rather than the demo name.
- Keep demo defaults inside the demo app, not in SDK defaults.

**Address in:** Package-boundary realignment phase

### Pitfall 2: Build Hygiene Blocking Compatibility Work
**What goes wrong:** The repo cannot reliably run the validation needed for a safe refactor because build artifacts pollute normal compilation.

**Why it happens:** The current concerns audit documents duplicate assembly attribute failures caused by nested `artifacts/audit/*_obj` directories under projects.

**Consequences:** CI/package validation becomes flaky, and compatibility regressions slip because the validation pipeline itself is not stable.

**Warning signs:**
- `dotnet build` or `dotnet test` intermittently fails depending on audit artifact presence.
- Validation work requires manual cleanup before every run.
- Refactor branches avoid running the full solution because the build is noisy.

**Prevention strategy:**
- Exclude audit artifacts from compile items or relocate them outside project directories before adding more validation steps.
- Stabilize the solution build before introducing package compatibility gates.

**Address in:** Test-hardening and build-hygiene phase

## Phase-Specific Warnings

| Phase Topic | Likely Pitfall | Mitigation |
|-------------|---------------|------------|
| UI component decomposition | More controls but same hidden dependency on full `GraphEditorViewModel` | Define narrow contracts per component and prove standalone hosting in samples/tests |
| Public API stabilization | Internal helper promotion becomes permanent API debt | Require host scenario + docs/sample before exposing any new public type |
| Package-boundary realignment | Types move across assemblies without compatibility story | Use package validation, staged moves, and type forwarding/shims where feasible |
| Diagnostics/observability | Status text becomes the de facto API | Publish stable diagnostic codes/payloads separate from localized UI text |
| Migration | Upgrade becomes a one-release rewrite for hosts | Ship migration tables, `[Obsolete]` bridges, and compatibility windows |
| Test hardening | Public regressions escape because only helper tests exist | Add contract tests, package smoke tests, and CI before larger public refactors |
| Performance hardening | Modularity adds more redraws and snapshot churn | Set budgets, coalesce signals, and benchmark large graphs before/after extraction |

## Sources

**Repository sources**
- `.planning/PROJECT.md`
- `.planning/codebase/CONCERNS.md`
- `.planning/codebase/CONVENTIONS.md`
- `.planning/codebase/TESTING.md`
- `README.md`
- `docs/host-integration.md`
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`
- `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs`
- `src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs`
- `tools/AsterGraph.HostSample/Program.cs`
- `Directory.Build.props`

**Official documentation**
- Microsoft Learn, “Breaking changes and .NET libraries” — https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/breaking-changes (HIGH)
- Microsoft Learn, “Package validation” — https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/overview (HIGH)
- Microsoft Learn, “Validate against a baseline package version” — https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/baseline-version-validator (HIGH)
- Microsoft Learn, “API compatibility tools” — https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/overview (HIGH)
- Microsoft Learn, “Type forwarding in the common language runtime” — https://learn.microsoft.com/en-us/dotnet/standard/assembly/type-forwarding (HIGH)
- Avalonia Docs, “UI composition” — https://docs.avaloniaui.net/docs/fundamentals/ui-composition (MEDIUM)
- Avalonia Docs, “Choosing A Custom Control Type” — https://docs.avaloniaui.net/docs/basics/user-interface/controls/creating-controls/choosing-a-custom-control-type (MEDIUM)
