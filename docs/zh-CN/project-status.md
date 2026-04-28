# AsterGraph 项目状态

## 当前状态

- 包版本基线：`0.11.0-beta`
- 与当前包版本配对的对外 SemVer prerelease 标签：`v0.11.0-beta`
- 历史仓库里程碑标签系列：`v1.x` 风格的公开前检查点
- 仓库阶段：公开 Beta（稳定化收口）
- v0.56 adoption-readiness handoff markers：`ADOPTION_READINESS_HANDOFF_OK:True`、`ADOPTION_SCOPE_BOUNDARY_OK:True` 和 `V056_MILESTONE_PROOF_OK:True`
- v0.58 authoring-depth handoff markers：`AUTHORING_DEPTH_HANDOFF_OK:True`、`AUTHORING_DEPTH_SCOPE_BOUNDARY_OK:True` 和 `V058_MILESTONE_PROOF_OK:True`
- v0.59 large-graph UX baseline markers：`LARGE_GRAPH_UX_POLICY_OK:True`、`LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True` 和 `LARGE_GRAPH_UX_PROOF_BASELINE_OK:True`
- v0.59 viewport LOD markers：`VIEWPORT_LOD_POLICY_OK:True`、`SELECTED_HOVERED_ADORNER_SCOPE_OK:True`、`LARGE_GRAPH_BALANCED_UX_OK:True` 和 `VIEWPORT_LOD_SCOPE_BOUNDARY_OK:True`
- v0.59 edge interaction markers：`EDGE_INTERACTION_CACHE_OK:True`、`EDGE_DRAG_ROUTE_SIMPLIFICATION_OK:True`、`SELECTED_EDGE_FEEDBACK_OK:True` 和 `EDGE_RENDERING_SCOPE_BOUNDARY_OK:True`
- 对外版本说明：[Versioning](./versioning.md)
- 当前公开支持的发布包：
  - `AsterGraph.Abstractions`
  - `AsterGraph.Core`
  - `AsterGraph.Editor`
  - `AsterGraph.Avalonia`
- 示例与验证工具：
  - `tools/AsterGraph.HelloWorld`：最快的 runtime-only 第一跑样例
  - `tools/AsterGraph.Starter.Avalonia`：shipped 的 Avalonia starter scaffold
  - `tools/AsterGraph.HelloWorld.Avalonia`：在 starter scaffold 之后的默认 UI 第一跑样例
  - `tools/AsterGraph.ConsumerSample.Avalonia`：介于 HelloWorld 和 Demo 之间的真实宿主样例
  - `tools/AsterGraph.HostSample`：这条 canonical adoption route 在 ladder 之后的 proof harness
  - `tools/AsterGraph.PackageSmoke`：打包消费验证
  - `tools/AsterGraph.ScaleSmoke`：公开的大图基线与状态连续性验证
- `adoption-intake-dry-run.md` 里的合成 dry-run fixtures 只是维护者/内部预演，不是外部验证，也不会扩大 support 或 capability 声明
- 推荐接入路径：
  - runtime-only 宿主使用 `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`
  - Avalonia UI 宿主使用 `AsterGraphEditorFactory.Create(...)` 加 `AsterGraphAvaloniaViewFactory.Create(...)`
- 当前已锁定的 adapter 2 验证目标：`WPF`，合同见 [Adapter Capability Matrix](./adapter-capability-matrix.md)；未写成 `Supported` 的行都不应被解读成与 Avalonia 已经对齐

## 已经足够对外评估的部分

- 四个可发布 SDK 包边界
- kernel/session-first 的运行时状态所有权
- 默认 Avalonia 壳层与 standalone surfaces
- trusted / loaded / blocked 的 runtime inspection surface
- command/trust timeline 和 perf overlay 这类 showcase surface
- demo scenario presets 保持为宿主自管 proof catalog entries：`DEMO_SCENARIO_PRESETS_OK:True`
- ConsumerSample snippets 保持为宿主自管 proof catalog entries：`GRAPH_SNIPPET_CATALOG_OK:True`、`GRAPH_SNIPPET_INSERT_OK:True`、`FRAGMENT_LIBRARY_SEARCH_OK:True`、`FRAGMENT_LIBRARY_PREVIEW_OK:True`、`FRAGMENT_LIBRARY_RECENTS_FAVORITES_OK:True`、`FRAGMENT_LIBRARY_SCOPE_BOUNDARY_OK:True`
- ConsumerSample authoring flow proof 继续绑定到现有 session commands：`AUTHORING_FLOW_PROOF_OK:True`、`AUTHORING_FLOW_HANDOFF_OK:True`、`AUTHORING_FLOW_SCOPE_BOUNDARY_OK:True`
- experience handoff proof 会在不扩大支持边界的前提下汇总：`EXPERIENCE_POLISH_HANDOFF_OK:True`、`FEATURE_ENHANCEMENT_PROOF_OK:True`、`EXPERIENCE_SCOPE_BOUNDARY_OK:True`
- authoring-depth proof 汇总 v0.58 port、validation、toolbar 和 fragment-library convenience polish，但不新增 runtime model 声明：`AUTHORING_DEPTH_HANDOFF_OK:True`、`AUTHORING_DEPTH_SCOPE_BOUNDARY_OK:True`、`V058_MILESTONE_PROOF_OK:True`
- large-graph UX baseline proof 汇总 hosted performance mode、LOD policy 和 widened-surface metrics，但不创建新的图规模支持层级：`LARGE_GRAPH_UX_POLICY_OK:True`、`LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True`、`LARGE_GRAPH_UX_PROOF_BASELINE_OK:True`
- viewport LOD proof 把 selected/hovered affordances 保持在 hosted workbench policy 内，而不是 runtime graph contract：`VIEWPORT_LOD_POLICY_OK:True`、`SELECTED_HOVERED_ADORNER_SCOPE_OK:True`、`LARGE_GRAPH_BALANCED_UX_OK:True`、`VIEWPORT_LOD_SCOPE_BOUNDARY_OK:True`
- edge interaction proof 在 hosted proof route 上汇总现有 edge quick-tool、toolbar 和 geometry 证据，但不创建 runtime renderer contract：`EDGE_INTERACTION_CACHE_OK:True`、`EDGE_DRAG_ROUTE_SIMPLIFICATION_OK:True`、`SELECTED_EDGE_FEEDBACK_OK:True`、`EDGE_RENDERING_SCOPE_BOUNDARY_OK:True`
- 图面可用性 proof marker：
  - `COMMAND_SURFACE_OK:True`
  - `TIERED_NODE_SURFACE_OK:True`
  - `FIXED_GROUP_FRAME_OK:True`
  - `NON_OBSCURING_EDITING_OK:True`
  - `VISUAL_SEMANTICS_OK:True`
- advanced-editing 收口 marker：
  - `HIERARCHY_SEMANTICS_OK:True`
  - `COMPOSITE_SCOPE_OK:True`
  - `EDGE_NOTE_OK:True`
  - `EDGE_GEOMETRY_OK:True`
  - `DISCONNECT_FLOW_OK:True`
- plugin discovery、trust policy、loading、inspection
- `IGraphEditorSession.Automation`
- contract、maintenance、release 验证 lanes
- release lane 里的 `.NET 10` 打包 `HostSample` 兼容性验证
- public API guidance proof 继续和 template/plugin proof 放在同一条 release story：`PUBLIC_API_SURFACE_OK`、`PUBLIC_API_SCOPE_OK`、`PUBLIC_API_GUIDANCE_OK`、`ASTERGRAPH_TEMPLATE_SMOKE_OK`、`TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK`

## 当前优先事项

当前对外仓库的重点，是把 public beta 收口成一套连贯可评估的 SDK surface，而不是一堆分散功能：

- 对外文档入口集中在 `README.md`、`README.zh-CN.md`、`docs/en`、`docs/zh-CN`
- advanced editing 要继续被描述成 canonical capability modules，而不是 retained-only 行为
- 源码、测试、sample、proof tool、workflow、governance 文件继续公开保留
- 内部工作流痕迹和本地环境文件不再作为公开仓库跟踪内容

## 近期路线

- 继续保持 canonical runtime/session surface 稳定，同时扩展 official capability modules 与 proof guidance
- 在 advanced editing 收口时保持 public beta 文档和验证指引清晰可执行
- 继续维护托管 CI 与核心验证 lane 的一致性
- 在不突然 breaking 的前提下继续保留兼容迁移窗口
- 让 shipped starter scaffold、runtime inspection surface、command/trust timeline 和 perf overlay 继续对齐 canonical session-first 路线
- 在不扩 runtime surface 的前提下继续验证 `WPF` 作为 adapter 2，并用 `supported` / `partial` / `fallback` 发布 Avalonia/WPF 的公开状态；不要把 `partial` / `fallback` 写成 parity

## 外部能力就绪闸门

当 release notes、维护者回复或 beta intake 需要回答“当前到底有哪些能力已经被外部证据证明”时，就统一引用这一节。维护者种子预演证据记录在 adoption feedback loop 里，但只有同一个受限风险上的真实外部报告才计入 3 到 5 的门槛。下面每条公开声明都必须回到路线级证据，而不是内部信心或 parity 想象。

用于这个门禁的每条真实外部报告，都必须带同一套 intake 词汇：报告类型、采用者上下文、route、version、proof 标记、摩擦点、support bundle 附件备注和 claim-expansion status。单条报告不会扩大公开声明；扩大 support 或 capability 需要 3 到 5 条真实外部报告聚焦在同一个受限风险上。

当前 intake gate markers：`ADOPTION_INTAKE_EVIDENCE_OK:True`、`SUPPORT_BUNDLE_INTAKE_HANDOFF_OK:True` 和 `REAL_EXTERNAL_REPORT_GATE_OK:True`。

### 当前已被外部证据证明

| 声明 | 路线级证据 |
| --- | --- |
| canonical runtime/session 路线和维护中的评估阶梯，已经在当前防守住的 beta 线上被外部证据证明。 | `tools/AsterGraph.HelloWorld`、`tools/AsterGraph.Starter.Avalonia`、`tools/AsterGraph.HelloWorld.Avalonia`、`tools/AsterGraph.ConsumerSample.Avalonia`、`tools/AsterGraph.HostSample`、`HOST_SAMPLE_OK`、`CONSUMER_SAMPLE_OK`、`GRAPH_SNIPPET_CATALOG_OK`、`GRAPH_SNIPPET_INSERT_OK`、`FRAGMENT_LIBRARY_SEARCH_OK`、`FRAGMENT_LIBRARY_PREVIEW_OK`、`FRAGMENT_LIBRARY_RECENTS_FAVORITES_OK`、`FRAGMENT_LIBRARY_SCOPE_BOUNDARY_OK`、`AUTHORING_FLOW_PROOF_OK`、`AUTHORING_FLOW_HANDOFF_OK`、`AUTHORING_FLOW_SCOPE_BOUNDARY_OK`、`EXPERIENCE_POLISH_HANDOFF_OK`、`FEATURE_ENHANCEMENT_PROOF_OK`、`EXPERIENCE_SCOPE_BOUNDARY_OK`、`AUTHORING_DEPTH_HANDOFF_OK`、`AUTHORING_DEPTH_SCOPE_BOUNDARY_OK`、`V058_MILESTONE_PROOF_OK` |
| showcase authoring surface 和宿主自管 runtime feedback 已经作为有边界的 beta 宿主体验被外部证据证明。 | `src/AsterGraph.Demo`、`tools/AsterGraph.ConsumerSample.Avalonia`、`DEMO_OK:True`、`DEMO_SCENARIO_PRESETS_OK:True`、`COMMAND_SURFACE_OK:True`、`COMPOSITE_SCOPE_OK:True`、`EDGE_NOTE_OK:True`、`EDGE_GEOMETRY_OK:True`、`DISCONNECT_FLOW_OK:True`、`RUNTIME_DEBUG_PANEL_INTERACTION_OK:True`、`RUNTIME_LOG_LOCATE_OK:True`、`RUNTIME_LOG_EXPORT_OK:True`、`AI_PIPELINE_MOCK_RUNNER_POLISH_OK:True`、`AI_PIPELINE_PAYLOAD_PREVIEW_OK:True`、`AI_PIPELINE_ERROR_DEBUG_EVIDENCE_OK:True` |
| 打包后的 consumer proof 已被外部证据证明，而且没有扩大 SDK 边界。 | `tools/AsterGraph.PackageSmoke`、`PACKAGE_SMOKE_OK`、`HOST_SAMPLE_NET10_OK` |
| Scale proof 已在 defended `baseline`/`large` 层级和 5000 节点 `stress` 上被外部证据证明：performance、authoring、SVG export、保守 PNG/JPEG raster export 和 reload 受防守。 | `tools/AsterGraph.ScaleSmoke`、`SCALE_PERFORMANCE_BUDGET_OK:baseline:True`、`SCALE_PERFORMANCE_BUDGET_OK:large:True`、`SCALE_PERFORMANCE_BUDGET_OK:stress:True`、`SCALE_EXPORT_BUDGET:stress:svg<=300:png<=120000:jpeg<=100000:reload<=800`、`SCALE_RASTER_EXPORT_STRESS_OK:True` |

### 仅验证通过或受边界约束的声明

| 声明 | 当前公开口径 | 路线级证据 |
| --- | --- | --- |
| `WPF` 作为 adapter 2 | 只算 validation-only，不代表 Avalonia parity，也不是 public WPF support。WPF support expansion 必须等 3-5 real external reports 聚焦在同一个受限风险后才能讨论。当前证据只覆盖有边界的 hosted shell accessibility、performance 和 export-breadth 路径。 | `HELLOWORLD_WPF_OK`、`HOSTED_ACCESSIBILITY_BASELINE_OK`、`HOSTED_ACCESSIBILITY_FOCUS_OK`、`HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK`、`HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK`、`HOSTED_ACCESSIBILITY_OK`、`ADAPTER2_PERFORMANCE_BASELINE_OK`、`ADAPTER2_EXPORT_BREADTH_OK`、`ADAPTER2_PROJECTION_BUDGET_OK`、`ADAPTER2_COMMAND_BUDGET_OK`、`ADAPTER2_SCENE_BUDGET_OK`、`ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`、`ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`、[Adapter Capability Matrix](./adapter-capability-matrix.md) |
| retained 路线 | 只作为迁移桥，不是新的 primary host path。 | [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)、[稳定化支持矩阵](./stabilization-support-matrix.md) |
| stress raster export budget | 5000 节点 PNG/JPEG export 有保守 defended 红线；这是防回归 guard，不是 fast-export 声明。 | `SCALE_EXPORT_BUDGET:stress:svg<=300:png<=120000:jpeg<=100000:reload<=800`、`SCALE_RASTER_EXPORT_STRESS_OK:True`、[ScaleSmoke 基线](./scale-baseline.md) |
| XLarge telemetry | 10000 节点 ScaleSmoke 只是 telemetry-only，不是支持承诺或 virtualization 声明。 | `SCALE_TIER_BUDGET:xlarge:nodes=10000:selection=512:moves=128:budget=informational-only`、[ScaleSmoke 基线](./scale-baseline.md) |

### 在更多采用者证据出现前继续延后

- 超出当前保守 5000 节点 `stress` raster export gate 的更快 defended 声明
- 除 Avalonia 加当前 `WPF` 验证通道之外的新 hosted adapter 或更宽的 adapter 声明
- marketplace、远程安装/更新、unload lifecycle、sandboxed plugin 这类故事
- algorithm execution engine 或 workflow scripting UI 这类超出宿主自管 runtime feedback 展示证据的故事
- stable / GA / `1.0` 级别的支持保证
- GA prep checklist：adoption evidence、API drift、support boundary 和 release proof gate 都复核通过后，才允许写 GA 或 `1.0` 级别语言。
- Release-candidate proof handoff markers：`API_RELEASE_CANDIDATE_PROOF_OK:True`、`PUBLIC_API_GUIDANCE_HANDOFF_OK:True` 和 `RELEASE_BOUNDARY_STABILITY_OK:True`。
- 当前 `0.xx` alpha/beta hardening 线命名为 `Adoption Readiness / Release Candidate Hygiene`：先把公开推荐、API drift、support boundary 和 release proof gate 对齐，再写 release-candidate、GA 或 `1.0` 级别语言；`ADOPTION_RECOMMENDATION_CURRENT_OK:True` 和 `CLAIM_HYGIENE_BOUNDARY_OK:True` 是 proof handoff markers
- 维护者种子预演证据不计入 3 到 5 的门槛
- 如果新的报告放不进上面的“已证明”或“受边界约束”两类，就走 [Adoption Feedback Loop](./adoption-feedback.md) 和 [Beta Support Bundle](./support-bundle.md)，不要临时扩大公开声明；在满足 3 到 5 条真实外部报告门禁前，claim-expansion status 只作为分诊字段

## 公开入口分工

这条 hosted route ladder 是 `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`；`HostSample` 放在这条 ladder 之后，作为 proof harness。

- `tools/AsterGraph.HelloWorld` = runtime-only 第一跑样例
- `tools/AsterGraph.Starter.Avalonia` = shipped 的 Avalonia starter scaffold
- `tools/AsterGraph.HelloWorld.Avalonia` = 在 starter scaffold 之后的默认 UI 第一跑样例
- `tools/AsterGraph.ConsumerSample.Avalonia` = 真实 hosted-UI 宿主样例
- `tools/AsterGraph.Starter.Wpf` = validation-only adapter-2 组合验证样例
- `tools/AsterGraph.HelloWorld.Wpf` = validation-only adapter-2 proof 样例
- `tools/AsterGraph.HostSample` = 这条 ladder 之后的 canonical adoption proof
- `tools/AsterGraph.PackageSmoke` = 打包消费验证
- `tools/AsterGraph.ScaleSmoke` = 大图基线加历史记录与状态连续性验证
- `src/AsterGraph.Demo` = 可视化展示宿主

## 对外入口

- [Versioning](./versioning.md)
- [公开 Beta 评估路径](./evaluation-path.md) = 从第一次安装到真实宿主 proof 的单一路径
- [稳定化支持矩阵](./stabilization-support-matrix.md)
- [Quick Start](./quick-start.md)
- [Consumer Sample](./consumer-sample.md)
- [ScaleSmoke 基线](./scale-baseline.md)
- [Advanced Editing Guide](./advanced-editing.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
- [Plugin 与自定义节点 Recipe](./plugin-recipe.md)
- [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)
- [Alpha 状态](./alpha-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
- [Host Integration](./host-integration.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
- [Demo Guide](./demo-guide.md)
