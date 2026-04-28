# AsterGraph.HelloWorld.Wpf

`AsterGraph.HelloWorld.Wpf` is the richer validation-only adapter-2 proof sample for the WPF shell.

Run it with `--proof` to emit the bounded WPF validation markers used by CI and release notes:

```powershell
dotnet run --project tools/AsterGraph.HelloWorld.Wpf/AsterGraph.HelloWorld.Wpf.csproj --nologo -- --proof
```

These markers prove WPF portability on the canonical session/runtime route. They do not create a
second onboarding path, widen public WPF support, or claim Avalonia/WPF parity.
The v0.60 scope baseline also emits `ADAPTER2_VALIDATION_SCOPE_OK:True`,
`ADAPTER2_MATRIX_HANDOFF_OK:True`, and `ADAPTER2_SCOPE_BOUNDARY_OK:True` to keep the
matrix handoff tied to validation-only evidence.

For first-time hosted UI adoption, use the Avalonia route ladder:
`Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.

