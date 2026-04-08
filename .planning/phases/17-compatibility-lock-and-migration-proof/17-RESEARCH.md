# Phase 17: Compatibility Lock And Migration Proof - Research

## Summary

Phase 16 proved that the canonical runtime and Avalonia adapter boundaries are in place, but the remaining milestone risk is no longer "does the architecture exist?" It is "will the retained compatibility path drift while the canonical path becomes clearer?"

The strongest Phase 17 strategy is therefore migration-proof tightening rather than another deep refactor:

1. extend focused parity regressions so legacy constructor/view paths, factory/facade paths, and canonical runtime/session paths are compared deliberately instead of incidentally
2. align public guidance and API remarks so the canonical route is explicit and the retained route is explicitly called a migration/compatibility path
3. lock that same story into HostSample and PackageSmoke with visible Phase 17 markers

## Why this split

- It satisfies `MIG-01` by preserving and clarifying the staged migration window instead of destabilizing it.
- It satisfies `MIG-02` by using the repo's existing proof ring pattern: tests for correctness, HostSample for human-readable host proof, and PackageSmoke for machine-checkable markers.
- It avoids reopening Phase 14-16 architectural work unless a tiny additive change is needed to make proof and documentation stable.

## Primary technical risks

- behavior drift between legacy constructor/direct-view paths and factory/session paths going unnoticed because current parity assertions are spread across many tests
- host-facing docs, XML remarks, and proof outputs telling slightly different canonical-vs-compatibility stories
- overreaching into removal/deprecation work and turning a proof phase into a breaking-change phase
- sample/smoke proof markers proving different things from what focused regressions actually guard

## Recommended planning shape

- `17-01`: strengthen migration parity regressions and proof-ring assertions around retained-vs-canonical behavior
- `17-02`: align XML docs, package READMEs, quick start, and host integration guidance around one canonical migration story
- `17-03`: lock that migration story into HostSample, PackageSmoke, and final phase summary/state artifacts

## Recommended scope controls

- treat the known history/save baseline failures as pre-existing unless Phase 17 changes make them relevant
- avoid new public runtime/editor features unless a small additive seam is required for proof clarity
- do not obsolete `GraphEditorViewModel` or `GraphEditorView` wholesale in this phase; use wording and proof first
