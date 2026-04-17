# AsterGraph Public Launch Checklist

Use this checklist immediately before making the repository public or pushing the first public prerelease tag.

## 1. Visibility And Branch Policy

- confirm the default branch is the intended public branch
- enable branch protection for the default branch
- require the `ci` workflow checks that represent the shipped gate
- confirm release permissions and NuGet publication permissions are limited to maintainers

## 2. Public Repo Surface

- confirm `README.md` and `README.zh-CN.md` both point to the current public-alpha docs and entry matrix
- confirm `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, and `SECURITY.md` are present and still accurate
- confirm issue templates and the pull request template are enabled in `.github`
- confirm repository description, topics, and homepage match the current alpha narrative

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
- `artifacts/proof/hostsample-net10-packed.txt`
- `artifacts/proof/package-smoke.txt`
- `artifacts/proof/scale-smoke.txt`
- `artifacts/proof/coverage-report.txt`
- `artifacts/coverage/release-summary.json`

Expected high-signal markers:

- `PUBLIC_REPO_HYGIENE_OK:True`
- `HOST_SAMPLE_OK:True`
- `HOST_SAMPLE_NET10_OK:True`
- `PACKAGE_SMOKE_OK:True`
- `SCALE_HISTORY_CONTRACT_OK:...`
- `COVERAGE_REPORT_OK:...`

## 5. First Public Prerelease Tag

- confirm the working tree is clean
- push the release branch or `master` state that should back the tag
- create and push the next public `v*` tag
- watch `.github/workflows/release.yml` from start to finish
- if `NUGET_API_KEY` is configured, confirm package publication succeeds
- if `NUGET_API_KEY` is not configured, confirm the workflow reports a deliberate NuGet publish skip instead of a failure

## 6. Public Entry Guidance

Keep the consumer entry story explicit in release notes and public announcements:

- `tools/AsterGraph.HostSample` = minimal consumer proof
- `tools/AsterGraph.PackageSmoke` = packaged-consumption proof
- `tools/AsterGraph.ScaleSmoke` = scale/history/state-continuity proof
- `src/AsterGraph.Demo` = showcase host
- `docs/en/quick-start.md` = canonical adoption path
- `docs/en/alpha-status.md` = current alpha scope and limitations
