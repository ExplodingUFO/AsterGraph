# Phase 0 React Flow 对齐审计

## Phase 478 刷新

本文最初是基于 `0.11.0-beta` 的 Phase 0 审计。Phase 478 按当前 `master` 状态重新校准，避免后续 issue wave 重复 GitHub #46-#52 以及它们的后续子任务已经完成的工作。

本次刷新只修改文档和 tracker 状态，不修改产品代码、运行时行为、渲染器契约或公开支持声明。

## Phase 490 更新

Phase 490 是 GitHub #103 / `avalonia-node-map-3x0`，用于在 Phases 485-489 全部关闭后做 stale-roadmap repair。本 slice 是 docs/tests only：更新中英文 parity roadmap，把已完成工作移出候选队列，并选择 Accessibility breadth audit 是下一项 open parity gap。No Core/Editor/Avalonia runtime or public API changes。

## Phase 491 更新

Phase 491 是 GitHub #105 / `avalonia-node-map-44i`，承接 Phase 490 选出的 accessibility breadth audit。本 slice 仍然是 docs/tests first：按现有 automation name、focusability、keyboard routes 和 intentionally decorative surfaces 审计 public Avalonia built-ins 与 hosted shell states。不授权 retained migration removal、public API changes、visual redesign，也不在缺少人工 assistive-technology 验证时声明 full screen-reader certification。

## Phase 489 更新

Phase 489 通过 PR #102 关闭 GitHub #101 / `avalonia-node-map-6sc`，完成 `perf/renderer-virtualization-spike` 分支上的 renderer virtualization design spike。本 slice 只做 docs/tests：先定义未来声明 ItemsRepeater/Skia-style renderer virtualization、background graph index 或扩大 graph-size claim 前必须满足的 proof contract。不做 public API change，也不做 runtime change。当前证据仍只支持 viewport-budgeted scene projection/rendering，不是真正的 renderer virtualization contract；`xlarge` 继续保持 telemetry-only。

## Phase 488 更新

Phase 488 通过 PR #100 关闭 GitHub #99 / `avalonia-node-map-ce1`，完成 P2 的 layout provider 与 background/cancel proof refresh。本 slice 先做 docs/tests：把 `IGraphLayoutProvider`、`PreviewLayoutPlan`、`TryApplyLayoutPlan`、`TryApplyLayoutRequest`、snap-to-grid commands 和 `background-grid-density` route 继续绑定到现有 source-backed proof。不要在这个 issue 中把同步 provider seam 扩大成异步或可取消的 layout execution。

## Phase 487 更新

Phase 487 通过 PR #98 关闭 GitHub #97 / `avalonia-node-map-i8s`，强化 custom-node copyable-host recipe docs 与 templates。本 slice 只触碰 host recipe docs、templates、Demo cookbook route text 和 consistency tests，没有修改 Core/Editor/Avalonia API。

## Phase 486 更新

Phase 486 通过 PR #96 关闭 GitHub #95 / `avalonia-node-map-0xr`，扩展 full-window shell gate，但不加入 strict pixel baselines，也不改 Demo runtime。`CookbookShellVisualGateStates.json` 现在基于同一条 default `starter-host-route` / `ai-pipeline` fixture 驱动两条确定性 shell state：`shell-cookbook-default-open` 和 `shell-runtime-diagnostics-open`。这个 gate 会验证 drawer state、dimensions、nonblank pixels、distinct colors、output metadata 和 required named shell parts；flyouts、context menus、language/theme variants 和 hash baselines 仍然不在本阶段范围内。

## Phase 485 更新

Phase 485 通过 PR #94 关闭 GitHub #93 / `avalonia-node-map-3hc`，澄清 Cookbook host-copy architecture path。完成内容记录 guarded host-copy architecture route 和双语文档指引，不修改 Core/Editor/Avalonia runtime API。

## Phase 484 更新

Phase 484 在 Phase 478-483 wave 全部关闭、Phase 480 rotatable-node surface 已合入后重新刷新路线图。此前的下一轮队列已经过期，因为 GitHub #80 / `avalonia-node-map-p480` 已关闭。本次更新保持 docs-only，改用优先级明确的拆分计划替换单项队列，并继续把公开声明绑定到当前 source、tests、docs 和 CI 证据。

## Phase 481 更新

Phase 481 在现有 scene PNG gate 之外新增了确定性的 full-window Cookbook shell capture。新 gate 覆盖 default Cookbook shell route，把 artifact 写到 `artifacts/test-results/cookbook-shell-visual-gate`，并验证 dimensions、nonblank pixels、shell-part coverage 和 metadata。严格 pixel-baseline comparison 仍然等 Skia/native drift 在 CI hosts 上测量清楚后再做。

## Phase 480 更新

Phase 480 通过新增明确的持久化节点旋转 surface contract 关闭 GitHub #80。受支持路线是持久化模型中的 `GraphNodeSurfaceState.RotationDegrees`、用于 mutation 的 `IGraphEditorCommands.TrySetNodeRotation(...)`，以及宿主投影中的 `GraphEditorNodeSurfaceSnapshot.RotationDegrees`。`GraphNodeRotationGeometry` 仍是 geometry helper；公开指引要求宿主优先使用 command/query state contract，而不是把 helper 当作主要集成 API。

## Phase 479 更新

Phase 479 在 `AsterGraph.Avalonia.Presentation` 中新增 `NodeDragHandle`，作为 hosted Avalonia 路线下 React Flow-style 节点 drag handle 的公开入口。宿主在 stock-shell custom node body presenter 中标记一个 control；stock node shell 只允许从该 handle 或其 descendant 启动节点拖动，不依赖 Demo-only private hook。

## Phase 483 更新

Phase 483 通过选择 bounded-docs 路径关闭 GitHub #82，而不是重写 renderer。当前证据支持 viewport-budgeted scene projection/rendering contract：visible-scene projector 计算 node/group/connection ID，`NodeCanvasSceneHost` 在 viewport dimensions 和 zoom 可用时 materialize 可见 node/group visual，`NodeCanvasConnectionSceneRenderer` 对已提交 connection route 做 scope，同时保留 pending preview。这不是真正的 renderer virtualization contract，也不是 10000 节点支持层级；`xlarge` 仍是 telemetry-only。

## 仓库基线

- 当前包版本仍是 `Directory.Build.props` 中的 `0.11.0-beta`；可发布库为 `AsterGraph.Abstractions`、`AsterGraph.Core`、`AsterGraph.Editor` 和 `AsterGraph.Avalonia`。
- 可发布库目标框架为 `net8.0;net9.0;net10.0`；Demo 和测试项目当前目标框架为 `net9.0`。
- `AsterGraph.Demo` 仍是可见 demo host 和 Cookbook proof surface。
- `src/AsterGraph.Demo/Cookbook/DemoCookbookCatalog.Recipes.cs` 当前包含 25 个 `DemoCookbookRecipe` 条目。
- `tests/AsterGraph.Demo.Tests/CookbookScreenshotGateRoutes.json` 当前定义 15 条确定性的 scene PNG 捕获路线。
- `tests/AsterGraph.Demo.Tests/DemoCookbookScreenshotGateTests.cs` 通过 `GraphSceneImageExportService` 捕获规范 graph scene，写入 `artifacts/test-results/cookbook-screenshot-gate/metadata.json`，验证 route metadata，并且会把 manifest-driven full-window shell states 捕获到 `artifacts/test-results/cookbook-shell-visual-gate`。
- `tests/AsterGraph.Demo.Tests/CookbookShellVisualGateStates.json` 当前定义 `shell-cookbook-default-open` 和 `shell-runtime-diagnostics-open`。
- `.planning/` 和 `docs/plans/` 在本仓库中被 gitignore。Phase 478 将 GitHub #79 和 Beads `avalonia-node-map-p478` 中的 `.planning/*` write set 视为 tracker drift；本 slice 的持久规划状态放在 tracked docs 与 GitHub/Beads issues 中。

## 架构盘点

| 层 | 当前 owner | 当前证据 | Phase 478 评估 |
| --- | --- | --- | --- |
| Host-facing identifiers and contracts | `src/AsterGraph.Abstractions` | Node definitions、catalogs、styling、builder helpers、compatibility policies 和 plugin-neutral extension points。 | 仍是 host 的最低依赖层；新增 parity contract 应保持薄而明确。 |
| 持久化模型和序列化 | `src/AsterGraph.Core` | `GraphDocument`、groups、scopes、edge presentation、schema migration helpers 和 legacy import boundary。 | #48 的兼容面清理已完成；新增模型字段例如 rotation 必须先说明 schema 行为。 |
| Session、commands、queries、state services | `src/AsterGraph.Editor` | Selection、undo/redo、clipboard、workspace save/load、connection authoring、validation、layout、events、mutations 和 presentation snapshots。 | canonical route 已经较强；后续 parity 工作应走 commands/queries，不绕开 session ownership。 |
| Layout integration | `src/AsterGraph.Editor/Runtime` 和 services | `GraphLayoutRequest`、`GraphLayoutPlan`、`IGraphLayoutProvider`、preview/apply evidence、command-surface cancel evidence 和 snap/grid commands。 | Provider seam 已存在且是同步契约；扩展 layout 声明时仍需明确 background/provider proof，且 cancellation wording 不能暗示 async layout cancellation。 |
| Avalonia rendering and interaction | `src/AsterGraph.Avalonia` | `NodeCanvas`、scene host projection、edge renderer、interaction coordinators、automation peers、MiniMap、Background、Controls、Panel、NodeToolbar、EdgeToolbar、NodeResizer、`NodeDragHandle` 和 stock rotation projection。 | Built-ins 已成为可复用 public components。剩余 UI parity 缺口集中在更广的 full-window visual regression、Cookbook 示例易用性，以及任何未来需要真正 renderer virtualization 证明的声明。 |
| Hosted workbench shell | `GraphEditorView` 和 hosted factories | Header、library、canvas、inspector、validation panel、authoring tools、minimap、command projection 和 status chrome。 | 是有用的 supported route，但新增公开功能不应把 Demo shell chrome 变成隐藏依赖。 |
| Demo/Cookbook | `src/AsterGraph.Demo` | 25 个 recipes、route clarity docs、built-in/interaction/lifecycle batches、scene PNG screenshot gate metadata，以及两条 manifest-driven full-window shell visual states。 | Cookbook breadth、scene coverage、default Cookbook drawer 和 runtime diagnostics drawer shell captures 已覆盖。Pixel-baseline comparison 与更广的 flyout/theme/language shell-state coverage 仍作为后续有界工作。 |
| CI and release gates | `.github/workflows`、`eng/ci.ps1`、test projects | Build/test/maintenance/contract/release/hygiene lanes、public API baseline、package validation、docs route checks、scene PNG gate tests 和 manifest-driven full-window shell gate tests。 | Text/API/scene/shell gates 较强。严格 pixel baseline 继续等 deterministic drift 测量后再做。 |

## React Flow 对齐矩阵

| 能力 | 当前状态 | 证据 / 剩余缺口 | 下一步 |
| --- | --- | --- | --- |
| Custom nodes with arbitrary Avalonia content | Partial / AsterGraph idiom 下支持 | Node definitions、`IGraphNodeVisualPresenter`、authoring-surface docs、templates 和 hosted controls 已存在。公开故事仍不如 React Flow custom node component 直接。 | 用具体 host-owned visual presenter 示例继续补文档。 |
| Node drag handles | Present / guarded | `NodeDragHandle` 为 custom node visual/body presenter 暴露 public Avalonia attached property。Focused headless tests 覆盖从标记 handle 拖动，以及存在 handle 时未标记 body surface 不启动拖动。 | 继续用 `StandaloneCanvas_NodeDragHandle_*` tests 和 public API baseline checks 守住。 |
| Node resizer | Present | `NodeResizer`、`TrySetNodeSize`、built-in component catalog、Cookbook route 和 focused tests 已存在。 | 继续用 built-in tests 与 screenshot route 守住。 |
| Rotatable nodes | Present / guarded | `GraphNodeSurfaceState.RotationDegrees`、`IGraphEditorCommands.TrySetNodeRotation(...)`、`GraphEditorNodeSurfaceSnapshot.RotationDegrees`、serializer compatibility、renderer/hit-test geometry、public API baseline 和中英文 host docs 已到位。 | 继续用 model/session/Avalonia tests、public API validation 和 command/query 指引守住。 |
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
| Auto layout integration | Partial | `IGraphLayoutProvider`、preview/apply、snap-to-grid、command-surface cancel evidence 和 route evidence 已存在。Provider example 由宿主拥有，当前 layout planning 是同步契约，不是 async-cancellable。 | Phase 488 保持 docs/tests proof 边界；除非 adopter evidence 证明需要新的 provider contract issue。 |
| Virtualization / thousands of nodes | Partial / bounded | Scale baseline、visible-scene projection、MiniMap lightweight projection 和 hosted performance policy 已存在。当前被防守的是 viewport-budgeted scene projection/rendering contract，不是 ItemsRepeater/Skia-style renderer virtualization，xlarge evidence 也只是 telemetry-only。 | 继续维护 scale docs 和 renderer projection tests；扩大声明前先开新 issue。 |
| Declarative + code-first API | Partial | Host builder、definitions、builders、templates 和 session APIs 已存在。Avalonia markup-first ergonomics 不等同 React Flow hooks/components。 | 文档保持诚实，不声明 React hook parity。 |
| Accessibility breadth | Partial / audited breadth in progress | Keyboard navigation、automation peers、built-in action names 与 shell-state focus routes 已有目标测试。Phase 491 补上 standalone built-ins 与 hosted shell states 的明确 breadth contract；dynamic screen-reader announcement behavior 仍需人工 assistive-technology validation 后才能扩大声明。 | Phase 491 保持 docs/tests 和 source-backed controls 范围；修改 runtime announcement behavior 前另开 follow-up。 |
| Host events | Present | `IGraphEditorEvents`、mutation batching 和 host-event Cookbook route 已存在。 | Keep guarded。 |
| Screenshot-driven UI quality | Partial / guarded | Scene PNG gate 已覆盖规范 graph scenes。Full-window shell gate 现在会捕获 default Cookbook drawer state 和 runtime diagnostics drawer state，包含 host menu、drawer、graph host、named shell parts 和 artifact metadata。Pixel-baseline comparison 与更广的 flyout/theme/language coverage 尚未覆盖。 | Phase 486 保持在 manifest-driven shell states 范围内；只有 visual drift evidence 需要时再追加更广的 baseline follow-up。 |

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
| #82 | `avalonia-node-map-p483` | Large-graph virtualization claim 已收窄到 viewport-budgeted scene projection/rendering；不声明 renderer virtualization 或 10000 节点支持。 |
| #80 | `avalonia-node-map-p480` | Rotatable node model、command/query projection、renderer geometry、compatibility overloads、API baseline 和 docs。 |
| #91 | `avalonia-node-map-4xr` | Phase 484 在 rotatable nodes 后刷新 roadmap。 |
| #93 | `avalonia-node-map-3hc` | Phase 485 Cookbook host-copy architecture path；已由 PR #94 关闭。 |
| #95 | `avalonia-node-map-0xr` | Phase 486 full-window shell visual-state expansion；已由 PR #96 关闭。 |
| #97 | `avalonia-node-map-i8s` | Phase 487 custom-node copyable-host recipe hardening；已由 PR #98 关闭。 |
| #99 | `avalonia-node-map-ce1` | Phase 488 layout provider 与 background/cancel proof refresh；已由 PR #100 关闭。 |
| #101 | `avalonia-node-map-6sc` | Phase 489 renderer virtualization proof-contract design spike；已由 PR #102 关闭。 |
| #103 | `avalonia-node-map-3x0` | Phase 490 stale React Flow parity roadmap repair；已由 PR #104 关闭。 |

## Phase 491 Accessibility Breadth Audit

Phase 491 把当前 accessibility posture 记录成 source-backed contract，而不是笼统声明。现有覆盖最强的是 hosted shell、canvas、nodes、ports、connections、inspector 和 command surfaces。本次 breadth audit 为 standalone built-ins 与 hosted shell states 增加明确检查，不改变 runtime behavior。

| Surface | Accessibility posture | Guarded evidence |
| --- | --- | --- |
| `AsterGraphControls` | Container 不进入 focus path；zoom/fit/reset buttons 保持 keyboard-focusable，并暴露 action names。 | `AsterGraphBuiltInControlsTests` |
| `NodeToolbar` / `EdgeToolbar` | Container 不进入 focus path；投影出来的 node/connection action buttons 保持 keyboard-focusable，并从 descriptors 暴露名称。 | `AsterGraphBuiltInToolbarTests` |
| `NodeResizer` | Resize handles 是有名称的 buttons，并继续由 focused built-in tests 守住。 | `AsterGraphBuiltInNodeResizerTests` |
| `GraphMiniMap` | Pointer-only overview surface；stock MiniMap 与 stock drawing surface 不进入 keyboard focus path。 | `GraphMiniMapStandaloneTests` |
| `AsterGraphPanel` | Overlay layout container 不抢焦点，同时保留 host-owned focusable children。 | `AsterGraphBuiltInPanelTests` |
| `GridBackground` | Decorative canvas grid 保持 non-focusable。 | `GridBackgroundTests` |
| Hosted validation/problems shell | Problem rows 与 focus buttons 暴露 automation names 和绑定 validation targets 的 help text。 | `GraphEditorViewTests` |
| Hosted export、fragment、command-palette、authoring shells | 现有 interactive controls 暴露 names 并保持 keyboard-focusable；command-palette focus recovery 由单独测试守住。 | `GraphEditorViewTests`、`GraphEditorNavigationFocusWorkflowTests` |

剩余有界缺口：headless tests 可以守住 names、peers、focusability 和 focus return，但不能证明 dynamic validation/export status changes 的实时 screen-reader announcements。未来 live-region 或 assistive-technology certification 工作需要新的 GitHub/Beads tracker。

## 下一轮 Issue Wave

Phase 490 在 Phase 485-489 已关闭后修复过期的 Phase 484 队列。GitHub #103 / `avalonia-node-map-3x0` 已由 PR #104 关闭，并且不授权 runtime 或 public API 修改。

Phase 491 现在负责 accessibility breadth audit，因为当前矩阵仍把 accessibility breadth 标为 Partial，而 visual、Cookbook architecture、layout proof 和 renderer proof-contract slices 都已有关闭的 tracker。Retained migration removal 仍是 later future tracker item，因为它依赖 v1 policy 和 public API baseline work。

| GitHub | Bead | 标题 | 优先级 | 可能 write set | 并行边界 |
| --- | --- | --- | --- | --- | --- |
| #105 | `avalonia-node-map-44i` | Phase 491: audit accessibility breadth across built-ins and shell states | P2 | Avalonia built-ins、automation/focus tests、keyboard/screen-reader coverage boundaries docs | 当前 docs/tests-first accessibility audit。除非证据证明具体 missing contract，否则不做 runtime、public API 或 visual changes。 |
| TBD | TBD | Retained migration removal roadmap | P3 | public API inventory、stabilization support matrix、retained migration docs/tests | Later future tracker item。必须与 v1 policy 和 public API baseline work 串行；不要在 parity docs work 中顺手删除 retained surfaces。 |

## 推荐并行 Worktree 计划

- `docs/phase-491-accessibility-breadth-audit`：负责 #105 / `avalonia-node-map-44i`；只审计 Avalonia built-ins 和 shell states 的 accessibility docs/tests。
- Future retained-migration branch name 应在 v1 policy 和 public API baseline 范围明确后再确定；预期 ownership 是 public API inventory 与 stabilization docs/tests。

## UI 验证策略

未来所有触碰 `src/AsterGraph.Avalonia` 或 `src/AsterGraph.Demo` 的 UI 变更应提供：

- 针对 control state、layout contract 或 interaction state 的 focused headless Avalonia test；
- 影响 graph scene rendering 时，通过 Cookbook screenshot gate 生成 deterministic scene PNG；
- 影响 Cookbook scene 的 visual PR 需要附 before/after PNG artifacts 和 `metadata.json`；
- 每个新的 public UI component 或 interaction 都应有 Cookbook route；
- 如果 UI 变更只是 structural-only 且不改变像素，需要显式说明。

当前覆盖包含 scene-level route captures 和两条 manifest-driven full-window shell captures。`DemoCookbookScreenshotGateTests`、`CookbookScreenshotGateRoutes.json` 和 `CookbookShellVisualGateStates.json` 能证明规范 graph scenes、route metadata、default Cookbook shell artifact 和 runtime diagnostics shell artifact；它们仍不提供严格 pixel-baseline comparisons 或广泛 flyout/theme/language coverage。

## Tracker 备注

- GitHub #79 和 Beads `avalonia-node-map-p478` 创建时把 `.planning/*` 列入 write set。由于 `.planning/` 被 ignore 且当前 worktree 中不存在，本次刷新将它记录为 tracker drift，而不是 force-add 本地 planning 文件。
- Beads 是本仓库的持久本地 tracker。Phase 491 现在通过 GitHub #105 / `avalonia-node-map-44i` 承接 accessibility breadth audit；后续 follow-up 在代码修改前也必须先拿到明确的 GitHub 与 Beads ID。
- Phase 478、Phase 484、Phase 490 和 Phase 491 都不修改产品代码；除非 focused test 证明存在具体 missing contract。
