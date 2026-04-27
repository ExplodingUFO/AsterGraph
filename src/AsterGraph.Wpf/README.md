# AsterGraph.Wpf

`AsterGraph.Wpf` is the validation-only WPF adapter-2 bootstrap package for AsterGraph.

It belongs to the hosted package set with `AsterGraph.Core` and `AsterGraph.Editor` and targets
`net8.0-windows` and `net9.0-windows`.

Avalonia remains the shipped hosted adapter and the default onboarding path. Use this package to
validate WPF portability on the canonical session/runtime route; do not present it as Avalonia/WPF
parity, public WPF support expansion, or a second beginner route.

## Reference This Package When

- maintainers need the bounded WPF adapter-2 validation shell
- a Windows host needs to inspect WPF platform seams while staying on the canonical session/runtime route

## This Package Owns

- `Controls.GraphEditorView`
- `Hosting.AsterGraphWpfViewFactory`
- `Hosting.AsterGraphWpfViewOptions`
- WPF platform-seam helpers (`ApplyPlatformServices`, clipboard bridge, host context, and seam glue)

## Validation Entry Path

- validation shell: `AsterGraphWpfViewFactory.Create(new AsterGraphWpfViewOptions { Editor = editor })`

For new product work, start with `CreateSession(...)` for custom UI or the shipped Avalonia route
for hosted UI. The WPF factory remains a validation sample entry point, not a new runtime model.

## No Ownership / No Porting Responsibilities

This package composes the retained `GraphEditorViewModel` facade with existing command/state
properties. It does not define new editor-runtime features, WPF-specific runtime APIs, a second
host-facing runtime model, or standalone WPF canvas, inspector, or mini-map factories.
