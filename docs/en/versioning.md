# AsterGraph Versioning

## Public Package Version

The consumer-facing version for AsterGraph is the NuGet package version on the four published packages:

- `AsterGraph.Abstractions`
- `AsterGraph.Core`
- `AsterGraph.Editor`
- `AsterGraph.Avalonia`

Current public baseline:

- package version: `0.2.0-alpha.3`
- latest semver-aligned public prerelease tag: `v0.2.0-alpha.3`
- latest legacy repository milestone tag: `v1.9`

When a host asks "which version of AsterGraph should I install?", this package version is the authoritative answer.

## Repository Tags And Releases

The repository now publishes public prerelease tags that match the installable package version, for example `v0.2.0-alpha.3`.

The repository also still carries a historical tag such as `v1.9`.

Those `v1.x` tags came from milestone-style repository checkpoints during the pre-launch workflow. They are useful for maintainer history, but they are **not** the package version that consumers install from NuGet.

Going forward, the public release convention is:

- public package releases use tags that match package SemVer, for example `v0.2.0-alpha.3`
- GitHub prereleases should use the same version number as the published packages
- milestone-style local planning versions can continue privately, but they should not be presented as the consumer package version

## Current Mapping

| Public concept | Current value | How to read it |
| --- | --- | --- |
| installable package version | `0.2.0-alpha.3` | the version consumers install from nuget.org |
| current public prerelease tag | `v0.2.0-alpha.3` | the GitHub prerelease tag that must match the installable package version |
| historical public repo milestone tag | `v1.9` | a pre-launch checkpoint tag kept for repo history only |

## Practical Rule

If you are:

- adopting the SDK: follow the NuGet package version
- reading release notes: expect public prerelease tags to match the NuGet package version
- maintaining the repo: treat old `v1.x` tags as historical milestone markers, not as the current public package line

## Release Note Header Rule

The first lines of a public prerelease note should always expose:

1. the installable package version
2. the matching public tag
3. an optional historical milestone reference only when older repo history needs explaining

The public prerelease workflow now generates and validates that header automatically. Maintainers should not hand-edit the first block of the GitHub prerelease body unless a legacy historical note must be added for context.

Example:

- package version: `0.2.0-alpha.3`
- public tag: `v0.2.0-alpha.3`
- historical repo checkpoint reference: `v1.9` (legacy, not installable)

The generated prerelease body also carries the public proof summary from the release lane so external adopters can see the same installation, compatibility, scale, and coverage signals that gate publication.

## Related Docs

- [Quick Start](./quick-start.md)
- [Project Status](./project-status.md)
- [Alpha Status](./alpha-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
