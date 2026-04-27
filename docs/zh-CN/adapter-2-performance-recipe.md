# Adapter-2 Performance Recipe

这份 recipe 只能在受防守的 Avalonia hosted proof 已经通过之后使用。它定义的是 `WPF` 作为 adapter 2 的一次有边界、validation-only 的 performance 跟进验证；它不会扩大支持承诺，也不代表 Avalonia/WPF parity。

## 交接顺序

- 先走受防守的 Avalonia hosted 路线，运行 `AsterGraph.ConsumerSample.Avalonia -- --proof`，并先把共享的 `HOST_NATIVE_METRIC:*` 行放进记录里。
- 复用同一条受限 intake 记录和来自受防守 Avalonia 路线的 support-bundle attachment note；`WPF` 不会打开第二条 intake 流程。
- 只把 `AsterGraph.HelloWorld.Wpf -- --proof` 当成 validation-only 的 adapter-2 跟进验证。
- 把 `ADAPTER2_PERFORMANCE_BASELINE_OK:True`、`ADAPTER2_EXPORT_BREADTH_OK:True`、`ADAPTER2_PROJECTION_BUDGET_OK:True:none`、`ADAPTER2_COMMAND_BUDGET_OK:True:none`、`ADAPTER2_SCENE_BUDGET_OK:True:none` 和 `HELLOWORLD_WPF_OK:True` 继续放在同一条本地记录里。
- 所有 `WPF` performance proof 行都只能当成验证证据；这些 marker 都不会扩大公开支持承诺。

## 命令

```powershell
# 先跑受防守的 Avalonia hosted proof
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof

# 只有在受防守的 Avalonia proof 之后才跑有边界的 adapter-2 performance 验证
dotnet run --project tools/AsterGraph.HelloWorld.Wpf/AsterGraph.HelloWorld.Wpf.csproj --nologo -- --proof
```

## 什么算成功

- 受防守的 Avalonia 路线仍然是第一个 proof gate
- `AsterGraph.HelloWorld.Wpf -- --proof` 会输出 `ADAPTER2_PERFORMANCE_BASELINE_OK:True`
- 同一轮 `WPF` proof 还会输出 `ADAPTER2_EXPORT_BREADTH_OK:True`
- 同一轮 `WPF` proof 还会输出 `ADAPTER2_PROJECTION_BUDGET_OK:True:none`
- 同一轮 `WPF` proof 还会输出 `ADAPTER2_COMMAND_BUDGET_OK:True:none`
- 同一轮 `WPF` proof 还会输出 `ADAPTER2_SCENE_BUDGET_OK:True:none`
- 同一轮 `WPF` proof 还会输出 `HELLOWORLD_WPF_OK:True`

## 不要误读

- `WPF` 仍然只是 validation-only 的 adapter-2 覆盖
- 这些 marker 不会变成新的上手路径
- 这些 marker 不代表 Avalonia/WPF parity
- 这些 marker 不代表公开 WPF support

## 相关文档

- [公开 Beta 评估路径](./evaluation-path.md)
- [Adapter-2 Accessibility Recipe](./adapter-2-accessibility-recipe.md)
- [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md)
- [Beta Support Bundle](./support-bundle.md)
