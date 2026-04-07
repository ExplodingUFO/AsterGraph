# Phase 15: Capability And Descriptor Contract Normalization - Research

## Summary

Phase 14 converged runtime ownership, but the public control plane still leaks MVVM shape in two critical places:

- hosts still discover optional behavior mostly through boolean flags, compatibility properties, or composition-time knowledge instead of explicit runtime descriptors
- editor-layer command and menu flows still depend on `ICommand`, `RelayCommand`, `NodeTemplateViewModel`, and other VM-facing types even when the underlying runtime state is now kernel-first

The safest Phase 15 strategy is therefore descriptor-before-adapter-thinning:

1. add explicit immutable capability/service discovery on the runtime path
2. make command and menu contracts canonical around stable IDs, payloads, and descriptor snapshots
3. keep compatibility adapters alive long enough to prove the new descriptor path without dragging Avalonia cleanup or final migration lock into the same phase

## Why this split

- It satisfies `CAP-01` and `CAP-02` directly without over-scoping into Phase 16 UI adapter cleanup.
- It builds on the new kernel-first runtime center of gravity instead of trying to redesign contracts while compatibility paths still own behavior.
- It creates the exact seam later plugin and automation work will need: explicit discovery plus stable command/menu descriptors.

## Primary technical risks

- duplicated discovery models if descriptor snapshots and the old boolean snapshot drift apart
- incomplete command normalization if canonical APIs still rely on `MenuItemDescriptor.Command` or VM-only template objects under the hood
- migration churn if compatibility augmentor/facade paths lose continuity before Phase 17 owns the broader proof story
- accidental Phase 16 bleed if Avalonia-specific rendering/input concerns get mixed into editor-layer contract redesign

## Recommended planning shape

- `15-01`: add explicit capability/service descriptor discovery to the canonical runtime path and inspection/proof surfaces
- `15-02`: normalize command and menu contracts around stable descriptors and compatibility adapters instead of `ICommand`/VM object shape
- `15-03`: lock descriptor-path proof in tests, sample output, package smoke, and compatibility annotations/docs
