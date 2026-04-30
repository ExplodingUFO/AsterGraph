# Phase 448 Verification

Verification commands:

```powershell
dotnet build src\AsterGraph.Abstractions\AsterGraph.Abstractions.csproj -c Release -f net9.0 --no-restore --nologo -v minimal
dotnet build src\AsterGraph.Core\AsterGraph.Core.csproj -c Release -f net9.0 --no-restore --nologo -v minimal
dotnet build src\AsterGraph.Editor\AsterGraph.Editor.csproj -c Release -f net9.0 --no-restore --nologo -v minimal
dotnet build src\AsterGraph.Avalonia\AsterGraph.Avalonia.csproj -c Release -f net9.0 --no-restore --nologo -v minimal
pwsh -NoProfile -File eng\validate-public-api-surface.ps1 -RepoRoot . -Configuration Release -Framework net9.0 -ProofPath artifacts\proof\public-api-surface-phase448.txt
dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj --no-restore --logger "console;verbosity=minimal"
git diff --check
prohibited external-name scan over `.planning`, `docs`, and root README files
```

Results:
- Public API gate passed: `PUBLIC_API_SURFACE_OK:3741:net9.0`, `PUBLIC_API_DIFF_GATE_OK:True`.
- Demo tests passed: 210 passed, 0 failed.
- `git diff --check` returned exit code 0.
- Prohibited external-name scan returned no matches.

Residual risk:
- Local verification is Windows-based. Linux/macOS execution is defended by workflow contract tests and still needs real GitHub Actions execution for platform runtime confirmation.
