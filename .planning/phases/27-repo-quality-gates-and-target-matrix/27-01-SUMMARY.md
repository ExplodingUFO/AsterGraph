## Summary

Plan 27-01 added the repo-root configuration baseline needed for Phase 27. The repo now has a tracked `.editorconfig`, centralized package versions through `Directory.Packages.props`, and a checked-in `NuGet.config` so restore behavior stays deterministic after enabling central package management.

## Changes

- added `.editorconfig` with conservative repo-wide whitespace and C# style defaults
- added `Directory.Packages.props` and moved repeated package versions there
- removed inline `Version=` attributes from the affected `PackageReference` items
- added `NuGet.config` with a repo-local source list so central package management no longer inherits arbitrary user-global package sources
- updated the Phase 27 plan artifact so the deterministic-restore adjustment is recorded in the execution plan

## Verification

- `dotnet build avalonia-node-map.sln --nologo -v minimal`
