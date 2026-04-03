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

**Status:** Active  
**Goal:** Harden the shipped SDK boundary so custom hosts depend less on concrete MVVM types, the Avalonia layer cooperates better with native desktop host behavior, and larger graphs remain responsive under common interaction paths.

**Planned phase span:** 07-12

**Roadmap focus:**
- runtime host boundary completion
- stable host extension contracts
- native Avalonia host integration
- canvas and state scaling
- proof-ring validation for host behavior and larger graphs
