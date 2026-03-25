# Feature Landscape

**Domain:** Reusable graph-editor SDK / host-extensible Avalonia component library
**Researched:** 2026-03-25
**Overall confidence:** HIGH

Current ecosystem expectation for SDK-style editor libraries is clear: hosts do not want a single black-box editor shell. They expect a stable editor kernel plus individually embeddable or replaceable surfaces, typed visual customization, explicit commands and queries, structured events, and service seams that let them integrate storage, menus, policy, and host-specific behavior without forking internals.

The strongest comparables converge on the same pattern from different stacks. React Flow treats minimap, controls, panels, custom nodes, and instance/store APIs as normal public building blocks. tldraw exposes component overrides, action/tool overrides, UI events, and a minimal editor without the default UI. Rete.js centers its plugin system and render plugins so minimap, readonly, context menu, and rendering are composable rather than baked in. JointJS and yFiles push the same direction in more diagram-SDK-oriented form with inspector, navigator/overview, commands, and event infrastructures. That makes the following recommendations appropriate for AsterGraph as a publishable host-facing SDK.

## Table Stakes

Features hosts will reasonably expect from a reusable graph-editor SDK in 2026. Missing these makes the package feel like an app shell, not a host platform.

| Feature Group | Why Expected | Complexity | Notes |
|---------------|--------------|------------|-------|
| Composable editor surfaces | React Flow ships `MiniMap`, `Controls`, and `Panel` as separate public components; tldraw lets hosts hide or replace UI components; Rete/JointJS/yFiles expose navigator/minimap and other UI pieces as plugins or adjunct components. | High | AsterGraph should let hosts embed full chrome or only `canvas`, `mini map`, `inspector`, `menu surface`, and future tool panels independently. Switching surface composition must not require rebuilding editor state. |
| Replaceable node, edge, port, and mini-map visuals | React Flow custom nodes are first-class; tldraw and Rete.js both treat rendering customization as a normal extension path, not a hack. | High | Separate interaction logic from rendered visuals. Hosts need to replace node cards, connection visuals, badges, port affordances, and mini-map glyphs without reimplementing drag/connect/selection behavior. |
| Declarative menu and inspector models | tldraw exposes action/menu overrides; JointJS Inspector uses declarative field definitions with custom input support. | Medium-High | Menus and inspectors should be renderer-agnostic models plus default Avalonia presenters. Hosts should extend or replace the presenter while keeping the intent model and command bindings. |
| Public command system with policy and read-only support | yFiles, JointJS, React Flow, and Rete all expose command or interaction-policy surfaces instead of burying behavior in private UI handlers. | Medium | Commands should have stable IDs, `CanExecute`, host-readable metadata, keyboard binding hooks, and policy gates for read-only, restricted editing, and feature-flagged hosts. Menus, shortcuts, buttons, and automation should all route through the same command layer. |
| Public query / instance API | ReactFlow exposes `ReactFlowInstance` and store access; tldraw exposes editor and store APIs for state reads and viewport/canvas control. | Medium | Hosts need on-demand queries for selection, viewport, graph snapshot, node positions, hit testing, active gesture/tool, permissions, and coordinate transforms. Queries should not force subscription or UI ownership. |
| Structured events and subscriptions | React Flow has selection and viewport events; tldraw separates UI events from store changes; Rete emits typed signals; yFiles exposes broad interaction events. | Medium | AsterGraph should expose typed events for document mutation, selection, viewport, command execution, connection gestures, lifecycle, warnings, and recoverable failures. Every subscription needs an explicit unsubscribe path and stable payload semantics. |
| Replaceable service seams | Plugin-based SDKs such as Rete.js and the composable core/UI split in tldraw show hosts expect service replacement instead of subclassing monoliths. | High | Storage, clipboard, serialization, node catalog, compatibility/validation, localization, presentation, host context, and diagnostics sinks should all be replaceable via small interfaces. Default services should be safe defaults, not demo-branded assumptions. |
| Accessibility, localization, and style-token contracts | React Flow exposes localization and ARIA label configuration; tldraw exposes translations and component overrides. | Medium | Public contracts should stay framework-neutral where possible. Hosts should be able to replace labels, keyboard hints, automation names, and semantic style tokens without touching Avalonia-specific internals. |

## Differentiators

Features that are not universal table stakes yet, but materially improve AsterGraph's value as a reusable SDK.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| First-class replacement kit for chrome surfaces | Most libraries let hosts replace pieces; fewer make replacement pleasant. Shipping stable presenter contracts and small UI primitives for menus, inspector sections, badges, and toolbars would make AsterGraph easier to adopt than a "replace it all yourself" SDK. | High | Recommended direction: keep default Avalonia presenters, but expose the same underlying menu/inspector/view models so hosts can rebuild only what they need. |
| Public diagnostics and observability surface | Rich host diagnostics are still a differentiator. Most peers expose events or debug toggles, but not a full integration-facing diagnostics contract. | Medium-High | Useful capabilities: state snapshot export, command journal, mutation correlation IDs, service-call tracing, warning/error stream, and a lightweight debug overlay or probe API. This directly supports the project's "secondary development" and troubleshooting goals. |
| Transaction and automation API | Secondary developers often need batch imports, macros, domain-specific commands, and scripted layout/editor flows. A stable transaction surface makes the SDK more than a visual component. | Medium | Recommended surface: begin/end transaction, batched undo labels, silent vs observable mutations, and automation-safe command invocation that still flows through the public command/event stack. |
| Versioned capability contracts | Publishable SDKs age better when optional capabilities are explicit instead of inferred from object shape. | Medium | Capability descriptors for optional surfaces and services would reduce future breaking changes and make host integration more intentional. |

## Anti-Features

Things to explicitly avoid while turning AsterGraph into a host-facing SDK.

| Anti-Feature | Why Avoid | What to Do Instead |
|--------------|-----------|-------------------|
| Monolithic shell ownership | A single `GraphEditorView` that owns every panel and behavior makes embedding and replacement brittle. It also recreates the exact coupling that modern SDK users are trying to escape. | Treat the shell as one composition option over smaller public controls and presenters. |
| UI customization by subclassing internals or walking the visual tree | This creates fragile host code tied to implementation details and prevents stable package evolution. | Expose typed slots, presenters, descriptors, and service interfaces. |
| Mutation-only API surface | If hosts can trigger commands but cannot query state or correlate results through events, integrations become guesswork. The reverse is also insufficient. | Ship commands, queries, and events as a coherent triad. |
| Demo-specific defaults in reusable packages | Demo-branded storage roots and sample-oriented assumptions leak into host apps and break multi-host correctness. | Require explicit host identity or configurable defaults for persistence, template libraries, and diagnostics output. |
| Private-only diagnostics with status-string failure handling | SDK hosts need machine-readable failures, not just UI text or ad hoc console output. | Expose structured warning/error events, diagnostic probes, and optional debug logging contracts. |
| Hard coupling of rendering and interaction | When node rendering, connection drawing, event subscriptions, and interaction policy live in one control, hosts cannot safely replace visuals or test behavior in isolation. | Keep interaction coordination, scene rendering, and visual factories separate. |

## Feature Dependencies

The recommended dependency order for feature groups is:

```text
Editor kernel + public state model
  -> Commands / Queries / Events
  -> Surface composition (canvas / mini map / inspector / menus)

Presentation contracts
  -> Replaceable node visuals
  -> Replaceable mini-map visuals
  -> Host-owned chrome/presenter replacements

Command registry + policy gates
  -> Menus
  -> Keyboard shortcuts
  -> Toolbar/control buttons
  -> Read-only and restricted-host modes
  -> Automation / batch operations

Queries + events + command correlation
  -> Diagnostics
  -> Host troubleshooting
  -> Test harnesses and integration verification

Service seams
  -> Storage / clipboard / serialization
  -> Definition catalogs / compatibility / validation
  -> Localization / presentation / host context
```

Practical ordering implication for AsterGraph:

1. Split the editor kernel from the current monolithic Avalonia shell and expose stable editor-state contracts.
2. Stabilize commands, queries, and events before building many new chrome components, because menus, inspector, diagnostics, and automation all depend on them.
3. Extract presenter-oriented seams for canvas visuals, mini map, inspector, and menus once the kernel contracts are stable.
4. Add public diagnostics after commands/events exist, so diagnostics can describe real public operations instead of internal implementation details.

## MVP Recommendation

Prioritize:

1. Composable surfaces: full shell, canvas-only, and individually embeddable `mini map`, `inspector`, and `menu` presenters.
2. Stable commands, queries, and events: enough for hosts to drive behavior without reaching into internal view models.
3. Replaceable presentation seams: host-defined node visuals, menu rendering, inspector rendering, and mini-map rendering.
4. Replaceable core services: persistence, clipboard, compatibility/validation, localization, presentation, and diagnostics sink.

Defer:

- Full diagnostics workbench UI: valuable, but it should be built on top of the public event/query/command surface rather than invented first.
- Broad new end-user editing features unrelated to extensibility: they add surface area without advancing the SDK goal.

## Sources

Official sources used for this feature landscape:

- React Flow API and customization docs: `https://reactflow.dev/api-reference/react-flow`, `https://reactflow.dev/api-reference/components/minimap`, `https://reactflow.dev/api-reference/components/controls`, `https://reactflow.dev/api-reference/components/panel`, `https://reactflow.dev/api-reference/hooks/use-react-flow`, `https://reactflow.dev/api-reference/hooks/use-store-api`, `https://reactflow.dev/learn/customization/custom-nodes` — HIGH confidence
- tldraw SDK docs: `https://tldraw.dev/sdk-features/ui-components`, `https://tldraw.dev/sdk-features/events`, `https://tldraw.dev/sdk-features/editor`, `https://tldraw.dev/docs/user-interface` — HIGH confidence
- Rete.js docs: `https://retejs.org/docs/concepts/editor/`, `https://retejs.org/docs/concepts/plugin-system/`, `https://retejs.org/docs/api/rete-area-plugin/`, `https://retejs.org/docs/guides/minimap/`, `https://retejs.org/docs/guides/readonly/` — HIGH confidence
- JointJS docs: `https://docs.jointjs.com/learn/features/property-editor-and-viewer/`, `https://docs.jointjs.com/api/ui/Navigator/`, `https://docs.jointjs.com/api/dia/CommandManager/` — HIGH confidence
- yFiles for HTML docs: `https://docs.yworks.com/yfiles-html/dguide/features/index.html`, `https://docs.yworks.com/yfiles-html/dguide/view/view_overview.html`, `https://docs.yworks.com/yfiles-html/dguide/customizing_concepts/customizing_concepts_commands.html`, `https://docs.yworks.com/yfiles-html/dguide/customizing_concepts/customizing_concepts_events.html` — HIGH confidence
