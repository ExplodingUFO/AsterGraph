# Milestones

## v1.9 Public Launch Gate and CI Stabilization (Shipped: 2026-04-16)

**Delivered:** AsterGraph closed the last public-opening operational gaps by making GitHub-hosted CI match the local proof story, stabilizing the prerelease workflow and cache behavior, adding an explicit packed `.NET 10` consumer proof, and publishing one short launch checklist tied to the real remaining repo-opening steps.

**Phases completed:** 42-44 (9 plans total)

**Archive:** [ROADMAP](./milestones/v1.9-ROADMAP.md) | [REQUIREMENTS](./milestones/v1.9-REQUIREMENTS.md) | [AUDIT](./milestones/v1.9-MILESTONE-AUDIT.md)

**Key accomplishments:**

- Removed the last hidden local-output assumptions from plugin proof tests, then aligned path-based contract assertions with canonical filesystem behavior so hosted Linux CI matches the local proof surface.
- Moved workflow restore/cache behavior onto a workspace-local `NUGET_PACKAGES` path and repaired `release.yml` so validation jobs schedule correctly on manual dispatch and tag-driven release entry.
- Added an explicit packed `.NET 10` HostSample proof into the release lane, alongside the existing `HostSample`, `PackageSmoke`, `ScaleSmoke`, and coverage outputs.
- Published a short public-launch checklist and tightened README/alpha-status/quick-start so docs only describe the real remaining launch blockers while keeping `HostSample` versus `Demo` roles explicit.

**Stats:**

- 41 files modified
- 842 insertions and 191 deletions across code, tests, workflows, docs, and planning artifacts
- 3 phases and 9 plans
- Timeline: 2026-04-16 to 2026-04-16

**Git range:** `docs: start milestone v1.9 public launch gate and ci stabilization` → `docs: audit v1.9 milestone`

**Notes:**

- `v1.9-MILESTONE-AUDIT.md` passed with all 10 milestone requirements satisfied.
- Hosted verification now exists for both branch CI (`24506891943`) and manual prerelease workflow dispatch (`24507210737`).
- Remaining non-critical debt at close: GitHub-hosted runs emit Node.js 20 deprecation warnings for current action pins.

**What's next:** Start the next milestone from a fresh requirements pass instead of carrying the public-launch closeout framing forward by default.

---

## v1.8 Public Alpha Readiness and Canonical Demo (Shipped: 2026-04-16)

**Delivered:** AsterGraph turned the already-shipped SDK baseline into a public-alpha-ready repo by aligning package and doc language around one alpha contract, adding the missing open-source governance files, moving the showcase host onto the canonical composition path, publishing bilingual guides plus localization proof, and wiring public CI/release automation around visible proof artifacts.

**Phases completed:** 38-41 (12 plans total)

**Archive:** [ROADMAP](./milestones/v1.8-ROADMAP.md) | [REQUIREMENTS](./milestones/v1.8-REQUIREMENTS.md) | [AUDIT](./milestones/v1.8-MILESTONE-AUDIT.md)

**Key accomplishments:**

- Realigned package metadata, README, planning artifacts, and public docs around the `0.2.0-alpha.1` public-alpha story, then added `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`, issue templates, a PR template, and `global.json`.
- Moved `AsterGraph.Demo` onto the canonical factory/session/view-factory route and made plugin trust, automation execution, standalone surfaces, presenter replacement, and `HostSample` positioning visible in the shipped showcase.
- Published paired English and `zh-CN` guides, added a Chinese root README, and proved the host/runtime localization seam through a real demo language toggle plus `AsterGraph.Demo.Tests`.
- Extended CI and release automation with concurrency, restore caching, Linux validation, uploaded proof artifacts, and tag-driven prerelease workflows that preserve verification-only pull requests.

**Stats:**

- 80 files modified
- 3,870 insertions and 346 deletions across code, docs, CI, tests, demo, and planning artifacts
- 4 phases and 12 plans
- Timeline: 2026-04-16 to 2026-04-16

**Git range:** `docs: start milestone v1.8 public alpha readiness and canonical demo` → `docs: audit v1.8 milestone`

**Notes:**

- `v1.8-MILESTONE-AUDIT.md` passed with all 16 milestone requirements satisfied.
- Remaining non-critical debt at close: Linux validation and tag-driven prerelease workflows were reviewed in tracked YAML but not executed against GitHub-hosted runners or live publish secrets from the local Windows workspace.

**What's next:** Start the next milestone from a fresh requirements pass instead of carrying the public-alpha closeout framing forward by default.

---

## v1.7 Consumer Closure / Release Hardening (Shipped: 2026-04-16)

**Delivered:** AsterGraph closed the productization milestone by aligning repo narrative to the real shipped proof surface, turning release validation into an explicit contract/release gate system, publishing a minimal consumer route plus explicit state semantics, and documenting the remaining stability, retirement, and extension-precedence rules without reopening runtime-boundary drift.

**Phases completed:** 34-37 (12 plans total)

**Archive:** [ROADMAP](./milestones/v1.7-ROADMAP.md) | [REQUIREMENTS](./milestones/v1.7-REQUIREMENTS.md) | [AUDIT](./milestones/v1.7-MILESTONE-AUDIT.md)

**Key accomplishments:**

- Realigned README, planning artifacts, and codebase maps around one truthful v1.7 proof story and restored `AsterGraph.HostSample` as the minimal consumer-facing sample.
- Added a focused `contract` lane, explicit CI job split, packed `HostSample` release proof, and a stronger release gate around contract proof, pack, smoke, coverage, and package validation.
- Published a compact consumer route matrix and an explicit history/save/dirty contract tied back to the `contract` lane and `SCALE_HISTORY_CONTRACT_OK`.
- Published stability tiers, compatibility-retirement guidance, extension-precedence rules, and lane ownership so maintainers can continue shrinking retained compatibility safely.

**Stats:**

- 64 files modified
- 3,292 insertions and 128 deletions across docs, CI, planning artifacts, samples, and proof wiring
- 4 phases and 12 plans
- Timeline: 2026-04-16 to 2026-04-16

**Git range:** `docs: start milestone v1.7 consumer closure and release hardening` → `docs: audit v1.7 milestone`

**Notes:**

- `v1.7-MILESTONE-AUDIT.md` passed with all 14 milestone requirements satisfied.
- Remaining non-critical debt at close: Nyquist frontmatter normalization across `34-37` validation files.

**What's next:** Start the next milestone from a fresh requirements pass instead of carrying the v1.7 closeout framing forward by default.

---

## v1.6 Facade Convergence and Proof Guardrails (Shipped: 2026-04-16)

**Delivered:** AsterGraph completed the contraction milestone by archiving stale planning history, locking one explicit retained history/save contract into proof, narrowing the retained facade and downstream hotspots without public API churn, and scoping XML-doc debt to the real remaining project boundary.

**Phases completed:** 30-33 (12 plans total)

**Archive:** [ROADMAP](./milestones/v1.6-ROADMAP.md) | [REQUIREMENTS](./milestones/v1.6-REQUIREMENTS.md) | [AUDIT](./milestones/v1.6-MILESTONE-AUDIT.md)

**Key accomplishments:**

- Reconstructed the missing `v1.4` milestone archive and aligned live planning/docs around one current proof story.
- Added `eng/ci.ps1 -Lane maintenance` as the checked-in hotspot refactor gate over focused editor regressions plus `ScaleSmoke`.
- Replaced the carried `STATE_HISTORY_OK` mismatch with one explicit retained history/save contract backed by focused suites, proof-ring tests, and smoke output.
- Narrowed `GraphEditorViewModel` further by moving retained bootstrap, descriptor, compatibility-menu, and fragment orchestration behind dedicated internal collaborators.
- Split the next `GraphEditorKernel` and `NodeCanvas` hotspots behind dedicated internal helpers without changing public embedding behavior.
- Scoped `CS1591` debt to `AsterGraph.Editor` so the other publishable packages now prove clean package-boundary XML-doc builds.

**Stats:**

- 372 files modified
- 5,305 insertions and 23,155 deletions across code, tests, docs, and planning artifacts
- 4 phases and 12 plans
- Timeline: 2026-04-16 to 2026-04-16

**Git range:** `docs: start milestone v1.6 facade convergence and proof guardrails` → `docs: audit v1.6 milestone`

**Notes:**

- `v1.6-MILESTONE-AUDIT.md` passed with all 11 milestone requirements satisfied.
- Remaining non-critical debt at close: `NU1901` warning noise from `NuGet.Packaging` 7.3.0 and missing Nyquist frontmatter normalization in `33-VALIDATION.md`.

**What's next:** Start the next milestone from a fresh requirements pass rather than carrying v1.6 contraction framing forward by default.

---

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

## v1.4 Plugin Loading and Automation Execution (Shipped: 2026-04-08)

**Delivered:** AsterGraph shipped the first public plugin-loading and automation-execution baseline on the canonical runtime/session boundary, with runtime inspection, typed automation signals, and proof coverage that no longer depended on retained `GraphEditorViewModel`-only host paths.

**Phases completed:** 22-25 (12 plans total)

**Archive:** [ROADMAP](./milestones/v1.4-ROADMAP.md) | [REQUIREMENTS](./milestones/v1.4-REQUIREMENTS.md)

**Key accomplishments:**

- Added public plugin composition and loading through `AsterGraphEditorFactory` / `AsterGraphEditorOptions` instead of editor-internal or Avalonia-internal construction paths.
- Let loaded plugins contribute node definitions, menu augmentation, localization, node presentation, and recoverable diagnostics through the canonical runtime/session boundary while host-provided seams kept final override authority.
- Added a descriptor-first automation runner over canonical command IDs, query snapshots, batching, and typed runtime signals suitable for non-Avalonia or headless consumers.
- Closed the original v1.4 proof ring around plugin loading and automation execution through focused regressions plus `PackageSmoke`, `ScaleSmoke`, and README-backed proof commands.

**Stats:**

- 85 files modified
- 6,392 insertions and 86 deletions across code, docs, tests, and planning artifacts
- 4 phases and 12 plans
- Timeline: 2026-04-08 to 2026-04-08

**Git range:** `docs: define milestone v1.4 requirements` → `feat(proof): close plugin and automation proof ring`

**Notes:**

- This archive was reconstructed retrospectively on 2026-04-16 from the historical requirements snapshot at `7b99800` and roadmap snapshot at `5622eb7`; the archive files preserve that original framing instead of rewriting it into current milestone language.
- The broader pre-`v1.5` evidence range `7b99800..ec8d566` covers 266 files changed, 27,048 insertions, and 8,532 deletions because later plugin trust, discovery, staging, smoke-surface, and refactor follow-up work landed before `v1.5` was initialized.
- That later work is part of the delivered pre-`v1.5` surface, but it was not part of the original four-phase `v1.4` roadmap and is intentionally called out here rather than merged back into the archived snapshots.

**What's next:** Use the reconstructed `v1.4` archive as historical context, but track current proof, refactor, and state-sync work through the live `v1.6` planning artifacts.

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
