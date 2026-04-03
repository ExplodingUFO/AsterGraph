# Milestones

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

## v1.2 Kernel Extraction, Capability Contracts, and Plugin Readiness

**Status:** Active  
**Goal:** Move AsterGraph from a `GraphEditorViewModel`-centered architecture toward a true editor kernel with thinner adapters, explicit capability contracts, and a cleaner runway for plugin loading and automation.

**Planned phase span:** 13-18

**Roadmap focus:**
- editor kernel state-owner extraction
- session/facade decoupling
- capability and descriptor contract normalization
- Avalonia adapter boundary cleanup
- migration proof and plugin-readiness validation
