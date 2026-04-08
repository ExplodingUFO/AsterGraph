# 25-03 Summary

## Outcome

Wave 3 closed the public proof gap by aligning docs and focused regressions with the same canonical story proven by the runnable tools.

- `README.md` now documents the shipped plugin-loading and automation-execution baseline through `AsterGraphEditorFactory`, `PluginRegistrations`, `IGraphEditorSession.Queries.GetPluginLoadSnapshots()`, and `IGraphEditorSession.Automation`.
- Focused regressions now assert direct plugin composition parity, plugin inspection/readiness, and automation over plugin-contributed node definitions.
- The proof ring is now aligned across `HostSample`, `PackageSmoke`, `ScaleSmoke`, focused tests, and the README commands so the same public claims are backed everywhere.
- Pre-v1.4 README drift around runtime plugin loading being a non-goal was removed so the public narrative no longer contradicts the shipped baseline.

## Notes

- The detached-baseline transaction/history failures stayed out of scope and were carried forward only as verification context.
