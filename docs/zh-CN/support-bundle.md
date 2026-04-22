# Beta Support Bundle

这份文档定义的是 public beta 的本地证据合同。
它不会上传任何东西，也不代表遥测、云服务或托管后端。

## Canonical 生成入口

当前推荐从 `ConsumerSample.Avalonia` 生成 support bundle，因为它是这条 beta 路线里已经防守住的真实宿主 proof。

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle artifacts/consumer-support-bundle.json --support-note "what you were trying to validate"
```

额外会输出两条 proof marker：

- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`

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

`proofLines` 里应该包含 proof mode 已经输出的同一组 marker，包括 `COMMAND_SURFACE_OK:True`、`CONSUMER_SAMPLE_OK:True` 和 `HOST_NATIVE_METRIC:*`。

`environment` 记录本次运行所在的 runtime 和操作系统信息。

`reproduction` 记录：

- 精确命令
- 工作目录
- 可选的人工备注

## 什么时候用

- 提 beta support issue 时附上它
- 走到真实宿主 proof 之后提交 adopter feedback 时附上它
- 包版本、路线或环境变化后重新生成

## 相关文档

- [公开 Beta 评估路径](./evaluation-path.md)
- [Consumer Sample](./consumer-sample.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
