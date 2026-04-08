# 24-01 Summary

## Outcome

Wave 1 established the canonical automation contract surface over the runtime boundary.

- Added `AsterGraph.Editor.Automation` with the public runner, run-request, step, and execution-snapshot contracts.
- Extended `IGraphEditorSession` with a first-class `Automation` entry so hosts can discover automation from the canonical session root.
- Widened the stable generic command path for automation-critical commands including selection, node movement, connection completion, and viewport pan/resize/center operations.
- Added explicit automation discoverability markers through `surface.automation.runner` and `event.automation.*` feature descriptors.
- Added focused contract regressions to lock the automation DTO surface away from Avalonia and `GraphEditorViewModel` coupling.

## Notes

- The new automation baseline stays command-first and session-first; it does not introduce a second retained-facade API or a scripting language.
