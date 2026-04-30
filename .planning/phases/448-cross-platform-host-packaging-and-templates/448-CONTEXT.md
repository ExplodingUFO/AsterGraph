# Phase 448 Context: Cross-Platform Host Packaging And Templates

Bead: `avalonia-node-map-mqm.5`

Goal: defend the desktop host route across Windows, Linux, and macOS with explicit `net8.0`, `net9.0`, and `net10.0` awareness.

Inputs:
- Phase 445 established rendering and viewport performance proof.
- Phase 446 established interaction and connection contract proof.
- Existing CI already has Windows framework matrix, Linux/macOS validation lanes, release package validation, template smoke, and packed HostSample proof.

Constraints:
- Keep packaging proof in CI and repo-maintainer tests, not in copied host runtime code.
- Do not add compatibility or fallback layers.
- Keep the host route Avalonia-first and package-boundary focused.
- Do not mention external inspiration project names in docs or planning.

Write boundary:
- CI/package/template contract tests.
- Starter/template/quick-start proof handoff docs.
- Phase 448 planning and verification artifacts.
