# Phase 0 React Flow 对齐审计

## Phase 478 刷新

本文最初是基于 `0.11.0-beta` 的 Phase 0 审计。Phase 478 按当前 `master` 状态重新校准，避免后续 issue wave 重复 GitHub #46-#52 以及它们的后续子任务已经完成的工作。

本次刷新只修改文档和 tracker 状态，不修改产品代码、运行时行为、渲染器契约或公开支持声明。

## Phase 481 更新

Phase 481 在现有 scene PNG gate 之外新增了确定性的 full-window Cookbook shell capture。新 gate 覆盖 default Cookbook shell route，把 artifact 写到 `artifacts/test-results/cookbook-shell-visual-gate`，并验证 dimensions、nonblank pixels、shell-part coverage 和 metadata。严格 pixel-baseline comparison 仍然等 Skia/native drift 在 CI hosts 上测量清楚后再做。

## Phase 479 更新

Phase 479 在 `AsterGraph.Avalonia.Presentation` 中新增 `NodeDragHandle`，作为 hosted Avalonia 路线下 React Flow-style 节点 drag handle 的公开入口。宿主在 stock-shell custom node body presenter 中标记一个 control；stock node shell 只允许从该 handle 或其 descendant 启动节点拖动，不依赖 Demo-only private hook。

## 仓库基线

- 当前包版本仍是 `Directory.Build.props` 中的 `0.11.0-beta`；可发布库为 `AsterGraph.Abstractions`、`AsterGraph.Core`、`AsterGraph.Editor` 和 `AsterGraph.Avalonia`。
- 可发布库目标框架为 `net8.0;net9.0;net10.0`；Demo 和测试项目当前目标框架为 `net9.0`。
- `AsterGraph.Demo` 仍是可见 demo host 和 Cookbook proof surface。
- `src/AsterGraph.Demo/Cookbook/DemoCookbookCatalog.Recipes.cs` 当前包含 25 个 `DemoCookbookRecipe` 条目。
- `tests/AsterGraph.Demo.Tests/CookbookScreenshotGateRoutes.json` 当前定义 15 条确定性的 scene PNG 捕获路线。
- `tests/AsterGraph.Demo.Tests/DemoCookbookScreenshotGateTests.cs` 通过 `GraphSceneImageExportService` 捕获规范 graph scene，写入 `artifacts/test-results/cookbook-screenshot-gate/metadata.json`，验证 route metadata，并且现在会把 default Cookbook full-window shell 捕获到 `artifacts/test-results/cookbook-shell-visual-gate`。
- `.planning/` 和 `docs/plans/` 在本仓库中被 gitignore。Phase 478 将 GitHub #79 和 Beads `avalonia-node-map-p478` 中的 `.planning/*` write set 视为 tracker drift；本 slice 的持久规划状态放在 tracked docs 与 GitHub/Beads issues 中。

## 架构盘点

| 层 | 当前 owner | 当前证据 | Phase 478 评估 |
| --- | --- | --- | --- |
| Host-facing identifiers and contracts | `src/AsterGraph.Abstractions` | Node definitions、catalogs、styling、builder helpers、compatibility policies 和 plugin-neutral extension points。 | 仍是 host 的最低依赖层；新增 parity contract 应保持薄而明确。 |
| 持久化模型和序列化 | `src/AsterGraph.Core` | `GraphDocument`、groups、scopes、edge presentation、schema migration helpers 和 legacy import boundary。 | #48 的兼容面清理已完成；新增模型字段例如 rotation 必须先说明 schema 行为。 |
| Session、commands、queries、state services | `src/AsterGraph.Editor` | Selection、undo/redo、clipboard、workspace save/load、connection authoring、validation、layout、events、mutations 和 presentation snapshots。 | canonical route 已经较强；后续 parity 工作应走 commands/queries，不绕开 session ownership。 |
| Layout integration | `src/AsterGraph.Editor/Runtime` 和 services | `GraphLayoutRequest`、`GraphLayoutPlan`、`IGraphLayoutProvider`、preview/apply/cancel evidence 和 snap/grid commands。 | Provider seam 已存在；扩展 layout 声明时仍需明确 background/cancel/provider proof。 |
| Avalonia rendering and interaction | `src/AsterGraph.Avalonia` | `NodeCanvas`、scene host projection、edge renderer、interaction coordinators、automation peers、MiniMap、Background、Controls、Panel、NodeToolbar、EdgeToolbar、NodeResizer 和 `NodeDragHandle`。 | Built-ins 已成为可复用 public components。剩余 UI parity 缺口集中在 rotation 和更广的 full-window visual regression。 |
| Hosted workbench shell | `GraphEditorView` 和 hosted factories | Header、library、canvas、inspector、validation panel、authoring tools、minimap、command projection 和 status chrome。 | 是有用的 supported route，但新增公开功能不应把 Demo shell chrome 变成隐藏依赖。 |
| Demo/Cookbook | `src/AsterGraph.Demo` | 25 个 recipes、route clarity docs、built-in/interaction/lifecycle batches、scene PNG screenshot gate metadata 和 default full-window shell visual metadata。 | Cookbook breadth、scene coverage 和第一条 shell-level capture 已覆盖。Pixel-baseline comparison 与更广的 shell-state coverage 仍作为后续有界工作。 |
| CI and release gates | `.github/workflows`、`eng/ci.ps1`、test projects | Build/test/maintenance/contract/release/hygiene lanes、public API baseline、package validation、docs route checks、scene PNG gate tests 和 default full-window shell gate tests。 | Text/API/scene/shell gates 较强。严格 pixel baseline 继续等 deterministic drift 测量后再做。 |

## React Flow 对齐矩阵

| 能力 | 当前状态 | 证据 / 剩余缺口 | 下一步 |
| --- | --- | --- | --- |
| Custom nodes with arbitrary Avalonia content | Partial / AsterGraph idiom 下支持 | Node definitions、`IGraphNodeVisualPresenter`、authoring-surface docs、templates 和 hosted controls 已存在。公开故事仍不如 React Flow custom node component 直接。 | 用具体 host-owned visual presenter 示例继续补文档。 |
| Node drag handles | Present / guarded | `NodeDragHandle` 为 custom node visual/body presenter 暴露 public Avalonia attached property。Focused headless tests 覆盖从标记 handle 拖动，以及存在 handle 时未标记 body surface 不启动拖动。 | 继续用 `StandaloneCanvas_NodeDragHandle_*` tests 和 public API baseline checks 守住。 |
| Node resizer | Present | `NodeResizer`、`TrySetNodeSize`、built-in component catalog、Cookbook route 和 focused tests 已存在。 | 继续用 built-in tests 与 screenshot route 守住。 |
| Rotatable nodes | Missing | 只发现 viewport transforms 和 selection transforms；未发现持久化 node rotation model、command、renderer contract 或 tests。 | GitHub #80 / `avalonia-node-map-p480`。 |
| Custom edges | Present / guarded | `GraphEdgePresentation`、connection geometry snapshots、route vertices、reconnect/edit commands、labels、path kinds、markers、animation flag 和 floating endpoints 已存在。 | Edge claims 变化时同步维护 API 与 renderer tests。 |
| Bezier、SmoothStep、Step、Straight edges | Present | `GraphEdgePathKind` 通过 `GraphEditorConnectionGeometryProjector` 映射为 route styles。 | Keep guarded。 |
| Animated and floating edges | Present / proof-bounded | `IsAnimated` 和 `UsesFloatingEndpoints` 已进入 edge presentation 与 geometry snapshots。视觉动画 proof 比静态 geometry proof 更窄。 | 保持声明边界；只有补足 screenshot/full-window evidence 后再扩大。 |
| Editable/reconnectable edges | Present | Reconnect、route-vertex commands 和 edge toolbar evidence 已存在。 | Keep guarded。 |
| Edge labels and markers | Present | Labels 与 source/target marker fields 已在 edge presentation/geometry snapshots 中建模。 | Keep guarded。 |
| Drag, pan, zoom | Present | Canvas pointer/wheel coordinators 与 viewport commands 已存在。 | Keep guarded。 |
| Marquee/box selection and multi-select | Present | `GetSelectionRectangleSnapshot`、overlay coordinator marquee selection、`SelectAll`、`SelectNone`、`InvertSelection` 有 tests 和 Cookbook routes。 | Keep guarded。 |
| Connection preview and validation | Present / guarded | Pending connection、compatible targets、validation snapshots、repair commands 和 `validation-prevent-cycle` fixture 已存在。Cycle prevention 由 canonical connection completion path 强制执行，并通过 pending snapshot 暴露 `GraphEditorPendingConnectionRejectionReason.WouldCreateCycle`。 | 继续守住 `RuntimeSession_CompleteConnection_RejectsDirectCycleWithStableReason`、`RuntimeSession_CompleteConnection_RejectsIndirectCycleThroughNormalCommandPath` 和 `RuntimeSession_TryExecuteCommand_RejectsCycleThroughConnectionsConnectRoute`。 |
| Context menu | Present | Menu descriptors 与 hosted context menu plumbing 已存在。 | Keep guarded。 |
| Undo/redo | Present | Session commands 与 history tests 覆盖正常 command semantics。 | Keep guarded。 |
| Copy/paste | Present | Clipboard commands、fragment serialization 和 compatibility payload tests 已存在。 | Keep guarded。 |
| Save/restore | Present | Workspace save/load commands、serializer contracts 和 compatibility import boundary 已存在。 | Keep guarded。 |
| Helper lines / snap guides | Present / proof-bounded | Snap/grid commands 和 projection evidence 已存在；文档声明应继续绑定 supported command route。 | Keep guarded。 |
| Prevent cycles | Present / guarded | Direct 和 indirect cycle 都会通过 `StartConnection` / `CompleteConnection` 以及 descriptor-driven `connections.connect` 被拒绝；拒绝时保留 pending connection、不修改 document，并以 `WouldCreateCycle` 作为稳定原因。Compatible-target queries 仍是 type-compatibility discovery；completion 才是最终 policy authority。 | 继续由 Phase 482 session tests 和 diagnostics contract tests 守住。 |
| MiniMap | Present | `GraphMiniMap`、`AsterGraphMiniMapViewFactory`、lightweight projection 和 Cookbook route 已存在。 | Keep guarded。 |
| Controls | Present | Standalone `AsterGraphControls`、hosted action factory、built-in catalog entry 和 tests 已存在。 | Keep guarded。 |
| Background | Present | `GridBackground`、style options、grid-density tests 和 Cookbook route 已存在。 | Keep guarded。 |
| Panel | Present | `AsterGraphPanel`、position enum、catalog entry、tests 和 Cookbook route 已存在。 | Keep guarded。 |
| NodeToolbar / EdgeToolbar | Present | Standalone `NodeToolbar` 与 `EdgeToolbar` 投影 canonical node/connection actions，并有 tests/routes。 | Keep guarded。 |
| Groups / subflows | Present / proof-bounded | Groups、composite scopes、hierarchy snapshots、promotion/expose commands 和 Cookbook route 已存在。 | Keep guarded。 |
| Auto layout integration | Partial | `IGraphLayoutProvider`、preview/apply/cancel、snap-to-grid 和 route evidence 已存在。Provider examples 与 long-running cancellation semantics 仍需明确。 | 除非 adopter evidence 提升优先级，否则排在高风险 parity gap 之后。 |
| Virtualization / thousands of nodes | Partial / bounded | Scale baseline、visible-scene projection、MiniMap lightweight projection 和 hosted performance policy 已存在。尚未证明真正的 renderer virtualization contract，xlarge evidence 也只是 telemetry-only。 | GitHub #82 / `avalonia-node-map-p483`。 |
| Declarative + code-first API | Partial | Host builder、definitions、builders、templates 和 session APIs 已存在。Avalonia markup-first ergonomics 不等同 React Flow hooks/components。 | 文档保持诚实，不声明 React hook parity。 |
| Accessibility breadth | Partial | Keyboard navigation 与 automation peers 有目标测试。所有 built-ins 与 shell states 的广泛 accessibility audit 仍偏弱。 | 仅在 adopter/release evidence 需要时再开 issue。 |
| Host events | Present | `IGraphEditorEvents`、mutation batching 和 host-event Cookbook route 已存在。 | Keep guarded。 |
| Screenshot-driven UI quality | Partial / guarded | Scene PNG gate 已覆盖规范 graph scenes。Default Cookbook full-window shell route 现在会捕获 host menu、drawer、left navigation、graph host 和 recipe panel metadata。Pixel-baseline comparison 与更广的 flyout/shell-state coverage 尚未覆盖。 | #83 先保持为第一条 shell gate；只有 visual drift evidence 需要时再追加更广的 baseline follow-up。 |

## 已完成的 Phase 0 Issue Wave

原始 first wave 不再是下一轮工作队列。下面这些 tracker 项只作为历史上下文：

| GitHub | Bead | 结果 |
| --- | --- | --- |
| #46 | `avalonia-node-map-vqt` | 原始 Phase 0 parity audit 和 cleanup roadmap。 |
| #47 | `avalonia-node-map-x24` | Public docs 与 removed sample route reconciliation。 |
| #48 | `avalonia-node-map-25d` | Retained compatibility surface cleanup 和 public metadata update。 |
| #49 | `avalonia-node-map-dui` | Runtime/model/session/rendering boundary split。 |
| #50 | `avalonia-node-map-3qs` | React Flow-grade edge model and renderer parity。 |
| #51 | `avalonia-node-map-y1e` | Built-in component catalog 和 reusable Avalonia component wave。 |
| #52 | `avalonia-node-map-a08` | Cookbook expansion 和 screenshot-gate parent。 |
| #59、#61、#63、#65 | `avalonia-node-map-a08.*` | Scene screenshot foundation，以及 built-in、interaction、lifecycle Cookbook batches。 |
| #67、#69、#71、#73、#75、#77 | `avalonia-node-map-y1e.*` | Built-in catalog、NodeToolbar、EdgeToolbar、NodeResizer、Panel、Controls，以及 standalone built-ins 的 screenshot coverage。 |
| #83 | `avalonia-node-map-p481` | 第一条 full-window Cookbook shell visual gate，带 artifact metadata 和 CI lane coverage。 |
| #84 | `avalonia-node-map-p482` | Guarded cycle-prevention connection policy。 |
| #81 | `avalonia-node-map-p479` | 通过 `NodeDragHandle` 提供 public hosted-Avalonia node drag-handle API。 |

## 下一轮 Issue Wave

| GitHub | Bead | 标题 | 优先级 | 可能 write set | 并行边界 |
| --- | --- | --- | --- | --- | --- |
| #80 | `avalonia-node-map-p480` | Phase 480: add rotatable node model and rendering contract | P2 | `AsterGraph.Core`、`AsterGraph.Editor`、`AsterGraph.Avalonia`、serialization/API/tests/docs | 与 drag-handle 分开，因为两者都可能触碰 node hit testing 与 adorners。 |
| #82 | `avalonia-node-map-p483` | Phase 483: prove or bound large-graph rendering virtualization | P1 | 若实现则涉及 `AsterGraph.Avalonia` renderer/projection；否则涉及 scale docs/tests | 先作为 evidence/decision branch 启动，不要直接重写 renderer。 |

Phase 479 之后的推荐下一条分支：如果下一条可以触碰 node transform 与 hit testing，做 `avalonia-node-map-p480` / GitHub #80 的 rotatable nodes；如果下一条要保持 evidence/docs-first，则做 `avalonia-node-map-p483` / GitHub #82 的 virtualization claim 边界。

## 推荐并行 Worktree 计划

- `docs/phase478-parity-refresh`：只负责本审计刷新、中文镜像、tracker split 和 Phase 478 关闭。
- `feature/cycle-prevention-policy`：负责 #84 / `avalonia-node-map-p482`，除文档/示例外避免 Avalonia shell 工作。
- `feature/rotatable-nodes`：负责 #80 / `avalonia-node-map-p480`，除非明确批准共享 transform abstraction，否则不要混入 drag-handle API。
- `perf/rendering-virtualization-boundary`：负责 #82 / `avalonia-node-map-p483`，先证明或收紧声明边界，再碰 renderer internals。

## UI 验证策略

未来所有触碰 `src/AsterGraph.Avalonia` 或 `src/AsterGraph.Demo` 的 UI 变更应提供：

- 针对 control state、layout contract 或 interaction state 的 focused headless Avalonia test；
- 影响 graph scene rendering 时，通过 Cookbook screenshot gate 生成 deterministic scene PNG；
- 影响 Cookbook scene 的 visual PR 需要附 before/after PNG artifacts 和 `metadata.json`；
- 每个新的 public UI component 或 interaction 都应有 Cookbook route；
- 如果 UI 变更只是 structural-only 且不改变像素，需要显式说明。

当前覆盖包含 scene-level route captures 和第一条 default full-window Cookbook shell capture。`DemoCookbookScreenshotGateTests` 和 `CookbookScreenshotGateRoutes.json` 能证明规范 graph scenes、route metadata 和 default hosted shell artifact；它们仍不提供严格 pixel-baseline comparisons 或广泛 flyout/state coverage。

## Tracker 备注

- GitHub #79 和 Beads `avalonia-node-map-p478` 创建时把 `.planning/*` 列入 write set。由于 `.planning/` 被 ignore 且当前 worktree 中不存在，本次刷新将它记录为 tracker drift，而不是 force-add 本地 planning 文件。
- Beads 是本仓库的持久本地 tracker。刷新后的下一轮 wave 已同时创建 Beads ID 与 GitHub issue 链接。
- Phase 478 不修改产品代码。
