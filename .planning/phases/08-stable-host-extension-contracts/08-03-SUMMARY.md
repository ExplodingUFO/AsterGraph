---
phase: 08-stable-host-extension-contracts
plan: 03
subsystem: docs-and-proof
completed: 2026-04-03
---

# Phase 08 Plan 03 Summary

Closed the public proof ring for Phase 8:

- Host-facing docs now describe the stable extension contexts and the full-shell opt-out story.
- `HostSample` prints a full-shell opt-out marker.
- `PackageSmoke` prints `SURFACE_FULLSHELL_OPTOUT_OK`.
- The docs now consistently say:
  - prefer `GraphContextMenuAugmentationContext`
  - prefer `NodePresentationContext`
  - keep the raw MVVM seam roots only as compatibility bridges
