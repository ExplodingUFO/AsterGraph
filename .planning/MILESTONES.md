# Milestones

## v1.5 Runtime Boundary Cleanup and Quality Gates (Shipped: 2026-04-14)

**Delivered:** AsterGraph now ships a release-grade validation lane and one short canonical host-adoption path on top of the already-shipped kernel-first SDK boundary, so package proof, coverage, package validation, CI, and public host guidance all point at the same runtime-first contract story.

**Phases completed:** 26-29 (12 plans total)

**Archive:** [ROADMAP](./milestones/v1.5-ROADMAP.md) | [REQUIREMENTS](./milestones/v1.5-REQUIREMENTS.md)

**Key accomplishments:**

- Canonicalized the remaining compatible-target runtime boundary so retained MVVM shims are explicitly compatibility-only and no longer leak into the internal session/kernel contract.
- Added the repo-quality baseline for v1.5 with tracked `.editorconfig`, centralized package versions, deterministic restore sources, one shared `eng/ci.ps1`, and matrix CI over `net8.0` and `net9.0`.
- Aligned the live proof surface around `PackageSmoke`, `ScaleSmoke`, and split core-vs-demo regression lanes so docs, solution membership, and automation describe the same verification surface.
- Shipped `eng/ci.ps1 -Lane release` to pack the four publishable packages, run `PackageSmoke`, run `ScaleSmoke`, collect checked-in coverage, and enforce SDK package validation from the same repo-local command path used by CI.
- Made `docs/quick-start.md` the canonical three-way adoption guide for runtime-only, shipped-UI, and retained-migration hosts, then synchronized README and Host Integration to the same route.

**Stats:**

- 91 files modified
- 4,165 insertions and 342 deletions across code, docs, tests, CI, and planning artifacts
- 4 phases and 12 plans
- Timeline: 2026-04-14 to 2026-04-14

**Git range:** `docs: define milestone v1.5 requirements` → `docs(29): sync release lane and milestone state`

**Notes:**

- No separate `v1.5-MILESTONE-AUDIT.md` was present at archive time; the milestone was archived from green phase verification plus complete requirements traceability.

**What's next:** Archive the older v1.4 milestone history cleanly, then start the next milestone from a fresh requirements pass.

---

## v1.3 Demo Showcase (Shipped: 2026-04-08)

**Delivered:** AsterGraph now ships a graph-first, host-menu-first demo showcase that keeps one live session on screen while proving host-owned seams, shared runtime state, and live configuration through compact in-context controls and proof cues.

**Phases completed:** 19-21 (9 plans total)

**Archive:** [ROADMAP](./milestones/v1.3-ROADMAP.md) | [REQUIREMENTS](./milestones/v1.3-REQUIREMENTS.md)

**Key accomplishments:**

- Rebuilt `AsterGraph.Demo` around a graph-first shell led by a host-level menu instead of the old explanation-heavy capability console.
- Consolidated view, behavior, and runtime controls into compact host-menu and drawer sections that all act on one retained `Editor` / `Session`.
- Added proof-focused labels, runtime/proof sections, and README alignment so seam ownership and live configuration are obvious on first read.
- Locked the new story with focused demo-shell tests plus milestone summaries and verification artifacts.

**Stats:**

- 41 files modified
- 3,521 insertions and 507 deletions across code, docs, tests, and planning artifacts
- 3 phases and 9 plans
- Timeline: 2026-04-08 to 2026-04-08

**Git range:** `docs: define milestone v1.3 requirements` → `chore: archive v1.3 milestone`

**What's next:** Turn the deferred plugin-loading and automation-execution requirements into real host-facing features now that the showcase story is clear.

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
