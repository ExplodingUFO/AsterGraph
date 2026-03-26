# Architecture Patterns

**Domain:** Avalonia-based graph editor SDK  
**Project:** AsterGraph  
**Researched:** 2026-03-25  
**Confidence:** HIGH for package/control guidance, MEDIUM for migration sequencing

## Recommended Architecture

Evolve AsterGraph into a **framework-neutral editor runtime with a thin Avalonia presentation package**. Keep the existing four-package shape, but refactor responsibilities inside `AsterGraph.Editor` and `AsterGraph.Avalonia` before adding more NuGet packages. The current package split is already directionally correct; the problem is not the top-level package count, it is that `GraphEditorViewModel` and `NodeCanvas` still own too many responsibilities.

The target shape should be:

```text
Host App
\- Composition Root
   |- host services, config, logging, diagnostics subscribers
   |- AddAsterGraphEditor(...)
   \- AddAsterGraphAvalonia(...)

AsterGraph.Abstractions
\- Stable contracts: definitions, IDs, style/behavior tokens, extension contracts

AsterGraph.Core
\- Immutable graph models, serialization, compatibility policies

AsterGraph.Editor
|- Editor session/facade
|- mutable runtime state store
|- command/use-case services
|- workspace/clipboard/template abstractions
|- menu/presentation/localization descriptors
\- diagnostics/events

AsterGraph.Avalonia
|- shell composition controls
|- viewport/canvas controls
|- Avalonia adapters for clipboard, menus, styling, host context
\- default themes, templates, and renderers
```

The migration rule is simple: **Editor owns intent, state, policies, and contracts. Avalonia owns input translation, visual composition, and platform adapters.**

## Component Boundaries

| Component | Responsibility | Communicates With |
|-----------|---------------|-------------------|
| `AsterGraph.Abstractions` | Stable public contracts: node definitions, IDs, style tokens, behavior tokens, extension interfaces | `Core`, `Editor`, hosts |
| `AsterGraph.Core` | Immutable `GraphDocument` model, serializer, compatibility rules | `Editor`, hosts that need raw documents |
| `AsterGraph.Editor.State` | Mutable editor state: nodes, connections, selection, viewport, pending connection, dirty state | Editor use-case services, diagnostics |
| `AsterGraph.Editor.UseCases` | Commands and workflows: node edit, connect, layout, history, workspace, fragments, templates | State store, repositories, policies |
| `AsterGraph.Editor.Extensions` | Host seams: localization, menu augmentation, node presentation, diagnostics subscriptions | Hosts, use-case services |
| `AsterGraph.Editor.Diagnostics` | Explicit events, inspection snapshots, `ILogger`/`ActivitySource` integration | Hosts, tooling, tests |
| `AsterGraph.Avalonia.Controls` | Shell, viewport, mini map, inspector, toolbox, status bar | Editor session and Avalonia adapters |
| `AsterGraph.Avalonia.Adapters` | Clipboard bridge, context menu presenter, style adapter, host-context adapter | Avalonia controls, Editor extension contracts |
| Host app | Node catalog, business actions, storage roots, runtime presentation data, composition | Editor and Avalonia registration APIs |

## What Stays In Editor Core Vs Avalonia Package

### Keep In `AsterGraph.Editor`

- Command permissions and behavior policies
- Selection, viewport, pending-connection, and document mutation logic
- Undo/redo, dirty tracking, and snapshot rebuilding
- Workspace/fragment/template workflows as **interfaces plus default implementations**
- Menu intent as data descriptors, not Avalonia controls
- Localization, node-presentation, and host-context contracts
- Diagnostics API: editor events, inspection snapshots, `ILogger`, `ActivitySource`
- All public APIs that hosts should consume without referencing Avalonia

### Keep In `AsterGraph.Avalonia`

- Pointer, wheel, keyboard, drag, and focus handling
- Clipboard implementation against Avalonia `TopLevel`
- Context menu rendering from editor descriptors into Avalonia controls
- Theme resources and control themes
- Default shell layout: toolbar, node library, inspector, status bar, mini map
- Visual adapters from style tokens to brushes, spacing, and control resources
- Node/connection rendering and view composition

### Do Not Leak Across The Boundary

- No `AvaloniaObject`, `Control`, `Brush`, `KeyEventArgs`, or `PointerEventArgs` in `AsterGraph.Editor` public contracts
- No filesystem defaults, app identity, or demo paths baked into editor core services
- No direct file I/O, clipboard access, or context-menu control creation inside editor workflows
- No host business actions implemented inside Avalonia controls

## Recommended Data Flow

Use a single directional flow:

```text
Host config/services
    ->
Editor session + state store
    ->
Read models / descriptors / events
    ->
Avalonia adapters and controls
    ->
User gestures
    ->
Avalonia input translators
    ->
Editor use-case services
    ->
State mutations + diagnostics/events
```

More concretely:

1. The host composes node definitions, storage options, localization, presentation providers, and optional diagnostics sinks.
2. The editor session owns the mutable runtime state and exposes stable commands, queries, descriptors, and events.
3. Avalonia controls bind to editor state and descriptors but never implement editor rules themselves.
4. Avalonia gesture handlers translate pointer or keyboard input into editor use-case calls.
5. Editor services mutate state, emit structured events, and optionally emit diagnostics spans/logs.
6. Avalonia controls re-render from updated state; hosts observe the same changes through editor events, not through control internals.

That keeps UI and host integrations symmetric: the host and the Avalonia layer both talk to the editor through the same public seams.

## UI Composition Recommendation

Avalonia's current guidance is a good fit here:

- Use `UserControl` for specialized composed views.
- Use `TemplatedControl` for reusable general-purpose controls whose visuals should be replaceable.
- Use custom-drawn `Control` types only for mostly graphical surfaces.

Applied to AsterGraph:

| UI piece | Recommended control type | Why |
|---------|---------------------------|-----|
| `GraphEditorView` shell | `UserControl` | It is a composed application view, not a primitive control |
| New `GraphViewportControl` replacing the monolithic `NodeCanvas` | `TemplatedControl` | Behavior should stay stable while hosts can restyle or replace structural parts |
| Node item visuals | `ItemsControl` + `DataTemplate` | Hosts need replaceable node visuals without forking the canvas |
| Connection layer | custom-drawn `Control` | Rendering-heavy, mostly graphical |
| Selection/pending-connection overlay | custom-drawn `Control` or lightweight templated part | Keeps gesture adorners separate from node rendering |
| Mini map | custom-drawn `Control` | Already render-oriented and should stay isolated |
| Inspector/toolbox/status panes | `UserControl` initially, optionally `TemplatedControl` once public styling stabilizes | Split chrome before over-generalizing |

Recommended viewport template:

```text
GraphViewportControl
|- PART_ConnectionLayer
|- PART_NodeItemsHost
|- PART_OverlayLayer
\- PART_ContextMenuHost
```

This is the key decoupling move. Right now `NodeCanvas` creates node visuals, handles gestures, renders links, and opens menus. Split those so hosts can replace node visuals through templates while keeping editor behavior unchanged.

## Service Seams To Introduce

Do not replace the current `GraphEditorViewModel` API immediately. First, turn it into a compatibility facade over narrower services.

Recommended seams:

| Seam | Purpose |
|------|---------|
| `IGraphEditorSession` | Stable host-facing facade for commands, queries, events |
| `IGraphEditorState` | Read-only runtime snapshot surface for controls and diagnostics |
| `IGraphMutationService` | Node/connection creation, deletion, movement, parameter edits |
| `IGraphSelectionService` | Selection semantics and marquee/batch helpers |
| `IGraphViewportService` | Zoom, pan, fit, center, screen/world transforms |
| `IGraphWorkspaceStore` | Save/load graph documents |
| `IGraphFragmentStore` | Fragment import/export |
| `IGraphTemplateLibrary` | Template enumeration and delete/import |
| `IGraphClipboardService` | Clipboard read/write abstraction |
| `IGraphMenuService` | Build framework-neutral menu descriptors |
| `IGraphDiagnostics` | Structured events, inspection snapshots, logger/tracing hooks |

Migration note: `GraphWorkspaceService`, `GraphFragmentWorkspaceService`, and `GraphFragmentLibraryService` should become default implementations behind those interfaces, with storage root configured by host options instead of demo defaults.

## Migration Layering

Use a compatibility-first migration:

### Layer 1: Compatibility Facade

Keep `GraphEditorViewModel` public. Internally, move logic into services and have the view model delegate. This avoids breaking hosts while creating testable seams.

### Layer 2: Replace Concrete Platform Dependencies

Replace `SetTextClipboardBridge`, `SetHostContext`, and direct file-service construction with injected interfaces/options. Avalonia should supply default adapters, but the editor package should no longer assume it owns platform access.

### Layer 3: Split The Canvas

Refactor `NodeCanvas` into:

- a viewport control coordinating template parts
- a retained connection renderer
- a node items host using templates
- a separate interaction coordinator
- a menu adapter

This is the highest-value UI split because it removes the current rendering/input monolith.

### Layer 4: Split Shell Chrome

Break `GraphEditorView` into embed-friendly pieces:

- `GraphToolbarView`
- `GraphNodeLibraryView`
- `GraphInspectorView`
- `GraphMiniMapView`
- `GraphStatusBarView`
- `GraphViewportView`

Hosts should be able to compose these pieces directly instead of toggling only `ChromeMode`.

## Build Order

Build in this order:

1. **Stabilize service registration and options**
   - Add `AddAsterGraphEditor` and `AddAsterGraphAvalonia` registration extensions.
   - Make Generic Host support first-class, but optional.
   - Move storage roots and diagnostics settings into options objects.

2. **Extract editor internals behind interfaces**
   - Introduce `IGraphEditorSession`, `IGraphViewportService`, `IGraphWorkspaceStore`, and `IGraphDiagnostics`.
   - Keep `GraphEditorViewModel` delegating to these internals.

3. **Fix persistence and diagnostics seams**
   - Remove demo-branded storage defaults.
   - Add structured diagnostics alongside existing UI status messages.
   - Emit editor/component-level `ActivitySource` spans and `ILogger` logs from the runtime layer.

4. **Split `NodeCanvas` first**
   - Extract interaction coordinator.
   - Extract connection renderer.
   - Replace manual node visual construction with templated node items.
   - Keep host sample working through the same high-level editor API.

5. **Split shell composition**
   - Publish reusable chrome controls.
   - Keep `GraphEditorView` as the default composed shell built from those parts.

6. **Expand host replacement points**
   - Allow host-provided node templates, inspector panels, menu presenters, and chrome composition.
   - Add diagnostics/inspection tooling APIs after the runtime/UI split is stable.

7. **Only then consider new packages**
   - If package pressure remains, split `AsterGraph.Editor.Diagnostics` or `AsterGraph.Avalonia.Controls`.
   - Do not start by multiplying packages.

## Patterns To Follow

### Pattern 1: Runtime-Owns-Intent, UI-Owns-Rendering

**What:** Editor package exposes commands, queries, descriptors, and events; Avalonia consumes them and renders.

**When:** Everywhere a rule could otherwise be duplicated in controls and hosts.

**Why:** It keeps the SDK portable and makes host extensions work without UI forks.

### Pattern 2: Template Node Rendering, Not Canvas-Owned Node Construction

**What:** Nodes should render through an item template pipeline, while the viewport/control owns positioning and interaction routing.

**When:** Any host wants to replace node visuals, badges, or inspector-linked affordances.

**Why:** Avalonia's collection controls plus data templates are the right mechanism for replaceable item visuals; a monolithic code-behind canvas is not.

### Pattern 3: Options + Registration Extensions

**What:** Group related registrations behind `AddAsterGraphEditor` / `AddAsterGraphAvalonia`, and group settings by concern with options classes.

**When:** DI-based host integration.

**Why:** This matches current .NET guidance and keeps configuration isolated by scenario.

### Pattern 4: Library Diagnostics Via `ILogger` + `ActivitySource`

**What:** Emit logs and tracing from the editor runtime, not from Avalonia controls.

**When:** Save/load, large graph operations, menu augmentation failures, host-provider failures, expensive redraw paths.

**Why:** .NET guidance explicitly supports library-owned `ActivitySource` instrumentation without forcing a telemetry vendor, while `ILogger` keeps logs host-routable.

## Anti-Patterns To Avoid

### Anti-Pattern 1: Public API Bound To `GraphEditorViewModel` Implementation Details

**Why bad:** It freezes today's monolith into the long-term SDK.

**Instead:** Keep it as a facade while extracting stable interfaces underneath.

### Anti-Pattern 2: Avalonia Control Owning Business Workflows

**Why bad:** Persistence, compatibility, and command rules become impossible to reuse or test outside the control tree.

**Instead:** Controls translate gestures into runtime commands.

### Anti-Pattern 3: Demo Defaults In Publishable Services

**Why bad:** It leaks one app's identity into all hosts and creates unsafe shared storage behavior.

**Instead:** Require host identity/storage options or a host-supplied store.

### Anti-Pattern 4: Status Text As The Only Diagnostics Surface

**Why bad:** UI strings are not a supportable SDK debugging contract.

**Instead:** Keep status text for UX, but add structured events, logs, and inspection snapshots.

## Scalability Considerations

| Concern | At 100 users | At 10K users | At 1M users |
|---------|--------------|--------------|-------------|
| Host integrations | Constructor composition is still workable | DI registration extensions become important | Stable service seams and versioned options are mandatory |
| UI replaceability | `ChromeMode` may be enough | Hosts need subcontrol composition | Shell must be fully modular |
| Graph size | Snapshot history is acceptable | History caps and retained visuals become necessary | Operation/diff-based history and selective redraw are required |
| Diagnostics | UI status messages may suffice | `ILogger` and inspection snapshots needed | `ActivitySource`, structured events, and tooling integration needed |

## Recommended First Refactor Targets

1. `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
2. `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
3. `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`
4. `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`
5. `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs`

## Sources

### Primary Sources

- Avalonia Docs: UI composition  
  https://docs.avaloniaui.net/docs/fundamentals/ui-composition  
  Confidence: HIGH

- Avalonia Docs: Choosing a custom control type  
  https://docs.avaloniaui.net/docs/custom-controls/choosing-a-custom-control-type  
  Confidence: HIGH

- Avalonia Docs: How to create templated controls  
  https://docs.avaloniaui.net/docs/custom-controls/templated-controls  
  Confidence: HIGH

- Avalonia Docs: Data Templates  
  https://docs.avaloniaui.net/docs/concepts/templates/  
  Confidence: HIGH

- Avalonia Docs: Implementing dependency injection  
  https://docs.avaloniaui.net/docs/guides/implementation-guides/how-to-implement-dependency-injection  
  Confidence: HIGH

- Microsoft Learn: .NET Generic Host  
  https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host  
  Confidence: HIGH

- Microsoft Learn: Service registration in dependency injection  
  https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/service-registration  
  Confidence: HIGH

- Microsoft Learn: Options pattern in .NET  
  https://learn.microsoft.com/en-us/dotnet/core/extensions/options  
  Confidence: HIGH

- Microsoft Learn: Distributed tracing instrumentation walkthroughs  
  https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs  
  Confidence: HIGH

### Repository Sources

- `.planning/PROJECT.md`
- `.planning/codebase/ARCHITECTURE.md`
- `.planning/codebase/CONCERNS.md`
- `.planning/codebase/STRUCTURE.md`
- `README.md`
- `docs/host-integration.md`
- `docs/plans/2026-03-23-astergraph-host-extensibility.md`
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`
- `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`
- `tools/AsterGraph.HostSample/Program.cs`
