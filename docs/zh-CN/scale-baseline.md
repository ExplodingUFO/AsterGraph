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
| `stress` | 5000 | 256 | 96 | 对外发布的信息型遥测 |

## 场景

每次运行都会测同一组场景：

- 在可重复生成的图上完成 editor/session setup
- 对一批代表性节点执行 selection
- 走一次 canonical connection 路径的删除和重连
- history interaction 加 save/undo/redo dirty 语义
- viewport resize 加 fit-to-viewport 导航
- workspace save 以及基于已保存文档的 reload

原有正确性 marker 仍然保留，比如 `SCALE_HISTORY_CONTRACT_OK`、`PHASE25_SCALE_AUTOMATION_OK` 和 `PHASE18_SCALE_READINESS_OK`。

## 当前防回归红线

release lane 现在会同时守住 `baseline` 和 `large` 两个层级：

这些红线刻意按当前公开 release lane 使用的 GitHub hosted Windows runner 冷启动测量来设定，不是按本地高性能开发机的最佳成绩来定。

### baseline 红线

| 指标 | baseline 红线 |
| --- | ---: |
| setup | 1500 ms |
| selection | 500 ms |
| connection | 150 ms |
| history interaction | 400 ms |
| viewport / fit | 150 ms |
| save | 150 ms |
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

只要任一 defended 层级超过这些数字，`ScaleSmoke` 就会输出 `SCALE_PERFORMANCE_BUDGET_OK:<tier>:False:...`，release gate 会失败。

## 信息型遥测

`stress` 层级仍然只是信息型遥测。它现在会额外发布一行 `SCALE_PERF_SUMMARY:stress:...`，把 p50/p95 时间直接给出来，但不会因此变成 defended release budget。

## 运行方式

```powershell
# release lane 使用的防回归 baseline
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier baseline

# release lane 守住的大图预算
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier large

# 对外发布的 stress 遥测
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier stress --samples 3
```

## 怎么看输出

最有信号的几行是：

- `SCALE_TIER_INFO:...`
- `SCALE_TIER_BUDGET:...`
- `SCALE_PERF_METRICS:...`
- `SCALE_PERFORMANCE_BUDGET_OK:...`
- `SCALE_PERF_SUMMARY:...`
- `SCALE_HISTORY_CONTRACT_OK:...`

其中 `SCALE_TIER_BUDGET` 是当前运行的机器可读预算声明，直接把防守层级、场景规模和阈值策略编码成一行，release note 和 proof summary 可以直接引用。

对 `baseline` 和 `large` 来说，真正对外承诺的 release 信号还是 `SCALE_PERFORMANCE_BUDGET_OK`。

对 `stress` 来说，`SCALE_PERF_SUMMARY:stress:...` 才是对外发布的 p50/p95 遥测行，它告诉宿主当前 5000 节点场景下的大致表现，但不代表正式预算承诺。

如果后续真的要对 5000 节点做更强承诺，应该体现在非 informational 的 `SCALE_TIER_BUDGET:stress:...` marker 加上 defended 的 `SCALE_PERFORMANCE_BUDGET_OK:stress:True:none`，或者单独再开一个新的 defended 层级。
