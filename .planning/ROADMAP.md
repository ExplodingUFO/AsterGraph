# Roadmap: AsterGraph

## Milestones

- ✅ **v1.3 Demo Showcase** — Phases 19-21, completed 2026-04-08
- ✅ **[v1.2 Kernel Extraction, Capability Contracts, and Plugin Readiness](./milestones/v1.2-ROADMAP.md)** — Phases 13-18, shipped 2026-04-08
- ✅ **v1.1 Host Boundary, Native Integration, and Scaling** — Phases 07-12, shipped before milestone archive split
- ✅ **v1.0 Foundation Milestone** — Phases 01-06, shipped before milestone archive split

## Overview

The kernel-first runtime, descriptor contracts, and readiness proof shipped in v1.2. The next problem is not missing architectural seams. It is that `AsterGraph.Demo` still presents those seams through a capability-console layout that buries the graph and fragments host controls across explanation-heavy side panels.

This milestone turns the demo into a graph-first SDK showcase: the user should first see the live node graph and a host-level menu, then use compact controls to adjust shell, behavior, and runtime-facing capabilities on the same live session.

## Milestone

**Milestone:** v1.3 Demo Showcase
**Goal:** Rebuild `AsterGraph.Demo` into a graph-first, host-menu-first showcase that makes replaceable seams and live runtime behavior obvious without relying on a dense three-column information wall.

## Phases

- [x] **Phase 19: Graph-First Demo Shell** - Replace the current explanation-heavy layout with a graph-first shell whose first read is the live graph plus a host-level menu.
- [x] **Phase 20: Host Menu Control Consolidation** - Move shell, behavior, and runtime-facing demo controls into a compact host-level menu or drawer flow over the same editor session. (completed 2026-04-08)
- [x] **Phase 21: In-Context Proof And Narrative Polish** - Replace long explanation panels with compact proof cues, live configuration summaries, and updated demo/docs messaging. (completed 2026-04-08)

## Phase Details

### Phase 19: Graph-First Demo Shell
**Goal**: Replace the current three-column capability-console shell with a graph-first showcase layout led by a host-level menu.
**Depends on**: v1.2 shipped baseline
**Requirements**: SHOW-01, SHOW-02
**Success Criteria**:
1. Opening the demo shows a live node graph and a host-level menu without large explanation panels taking first-screen priority.
2. Common desktop window sizes keep the graph as the primary visual surface while secondary controls remain compact or on-demand.
3. The shell restructure keeps the existing live editor session model intact rather than introducing scene switching or session rebuilds.
**Plans**: 3 plans
**UI hint**: yes

### Phase 20: Host Menu Control Consolidation
**Goal**: Consolidate view, behavior, and runtime-facing demo controls into compact host-level menu groups that act on the same live graph session.
**Depends on**: Phase 19
**Requirements**: CTRL-01, CTRL-02, CTRL-03
**Success Criteria**:
1. Shell and chrome visibility controls are grouped under a host-level menu and apply live to the current demo shell.
2. Editing-behavior controls are grouped compactly and change the current graph experience immediately.
3. Runtime-facing controls or readouts are available from the same host menu structure without switching scenes or rebuilding the session.
**Plans**: 3 plans
**UI hint**: yes

### Phase 21: In-Context Proof And Narrative Polish
**Goal**: Replace heavy explanation panels with compact in-context proof, live configuration summaries, and a clearer SDK narrative.
**Depends on**: Phase 20
**Requirements**: PROOF-01, PROOF-02
**Success Criteria**:
1. Users can tell which adjustments are host-owned seams versus shared editor/runtime state through compact in-context cues.
2. The current live showcase configuration and key runtime signals are visible without a diagnostics-heavy side layout.
3. Demo-facing docs or proof surfaces align with the new graph-first showcase story rather than the old capability-console narrative.
**Plans**: 3 plans
**UI hint**: yes

## Progress

| Phase | Requirements | Status |
|-------|--------------|--------|
| 19. Graph-First Demo Shell | SHOW-01, SHOW-02 | Completed |
| 20. Host Menu Control Consolidation | CTRL-01, CTRL-02, CTRL-03 | Completed |
| 21. In-Context Proof And Narrative Polish | PROOF-01, PROOF-02 | Completed |

## Next Action

**v1.3 Demo Showcase** phase work is complete. The next workflow step is to complete/archive the milestone and choose the next roadmap target.
