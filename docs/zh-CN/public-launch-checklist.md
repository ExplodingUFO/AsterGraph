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
- `artifacts/proof/template-smoke.txt`
- `artifacts/proof/public-api-surface.txt`
- `artifacts/proof/scale-smoke.txt`
- `artifacts/proof/coverage-report.txt`
- `artifacts/coverage/release-summary.json`

重点 marker：

- `PUBLIC_REPO_HYGIENE_OK:True`
- `HOST_SAMPLE_OK:True`
- `CONSUMER_SAMPLE_OK:True`
- `DEMO_OK:True`
- `COMMAND_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_BASELINE_OK:True`
- `HOSTED_ACCESSIBILITY_FOCUS_OK:True`
- `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_OK:True`
- `ADAPTER2_PERFORMANCE_BASELINE_OK:True`
- `ADAPTER2_EXPORT_BREADTH_OK:True`
- `ADAPTER2_PROJECTION_BUDGET_OK:True:none`
- `ADAPTER2_COMMAND_BUDGET_OK:True:none`
- `ADAPTER2_SCENE_BUDGET_OK:True:none`
- `ADAPTER2_VALIDATION_SCOPE_OK:True`
- `ADAPTER2_MATRIX_HANDOFF_OK:True`
- `ADAPTER2_SCOPE_BOUNDARY_OK:True`
- `ADAPTER2_WPF_SAMPLE_PROOF_OK:True`
- `ADAPTER2_CANONICAL_ROUTE_OK:True`
- `ADAPTER2_SAMPLE_SCOPE_BOUNDARY_OK:True`
- `ADAPTER2_PERFORMANCE_ACCESSIBILITY_HANDOFF_OK:True`
- `ADAPTER2_RECIPE_ALIGNMENT_OK:True`
- `ADAPTER2_PROOF_BUDGET_OK:True`
- `ADAPTER2_VALIDATION_HANDOFF_OK:True`
- `ADAPTER2_VALIDATION_SCOPE_BOUNDARY_OK:True`
- `V060_MILESTONE_PROOF_OK:True`
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
- `ASTERGRAPH_TEMPLATE_SMOKE_OK:True`
- `TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK:True`
- `TEMPLATE_SMOKE_PLUGIN_CAPABILITY_SUMMARY_OK:True`
- `TEMPLATE_SMOKE_PLUGIN_TRUST_HASH_OK:True`
- `PUBLIC_API_SURFACE_OK:...:net9.0`
- `PUBLIC_API_SCOPE_OK:AsterGraph.Abstractions,AsterGraph.Core,AsterGraph.Editor,AsterGraph.Avalonia`
- `PUBLIC_API_GUIDANCE_OK:True`
- `SCALE_PERFORMANCE_BUDGET_OK:baseline:True:...`
- `SCALE_PERFORMANCE_BUDGET_OK:large:True:...`
- `SCALE_PERFORMANCE_BUDGET_OK:stress:True:...`
- `SCALE_AUTHORING_BUDGET_OK:stress:True:...`
- `SCALE_EXPORT_BUDGET:stress:svg<=300:png<=120000:jpeg<=100000:reload<=800`
- `SCALE_TIER_BUDGET:xlarge:nodes=10000:selection=512:moves=128:budget=informational-only`
- `SCALE_RASTER_EXPORT_STRESS_OK:True`
- `EXPORT_PROGRESS_OK:True`
- `EXPORT_CANCEL_OK:True`
- `EXPORT_SCOPE_OK:True`
- `EXPORT_SELECTION_SCOPE_OK:True`
- `SCALE_PERF_SUMMARY:stress:...`
- `SCALE_HISTORY_CONTRACT_OK:...`
- `COVERAGE_REPORT_OK:...`
- `ADOPTION_INTAKE_EVIDENCE_OK:True`
- `SUPPORT_BUNDLE_INTAKE_HANDOFF_OK:True`
- `REAL_EXTERNAL_REPORT_GATE_OK:True`
- `API_RELEASE_CANDIDATE_PROOF_OK:True`
- `PUBLIC_API_GUIDANCE_HANDOFF_OK:True`
- `RELEASE_BOUNDARY_STABILITY_OK:True`
- `ADOPTION_READINESS_HANDOFF_OK:True`
- `ADOPTION_SCOPE_BOUNDARY_OK:True`
- `V056_MILESTONE_PROOF_OK:True`
- `AUTHORING_DEPTH_HANDOFF_OK:True`
- `AUTHORING_DEPTH_SCOPE_BOUNDARY_OK:True`
- `V058_MILESTONE_PROOF_OK:True`
- `LARGE_GRAPH_UX_POLICY_OK:True`
- `LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True`
- `LARGE_GRAPH_UX_PROOF_BASELINE_OK:True`
- `VIEWPORT_LOD_POLICY_OK:True`
- `SELECTED_HOVERED_ADORNER_SCOPE_OK:True`
- `LARGE_GRAPH_BALANCED_UX_OK:True`
- `VIEWPORT_LOD_SCOPE_BOUNDARY_OK:True`
- `EDGE_INTERACTION_CACHE_OK:True`
- `EDGE_DRAG_ROUTE_SIMPLIFICATION_OK:True`
- `SELECTED_EDGE_FEEDBACK_OK:True`
- `EDGE_RENDERING_SCOPE_BOUNDARY_OK:True`
- `MINIMAP_LIGHTWEIGHT_PROJECTION_OK:True`
- `INSPECTOR_NARROW_PROJECTION_OK:True`
- `LARGE_GRAPH_PANEL_SCOPE_OK:True`
- `PROJECTION_PERFORMANCE_EVIDENCE_OK:True`
- `LARGE_GRAPH_UX_HANDOFF_OK:True`
- `V059_MILESTONE_PROOF_OK:True`
- `adoption-intake-dry-run.md` 里的合成 dry-run 记录只是维护者/内部预演；不要计入 3 到 5 条真实外部报告的门槛，也不要扩大 support/capability 声明
- 每条 beta intake 记录都包含报告类型、采用者上下文、route、version、proof 标记、摩擦点、support bundle 附件备注和 claim-expansion status；单条报告不会扩大公开声明

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
- 确认 proof summary 把 public API guidance proof 放在 template/plugin proof 附近：`ASTERGRAPH_TEMPLATE_SMOKE_OK:True`、`TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK:True`、`PUBLIC_API_SURFACE_OK:...:net9.0`、`PUBLIC_API_SCOPE_OK:AsterGraph.Abstractions,AsterGraph.Core,AsterGraph.Editor,AsterGraph.Avalonia` 和 `PUBLIC_API_GUIDANCE_OK:True`
- 确认 runtime feedback proof 仍是宿主自管并出现在 release notes：`RUNTIME_DEBUG_PANEL_INTERACTION_OK:True`、`RUNTIME_LOG_LOCATE_OK:True`、`RUNTIME_LOG_EXPORT_OK:True`、`AI_PIPELINE_MOCK_RUNNER_POLISH_OK:True`、`AI_PIPELINE_PAYLOAD_PREVIEW_OK:True` 和 `AI_PIPELINE_ERROR_DEBUG_EVIDENCE_OK:True`
- 确认 runtime feedback 文案不暗示 algorithm execution engine、workflow scripting UI、plugin marketplace、sandbox、WPF parity 或 GA / `1.0` 支持承诺
- 确认自动生成的 notes 和公告文案明确写出冻结的 support boundary 叙事和 adapter matrix 叙事，并同步给出 `HOSTED_ACCESSIBILITY_BASELINE_OK:True`、`HOSTED_ACCESSIBILITY_FOCUS_OK:True`、`HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`、`HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`、`HOSTED_ACCESSIBILITY_OK:True`、`ADAPTER2_PERFORMANCE_BASELINE_OK:True`、`ADAPTER2_EXPORT_BREADTH_OK:True`、`ADAPTER2_PROJECTION_BUDGET_OK:True:none`、`ADAPTER2_COMMAND_BUDGET_OK:True:none`、`ADAPTER2_SCENE_BUDGET_OK:True:none`、`ADAPTER2_VALIDATION_SCOPE_OK:True`、`ADAPTER2_MATRIX_HANDOFF_OK:True`、`ADAPTER2_SCOPE_BOUNDARY_OK:True`、`ADAPTER2_WPF_SAMPLE_PROOF_OK:True`、`ADAPTER2_CANONICAL_ROUTE_OK:True`、`ADAPTER2_SAMPLE_SCOPE_BOUNDARY_OK:True`、`ADAPTER2_PERFORMANCE_ACCESSIBILITY_HANDOFF_OK:True`、`ADAPTER2_RECIPE_ALIGNMENT_OK:True`、`ADAPTER2_PROOF_BUDGET_OK:True`、`ADAPTER2_VALIDATION_HANDOFF_OK:True`、`ADAPTER2_VALIDATION_SCOPE_BOUNDARY_OK:True`、`V060_MILESTONE_PROOF_OK:True`、`HELLOWORLD_WPF_OK:True`、`ADAPTER_CAPABILITY_MATRIX_FORMAT:1`、`ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`、`ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`
- 把 `HELLOWORLD_WPF_OK` 只当成 adapter-2 验证通过，不要写成 Avalonia/WPF parity 或公开 WPF support
- WPF support expansion 继续保持 validation-only，不要写进 public WPF support 口径；必须等 3-5 real external reports 聚焦在同一个受限风险后再讨论扩展
- 需要从受防守的 Avalonia accessibility proof 交接到 validation-only 的 WPF 验证时，统一参考 [Adapter-2 Accessibility Recipe](./adapter-2-accessibility-recipe.md)
- 需要从受防守的 Avalonia hosted metrics 交接到 validation-only 的 WPF performance 验证时，统一参考 [Adapter-2 Performance Recipe](./adapter-2-performance-recipe.md)
- 确认每条 beta 反馈都按同一套受限字段记录：报告类型、采用者上下文、`route`、`version`、proof 标记、摩擦点、support bundle 附件备注，以及 claim-expansion status
- 确认 claim-expansion status 在 3 到 5 条真实外部报告聚焦到同一个受限风险之前，只作为分诊输入
- GA prep checklist：adoption evidence、API drift、support boundary 和 release proof gate 都必须显式保留，之后才允许写 GA 或 `1.0` 公告口径
- 在 release messaging 里重复当前 `0.xx` alpha/beta hardening 线的 handoff：`Adoption Readiness / Release Candidate Hygiene` 表示先把公开推荐、API drift、support boundary 和 release proof gate 对齐，再写 release-candidate、GA 或 `1.0` 级别语言；同时包含 `ADOPTION_RECOMMENDATION_CURRENT_OK:True` 和 `CLAIM_HYGIENE_BOUNDARY_OK:True`
- 保持 `xlarge` 为 telemetry-only，不把它说成 10000 节点支持承诺或 virtualization commitment
- 如果配置了 `NUGET_API_KEY`，确认包发布成功
- 如果没有配置 `NUGET_API_KEY`，确认 workflow 是有意跳过 NuGet publish，而不是失败
- 不要再把 `v1.x` 风格的历史里程碑 checkpoint 当成当前公开包版本；对外统一以 [Versioning](./versioning.md) 为准
- release note 第一屏先写可安装包版本，再写与之匹配的公开 tag；`v1.x` 风格的旧 milestone 只作为历史说明补充出现
- `prerelease notes` / release messaging 中还必须核对并回填 `HOSTED_ACCESSIBILITY_BASELINE_OK:True`、`HOSTED_ACCESSIBILITY_FOCUS_OK:True`、`HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`、`HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`、`HOSTED_ACCESSIBILITY_OK:True`、`ADAPTER2_EXPORT_BREADTH_OK:True`、`HELLOWORLD_WPF_OK:True`、`ADAPTER_CAPABILITY_MATRIX_FORMAT:1`、`ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`、`ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`，避免只写成功 tag 而漏能力核对
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
- `tools/AsterGraph.Starter.Wpf` = validation-only adapter-2 组合验证样例，不是上手入口
- `tools/AsterGraph.HelloWorld.Wpf` = validation-only adapter-2 proof 样例，不代表 parity
- `tools/AsterGraph.HostSample` = 最小接入验证
- `tools/AsterGraph.PackageSmoke` = 打包消费验证
- `tools/AsterGraph.ScaleSmoke` = 规模基线加历史记录与状态连续性验证
- `src/AsterGraph.Demo` = 展示宿主
- `docs/zh-CN/versioning.md` = 包版本与历史仓库 tag 的对应说明
- `docs/zh-CN/project-status.md` = 外部能力就绪闸门，对应当前已被外部证据证明、仅验证通过或受边界约束、以及在更多采用者证据出现前继续延后的声明
- `docs/zh-CN/evaluation-path.md` = 从第一次安装到真实宿主 proof 的单一路径
- `docs/zh-CN/quick-start.md` = 推荐接入路径
- `docs/zh-CN/stabilization-support-matrix.md` = 冻结的 support boundary 和升级指引
- `docs/zh-CN/adapter-capability-matrix.md` = adapter matrix 叙事与验证矩阵
- `docs/zh-CN/alpha-status.md` = 历史 alpha 参考，服务于当前 beta support story
- `docs/zh-CN/advanced-editing.md` = advanced-editing capability split 与 proof map
- `docs/zh-CN/adoption-feedback.md` = 受限 public beta intake loop，以及 support-bundle 附件备注如何复用
- `docs/zh-CN/adopter-triage.md` = 对应一套 beta 证据合同的采用者分诊清单
- `docs/zh-CN/support-bundle.md` = support bundle 的本地 JSON 合同与采集流程
- `docs/zh-CN/adoption-intake-dry-run.md` = 只做内部预演的 synthetic fixture，不会扩大 support 声明
