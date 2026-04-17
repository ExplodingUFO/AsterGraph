# AsterGraph Versioning

## Public Package Version

The consumer-facing version for AsterGraph is the NuGet package version on the four published packages:

- `AsterGraph.Abstractions`
- `AsterGraph.Core`
- `AsterGraph.Editor`
- `AsterGraph.Avalonia`

Current public baseline:

- package version: `0.2.0-alpha.1`

When a host asks "which version of AsterGraph should I install?", this package version is the authoritative answer.

## Repository Tags And Releases

The repository has historical tags such as `v1.9`, `v1.10`, and `v1.11`.

Those tags came from milestone-style repository checkpoints during the pre-launch workflow. They are useful for maintainer history, but they are **not** the package version that consumers install from NuGet.

Going forward, the public release convention is:

- public package releases use tags that match package SemVer, for example `v0.2.0-alpha.2`
- GitHub prereleases should use the same version number as the published packages
- milestone-style local planning versions can continue privately, but they should not be presented as the consumer package version

## Practical Rule

If you are:

- adopting the SDK: follow the NuGet package version
- reading release notes: expect public prerelease tags to match the NuGet package version
- maintaining the repo: treat old `v1.x` tags as historical milestone markers, not as the current public package line

## Related Docs

- [Quick Start](./quick-start.md)
- [Project Status](./project-status.md)
- [Alpha Status](./alpha-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
