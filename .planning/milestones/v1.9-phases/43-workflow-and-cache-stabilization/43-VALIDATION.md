---
phase: 43
title: Workflow And Cache Stabilization
status: completed
last_updated: 2026-04-16
---

# Phase 43 Validation

## Required Checks

1. Red/green `.NET 10` consumer proof:
   - `dotnet build tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -c Release -f net10.0 /p:EnableNet10ConsumerProof=true /p:UsePackedAsterGraphPackages=true`
2. Release lane with the new consumer proof included:
   - `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release`
3. Branch-validation lane still stays green after workflow/script changes:
   - `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release`
4. Workflow structural validation:
   - `python -c "from pathlib import Path; import yaml; [yaml.safe_load(Path(p).read_text(encoding='utf-8')) for p in ['.github/workflows/ci.yml','.github/workflows/release.yml']]; print('WORKFLOW_YAML_OK:2')"`
   - `python -c "from pathlib import Path; import yaml; data=yaml.safe_load(Path('.github/workflows/release.yml').read_text(encoding='utf-8')); bad=[name for name,job in data['jobs'].items() if 'if' in job and 'secrets.' in str(job['if'])]; print('RELEASE_JOB_SECRET_IF_OK:' + str(len(bad)==0))"`

## Acceptance

- Hosted-runner workflows use an explicit, workspace-local `NUGET_PACKAGES` path instead of depending on the default user-profile location to exist.
- `release.yml` no longer references `secrets.*` inside a job-level `if:` and remains tag/manual only.
- Release artifacts include one `.NET 10` consumer-proof marker from the packed `HostSample` path.
