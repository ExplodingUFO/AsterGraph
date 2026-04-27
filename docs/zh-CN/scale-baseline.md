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
| `stress` | 5000 | 256 | 96 | 带保守 raster export 红线的 defended 5000 节点 gate |
| `xlarge` | 10000 | 512 | 128 | telemetry-only 探针；不是支持承诺 |

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
| connection | 450 ms |
| history interaction | 1000 ms |
| viewport / fit | 200 ms |
| save | 300 ms |
| reload | 1500 ms |

### stress performance 红线

| 指标 | stress 红线 |
| --- | ---: |
| setup | 1500 ms |
| selection | 200 ms |
| connection | 2500 ms |
| history interaction | 2500 ms |
| viewport / fit | 100 ms |
| save | 700 ms |
| reload | 500 ms |

只要任一 defended 指标超过这些数字，`ScaleSmoke` 就会输出 `SCALE_PERFORMANCE_BUDGET_OK:<tier>:False:...`，release gate 会失败。

### authoring 红线

| 层级 | stencil | command-surface | quick-tool-projection | quick-tool-execution |
| --- | ---: | ---: | ---: | ---: |
| `baseline` | 100 ms | 250 ms | 100 ms | 150 ms |
| `large` | 150 ms | 400 ms | 250 ms | 300 ms |
| `stress` | 150 ms | 900 ms | 1000 ms | 1200 ms |

`ScaleSmoke` 会为这些 defended 层级输出 `SCALE_AUTHORING_BUDGET:...`、`SCALE_AUTHORING_METRICS:...`、`SCALE_AUTHORING_BUDGET_OK:...` 和 `SCALE_AUTHORING_SUMMARY:...`。

### export 红线

| 层级 | svg | png | jpeg | reload |
| --- | ---: | ---: | ---: | ---: |
| `baseline` | 300 ms | 2500 ms | 3500 ms | 250 ms |
| `large` | 300 ms | 16000 ms | 12000 ms | 400 ms |
| `stress` | 300 ms | 120000 ms | 100000 ms | 800 ms |

`ScaleSmoke` 会为这些 defended 层级输出 `SCALE_EXPORT_BUDGET:...`、`SCALE_EXPORT_METRICS:...`、`SCALE_EXPORT_BUDGET_OK:...`、`EXPORT_PROGRESS_OK:...`、`EXPORT_CANCEL_OK:...`、`EXPORT_SCOPE_OK:...`、`EXPORT_SELECTION_SCOPE_OK:...` 和 `SCALE_EXPORT_SUMMARY:...`。

如果你想把 `ConsumerSample.Avalonia` 的宿主指标和这些 defended `ScaleSmoke` 预算收成同一条可复制路线，就配合 [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md) 一起看。

## Stress Raster Export

`stress` 层级现在防守 performance、authoring、SVG export、PNG/JPEG raster export 和 export reload。第一版 raster 红线刻意保守，用于让 release validation 捕捉病态回归，而不是宣称 5000 节点 raster export 已经很快。

## 运行方式

```powershell
# release lane 使用的防回归 baseline
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier baseline

# release lane 守住的大图预算
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier large

# 带保守 raster 红线的 defended 5000 节点 stress gate
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier stress --samples 3

# telemetry-only 10000 节点探针；不属于 release gate
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier xlarge --samples 1
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
- `EXPORT_PROGRESS_OK:...`
- `EXPORT_CANCEL_OK:...`
- `EXPORT_SCOPE_OK:...`
- `EXPORT_SELECTION_SCOPE_OK:...`
- `SCALE_EXPORT_METRICS:...`
- `SCALE_EXPORT_BUDGET_OK:...`
- `SCALE_EXPORT_SUMMARY:...`
- `SCALE_HISTORY_CONTRACT_OK:...`

其中 `SCALE_TIER_BUDGET` 是当前运行的机器可读预算声明，直接把防守层级、场景规模和阈值策略编码成一行，release note 和 proof summary 可以直接引用。

对 `baseline`、`large` 和已提升的 `stress` performance 指标来说，真正对外承诺的 release 信号是 `SCALE_PERFORMANCE_BUDGET_OK`。

`SCALE_AUTHORING_BUDGET_OK` 是三个层级共同的 defended authoring 信号。

`SCALE_EXPORT_BUDGET:stress:svg<=300:png<=120000:jpeg<=100000:reload<=800` 和 `SCALE_RASTER_EXPORT_STRESS_OK:True` 是 5000 节点 export 故事的边界：raster export 受保守红线防守，但不表示它已经很快。

对 `xlarge` 来说，`SCALE_TIER_BUDGET:xlarge:nodes=10000:selection=512:moves=128:budget=informational-only`、`SCALE_AUTHORING_BUDGET:xlarge:budget=informational-only` 和 `SCALE_EXPORT_BUDGET:xlarge:budget=informational-only` 都只是 telemetry marker。

不要把这些 marker 解读成 10000 节点承诺，也不要解读成通用 virtualization 承诺。后续如果要提升 5000 节点 raster 承诺，必须先补非 informational 阈值和重复证明。
