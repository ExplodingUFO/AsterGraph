# AsterGraph Versioning

## Public Package Version

The consumer-facing version for AsterGraph is the NuGet package version on the four published packages:

- `AsterGraph.Abstractions`
- `AsterGraph.Core`
- `AsterGraph.Editor`
- `AsterGraph.Avalonia`

Current public beta baseline:

- package version: `0.11.0-beta`
- matching public prerelease tag for this package line: `v0.11.0-beta`
- historical legacy repository milestone tag series: `v1.x`-style pre-launch checkpoints

When a host asks "which version of AsterGraph should I install?", this package version is the authoritative answer.

## Stabilization Boundary

The stabilization support matrix freezes the consumer-facing boundary that leads into `v1.0.0`:

- four published SDK packages only
- `net8.0` and `net9.0` as the supported published frameworks
- Avalonia as the supported hosted adapter
- `WPF` as validation-only and partial-fallback per the adapter matrix; do not read it as a parity or alignment promise
- retained MVVM as migration-only

`v1.0.0` should be read as promotion of that same defended boundary to stable, not as a widening of package, framework, or adapter support.

## Repository Tags And Releases

The repository now publishes public prerelease tags that match the installable package version, for example `v0.11.0-beta`.

The repository also still carries historical tags from that pre-launch workflow, such as `v1.x`-style checkpoint tags.

Those legacy `v1.x`-style tags came from milestone-style repository checkpoints during the pre-launch workflow. They are useful for maintainer history, but they are **not** the package version that consumers install from NuGet.

Going forward, the public release convention is:

- public package releases use tags that match the published package version, for example `v0.11.0-beta`
- GitHub prereleases should use the same version number as the published packages
- milestone-style local planning versions can continue privately, but they should not be presented as the consumer package version
- local planning-only milestone labels are private maintainer bookkeeping, not release identifiers and not the current public tag

## Current Mapping

| Public concept | Current value | How to read it |
| --- | --- | --- |
| installable package version | `0.11.0-beta` | the version consumers install from nuget.org |
| matching public prerelease tag for this package line | `v0.11.0-beta` | the GitHub prerelease tag that must match the installable package version |
| historical public repo milestone tag series | `v1.x`-style checkpoint tags | a pre-launch checkpoint pattern kept for repo history only |

## Practical Rule

If you are:

- adopting the SDK: follow the NuGet package version
- reading release notes: expect public prerelease tags to match the NuGet package version
- maintaining the repo: treat old `v1.x`-style tags as historical milestone markers, not as the current public package line
- reading local planning notes: treat local planning-only milestone labels as private scheduling markers, not release identifiers

## Release Note Header Rule

The first lines of a public prerelease note should always expose:

1. the installable package version
2. the matching public tag
3. an optional historical milestone reference only when older repo history needs explaining

The public prerelease workflow now generates and validates that header automatically. Maintainers should not hand-edit the first block of the GitHub prerelease body unless a legacy historical note must be added for context.

Example:

- package version: `0.11.0-beta`
- public tag: `v0.11.0-beta`
- historical repo checkpoint reference: `legacy v1.x-style milestone checkpoint` (not installable)

The generated prerelease body also carries the public proof summary from the release lane so external adopters can see the same installation, compatibility, scale, and coverage signals that gate publication.

## Related Docs

- [Quick Start](./quick-start.md)
- [Project Status](./project-status.md)
- [Stabilization Support Matrix](./stabilization-support-matrix.md)
- [Alpha Status](./alpha-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
