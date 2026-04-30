# State

## Current Position

Phase: 457
Plan: `.planning/ROADMAP.md`
Status: v0.76 complete; ready to plan the next milestone
Last activity: 2026-05-01 — Completed Phase 457 extension contracts, documentation, release proof, and milestone handoff

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-30)

**Core value:** External hosts can embed AsterGraph as a high-performance, definition-driven desktop node graph library with professional canvas depth, composable interactions, customization seams, designer workbench UX, and proof-backed cross-platform verification.
**Current focus:** Move from v0.75 library-grade proof into v0.76 professional canvas engine and authoring workbench capabilities.

## Active Beads

- Completed epic: `avalonia-node-map-y7i` — v0.76 Professional Canvas Engine And Authoring Workbench
- Complete: `avalonia-node-map-y7i.1` — Phase 451: Canvas engine architecture and scale audit
- Complete: `avalonia-node-map-y7i.2` — Phase 452: Virtualized scene index and viewport pipeline
- Complete: `avalonia-node-map-y7i.3` — Phase 453: Professional edge routing and connection geometry
- Complete: `avalonia-node-map-y7i.4` — Phase 454: Groups subgraphs and collapsible containers
- Complete: `avalonia-node-map-y7i.5` — Phase 455: Layout services and alignment tools
- Complete: `avalonia-node-map-y7i.6` — Phase 456: Designer workbench authoring UX
  - Complete: `avalonia-node-map-y7i.6.1` — Navigator and outline projection
  - Complete: `avalonia-node-map-y7i.6.2` — Route group layout recovery affordances
  - Complete: `avalonia-node-map-y7i.6.3` — Designer cookbook proof and docs
- Complete: `avalonia-node-map-y7i.7` — Phase 457: Extension contracts documentation and release proof
  - Complete: `avalonia-node-map-y7i.7.1` — Contract docs and requirement mapping audit
  - Complete: `avalonia-node-map-y7i.7.2` — Release proof and verification gate
  - Complete: `avalonia-node-map-y7i.7.3` — Milestone clean closeout

## Notes

- v0.75 is complete: capability audit, rendering/viewport performance, interaction contracts, extension seams, host packaging, professional examples, release proof markers, and clean handoff were closed.
- v0.76 intentionally takes a larger step into professional canvas depth: virtualized scene/indexing, routing, groups/subgraphs, layout services, designer workbench UX, extension contracts, and release proof.
- Do not add runtime rewrites, compatibility layers, fallback behavior, generated runnable Demo code, macro/query/scripting systems, marketplace behavior, sandboxing, or unsupported adapter/platform claims.
- Do not name external inspiration projects or packages in planning artifacts or docs.
- Keep supported SDK contracts in packages, public APIs, ConsumerSample, starter templates, docs, and proof markers.
- Keep performance and graph-size claims tied to measured verification.
- Use beads as the task split/status/handoff spine.
- Use isolated worktrees for parallel implementation when phases split into independent beads.
- Phase 452 and 453 ran in parallel after Phase 451. Phase 455 started after 452 and 454. Phase 456 waited for 454 and 455. Phase 457 closed the milestone.
- Public API baseline drift found during Phase 457 was source-backed to the Phase 456 navigator/outline query surface and was resolved by updating `eng/public-api-baseline.txt` plus public API inventory classification.

---
*Last updated: 2026-05-01 after completing v0.76*
