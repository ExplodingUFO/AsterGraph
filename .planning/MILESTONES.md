# Milestones

## v1.2 Kernel Extraction, Capability Contracts, and Plugin Readiness (Shipped: 2026-04-08)

**Delivered:** AsterGraph now ships a kernel-first editor runtime with descriptor-driven host contracts, thinner Avalonia adapters, and explicit migration/readiness proof for later plugin and automation work.

**Phases completed:** 13-18 (18 plans total)

**Archive:** [ROADMAP](./milestones/v1.2-ROADMAP.md) | [REQUIREMENTS](./milestones/v1.2-REQUIREMENTS.md)

**Key accomplishments:**
- Extracted `GraphEditorKernel` and made `CreateSession(...)` kernel-first instead of `GraphEditorViewModel`-owned.
- Converted retained `GraphEditorViewModel.Session` into an adapter-backed compatibility path over the same shared runtime boundary.
- Normalized capability, command, and menu discovery around stable descriptors and snapshot reads rather than MVVM object shape.
- Consolidated Avalonia shortcut, stock-menu, clipboard, and host-context wiring behind shared adapters and binders.
- Locked migration posture and plugin/automation readiness into focused tests plus `HostSample`, `PackageSmoke`, and `ScaleSmoke` markers.

**Stats:**
- 104 files modified
- 8,970 insertions and 1,505 deletions across code, docs, and planning artifacts
- 6 phases and 18 plans; milestone work was tracked at plan-summary granularity rather than separate task ledgers
- Timeline: 2026-04-04 to 2026-04-08

**Git range:** `feat(13): extract kernel-first runtime session path` → `docs(state): trim stale phase 18 concern`

**What's next:** Define the next milestone around actual plugin loading, automation APIs, and/or diagnostics tooling on top of the shipped kernel-first boundary.

---

## v1.1 Host Boundary, Native Integration, and Scaling

**Status:** Completed
**Goal:** Harden the shipped SDK boundary so custom hosts depend less on concrete MVVM types, the Avalonia layer cooperates better with native desktop host behavior, and larger graphs remain responsive under common interaction paths.

**Phase span:** 07-12

**Delivered:**

- Runtime/session host boundary completion with retained compatibility shims
- Stable host extension contexts for menus and node presentation
- More native/cooperative Avalonia shell and canvas integration behavior
- Canvas, inspector, history, and dirty-tracking hot-path reductions
- HostSample, PackageSmoke, proof-ring regressions, and repeatable large-graph smoke validation

## v1.0 Foundation Milestone

**Status:** Completed
**Scope:** Package boundary, runtime/session contracts, embeddable Avalonia surfaces, replaceable presentation, diagnostics, demo/sample proof, and documentation hardening.

**Delivered:**

- Public four-package SDK boundary with documented host integration paths
- Runtime/session contract surface in `AsterGraph.Editor`
- Full shell plus standalone Avalonia surfaces in `AsterGraph.Avalonia`
- Replaceable presenter seams for nodes, menus, inspector, and mini map
- Diagnostics/session inspection baseline
- HostSample, PackageSmoke, package validation, and follow-up XML documentation cleanup

**Phase span:** 01-06
