# ScaleSmoke 基线

`tools/AsterGraph.ScaleSmoke` 是 AsterGraph 对外公开的“大图信心样例”。

它现在有两层用途：

- 作为发布验证 lane 的正确性验证，继续守住 history/save/state 语义
- 作为公开性能基线，给出一套可重复的规模分层和时间预算

## 分层

| 层级 | 节点数 | 选择批量 | 移动批量 | 用途 |
| --- | ---: | ---: | ---: | --- |
| `baseline` | 180 | 48 | 24 | release lane 防回归红线 |
| `large` | 1000 | 128 | 64 | 公开信息型遥测 |
| `stress` | 5000 | 256 | 96 | 手动拉伸遥测 |

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

release lane 现在只对 `baseline` 层级守这些红线：

| 指标 | baseline 红线 |
| --- | ---: |
| setup | 1500 ms |
| selection | 150 ms |
| connection | 150 ms |
| history interaction | 400 ms |
| viewport / fit | 150 ms |
| save | 150 ms |
| reload | 1200 ms |

只要 `baseline` 层级超过这些数字，`ScaleSmoke` 就会输出 `SCALE_PERFORMANCE_BUDGET_OK:baseline:False:...`，release gate 会失败。

## 信息型指标

`large` 和 `stress` 目前只做信息型指标，不会直接卡 CI。它们的价值是让维护者和外部使用者都能用同一条命令观察更大图规模下的表现。

## 运行方式

```powershell
# release lane 使用的防回归 baseline
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier baseline

# 更大的信息型测试
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier large

# 手动拉伸测试
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier stress
```

## 怎么看输出

最有信号的几行是：

- `SCALE_TIER_INFO:...`
- `SCALE_PERF_METRICS:...`
- `SCALE_PERFORMANCE_BUDGET_OK:...`
- `SCALE_HISTORY_CONTRACT_OK:...`

其中 budget marker 是当前真正对外承诺的 release 信号；更大层级的 metric 行暂时只算公开遥测，不算正式性能承诺。
