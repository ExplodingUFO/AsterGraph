---
phase: 37-maintainability-and-extension-contract-hardening
researched: 2026-04-16
status: complete
---

# Phase 37 Research

## Focus

Phase 37 needs to close the remaining maintainability and extension-contract gap without reopening runtime-boundary code churn:

1. make the maintenance-lane split explicit enough that failures localize to the right surface
2. publish which integration surfaces are stable, which are retained compatibility bridges, and what the retirement story is
3. document actual host-vs-plugin precedence rules from the code, not from assumptions

## Evidence Collected

### 1. Runtime ownership drift is already guarded in code and tests

- `GraphEditorViewModel` is no longer the canonical runtime owner.
- The repo already has focused tests and docs around the session-first boundary.

Conclusion: Phase 37 does not need to reopen structural runtime work. It needs to make the maintainability contract explicit.

### 2. The repo already encodes a real stability split

Current evidence in docs and code:

- canonical/stable:
  - `AsterGraphEditorFactory.CreateSession(...)`
  - `AsterGraphEditorFactory.Create(...)`
  - `IGraphEditorSession`
  - DTO/snapshot queries such as `GetCompatiblePortTargets(...)`
- retained/transitional:
  - `GraphEditorViewModel`
  - `GraphEditorView`
  - `GetCompatibleTargets(...)`
  - `CompatiblePortTarget`

The obsolete attribute on `IGraphEditorQueries.GetCompatibleTargets(...)` already carries a staged-removal message:

- keep during the v1.5 migration window
- later minor releases may add stronger warnings
- future major release may remove it

Gap: that staged retirement story is not yet published in one consumer-facing contract.

### 3. Host-vs-plugin precedence is already defined by implementation

Primary-source code paths show:

- plugin trust policy is host-owned and runs before activation
- blocked plugins contribute nothing
- localization composition runs plugin providers first, then host provider last, so host text wins final override
- node-presentation composition merges plugin states first, then host state last; host can override subtitle/description/status-bar while badges accumulate
- runtime session context menus apply plugin augmentors over stock descriptors
- retained `GraphEditorViewModel.BuildContextMenu(...)` then gives the host `IGraphContextMenuAugmentor` the final menu override point

Gap: these precedence rules are only inferable from code and tests today.

### 4. Lane split is real, but still under-documented as a maintainability contract

The repo now has:

- `all` for framework-matrix build/test
- `contract` for focused consumer/state proof
- `maintenance` for hotspot refactor regression
- `release` for publish validation
- separate demo/sample regression tests

Gap: the lane split is described, but not yet published as a maintainability contract tied to compatibility-hotspot work and extension safety.

## Conclusions

### Conclusion A

Phase 37 should primarily publish explicit stability tiers, retirement guidance, precedence rules, and lane roles instead of forcing more code movement.

### Conclusion B

The right place is a dedicated extension/maintenance contract doc plus short links from README, host docs, the editor package README, and the testing map.

### Conclusion C

Verification should prove both gates still run (`contract` and `maintenance`) and that the new docs point at real code-backed behavior.

## Phase 37 Shape

Recommended three-plan execution:

1. publish stability tiers and compatibility-retirement guidance
2. publish extension-precedence rules from the actual composition code
3. align testing/lane docs so maintainers can map a failure to the correct proof surface
