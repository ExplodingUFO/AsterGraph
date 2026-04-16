# Phase 43 Verification

## Results

- `dotnet build tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -c Release -f net10.0 /p:EnableNet10ConsumerProof=true /p:UsePackedAsterGraphPackages=true` => passed
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release` => passed
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release` => passed
- `python -c "from pathlib import Path; import yaml; [yaml.safe_load(Path(p).read_text(encoding='utf-8')) for p in ['.github/workflows/ci.yml','.github/workflows/release.yml']]; print('WORKFLOW_YAML_OK:2')"` => `WORKFLOW_YAML_OK:2`
- `python -c "from pathlib import Path; import yaml; data=yaml.safe_load(Path('.github/workflows/release.yml').read_text(encoding='utf-8')); bad=[name for name,job in data['jobs'].items() if 'if' in job and 'secrets.' in str(job['if'])]; print('RELEASE_JOB_SECRET_IF_OK:' + str(len(bad)==0))"` => `RELEASE_JOB_SECRET_IF_OK:True`
- `Get-Content artifacts\proof\hostsample-net10-packed.txt` => includes `HOST_SAMPLE_NET10_OK:True`

## Notes

- Both GitHub workflows now pin `NUGET_PACKAGES` to `${{ github.workspace }}/.nuget/packages` and create that directory before `actions/setup-dotnet` caching runs.
- The prerelease workflow keeps validation-first ordering, but the NuGet publish secret gate now lives inside the publish job instead of in a job-level `if:` expression.
- The release lane now carries an explicit packed `.NET 10` HostSample proof without disturbing the default `net8.0` HostSample consumer path.
