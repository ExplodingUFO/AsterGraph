# Phase 23 Research: Runtime Plugin Integration And Inspection

**Date:** 2026-04-08
**Phase:** 23-runtime-plugin-integration-and-inspection

## Research Questions

1. Which published plugin contribution contracts should Phase 23 make live, and which ones should remain deferred?
2. What canonical inspection surface best satisfies `PLUG-03` without turning diagnostics history into the only source of truth?
3. Where should composition happen so `CreateSession(...)` and `Create(...)` stay aligned and host-owned seams keep their compatibility posture?
4. What loader guidance from Phase 22 remains relevant, and what should explicitly stay unchanged in this phase?

## Findings

### 1. Phase 22 already published enough explicit contribution types to make Phase 23 useful

`GraphEditorPluginBuilder` already exposes these additive contracts:

- node definition providers
- context-menu augmentors
- node presentation providers
- localization providers

Implication:

- Phase 23 can deliver real runtime integration by wiring these exact contribution types into the canonical composition path
- this phase does not need to invent a wider arbitrary service-container or dependency-injection plugin model

### 2. Diagnostics history is necessary evidence, but not a sufficient inspection contract

The current baseline gives hosts:

- `integration.plugin-loader` in feature descriptors
- `plugin.load.succeeded` / `plugin.load.failed` diagnostics in recent history

That is good recoverable evidence, but it is not enough for `PLUG-03` because:

- diagnostics are append-only history, not a stable current-state query
- failures and successes are not grouped per registration in a canonical DTO
- hosts would have to infer plugin state from messages instead of reading explicit structure

Implication:

- Phase 23 should add a stable plugin-load snapshot/query surface and mirror it into `GraphEditorInspectionSnapshot`
- diagnostics stay as supporting evidence and failure telemetry, not the only inspection story

### 3. `GraphEditorSession` is already the right canonical runtime boundary for both inspection and menu composition

Current code places these responsibilities in `GraphEditorSession`:

- feature descriptors
- framework-neutral context-menu descriptor generation
- inspection snapshot capture
- recent diagnostics projection

Implication:

- plugin inspection belongs on `IGraphEditorQueries` and `GraphEditorInspectionSnapshot`
- plugin menu augmentation should be applied in `BuildContextMenuDescriptors(...)`, not only in retained `GraphEditorViewModel.BuildContextMenu(...)`

### 4. Retained compatibility should inherit plugin composition, not recreate it

`GraphEditorViewModel` currently:

- starts retained menus from `Session.Queries.BuildContextMenuDescriptors(...)`
- applies a singular host `ContextMenuAugmentor`
- applies singular host `INodePresentationProvider` and `IGraphLocalizationProvider`

Implication:

- factory composition should build composite plugin-aware providers before constructing the retained facade
- host-supplied providers should remain the last override stage so existing host behavior does not regress
- Phase 23 should avoid a second retained-only plugin application path

### 5. The loader result needs a stable per-registration report, not only aggregate descriptors

`AsterGraphPluginLoader.Load(...)` currently returns:

- aggregate successful descriptors
- one aggregate contribution set
- diagnostics
- load contexts

This is enough for Phase 22 load/no-load proof, but not for Phase 23 inspection because the host still cannot ask:

- which registration produced which descriptor
- which registration failed
- what contribution shape each loaded plugin exposed

Implication:

- Phase 23 should refine the internal load result into per-registration report data that can back a public inspection DTO
- the public DTO should expose registration source, resolved descriptor if any, success/failure outcome, and contribution summary

### 6. Catalog mutation would make host ownership unclear

`INodeCatalog` is host-supplied and Phase 22 treated plugin composition as additive over the canonical factory path.

Implication:

- plugin node-definition contribution should be represented through a composed catalog or wrapper rather than mutating the host-provided catalog instance in place
- this keeps host ownership and plugin contribution boundaries legible in tests and inspection

### 7. Phase 22 loader guidance still stands; Phase 23 should build on it rather than reopen it

Phase 22 already locked these loader decisions with code and verification:

- custom `AssemblyLoadContext` plus `AssemblyDependencyResolver`
- shared `AsterGraph.*` contracts in the default load context
- recoverable load failures instead of routine host-crashing exceptions

Implication:

- Phase 23 should keep loading strategy unchanged and focus on contribution wiring plus inspection contracts
- unloadability, trust, and marketplace behavior remain out of scope

## Recommended Planning Posture

### Wave 1: Canonical plugin inspection contracts

Add a stable plugin-load snapshot/query surface and extend `GraphEditorInspectionSnapshot` so current plugin state, contribution shape, and failures become readable without scraping diagnostics.

### Wave 2: Live contribution composition

Compose plugin node definitions, context-menu augmentors, localization providers, and node-presentation providers through the canonical factory/session path, while keeping host-supplied providers as the final override layer.

### Wave 3: Focused parity proof and phase closeout

Lock retained/runtime parity, plugin inspection behavior, and recoverable failure behavior with focused tests, then update planning state to route the next step to Phase 24.

## Risks And Guardrails

- Do not let Phase 23 widen into arbitrary plugin-owned service injection or command registration.
- Do not leave plugin inspection as diagnostics-text scraping under a different name.
- Do not integrate plugin menu augmentation only in retained `GraphEditorViewModel`; the canonical session boundary must own it first.
- Do not mutate the host-supplied catalog instance directly.
- Do not expand into broader samples, smoke tools, or docs beyond focused proof needed to close this phase.

## External References

- Microsoft Learn: [Create a .NET application with plugins](https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support)
- Microsoft Learn: [About System.Runtime.Loader.AssemblyLoadContext](https://learn.microsoft.com/en-us/dotnet/core/dependency-loading/understanding-assemblyloadcontext)
- Microsoft Learn: [AssemblyDependencyResolver Class](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.loader.assemblydependencyresolver?view=net-9.0)

---

*Research complete: 2026-04-08*
