# Phase 3: Embeddable Avalonia Surfaces - Research

**Researched:** 2026-03-26
**Domain:** Avalonia surface decomposition, embeddable control boundaries, default shell composition, and default menu/chrome publication
**Confidence:** MEDIUM-HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Phase 3 uses a medium-grain decomposition strategy. Do not split the Avalonia layer all the way down to tiny per-button or per-chrome widgets in this phase.
- **D-02:** The first-class embeddable surfaces for this phase must include: the full default shell, a standalone interactive canvas surface, a standalone inspector surface, and a standalone mini map surface.
- **D-03:** The standalone canvas surface must remain open-box usable, preserving default graph interaction behavior.
- **D-04:** Built-in default context menu and built-in keyboard shortcuts should stay enabled by default on the standalone canvas, but hosts must have explicit options to disable them.
- **D-05:** The standalone inspector surface is a pure inspector, covering selection details, node information, connection summaries, and parameter editing only.
- **D-06:** The standalone mini map surface stays narrow and focused on overview plus viewport navigation.
- **D-07:** Workspace controls, fragment/template controls, and shortcut-help blocks remain full-shell concerns in Phase 3.
- **D-08:** Header, library, and status chrome must become omittable through composition, but they do not need to be promoted into separate first-class public controls in this phase.
- **D-09:** The default Avalonia context-menu presenter should become a public, usable default surface.
- **D-10:** Full presenter replacement remains Phase 4 work.

### the agent's Discretion
- Exact control names, namespaces, and options/factory shape
- Whether `GraphEditorView` becomes a thin composition root over smaller public controls or stays the main public shell with additional standalone controls beside it
- Exact shape of menu/shortcut toggle options for standalone canvas

### Deferred Ideas (OUT OF SCOPE)
- Fine-grained first-class public controls for header/library/status
- Fully replaceable presenter interfaces and alternate visual implementations
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| EMBD-01 | Host can embed the full default editor shell as a convenience composition | Keep `GraphEditorView` / `AsterGraphAvaloniaViewFactory` as the full-shell path while refactoring internals behind it |
| EMBD-02 | Host can embed a standalone graph viewport or canvas surface without the full shell | Promote `NodeCanvas` into a documented standalone public surface with explicit behavior options |
| EMBD-03 | Host can embed the mini map independently of the full shell | Keep and document `GraphMiniMap` as an independent public control with its current narrow responsibility boundary |
| EMBD-04 | Host can embed the inspector independently of the full shell | Extract inspector content out of `GraphEditorView.axaml` into a dedicated public inspector surface |
| EMBD-05 | Host can embed or omit default menu and chrome presenters without rebuilding editor state | Decouple shell composition from canvas/inspector/minimap, and lift the default menu presenter into a public usable path |
</phase_requirements>

## Summary

The repository already contains two strong raw materials for Phase 3:

- `NodeCanvas` is already a real control with rendering plus rich graph interaction behavior
- `GraphMiniMap` is already a real narrow control with an independent responsibility boundary

The real work is therefore not inventing new surfaces from scratch. It is:

1. extracting the current inspector section out of `GraphEditorView.axaml`
2. moving shell-only responsibilities out of the standalone-surface path
3. introducing composition/options seams so hosts can omit shell chrome without reconstructing editor state
4. making the default Avalonia context-menu presenter usable without depending on internal-only implementation details

The safest Phase 3 strategy is to preserve the current full-shell entry point while turning `GraphEditorView` into a thinner composition root over reusable public controls. That satisfies the embedding requirements without prematurely taking on Phase 4 presenter-replacement concerns.

**Primary recommendation:** Keep `GraphEditorView` and `AsterGraphAvaloniaViewFactory` as the convenience full-shell path, promote `NodeCanvas` and `GraphMiniMap` into clearly documented first-class embeddable surfaces, extract a new standalone inspector control from the existing right-side inspector content, add explicit canvas behavior options for default menu/shortcut opt-out, and expose a public default context-menu presenter/service without introducing alternate presenter contracts yet.

## Standard Stack

### Core
| Area | Current State | Recommendation | Why |
|------|---------------|----------------|-----|
| Full shell | `GraphEditorView` is the only real shell entry point | Preserve it as the convenience composition root | Needed for `EMBD-01` and compatibility continuity |
| Canvas | `NodeCanvas` already handles scene render + interaction | Keep it interactive and public, add host-facing options around built-in menu/shortcut behavior | Best path to `EMBD-02` without rewriting graph interaction |
| Mini map | `GraphMiniMap` already exists as a narrow control | Keep it narrow and independent | Directly aligns with `EMBD-03` and current user decision |
| Inspector | Inspector is currently XAML embedded inside `GraphEditorView` | Extract a dedicated public inspector surface with the pure-inspector subset only | Needed for `EMBD-04` and clean surface boundaries |

### Supporting
| Area | Current State | Recommendation | Why |
|------|---------------|----------------|-----|
| Default menu presenter | `GraphContextMenuPresenter` exists but is `internal` and instantiated inside `NodeCanvas` | Make the default presenter publicly usable, but not swappable in this phase | Required for `EMBD-05` without stepping into Phase 4 |
| Chrome omission | `GraphEditorViewChromeMode` only gives `Default` / `CanvasOnly` | Replace or extend this with composition over smaller surfaces rather than multiplying shell enum states | The phase goal is embeddable surfaces, not an enum explosion |
| Style/host bridges | `GraphEditorView` currently applies style resources, clipboard bridge, and host context | Keep these bridges available in the full shell; reuse only the parts standalone surfaces truly need | Avoids duplicating shell responsibilities into every child control |

## Architecture Patterns

### Pattern 1: Thin Full-Shell Composition Root
**What:** Keep `GraphEditorView` public, but reduce it to composing smaller public controls instead of owning all UI sections inline forever.

**Why:** This preserves the working full-shell host story while allowing Phase 3 to expose standalone parts. It also reduces migration churn because hosts already know `GraphEditorView`.

**Consequence if skipped:** Phase 3 either stays too shallow (only standalone controls, shell still monolithic) or forces a breaking rewrite to replace the shell entirely.

### Pattern 2: Promote Existing Controls Before Inventing New Abstractions
**What:** Reuse `NodeCanvas` and `GraphMiniMap` as the starting public surfaces instead of creating parallel wrappers for them.

**Why:** These controls already exist, already render against `GraphEditorViewModel`, and already express the surface boundaries the user wants.

**Consequence if skipped:** The phase wastes time re-wrapping already working controls and increases API churn for no real gain.

### Pattern 3: Extract Inspector as Data-Bound Surface, Not Shell Fragment
**What:** Create a new public inspector control that binds to the same editor/session-backed state as the full shell, but only includes the pure-inspector content.

**Why:** The current inspector area in `GraphEditorView.axaml` mixes inspection content with shell concerns like workspace, fragments, minimap placement, and shortcut help. User decisions explicitly reject that mixture for the standalone inspector.

**Consequence if skipped:** Hosts either keep taking a heavy right-side shell panel or Phase 3 fails `EMBD-04`.

### Pattern 4: Opt-Out Options for Canvas Defaults
**What:** Keep default context-menu and default shortcut behavior enabled by default on standalone canvas, but expose explicit options to disable them.

**Recommended shape:**
- a small `NodeCanvasOptions` / `AsterGraphCanvasViewOptions` contract
- flags such as `EnableDefaultContextMenu` and `EnableDefaultShortcuts`

**Why:** This matches the user’s “open-box by default, but host can disable” decision and avoids branching the control into separate “interactive” vs “hosted” implementations.

### Pattern 5: Public Default Menu Presenter, Not Presenter Interface Hierarchy
**What:** Lift the existing default menu presenter into a public usable type or public service entry point, but stop there.

**Why:** The current code already isolates menu translation behind `GraphContextMenuPresenter`; making that default path public is enough for Phase 3. Full presenter replacement belongs in Phase 4 when node visuals, inspector visuals, and menu visuals are all handled consistently.

## Existing Code Signals

### Strong Existing Surface Candidates

**`NodeCanvas`**
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml`
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`

Signals:
- already a standalone `UserControl`
- owns rendering layers (`ConnectionLayer`, `NodeLayer`, `OverlayLayer`)
- already handles pointer, wheel, keyboard, marquee selection, panning, node drag, connection actions, and context-menu invocation
- already uses focused helper types like `NodeCanvasInteractionSession`, `NodeCanvasDragAssistCalculator`, and `NodeCanvasContextMenuContextFactory`

This is a strong sign that the canvas should be promoted, not redesigned.

**`GraphMiniMap`**
- `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs`

Signals:
- already independent from `GraphEditorView`
- already exposes a clear `ViewModel` property
- already has the narrow “overview + viewport drag” boundary the user wants

This is nearly Phase-3-ready already; planning should treat it as a publish/hardening task, not a fresh implementation.

### Full Shell Responsibilities That Need Separation

`GraphEditorView.axaml.cs` currently bundles:
- style resource application via `GraphEditorStyleAdapter`
- clipboard bridge attachment via `AvaloniaTextClipboardBridge`
- host-context propagation via `AvaloniaGraphHostContext`
- shell-wide keyboard shortcut routing
- chrome visibility control through `GraphEditorViewChromeMode`

`GraphEditorView.axaml` currently inlines:
- header chrome
- library chrome
- canvas frame
- inspector column
- mini map placement
- status chrome

This means the shell decomposition work should focus on moving layout sections into smaller controls or templates while deciding which shell services stay centralized at the shell root.

### Default Menu Presenter Is Already Partially Separated

`src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs` is already a distinct type that maps `MenuItemDescriptor` into Avalonia `ContextMenu` / `MenuItem`. It also already has a `BuildMenuControlForTest` helper used by `tests/AsterGraph.Editor.Tests/GraphContextMenuPresenterTests.cs`.

That is strong evidence that the phase should expose this default presenter/service path rather than inventing a completely different menu architecture now.

## Recommended Surface Breakdown

The smallest coherent Phase 3 surface set is:

1. **Full shell**
   - `GraphEditorView`
   - remains the convenience composition root

2. **Standalone interactive canvas**
   - `NodeCanvas` or a renamed canvas control built from it
   - default interaction retained
   - explicit built-in menu/shortcut toggles

3. **Standalone inspector**
   - new control extracted from current inspector XAML
   - binds to the same editor/session-backed state
   - only pure-inspector content

4. **Standalone mini map**
   - `GraphMiniMap`
   - no shell styling assumptions beyond what the host wraps around it

5. **Public default menu presenter**
   - current `GraphContextMenuPresenter` promoted to a public default-surface role

## Pitfalls

### Pitfall 1: Multiplying Shell Enum Modes Instead of Exposing Real Surfaces
If Phase 3 only adds more `GraphEditorViewChromeMode` values such as `InspectorOnly`, `MiniMapOnly`, `LibraryOnly`, or `CanvasAndInspector`, the host still cannot compose surfaces freely. That creates a brittle combinatorial shell API instead of real embeddable controls.

### Pitfall 2: Hollowing Out `NodeCanvas`
If menu/shortcut opt-out is implemented by stripping too much behavior out of `NodeCanvas`, the standalone canvas will stop feeling open-box usable and the phase will miss the user’s explicit choice.

### Pitfall 3: Reusing the Full Right-Side Panel as “Inspector”
If the extracted inspector carries workspace, fragment/template, minimap, or shortcut-help sections with it, the resulting surface will be too heavy and violate the pure-inspector decision.

### Pitfall 4: Duplicating Shell Services Across Standalone Surfaces
If every surface separately re-implements style application, clipboard bridge wiring, host context propagation, and global shortcut logic, the phase will increase duplication and make Phase 4 harder. Some of those responsibilities should remain shell-only or be factored into shared helpers.

### Pitfall 5: Going Too Far on Menu Abstraction
A full interface hierarchy for menu presenters, menu item renderers, and alternate visual implementations would overlap heavily with Phase 4. Phase 3 should stop at exposing the stock presenter path publicly.

## Code Examples

### Existing Interactive Canvas Hook Points
`NodeCanvas.axaml.cs` already has control-level event subscriptions in its constructor:
- `ContextRequested`
- `KeyDown`
- `PointerPressed`
- `PointerMoved`
- `PointerReleased`
- `PointerWheelChanged`

This is the natural place to add option-based branching for built-in menu/shortcut behavior.

### Existing Mini Map Narrow Boundary
`GraphMiniMap` already only depends on:
- current `ViewModel`
- scene/world bounds calculation
- viewport drag

It does not currently depend on shell chrome layout or shell command bars, which is exactly the right signal for keeping it narrow.

### Existing Menu Translation Boundary
`GraphContextMenuPresenter` already accepts:
- `Control target`
- `IReadOnlyList<MenuItemDescriptor> descriptors`
- `ContextMenuStyleOptions style`

That API shape is already close to a public default-surface service.

## Open Questions

1. **Should standalone surfaces keep binding directly to `GraphEditorViewModel`, or should Phase 3 introduce a thinner Avalonia-side adapter/view-model layer?**
   - Recommendation: stay on `GraphEditorViewModel` / `Session` in Phase 3. Introducing another Avalonia-only adapter layer is extra indirection that the current phase does not require.

2. **Where should standalone canvas options live?**
   - Recommendation: in `AsterGraph.Avalonia.Hosting` or adjacent Avalonia control options, not in `AsterGraph.Editor`. Menu/shortcut toggles are Avalonia-surface behavior, not framework-neutral runtime behavior.

3. **Should the extracted inspector be built as a single control first, or split internally into subviews immediately?**
   - Recommendation: first produce one coherent standalone inspector control. Internal subview extraction can happen opportunistically, but do not force extra public surface area in this phase.

## Environment Availability

| Dependency | Required By | Available | Notes |
|------------|-------------|-----------|-------|
| Existing full shell | `EMBD-01` baseline | ✓ | `GraphEditorView` and `AsterGraphAvaloniaViewFactory` already exist |
| Existing interactive canvas | `EMBD-02` | ✓ | `NodeCanvas` already exists and is substantial |
| Existing mini map control | `EMBD-03` | ✓ | `GraphMiniMap` already exists |
| Existing inspector data bindings | `EMBD-04` | ✓ | Inspector content already binds to `GraphEditorViewModel` in shell XAML |
| Existing menu presenter | `EMBD-05` | ✓ | Presenter exists but is not yet public |

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.2 + existing `tests/AsterGraph.Editor.Tests` project |
| Config file | none — existing test conventions |
| Quick run command | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorView|FullyQualifiedName~GraphContextMenuPresenter|FullyQualifiedName~NodeCanvas" -v minimal` |
| Full suite command | `dotnet test avalonia-node-map.sln -v minimal` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| EMBD-01 | Full default shell still embeds as convenience composition | integration | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorInitializationTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests"` | ✓ partial baseline |
| EMBD-02 | Standalone interactive canvas embeds without shell | integration | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~NodeCanvasStandalone"` | ❌ Wave 0 |
| EMBD-03 | Mini map embeds independently | integration | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphMiniMapStandalone"` | ❌ Wave 0 |
| EMBD-04 | Inspector embeds independently | integration | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphInspectorStandalone"` | ❌ Wave 0 |
| EMBD-05 | Hosts can omit chrome and use default menu presenter without rebuilding state | integration | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphContextMenuPresenter|FullyQualifiedName~GraphEditorSurfaceComposition"` | ✓ partial baseline |

### Wave 0 Gaps
- [ ] `tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs` — standalone canvas surface, built-in interaction toggles, and no-shell embedding
- [ ] `tests/AsterGraph.Editor.Tests/GraphMiniMapStandaloneTests.cs` — standalone minimap rendering and viewport dragging
- [ ] `tests/AsterGraph.Editor.Tests/GraphInspectorStandaloneTests.cs` — pure-inspector surface extraction
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorSurfaceCompositionTests.cs` — full shell plus mixed standalone-surface composition

## Sources

### Primary (HIGH confidence)
- `.planning/ROADMAP.md`
- `.planning/REQUIREMENTS.md`
- `.planning/PROJECT.md`
- `.planning/STATE.md`
- `.planning/phases/03-embeddable-avalonia-surfaces/03-CONTEXT.md`
- `.planning/phases/01-consumption-compatibility-guardrails/01-CONTEXT.md`
- `.planning/phases/02-runtime-contracts-service-seams/02-CONTEXT.md`
- `.planning/codebase/ARCHITECTURE.md`
- `.planning/codebase/STRUCTURE.md`
- `.planning/codebase/INTEGRATIONS.md`
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml`
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs`
- `src/AsterGraph.Avalonia/Controls/GraphEditorViewChromeMode.cs`
- `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs`
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs`
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewOptions.cs`
- `src/AsterGraph.Avalonia/Hosting/AvaloniaGraphHostContext.cs`
- `src/AsterGraph.Avalonia/Services/AvaloniaTextClipboardBridge.cs`
- `src/AsterGraph.Avalonia/Styling/GraphEditorStyleAdapter.cs`
- `src/AsterGraph.Avalonia/Controls/Internal/NodeCanvasInteractionSession.cs`
- `tests/AsterGraph.Editor.Tests/*.cs`
- `tools/AsterGraph.HostSample/Program.cs`

### Secondary (MEDIUM confidence)
- `src/AsterGraph.Avalonia/README.md`
- `README.md`
- `docs/host-integration.md`

### Notes
- This Phase 3 research was produced locally after a Windows-side research agent hang. Confidence remains medium-high because the phase is primarily about repo-internal surface boundaries and current Avalonia control structure.

## Metadata

**Confidence breakdown:**
- Surface decomposition direction: HIGH
- Canvas/minimap boundary: HIGH
- Inspector extraction scope: HIGH
- Public default menu presenter strategy: MEDIUM-HIGH
- Validation architecture: MEDIUM

**Research date:** 2026-03-26
**Valid until:** 2026-04-25
