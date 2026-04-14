# Phase 26 Research: Runtime Boundary Canonicalization

**Date:** 2026-04-14
**Phase:** 26-runtime-boundary-canonicalization

## Research Questions

1. What runtime-boundary debt is still real after the v1.2 kernel extraction and the v1.4 plugin/automation milestone?
2. Which public or compatibility APIs still leak MVVM-shaped data into the canonical runtime story?
3. Where should Phase 26 stop so it hardens the boundary without turning into a breaking-change milestone?
4. Which existing tests and proof surfaces already protect the migration window, and where do new checks need to land?

## Findings

### 1. The canonical runtime state owner is already the kernel; the main debt is boundary shape, not ownership

The current code no longer uses `GraphEditorViewModel` as the direct runtime state owner for `IGraphEditorSession`.

Evidence:

- `AsterGraphEditorFactory.CreateSession(...)` is the canonical runtime-first entry point.
- `GraphEditorKernel` implements `IGraphEditorSessionHost`.
- `GraphEditorViewModel` exposes an internal `SessionHost`, but that host is an adapter (`GraphEditorViewModelKernelAdapter`) over the kernel rather than the view model itself.
- `GraphEditorSessionTests` and `GraphEditorMigrationCompatibilityTests` already assert that the session host behind retained and factory-created sessions is not `GraphEditorViewModel`.

Implication:

- Phase 26 should not re-open kernel ownership extraction.
- The real target is to make the runtime/query surface more clearly canonical while keeping the retained facade explicitly adapter-backed.

### 2. The most visible canonical-boundary debt is the compatible-target shim

The runtime has already introduced `GraphEditorCompatiblePortTargetSnapshot` and the canonical query `IGraphEditorQueries.GetCompatiblePortTargets(...)`, but the compatibility shim still remains in both public and internal surfaces:

- `IGraphEditorQueries.GetCompatibleTargets(...)` still returns `CompatiblePortTarget` and is public, although obsolete.
- `CompatiblePortTarget` still exposes `NodeViewModel` / `PortViewModel`.
- `IGraphEditorSessionHost` still includes both `GetCompatiblePortTargets(...)` and `GetCompatibleTargets(...)`.
- `GraphEditorKernel` and `GraphEditorKernelCompatibilityQueries` still generate both shapes.
- `GraphEditorViewModel.CanvasSurface` still projects snapshot targets back into retained node/port instances.
- `GraphContextMenuBuilder` and `IGraphContextMenuHost` still use the legacy compatible-target path.

Implication:

- Phase 26 should treat the compatibility-target story as the main concrete boundary cleanup target.
- The correct direction is to make snapshot/descriptor queries authoritative, then isolate MVVM-shaped projection to retained-facade edges only.

### 3. The migration window is already protected, so the phase can be aggressive internally but should stay staged publicly

The test suite already distinguishes canonical runtime contracts from retained compatibility:

- `GraphEditorSessionTests` asserts that `GetCompatibleTargets(...)` and `CompatiblePortTarget` are compatibility-only shims marked `[Obsolete]`.
- `GraphEditorMigrationCompatibilityTests` explicitly verifies that legacy editors still return retained `NodeViewModel` / `PortViewModel` instances through `GetCompatibleTargets(...)`.
- `GraphEditorProofRingTests` covers adapter-backed session proof and shared canonical command/feature surfaces.

Implication:

- Phase 26 can move internal consumers away from the shim, strengthen compiler-visible warnings, and update docs/tests without removing the legacy API yet.
- Public breakage should remain out of scope for this phase; the shim can stay, but it should stop looking like a canonical runtime primitive.

### 4. The highest-value cleanup is to separate three layers clearly

The code naturally falls into three layers:

1. **Canonical runtime layer**  
   `IGraphEditorSession`, `IGraphEditorQueries`, kernel DTO/snapshot reads, descriptor-based menu building.

2. **Retained compatibility adapter layer**  
   `GraphEditorViewModelKernelAdapter`, `GraphEditorViewModel` facade methods, MVVM projection back into retained node/port objects.

3. **Retained UI/menu consumption layer**  
   `IGraphContextMenuHost`, `GraphContextMenuBuilder`, and any remaining call sites that still assume `CompatiblePortTarget`.

Implication:

- The safest planning cut is:
  - first harden canonical runtime/query contracts and tests,
  - then isolate the retained projection path,
  - then close with docs/deprecation/proof updates.

### 5. Phase 26 should not swallow broader repo-quality or proof-surface cleanup

The roadmap already assigns CI/package/config work to Phase 27 and proof/doc/test-lane alignment to Phases 28-29.

Implication:

- Phase 26 should only touch docs/tests where needed to support runtime-boundary migration guidance and parity proof.
- Wider cleanup like `.editorconfig`, `Directory.Packages.props`, solution membership, or demo-lane separation should stay out of this phase.

## Risks And Guardrails

- Do not remove `GetCompatibleTargets(...)` or `CompatiblePortTarget` outright in this phase; strengthen the migration path first.
- Do not move new canonical runtime consumers onto retained `NodeViewModel` / `PortViewModel` shapes just to reuse old helper code.
- Do not confuse menu-builder cleanup with a larger UI refactor; the target is boundary shape, not Avalonia redesign.
- Do not regress retained/factory parity while tightening internals; migration proof is part of the requirement.
- Do not let Phase 26 expand into repo-wide quality gates or proof-surface cleanup that belongs to later phases.

## Recommended Planning Posture

### Wave 1: Canonical query and host-surface tightening

Make snapshot-based compatible-target reads and related canonical contracts the clear primary path. Update interface/tests/docs so runtime-first hosts see one authoritative story.

### Wave 2: Retained-facade isolation

Move remaining MVVM-shaped projection logic to explicit compatibility edges and reduce internal dependence on `CompatiblePortTarget` outside retained-only APIs.

### Wave 3: Migration proof and staged-exit guidance

Strengthen obsolete guidance, parity tests, and documentation so hosts know the supported migration route and future removal posture.

## Recommendation

Plan Phase 26 around one narrow principle: MVVM-shaped compatibility data may remain available for retained hosts, but it should stop shaping the canonical runtime mental model.

That keeps the phase disciplined:

- the kernel/session-first story becomes clearer without reopening ownership extraction
- retained hosts stay supported through explicit adapter boundaries
- later phases can add quality gates and proof-surface cleanup on top of a cleaner runtime contract

---

*Research complete: 2026-04-14*
