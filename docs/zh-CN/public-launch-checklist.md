# AsterGraph 对外发布检查清单

在把仓库切成公开可见，或者推送与包版本匹配的公开 prerelease tag 之前，先跑完这份检查清单。

## 1. 可见性与分支策略

- 确认默认分支就是要对外公开的分支
- 给默认分支开启 branch protection
- 要求通过代表官方发布门禁的 `ci` workflow checks
- 确认 release 权限和 NuGet 发布权限只给维护者

## 2. 公开仓库表面

- 确认 `README.md` 和 `README.zh-CN.md` 都指向当前公开 beta 文档和入口矩阵
- 确认 `CONTRIBUTING.md`、`CODE_OF_CONDUCT.md`、`SECURITY.md` 都存在且内容仍然准确
- 确认 `.github` 里的 issue 模板和 pull request 模板已启用
- 确认仓库 description、topics、homepage 和当前 prerelease 叙事一致

## 3. 必跑验证

只跑维护中的官方入口，不要临时拼 ad-hoc 命令：

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane hygiene -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

## 4. 检查 Proof Artifact

公开前至少复核这些产物：

- `artifacts/proof/public-repo-hygiene.txt`
- `artifacts/proof/hostsample-packed.txt`
- `artifacts/proof/consumer-sample.txt`
- `artifacts/proof/demo-proof.txt`
- `artifacts/proof/hostsample-net10-packed.txt`
- `artifacts/proof/package-smoke.txt`
- `artifacts/proof/scale-smoke.txt`
- `artifacts/proof/coverage-report.txt`
- `artifacts/coverage/release-summary.json`

重点 marker：

- `PUBLIC_REPO_HYGIENE_OK:True`
- `HOST_SAMPLE_OK:True`
- `CONSUMER_SAMPLE_OK:True`
- `DEMO_OK:True`
- `COMMAND_SURFACE_OK:True`
- `HELLOWORLD_WPF_OK:True`
- `TIERED_NODE_SURFACE_OK:True`
- `FIXED_GROUP_FRAME_OK:True`
- `NON_OBSCURING_EDITING_OK:True`
- `VISUAL_SEMANTICS_OK:True`
- `HIERARCHY_SEMANTICS_OK:True`
- `COMPOSITE_SCOPE_OK:True`
- `EDGE_NOTE_OK:True`
- `EDGE_GEOMETRY_OK:True`
- `DISCONNECT_FLOW_OK:True`
- `ADAPTER_CAPABILITY_MATRIX_FORMAT:1`
- `ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`
- `ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`
- `HOST_SAMPLE_NET10_OK:True`
- `PACKAGE_SMOKE_OK:True`
- `SCALE_PERFORMANCE_BUDGET_OK:baseline:True:...`
- `SCALE_PERFORMANCE_BUDGET_OK:large:True:...`
- `SCALE_PERF_SUMMARY:stress:...`
- `SCALE_HISTORY_CONTRACT_OK:...`
- `COVERAGE_REPORT_OK:...`

发布 notes/release messaging 核对要求：

- 必须在 prerelease notes 或 release messaging 里同步给出 WPF proof marker：`HELLOWORLD_WPF_OK`
- 必须在 prerelease notes 或 release messaging 里同步给出 adapter 能力矩阵输出：`ADAPTER_CAPABILITY_MATRIX_FORMAT:1`、`ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`、`ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`

## 5. 公开 Beta Prerelease Tag

- 确认工作区干净
- 先把要打 tag 的分支或 `master` 推上去
- 创建并推送与包版本匹配的公开 tag
- 从头到尾观察 `.github/workflows/release.yml`
- 现在 prerelease workflow 会强制校验：公开 tag 必须和包版本完全一致
- 确认自动生成的 prerelease notes 第一屏带有固定 header：
  - 可安装包版本
  - 与之匹配的公开 tag
  - 可选的 legacy 历史仓库检查点引用
- 确认自动生成的 prerelease notes 同时把 proof summary 发出来，而不只是留在 workflow artifact 里
- 确认自动生成的 notes 和公告文案明确写出冻结的 support boundary 叙事和 adapter matrix 叙事，并同步给出 `HELLOWORLD_WPF_OK:True`、`ADAPTER_CAPABILITY_MATRIX_FORMAT:1`、`ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`、`ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`
- 把 `HELLOWORLD_WPF_OK` 只当成 adapter-2 验证通过，不要写成 Avalonia/WPF parity 或公开 WPF support
- 确认每条 beta 反馈都按同一套受限字段记录：`route`、`version`、proof 标记、摩擦点，以及 support bundle 附件
- 在 release messaging 里重复下一条 `0.xx` alpha/beta 线的 handoff：继续优先做使用友好性和样例打磨；只有当 3 到 5 条真实外部反馈在同一个受限风险上聚焦时，才考虑把 defended large-tier performance 或更丰富的参数/元数据编辑提上来
- 如果配置了 `NUGET_API_KEY`，确认包发布成功
- 如果没有配置 `NUGET_API_KEY`，确认 workflow 是有意跳过 NuGet publish，而不是失败
- 不要再把 `v1.x` 风格的历史里程碑 checkpoint 当成当前公开包版本；对外统一以 [Versioning](./versioning.md) 为准
- release note 第一屏先写可安装包版本，再写与之匹配的公开 tag；`v1.x` 风格的旧 milestone 只作为历史说明补充出现
- `prerelease notes` / release messaging 中还必须核对并回填 `HELLOWORLD_WPF_OK:True`、`ADAPTER_CAPABILITY_MATRIX_FORMAT:1`、`ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`、`ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`，避免只写成功 tag 而漏能力核对
- 公开文案要保持和 matrix 一致：`HELLOWORLD_WPF_OK` 证明的是 adapter 2 验证通过，不是 WPF 与 Avalonia 已经 parity

如果你想在不新推 tag 的情况下手动发布 beta 包：

- 先在 GitHub 仓库 secret 里配置 `NUGET_API_KEY`
- 打开 `Actions > prerelease > Run workflow`
- 把 `publish_to_nuget` 设为 `true`
- 如果要从特定分支或 `v*` tag 打包，就填写 `release_ref`
- 保持仓库里已经提交好的包版本；手动触发不会替你改版本，只会发布当前提交里的版本
- GitHub prerelease 仍然建议走 tag 驱动；手动触发只作为 NuGet beta 发布的补充入口

## 6. 对外入口说明

在 release note、公告、README 里把入口说清楚：

- `tools/AsterGraph.HelloWorld` = 最快的 runtime-only 第一跑样例
- `tools/AsterGraph.HelloWorld.Avalonia` = 最快的默认 UI 第一跑样例
- `tools/AsterGraph.ConsumerSample.Avalonia` = 真实 hosted-UI consumer 样例，带宿主动作和一个可信插件
- `tools/AsterGraph.HostSample` = 最小接入验证
- `tools/AsterGraph.PackageSmoke` = 打包消费验证
- `tools/AsterGraph.ScaleSmoke` = 规模基线加历史记录与状态连续性验证
- `src/AsterGraph.Demo` = 展示宿主
- `docs/zh-CN/versioning.md` = 包版本与历史仓库 tag 的对应说明
- `docs/zh-CN/project-status.md` = 外部能力就绪闸门，对应已被外部证据证明与继续延后的能力声明
- `docs/zh-CN/evaluation-path.md` = 从第一次安装到真实宿主 proof 的单一路径
- `docs/zh-CN/quick-start.md` = 推荐接入路径
- `docs/zh-CN/stabilization-support-matrix.md` = 冻结的 support boundary 和升级指引
- `docs/zh-CN/adapter-capability-matrix.md` = adapter matrix 叙事与验证矩阵
- `docs/zh-CN/alpha-status.md` = 历史 alpha 参考，服务于当前 beta support story
- `docs/zh-CN/advanced-editing.md` = advanced-editing capability split 与 proof map
- `docs/zh-CN/adopter-triage.md` = 对应一套 beta 证据合同的采用者分诊清单
- `docs/zh-CN/support-bundle.md` = support bundle 的本地 JSON 合同与采集流程
