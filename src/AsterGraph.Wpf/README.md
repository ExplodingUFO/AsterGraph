# AsterGraph.Wpf

`AsterGraph.Wpf` is the WPF host adapter bootstrap package for AsterGraph.

It belongs to the hosted package set with `AsterGraph.Core` and `AsterGraph.Editor` and targets
`net8.0-windows` and `net9.0-windows`.

## Reference This Package When

- the host wants a default WPF shell for the retained editor facade
- the host wants a simple WPF entry point for binding command/state properties from
  `GraphEditorViewModel`

## This Package Owns

- `Controls.GraphEditorView`
- `Hosting.AsterGraphWpfViewFactory`
- `Hosting.AsterGraphWpfViewOptions`
- WPF platform-seam helpers (`ApplyPlatformServices`, clipboard bridge, host context, and seam glue)

## Canonical Entry Paths

- hosted shell: `AsterGraphWpfViewFactory.Create(new AsterGraphWpfViewOptions { Editor = editor })`

For new work, prefer the factory route over direct `new GraphEditorView`.

## No Ownership / No Porting Responsibilities

This package composes the retained `GraphEditorViewModel` facade with existing command/state
properties. It does not define new editor-runtime features and does not introduce standalone WPF canvas,
inspector, or mini-map factories.
