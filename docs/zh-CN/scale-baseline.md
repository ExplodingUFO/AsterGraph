# ScaleSmoke 基线

`tools/AsterGraph.ScaleSmoke` 是 AsterGraph 对外公开的“大图信心样例”。

它现在有两层用途：

- 作为发布验证 lane 的正确性验证，继续守住 history/save/state 语义
- 作为公开性能基线，给出一套可重复的规模分层和时间预算

## 分层

| 层级 | 节点数 | 选择批量 | 移动批量 | 用途 |
| --- | ---: | ---: | ---: | --- |
| `baseline` | 180 | 48 | 24 | release lane 防回归红线 |
| `large` | 1000 | 128 | 64 | release lane 守住的大图预算 |
| `stress` | 5000 | 256 | 96 | 部分 defended 的 5000 节点 gate，加 raster export 遥测 |

## 场景

每次运行都会测同一组场景：

- 在可重复生成的图上完成 editor/session setup
- 对一批代表性节点执行 selection
- 走一次 canonical connection 路径的删除和重连
- history interaction 加 save/undo/redo dirty 语义
- 覆盖 stencil filter、command-surface refresh 以及 quick-tool projection/execution 的 authoring probe
- viewport resize 加 fit-to-viewport 导航
- workspace save 以及基于已保存文档的 reload
- 基于同一份文档路径的 svg/png/jpeg export 加 reload

原有正确性 marker 仍然保留，比如 `SCALE_HISTORY_CONTRACT_OK`、`PHASE25_SCALE_AUTOMATION_OK` 和 `PHASE18_SCALE_READINESS_OK`。

## 当前防回归红线

release lane 现在会守住 `baseline`、`large`，以及 `stress` 中已提升的非 raster 指标：

这些红线刻意按当前公开 release lane 使用的 GitHub hosted Windows runner 冷启动测量来设定，不是按本地高性能开发机的最佳成绩来定。

### baseline 红线

| 指标 | baseline 红线 |
| --- | ---: |
| setup | 1500 ms |
| selection | 500 ms |
| connection | 150 ms |
| history interaction | 1500 ms |
| viewport / fit | 150 ms |
| save | 1300 ms |
| reload | 1200 ms |

### large 红线

| 指标 | large 红线 |
| --- | ---: |
| setup | 2500 ms |
| selection | 750 ms |
| connection | 350 ms |
| history interaction | 800 ms |
| viewport / fit | 200 ms |
| save | 300 ms |
| reload | 1500 ms |

### stress performance 红线

| 指标 | stress 红线 |
| --- | ---: |
| setup | 1500 ms |
| selection | 200 ms |
| connection | 1500 ms |
| history interaction | 2500 ms |
| viewport / fit | 100 ms |
| save | 700 ms |
| reload | 500 ms |

只要任一 defended 指标超过这些数字，`ScaleSmoke` 就会输出 `SCALE_PERFORMANCE_BUDGET_OK:<tier>:False:...`，release gate 会失败。

### authoring 红线

| 层级 | stencil | command-surface | quick-tool-projection | quick-tool-execution |
| --- | ---: | ---: | ---: | ---: |
| `baseline` | 100 ms | 250 ms | 100 ms | 150 ms |
| `large` | 150 ms | 400 ms | 150 ms | 200 ms |
| `stress` | 150 ms | 800 ms | 800 ms | 1000 ms |

`ScaleSmoke` 会为这些 defended 层级输出 `SCALE_AUTHORING_BUDGET:...`、`SCALE_AUTHORING_METRICS:...`、`SCALE_AUTHORING_BUDGET_OK:...` 和 `SCALE_AUTHORING_SUMMARY:...`。

### export 红线

| 层级 | svg | png | jpeg | reload |
| --- | ---: | ---: | ---: | ---: |
| `baseline` | 300 ms | 2500 ms | 3500 ms | 250 ms |
| `large` | 300 ms | 16000 ms | 12000 ms | 400 ms |
| `stress` | 300 ms | informational | informational | 800 ms |

`ScaleSmoke` 会为这些 defended 层级输出 `SCALE_EXPORT_BUDGET:...`、`SCALE_EXPORT_METRICS:...`、`SCALE_EXPORT_BUDGET_OK:...` 和 `SCALE_EXPORT_SUMMARY:...`。

如果你想把 `ConsumerSample.Avalonia` 的宿主指标和这些 defended `ScaleSmoke` 预算收成同一条可复制路线，就配合 [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md) 一起看。

## 信息型遥测

`stress` 层级现在是部分 defended。Performance、authoring、SVG export 和 export reload 有明确红线；PNG/JPEG raster export 因为 5000 节点重复证明仍然太慢，所以继续只作为 informational telemetry。

## 运行方式

```powershell
# release lane 使用的防回归 baseline
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier baseline

# release lane 守住的大图预算
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier large

# 部分 defended 的 5000 节点 stress gate，加 raster 遥测
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier stress --samples 3
```

## 怎么看输出

最有信号的几行是：

- `SCALE_TIER_INFO:...`
- `SCALE_TIER_BUDGET:...`
- `SCALE_PERF_METRICS:...`
- `SCALE_PERFORMANCE_BUDGET_OK:...`
- `SCALE_PERF_SUMMARY:...`
- `SCALE_AUTHORING_BUDGET:...`
- `SCALE_AUTHORING_METRICS:...`
- `SCALE_AUTHORING_BUDGET_OK:...`
- `SCALE_AUTHORING_SUMMARY:...`
- `SCALE_EXPORT_BUDGET:...`
- `SCALE_EXPORT_METRICS:...`
- `SCALE_EXPORT_BUDGET_OK:...`
- `SCALE_EXPORT_SUMMARY:...`
- `SCALE_HISTORY_CONTRACT_OK:...`

其中 `SCALE_TIER_BUDGET` 是当前运行的机器可读预算声明，直接把防守层级、场景规模和阈值策略编码成一行，release note 和 proof summary 可以直接引用。

对 `baseline`、`large` 和已提升的 `stress` performance 指标来说，真正对外承诺的 release 信号是 `SCALE_PERFORMANCE_BUDGET_OK`。

`SCALE_AUTHORING_BUDGET_OK` 是三个层级共同的 defended authoring 信号。

`SCALE_EXPORT_BUDGET:stress:svg<=300:png=informational:jpeg=informational:reload<=800` 是 5000 节点 export 故事的边界：SVG 和 reload 受防守，PNG/JPEG 仍只是遥测。

不要把这些 marker 解读成 10000 节点承诺，也不要解读成通用 virtualization 承诺。后续如果要提升 5000 节点 raster 承诺，必须先补非 informational 阈值和重复证明。
