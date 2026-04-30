# Phase 450 Plan

Success criteria:
- Public docs surface v0.75 library-grade closure markers.
- Launch checklist maps phases 445-449 to one bounded release gate.
- Tests defend the closure markers and external-name hygiene.
- Full required local verification passes.
- Phase bead, Dolt data, Git branch, and worktree are clean and pushed.

Tasks:
1. Add v0.75 closure markers to English and Chinese project status docs.
2. Add the same marker set to English and Chinese public launch checklist docs.
3. Add a focused Demo test that guards the v0.75 closure marker set and forbidden external-name hygiene.
4. Run focused and full release proof verification.
5. Record verification, close the bead, merge, push, and clean the worktree.

Verification commands:
- `dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj --no-restore --logger "console;verbosity=minimal" --filter "FullyQualifiedName~V075LibraryProofClosureTests"`
- `dotnet build src\AsterGraph.Abstractions\AsterGraph.Abstractions.csproj -c Release -f net9.0 --no-restore --nologo -v minimal`
- `dotnet build src\AsterGraph.Core\AsterGraph.Core.csproj -c Release -f net9.0 --no-restore --nologo -v minimal`
- `dotnet build src\AsterGraph.Editor\AsterGraph.Editor.csproj -c Release -f net9.0 --no-restore --nologo -v minimal`
- `dotnet build src\AsterGraph.Avalonia\AsterGraph.Avalonia.csproj -c Release -f net9.0 --no-restore --nologo -v minimal`
- `pwsh -NoProfile -File eng\validate-public-api-surface.ps1 -RepoRoot . -Configuration Release -Framework net9.0 -ProofPath artifacts\proof\public-api-surface-phase450.txt`
- `dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj --no-restore --logger "console;verbosity=minimal"`
- `dotnet test tests\AsterGraph.Editor.Tests\AsterGraph.Editor.Tests.csproj --no-restore --logger "console;verbosity=minimal"`
- `dotnet test tests\AsterGraph.ConsumerSample.Tests\AsterGraph.ConsumerSample.Tests.csproj --no-restore --logger "console;verbosity=minimal"`
- `dotnet test tests\AsterGraph.ScaleSmoke.Tests\AsterGraph.ScaleSmoke.Tests.csproj --no-restore --logger "console;verbosity=minimal"`
- `git diff --check`
- prohibited external-name scan over `.planning`, `docs`, and root README files
