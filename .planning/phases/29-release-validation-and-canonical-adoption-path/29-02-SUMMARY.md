## Summary

Plan 29-02 made `docs/quick-start.md` the canonical short adoption guide for v1.5. The public docs now point to one three-way host decision path instead of carrying competing route trees.

## Changes

- promoted [docs/quick-start.md](/F:/CodeProjects/DotnetCore/avalonia-node-map/docs/quick-start.md) to the canonical adoption entry with explicit runtime-only, shipped-UI, and retained-migration routes
- updated [README.md](/F:/CodeProjects/DotnetCore/avalonia-node-map/README.md) so the initialization story points back to the same three-way route guide
- updated [docs/host-integration.md](/F:/CodeProjects/DotnetCore/avalonia-node-map/docs/host-integration.md) to expand the Quick Start routes instead of defining a second canonical tree
- clarified that standalone Avalonia surface composition stays inside the `Create(...)` family and is not a fourth canonical entry path

## Verification

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release`
