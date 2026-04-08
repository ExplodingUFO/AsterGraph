# Requirements: AsterGraph v1.3

**Defined:** 2026-04-08
**Milestone:** v1.3 Demo Showcase
**Core Value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Milestone Requirements

### Showcase Shell

- [ ] **SHOW-01**: User can open the demo and immediately see a live node graph plus a host-level menu without large explanation panels taking first-screen priority
- [ ] **SHOW-02**: User can keep the graph as the primary visual surface while secondary controls open in compact on-demand UI instead of permanent three-column chrome

### Host Controls

- [ ] **CTRL-01**: User can adjust shell and chrome visibility from a host-level menu while staying on the same live editor session
- [ ] **CTRL-02**: User can adjust editing-behavior toggles such as read-only, snapping, guides, and host menu extensions from a host-level menu and see the effect immediately on the current graph
- [ ] **CTRL-03**: User can access runtime-facing demo controls or readouts from the same host-level menu structure without switching to another showcase scene or rebuilding the editor session

### In-Context Proof

- [ ] **PROOF-01**: User can tell which showcase adjustments are host-owned seams versus shared editor/runtime state through compact in-context cues rather than long static explanation cards
- [ ] **PROOF-02**: User can inspect the current live showcase configuration and key runtime signals without depending on a diagnostics-heavy side layout or external documentation

## Future Requirements

### Showcase Enhancements

- **PRESET-01**: User can save and recall named showcase presets for common host integration narratives
- **TOUR-01**: User can step through a guided capability tour that highlights specific seams in sequence

### Deferred Platform Work

- **PLUG-01**: Host can load runtime plugins on top of the readiness descriptors shipped in v1.2
- **AUTO-01**: Host can drive richer automation or macro workflows through canonical runtime descriptors and command IDs

## Out of Scope

| Feature | Reason |
|---------|--------|
| New graph-editing end-user features unrelated to the showcase shell | This milestone is about presentation of existing SDK capabilities, not feature expansion |
| Turning the demo into a fake production product UI | The goal is a persuasive SDK showcase, not a productized workflow editor |
| Scene switching as the primary way to explain capabilities | The milestone should prove changes on one live graph session |
| Runtime plugin loading or macro APIs | Those are valid next-step areas, but they are deferred until after the demo better communicates the current SDK story |

## Traceability

Roadmap mapping will be filled during milestone roadmapping.

**Coverage:**
- milestone requirements: 7 total
- mapped to phases: 0
- unmapped: 7

---
*Requirements defined: 2026-04-08*
