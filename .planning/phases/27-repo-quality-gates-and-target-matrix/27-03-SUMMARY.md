## Summary

Plan 27-03 checked in the Phase 27 CI workflow. GitHub Actions now runs the same repo-local validation script as local contributors, with explicit `net8.0` and `net9.0` matrix lanes on a Windows runner.

## Changes

- added `.github/workflows/ci.yml`
- wired `push`, `pull_request`, and `workflow_dispatch` triggers
- used `actions/checkout@v4` and `actions/setup-dotnet@v4`
- encoded a visible framework matrix for `net8.0` and `net9.0`
- delegated validation execution to `.\eng\ci.ps1 -Lane all -Framework ... -Configuration Release`

## Verification

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release`

