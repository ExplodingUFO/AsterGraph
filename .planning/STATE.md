# State

## Current Position

Phase: 455
Plan: `.planning/ROADMAP.md`
Status: v0.76 active; Phases 451-455 complete; ready to execute Phase 456
Last activity: 2026-04-30 — Completed Phase 455 layout services and alignment tools

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-30)

**Core value:** External hosts can embed AsterGraph as a high-performance, definition-driven desktop node graph library with professional canvas depth, composable interactions, customization seams, designer workbench UX, and proof-backed cross-platform verification.
**Current focus:** Move from v0.75 library-grade proof into v0.76 professional canvas engine and authoring workbench capabilities.

## Active Beads

- Active epic: `avalonia-node-map-y7i` — v0.76 Professional Canvas Engine And Authoring Workbench
- Complete: `avalonia-node-map-y7i.1` — Phase 451: Canvas engine architecture and scale audit
- Complete: `avalonia-node-map-y7i.2` — Phase 452: Virtualized scene index and viewport pipeline
- Complete: `avalonia-node-map-y7i.3` — Phase 453: Professional edge routing and connection geometry
- Complete: `avalonia-node-map-y7i.4` — Phase 454: Groups subgraphs and collapsible containers
- Complete: `avalonia-node-map-y7i.5` — Phase 455: Layout services and alignment tools
- Ready after Phases 454 and 455: `avalonia-node-map-y7i.6` — Phase 456: Designer workbench authoring UX
- Blocked by Phases 455 and 456: `avalonia-node-map-y7i.7` — Phase 457: Extension contracts documentation and release proof

## Notes

- v0.75 is complete: capability audit, rendering/viewport performance, interaction contracts, extension seams, host packaging, professional examples, release proof markers, and clean handoff were closed.
- v0.76 intentionally takes a larger step into professional canvas depth: virtualized scene/indexing, routing, groups/subgraphs, layout services, designer workbench UX, extension contracts, and release proof.
- Do not add runtime rewrites, compatibility layers, fallback behavior, generated runnable Demo code, macro/query/scripting systems, marketplace behavior, sandboxing, or unsupported adapter/platform claims.
- Do not name external inspiration projects or packages in planning artifacts or docs.
- Keep supported SDK contracts in packages, public APIs, ConsumerSample, starter templates, docs, and proof markers.
- Keep performance and graph-size claims tied to measured verification.
- Use beads as the task split/status/handoff spine.
- Use isolated worktrees for parallel implementation when phases split into independent beads.
- Phase 452 and 453 can run in parallel after Phase 451. Phase 455 can start after 452 and 454. Phase 456 waits for 454 and 455. Phase 457 closes the milestone.

---
*Last updated: 2026-04-30 after completing Phase 455*
