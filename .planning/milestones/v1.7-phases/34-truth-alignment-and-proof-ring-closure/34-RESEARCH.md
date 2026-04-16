---
phase: 34-truth-alignment-and-proof-ring-closure
researched: 2026-04-16
status: complete
---

# Phase 34 Research

## Focus

Phase 34 needs to make the repo tell one truthful story before any further hardening work:

1. align README, host docs, planning artifacts, and codebase maps to the current v1.7 baseline
2. remove capability and support-boundary contradictions
3. make the proof ring list only real, discoverable entry points

## Evidence Collected

### 1. Consumer-facing docs still contradict live capability and support claims

- `README.md` lists `Ctrl+Z` / `Ctrl+Y` undo and redo graph edits as a current capability.
- The same file still lists `undo/redo stack` under current non-goals.
- `README.md`, `docs/quick-start.md`, and `docs/host-integration.md` already point to `eng/ci.ps1 -Lane release` as the preferred release gate, but the surrounding narrative still mixes older proof wording with the newer lane split.

### 2. Proof-ring discoverability is incomplete

- `tools/AsterGraph.PackageSmoke` and `tools/AsterGraph.ScaleSmoke` both exist and are already part of the maintained proof surface.
- `tools/AsterGraph.HostSample` currently exists only as an empty directory with build leftovers (`bin/`, `obj/`); the actual project and program were deleted in commit `8ecfaa8` (`refactor: drop host sample and narrow package smoke`).
- `avalonia-node-map.sln` includes `AsterGraph.PackageSmoke` and `AsterGraph.ScaleSmoke`, but not `AsterGraph.HostSample`.
- Older milestone archives and retrospective notes still refer to `HostSample` as part of the proof ring, so the current live repo no longer matches some historical framing.

### 3. Several codebase maps are stale against the live planning state

- `.planning/codebase/CONCERNS.md`, `.planning/codebase/STRUCTURE.md`, `.planning/codebase/TESTING.md`, and `.planning/codebase/ARCHITECTURE.md` still describe the repo as if v1.5/Phase 29 were current.
- Those files also describe the proof surface and support boundaries in ways that no longer match the live v1.7 milestone framing.
- This is now the highest operational drift because contributors can read current code and still get routed by stale planning snapshots.

### 4. The release gate baseline already exists and should be treated as the official proof entry point

- `eng/ci.ps1 -Lane release` already restores, packs publishable packages, runs `PackageSmoke`, runs `ScaleSmoke`, collects coverage, and executes SDK-integrated package validation.
- `.github/workflows/ci.yml` already runs `eng/ci.ps1 -Lane all` on a `net8.0` / `net9.0` matrix and `eng/ci.ps1 -Lane release` after that matrix passes.
- Phase 34 should not invent a second verification story; it should align docs and proof discoverability to the existing scripted gate.

## Conclusions

### Conclusion A

Phase 34 should remove contradictions first, not add more narrative. The repo already ships more capability than some docs admit, so the immediate need is evidence-aligned cleanup.

### Conclusion B

The proof ring should be defined around:

- `eng/ci.ps1 -Lane release` as the official verification entry point
- `eng/ci.ps1 -Lane maintenance` as the hotspot refactor gate
- focused regression suites
- `PackageSmoke`
- `ScaleSmoke`
- one real minimal consumer host sample

That means the missing `HostSample` cannot remain an empty directory or historical footnote.

### Conclusion C

Phase 34 should restore a narrow `HostSample`, not the older giant proof program that was deleted in April. The current milestone needs a minimal consumer-facing host path, while the broader proof burden already lives in `PackageSmoke`, `ScaleSmoke`, and tests.

## Phase 34 Shape

Recommended three-plan execution:

1. align live README and host docs to the current milestone, proof ring, and support boundaries
2. refresh the stale codebase maps and planning state references
3. restore a minimal `AsterGraph.HostSample`, add it to solution/discoverability paths, and point docs at it as the canonical minimal host sample
