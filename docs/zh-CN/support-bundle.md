# Beta Support Bundle

这份文档定义的是 public beta 的本地证据合同。
提交 beta 反馈时，可用时把 support bundle 作为附件使用。
它不会上传任何东西，也不代表遥测、云服务或托管后端。
如果你需要来自受防守 hosted proof route 的本地证据附件，就把它和 [公开 Beta 评估路径](./evaluation-path.md) 一起看。

## Canonical 生成入口

当前推荐从 `ConsumerSample.Avalonia` 生成 support bundle，因为它是这条 beta 路线里已经防守住的真实宿主 proof。

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle <support-bundle-path> --support-note "what you were trying to validate"
```

额外会输出两条 proof marker：

- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`

## 仅限本地证据

可复制的本地证据参考：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle <support-bundle-path> --support-note "what you were trying to validate"
```

这个证据包只保留本地证据，仍然绑定在受防守的 hosted route 上，不会扩大 support 边界。把 proof 输出里的 `SUPPORT_BUNDLE_PATH:...` 这一行作为 intake 记录里的附件备注；如果某条 route 不能产出 bundle，就记录 `NO_SUPPORT_BUNDLE:route-cannot-produce-one`。

## 合同字段

support bundle 是一个本地 JSON 文件，顶层字段固定为：

- `schemaVersion`
- `packageVersion`
- `publicTag`
- `route`
- `generatedAtUtc`
- `proofLines`
- `environment`
- `reproduction`

`proofLines` 里应该包含 proof mode 已经输出的完整 marker 集：`CONSUMER_SAMPLE_HOST_ACTION_OK:True`、`CONSUMER_SAMPLE_PLUGIN_OK:True`、`CONSUMER_SAMPLE_PARAMETER_OK:True`、`CONSUMER_SAMPLE_WINDOW_OK:True`、`CONSUMER_SAMPLE_TRUST_OK:True`、`COMMAND_SURFACE_OK:True`、`HOST_NATIVE_METRIC:startup_ms=...`、`HOST_NATIVE_METRIC:inspector_projection_ms=...`、`HOST_NATIVE_METRIC:plugin_scan_ms=...`、`HOST_NATIVE_METRIC:command_latency_ms=...` 这些行，以及 `CONSUMER_SAMPLE_OK:True`。

`environment` 记录本次运行所在的 runtime 和操作系统信息。

`reproduction` 记录摩擦说明和：

- 捕获到的命令行
- 工作目录
- 可选的人工备注

## 什么时候用

- 提 beta support issue 时附上它
- 走到真实宿主 proof 之后提交 adopter feedback 时，把它作为附件一并附上
- 包版本、路线或环境变化后重新生成
- 把它当成 [Project Status](./project-status.md) 就绪门禁的反馈证据，而不是自动扩大支持边界的证明
- 如果当前 route 还生成不了，就继续保留同一条 route/version/proof/摩擦记录，并注明 support bundle 暂时不可用

## 相关文档

- [公开 Beta 评估路径](./evaluation-path.md)
- [Consumer Sample](./consumer-sample.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
