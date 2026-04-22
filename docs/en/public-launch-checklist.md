# AsterGraph Public Launch Checklist

Use this checklist immediately before making the repository public or pushing the public prerelease tag that matches the package version.

## 1. Visibility And Branch Policy

- confirm the default branch is the intended public branch
- enable branch protection for the default branch
- require the `ci` workflow checks that represent the shipped gate
- confirm release permissions and NuGet publication permissions are limited to maintainers

## 2. Public Repo Surface

- confirm `README.md` and `README.zh-CN.md` both point to the current public beta docs and entry matrix
- confirm `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, and `SECURITY.md` are present and still accurate
- confirm issue templates and the pull request template are enabled in `.github`
- confirm repository description, topics, and homepage match the current prerelease narrative

## 3. Required Validation

Run the maintained entrypoints, not ad-hoc local commands:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane hygiene -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

## 4. Proof Artifact Review

Review these release artifacts before opening the repo or announcing the prerelease:

- `artifacts/proof/public-repo-hygiene.txt`
- `artifacts/proof/hostsample-packed.txt`
- `artifacts/proof/consumer-sample.txt`
- `artifacts/proof/demo-proof.txt`
- `artifacts/proof/hostsample-net10-packed.txt`
- `artifacts/proof/package-smoke.txt`
- `artifacts/proof/scale-smoke.txt`
- `artifacts/proof/coverage-report.txt`
- `artifacts/coverage/release-summary.json`

Expected high-signal markers:

- `PUBLIC_REPO_HYGIENE_OK:True`
- `HOST_SAMPLE_OK:True`
- `CONSUMER_SAMPLE_OK:True`
- `DEMO_OK:True`
- `COMMAND_SURFACE_OK:True`
- `HELLOWORLD_WPF_OK:True`
- `TIERED_NODE_SURFACE_OK:True`
- `FIXED_GROUP_FRAME_OK:True`
- `NON_OBSCURING_EDITING_OK:True`
- `VISUAL_SEMANTICS_OK:True`
- `HIERARCHY_SEMANTICS_OK:True`
- `COMPOSITE_SCOPE_OK:True`
- `EDGE_NOTE_OK:True`
- `EDGE_GEOMETRY_OK:True`
- `DISCONNECT_FLOW_OK:True`
- `ADAPTER_CAPABILITY_MATRIX_FORMAT:1`
- `ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`
- `ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`
- `HOST_SAMPLE_NET10_OK:True`
- `PACKAGE_SMOKE_OK:True`
- `SCALE_PERFORMANCE_BUDGET_OK:baseline:True:...`
- `SCALE_PERFORMANCE_BUDGET_OK:large:True:...`
- `SCALE_PERF_SUMMARY:stress:...`
- `SCALE_HISTORY_CONTRACT_OK:...`
- `COVERAGE_REPORT_OK:...`

## 5. Public Prerelease Tag

- confirm the working tree is clean
- push the release branch or `master` state that should back the tag
- create and push the public tag that matches the package version
- watch `.github/workflows/release.yml` from start to finish
- remember that the prerelease workflow now enforces an exact tag-to-package-version match
- confirm the generated prerelease notes begin with the automated header block:
  - installable package version
  - matching public tag
  - optional legacy historical repo checkpoint reference
- confirm the generated prerelease notes also publish the proof summary block, not only workflow artifacts
- confirm the generated notes and announcement text explicitly carry the frozen support boundary story and the adapter matrix story, plus `HELLOWORLD_WPF_OK:True`, `ADAPTER_CAPABILITY_MATRIX_FORMAT:1`, `ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`, and `ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`
- treat `HELLOWORLD_WPF_OK` as adapter-2 validation only; do not present it as Avalonia/WPF parity or public WPF support when describing the adapter story
- confirm every beta intake item tracks route, version, proof markers, and support-bundle availability or path
- if `NUGET_API_KEY` is configured, confirm package publication succeeds
- if `NUGET_API_KEY` is not configured, confirm the workflow reports a deliberate NuGet publish skip instead of a failure
- do not present legacy `v1.x`-style historical milestone checkpoints as the current public package version; use [Versioning](./versioning.md) as the public rule
- in the first screen of release notes, list the installable package version first, the matching public tag second, and any legacy `v1.x`-style milestone reference only as historical context

For a maintainer-driven manual beta publish without pushing a new tag:

- add the `NUGET_API_KEY` repository secret in GitHub
- open `Actions > prerelease > Run workflow`
- set `publish_to_nuget` to `true`
- optionally set `release_ref` to the branch or `v*` tag that should be packed
- keep using the committed package version; the manual path publishes the version already checked into the repository
- leave GitHub prerelease creation tag-driven; manual dispatch is only the NuGet publish escape hatch

## 6. Public Entry Guidance

Keep the consumer entry story explicit in release notes and public announcements:

- `tools/AsterGraph.HelloWorld` = fastest runtime-only first-run sample
- `tools/AsterGraph.HelloWorld.Avalonia` = fastest hosted-UI first-run sample
- `tools/AsterGraph.ConsumerSample.Avalonia` = realistic hosted-UI consumer sample with one host action rail and one trusted plugin
- `tools/AsterGraph.HostSample` = minimal consumer proof
- `tools/AsterGraph.PackageSmoke` = packaged-consumption proof
- `tools/AsterGraph.ScaleSmoke` = scale baseline plus history/state-continuity proof
- `src/AsterGraph.Demo` = showcase host
- `docs/en/versioning.md` = package version versus historical repository-tag guidance
- `docs/en/project-status.md` = current public beta status snapshot
- `docs/en/evaluation-path.md` = single route ladder from first install to realistic hosted proof
- `docs/en/quick-start.md` = canonical adoption path
- `docs/en/stabilization-support-matrix.md` = frozen support boundary and upgrade guidance
- `docs/en/adapter-capability-matrix.md` = adapter capability story and validation matrix
- `docs/en/alpha-status.md` = historical alpha reference for the current beta support story
- `docs/en/advanced-editing.md` = advanced-editing capability split and proof map
- `docs/en/adopter-triage.md` = adopter triage checklist for the one beta evidence contract
- `docs/en/support-bundle.md` = required support bundle format and collection workflow
