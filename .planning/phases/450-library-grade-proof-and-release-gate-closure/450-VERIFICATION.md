# Phase 450 Verification

Verification is recorded after the final local release gate run.

Required:
- Focused v0.75 closure test.
- Public API gate against Release `net9.0` assemblies.
- Full Demo tests.
- Editor, ConsumerSample, and ScaleSmoke tests.
- `git diff --check`.
- Prohibited external-name scan over `.planning`, `docs`, and root README files.
- `gsd-sdk query roadmap.analyze`.

Results:
- Focused v0.75 closure test passed: 3 passed, 0 failed.
- Public API gate passed: `PUBLIC_API_SURFACE_OK:3741:net9.0`, `PUBLIC_API_SCOPE_OK:AsterGraph.Abstractions,AsterGraph.Core,AsterGraph.Editor,AsterGraph.Avalonia`, `PUBLIC_API_GUIDANCE_OK:True`, `PUBLIC_API_DIFF_GATE_OK:True`, `PUBLIC_API_USAGE_GUIDANCE_OK:True`, and `PUBLIC_API_STABILITY_SCOPE_OK:True`.
- Full Demo tests passed: 215 passed, 0 failed.
- Editor tests passed: 681 passed, 0 failed.
- ConsumerSample tests passed: 24 passed, 0 failed.
- ScaleSmoke tests passed: 37 passed, 0 failed.
- `git diff --check` returned exit code 0. Git reported line-ending normalization warnings for existing markdown files only.
- Prohibited external-name scan returned no matches.
- `gsd-sdk query roadmap.analyze` is reserved for the main workspace after merge because the isolated worktree does not carry ignored `.planning/ROADMAP.md` state.

Note:
- On this .NET 10 SDK, `dotnet test --no-restore` sometimes returned after build without launching the test host for already restored projects. Verification used explicit `dotnet build` followed by `dotnet test --no-build` where needed so the test counts above come from real test-host output.
