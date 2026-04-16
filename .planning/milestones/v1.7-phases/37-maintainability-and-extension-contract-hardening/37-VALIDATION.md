---
phase: 37-maintainability-and-extension-contract-hardening
validated: 2026-04-16
status: planned
requirements:
  - MAINT-01
  - MAINT-02
  - EXT-01
  - EXT-02
---

# Phase 37 Validation

## Goals

- prove that compatibility, retained, and canonical runtime surfaces are documented in explicit stability tiers
- prove that compatibility-retirement and extension-precedence rules are now written down and linked from host-facing docs
- prove that maintainers can identify which lane validates which maintenance surface

## Planned Validation Commands

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
rg -n "stability tiers|retirement|future major release|host wins|plugin trust|lane contract|lane maintenance|extension contracts" README.md docs src/AsterGraph.Editor/README.md .planning/codebase/TESTING.md
```

## Expected Evidence

- docs distinguish stable canonical runtime surfaces from retained/transitional compatibility bridges
- docs publish the staged retirement path for compatibility-only shims
- docs publish actual host-vs-plugin precedence rules
- docs map `contract`, `maintenance`, `release`, and demo/sample lanes to their intended maintenance surfaces
