# Phase 0 React Flow 对齐审计

## Phase 478 刷新

本文最初是基于 `0.11.0-beta` 的 Phase 0 审计。Phase 478 按当前 `master` 状态重新校准，避免后续 issue wave 重复 GitHub #46-#52 以及它们的后续子任务已经完成的工作。

本次刷新只修改文档和 tracker 状态，不修改产品代码、运行时行为、渲染器契约或公开支持声明。

## Phase 490 更新

Phase 490 是 GitHub #103 / `avalonia-node-map-3x0`，用于在 Phases 485-489 全部关闭后做 stale-roadmap repair。本 slice 是 docs/tests only：更新中英文 parity roadmap，把已完成工作移出候选队列，并选择 Accessibility breadth audit 是下一项 open parity gap。No Core/Editor/Avalonia runtime or public API changes。

## Phase 491 更新

Phase 491 是 GitHub #105 / `avalonia-node-map-44i`，承接 Phase 490 选出的 accessibility breadth audit。本 slice 仍然是 docs/tests first：按现有 automation name、focusability、keyboard routes 和 intentionally decorative surfaces 审计 public Avalonia built-ins 与 hosted shell states。不授权 retained migration removal、public API changes、visual redesign，也不在缺少人工 assistive-technology 验证时声明 full screen-reader certification。

## Phase 492 更新

Phase 492 是 GitHub #107 / `avalonia-node-map-j8v`，承接 Phase 491 accessibility breadth audit 关闭后的 retained migration removal roadmap。本 slice 是 inventory now, remove later：先记录 retained bridge、explicit legacy import 和已经退役的 compatibility-only surfaces，确保后续任何 removal issue 改变 API 形状前都有分类依据。不授权 runtime behavior changes、public API deletion、public API baseline updates 或 UI changes。

## Phase 493 更新

Phase 493 是 GitHub #109 / `avalonia-node-map-8qv`，承接 retained migration roadmap 关闭后的 custom node presenter cookbook parity proof。本 slice 是 docs/tests only：把 partial custom-node parity row 绑定到 Custom Node Host Recipe 中已经受防守的 host-owned presenter 路线，包括 `NodeBodyPresenter`、`NodeVisualPresenter`、`NodeDragHandle`、anchor maps、host-owned edge overlays 和 `CUSTOM_EXTENSION_SURFACE_OK` proof markers。不授权 runtime behavior changes、public API changes、React Flow hooks/components parity claims、Demo UI redesign 或 screenshot-gate expansion。

## Phase 494 更新

Phase 494 是 GitHub #111 / `avalonia-node-map-p5z`，承接 custom-node presenter parity proof 关闭后的 localized full-window shell visual gate expansion。本 slice 是 docs/tests only：新增 shell-state-owned language/theme metadata，并在现有 English Cookbook 与 runtime diagnostics captures 之外增加 Chinese Cookbook drawer capture：`shell-cookbook-default-open-zh-cn`。不授权 strict pixel hash baselines、runtime UI changes、broad language/theme certification、flyout capture、popup capture 或 context-menu capture。

## Phase 495 更新

Phase 495 是 GitHub #113 / `avalonia-node-map-wzt`，在 Phase 494 关闭后刷新 post-Phase-494 roadmap refresh。本 slice 是 docs/tests only：基于当前 GitHub 与 Beads 证据刷新 active issue split，记录至少三个具体 follow-up candidates，并更新 worktree/parallelism guidance。不授权 Core/Editor/Avalonia runtime changes、public API changes、Demo UI redesign、retained API deletion、strict pixel baselines 或 broad React Flow parity claim expansion。

## Phase 497 更新

Phase 497 是 GitHub #117 / `avalonia-node-map-5nl`，承接 Cookbook architecture contract 关闭后的 closed-drawer shell visual gate expansion。本 slice 是 docs/tests/manifest only：新增 `shell-cookbook-default-closed`，与已有 open Cookbook 和 runtime shell captures 并列，验证 `expectedPaneOpen: false`，并把 required parts 限定为可见 closed-shell chrome。不授权 runtime UI changes、public API changes、strict pixel baselines、flyout capture、popup capture、context-menu capture 或 broad language/theme certification。

## Phase 498 更新

Phase 498 是 GitHub #119 / `avalonia-node-map-3um`，承接 closed-drawer shell visual gate 关闭后的 retained migration removal execution gate。本 slice 是 docs/tests only：把 Phase 492 的 inventory 转成执行 gate，明确任何 removal PR 之前必须具备 exact symbols、blocker tests、support-window criteria、migration evidence，以及后续修改 `eng/public-api-baseline.txt` 的 approval path。不授权 no retained API removal、no public API baseline change、no runtime behavior change、no UI change。

## Phase 499 更新

Phase 499 是 GitHub #121 / `avalonia-node-map-9x7`，承接 retained migration removal execution gate 关闭后的 renderer virtualization execution-boundary proof。本 slice 是 docs/tests only：把当前证据钉在 viewport-budgeted scene projection/rendering，记录任何 ItemsRepeater/Skia-style renderer virtualization、background graph index 或 graph-size support claim 扩大前必须具备的 execution proof，并保持 `xlarge` telemetry-only。不授权 renderer rewrite、benchmark harness implementation、public API change、runtime behavior change、UI change 或 support-claim expansion。

## Phase 500 更新

Phase 500 是 GitHub #123 / `avalonia-node-map-66t`，承接 renderer virtualization execution-boundary proof 关闭后的 selected runtime shell visual gate state。本 slice 是 manifest/docs/tests only：新增 `shell-runtime-diagnostics-closed`，与已有 runtime diagnostics open state 并列，验证 `expectedPaneOpen: false`，并把 required parts 限定为可见 closed-shell chrome。不授权 runtime behavior change、public API change、styling redesign、strict pixel baselines、flyout capture、popup capture、context-menu capture、broad language/theme certification、retained API removal 或 renderer virtualization work。

## Phase 501 更新

Phase 501 是 GitHub #125 / `avalonia-node-map-38n`，承接 Phase 500 关闭后的 post-Phase-500 parity follow-up queue refresh。本 slice 是 docs/tests only：把 generic Phase 501 placeholder 替换为来自当前 React Flow parity matrix 的具体 next candidates，并把 renderer virtualization execution proof、declarative API ergonomics、layout provider evidence、manual assistive-technology validation 和 broader shell visual coverage 拆成可单独追踪的 follow-ups。它明确保持 no runtime behavior change、no public API change、no UI redesign、no strict pixel baselines、no retained API removal 和 no renderer virtualization implementation 的边界。

## Phase 502 更新

Phase 502 是 GitHub #127 / `avalonia-node-map-mai`，承接 post-Phase-500 queue refresh 选出的 renderer virtualization execution proof。本 slice 仍是 docs/tests only：把 focused renderer tests、scale docs、proof command 和 artifact metadata 串成后续工作必须满足的 executable proof contract，同时把当前声明继续限制为 viewport-budgeted scene projection/rendering，而不是 true renderer virtualization。任何后续 support claim 之前都需要 non-informational renderer thresholds；本 slice 不授权 no runtime behavior change、no public API change、no UI redesign、no retained API removal 或 no renderer virtualization implementation。

## Phase 503 更新

Phase 503 是 GitHub #129 / `avalonia-node-map-mzu`，承接 renderer virtualization execution proof 关闭后的 declarative API ergonomics audit。本 slice 仍是 docs/tests only：`DECLARATIVE_API_ERGONOMICS_AUDIT` 把当前可复制 API 入口映射到 `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`、`AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`、`AsterGraphHostBuilder.Create(...).BuildAvaloniaView()`、`templates/astergraph-avalonia` 和 `src/AsterGraph.Demo`。当前支持声明仍保持 partial：these routes are not equivalent to React Flow hooks/components，也不提供 `<ReactFlow>`-equivalent declarative DSL。本 slice 不授权 no runtime behavior change、no public API change、no UI redesign、no retained API removal 或 no public API baseline change。

## Phase 504 更新

Phase 504 是 GitHub #131 / `avalonia-node-map-8lf`，承接 declarative API ergonomics audit 关闭后的 layout provider evidence expansion。本 slice 仍是 docs/tests only：`LAYOUT_PROVIDER_EVIDENCE_EXPANSION` 把 `IGraphLayoutProvider.CreateLayoutPlan(GraphLayoutRequest)`、`GraphLayoutRequest`、`GraphLayoutPlan`、`IGraphEditorQueries.CreateLayoutPlan(...)`、`PreviewLayoutPlan`、`TryApplyLayoutPlan`、`TryApplyLayoutRequest`、`TrySnapSelectedNodesToGrid`、`TrySnapAllNodesToGrid`、`GraphEditorLayoutProviderSeamTests` 和 Cookbook `performance-viewport-route` evidence 串成同步且由宿主拥有的 proof story。它不授权 async/cancellable provider execution、runtime behavior changes、public API changes、UI redesign、retained API removal 或 new layout engine。

## Phase 505 更新

Phase 505 是 GitHub #133 / `avalonia-node-map-b4z`，承接 layout provider evidence expansion 关闭后的 accessibility manual assistive-technology validation plan。本 slice 仍是 docs/tests only：`ACCESSIBILITY_MANUAL_AT_VALIDATION_PLAN` 记录现有 hosted accessibility route 的 manual assistive-technology validation checklist，把当前 headless automation、focus、keyboard 和 accessible-name 证据与 unverified live screen-reader behavior 分开，并明确边界是 no live-region/runtime behavior change、no UI change、no public API change、no retained API removal 和 no broad screen-reader certification claim。

## Phase 506 更新

Phase 506 是 GitHub #135 / `avalonia-node-map-h7c`，承接 accessibility manual assistive-technology validation plan 关闭后的 broader shell visual coverage planning。本 slice 仍是 docs/tests only：`SHELL_VISUAL_COVERAGE_PLANNING` 把当前 five manifest-driven full-window shell captures 映射到后续 flyout capture、popup capture、context-menu capture、additional language/theme variants 和 pixel-baseline drift measurement。它不授权 no manifest rows、no strict pixel baselines、no runtime UI changes、no public API changes、no retained API removal 或 no broad visual/language/theme certification。

## Phase 507 更新

Phase 507 是 GitHub #137 / `avalonia-node-map-3tw`，承接 Phase 506 关闭后的 post-Phase-506 visual queue refresh。本 slice 是 docs/tests only：修复 Phases 503-506 已关闭后仍停留在 active queue 的 stale 状态，并把 Phase 506 visual planning candidates 转成具体 next tracker candidates：GitHub #139-#143 / `avalonia-node-map-2nu`、`avalonia-node-map-0ff`、`avalonia-node-map-8lu`、`avalonia-node-map-9rq` 和 `avalonia-node-map-1j4`。它不授权 no runtime UI behavior changes、no shell-state manifest rows、no strict pixel baselines、no public API changes、no retained API removal 或 no broad visual/language/theme certification。

## Phase 508 更新

Phase 508 是 GitHub #139 / `avalonia-node-map-2nu`，承接 post-Phase-506 queue refresh 选出的 shell flyout visual capture。本 slice 只修改 shell visual gate harness/manifest/docs/tests：新增 `shell-cookbook-default-view-menu-flyout`，打开 `PART_ViewMenu`，记录 `CaptureScope=full-window-shell-flyout-state`，并在 generated artifact metadata 中验证有界的 View menu headers。不授权 runtime UI behavior change、public API change、strict pixel baselines、popup 或 context-menu coverage、broad language/theme certification 或 retained API removal。

## Phase 509 更新

Phase 509 是 GitHub #140 / `avalonia-node-map-0ff`，承接有界 View menu flyout row 关闭后的 popup visual capture。本 slice 只修改 shell visual gate harness/manifest/docs/tests：新增 `shell-cookbook-default-host-command-tooltip-popup`，打开 `PART_HostCommand_history.undo` 上的 disabled host command tooltip，记录 `CaptureScope=full-window-shell-popup-state`，并在 generated artifact metadata 中验证有界的 popup text：`Nothing to undo yet.`。不授权 runtime UI behavior change、public API change、strict pixel baselines、context-menu coverage、超出这条 tooltip row 的 broad popup coverage、broad language/theme certification 或 retained API removal。

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
- `tests/AsterGraph.Demo.Tests/CookbookShellVisualGateStates.json` 当前定义 `shell-cookbook-default-open`、`shell-cookbook-default-open-zh-cn`、`shell-cookbook-default-closed`、`shell-runtime-diagnostics-open`、`shell-runtime-diagnostics-closed`、`shell-cookbook-default-view-menu-flyout` 和 `shell-cookbook-default-host-command-tooltip-popup`。
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
| Demo/Cookbook | `src/AsterGraph.Demo` | 25 个 recipes、route clarity docs、built-in/interaction/lifecycle batches、scene PNG screenshot gate metadata，以及 seven manifest-driven full-window shell captures。 | Cookbook breadth、scene coverage、English default open/closed shell captures、English runtime drawer open/closed captures、Chinese Cookbook drawer capture、一条 View menu flyout 和一条 disabled host-command tooltip popup 已覆盖。Pixel-baseline comparison 以及更广的 flyout/popup/context-menu/theme/language shell-state coverage 仍作为后续有界工作。 |
| CI and release gates | `.github/workflows`、`eng/ci.ps1`、test projects | Build/test/maintenance/contract/release/hygiene lanes、public API baseline、package validation、docs route checks、scene PNG gate tests 和 manifest-driven full-window shell gate tests。 | Text/API/scene/shell gates 较强。严格 pixel baseline 继续等 deterministic drift 测量后再做。 |

## React Flow 对齐矩阵

| 能力 | 当前状态 | 证据 / 剩余缺口 | 下一步 |
| --- | --- | --- | --- |
| Custom nodes with arbitrary Avalonia content | Partial / AsterGraph idiom 下支持 | Node definitions、`AsterGraphPresentationOptions.NodeBodyPresenter`、`AsterGraphPresentationOptions.NodeVisualPresenter`、`NodeDragHandle`、`GraphNodeVisual.PortAnchors`、`GraphNodeVisual.ConnectionTargetAnchors`、来自 `GetConnectionGeometrySnapshots()` 的 host-owned edge overlays、Custom Node Host Recipe 和 `CUSTOM_EXTENSION_SURFACE_OK` proof markers 已定义受防守路线。公开故事仍是 AsterGraph host-owned presenter guidance，不是 React Flow hooks/components parity。 | Phase 493 将 roadmap 绑定到受防守 recipe，并保持 docs/tests-only 边界；如果 presenter contracts 本身要变化，必须另开 tracker。 |
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
| Auto layout integration | Partial | `LAYOUT_PROVIDER_EVIDENCE_EXPANSION` 把 `IGraphLayoutProvider.CreateLayoutPlan(GraphLayoutRequest)`、`GraphLayoutRequest`、`GraphLayoutPlan`、preview/apply、snap-to-grid、`GraphEditorLayoutProviderSeamTests`、command-surface cancel evidence 和 Cookbook route evidence 绑定在一起。Provider example 由宿主拥有，当前 layout planning 是同步契约，不是 async-cancellable。 | Phase 504 保持 docs/tests proof 边界；除非 adopter evidence 证明需要新的 provider contract issue。 |
| Virtualization / thousands of nodes | Partial / bounded | Scale baseline、visible-scene projection、MiniMap lightweight projection 和 hosted performance policy 已存在。当前被防守的是 viewport-budgeted scene projection/rendering contract，不是 ItemsRepeater/Skia-style renderer virtualization，xlarge evidence 也只是 telemetry-only。 | 继续维护 scale docs 和 renderer projection tests；扩大声明前先开新 issue。 |
| Declarative + code-first API | Partial | `DECLARATIVE_API_ERGONOMICS_AUDIT` 把当前 code-first 和 hosted 路线绑定到 `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`、`AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`、`AsterGraphHostBuilder.Create(...).BuildAvaloniaView()`、`templates/astergraph-avalonia`、`src/AsterGraph.Demo` 和 definition builders。These routes are not equivalent to React Flow hooks/components，AsterGraph 也不声明 `<ReactFlow>`-equivalent declarative DSL。 | Phase 503 只做 docs/tests；任何 declarative DSL、source generator、XAML extension 或 hook-like public surface 都必须另开 API-change tracker。 |
| Accessibility breadth | Partial / manual validation plan defined | Keyboard navigation、automation peers、built-in action names 与 shell-state focus routes 已有目标测试。Phase 491 补上 standalone built-ins 与 hosted shell states 的明确 breadth contract。`ACCESSIBILITY_MANUAL_AT_VALIDATION_PLAN` 现在定义 hosted route 的 manual assistive-technology validation，同时把 headless automation 与 unverified live screen-reader behavior 分开，包括 dynamic screen-reader announcement evidence。 | Phase 505 保持 docs/tests 和 source-backed controls 范围；修改 live-region/runtime behavior 或声明 broad screen-reader certification claim 前另开 follow-up。 |
| Host events | Present | `IGraphEditorEvents`、mutation batching 和 host-event Cookbook route 已存在。 | Keep guarded。 |
| Screenshot-driven UI quality | Partial / guarded planning | Scene PNG gate 已覆盖规范 graph scenes。Full-window shell gate 现在会捕获 English default Cookbook drawer、Chinese default Cookbook drawer、English default Cookbook closed shell、English runtime diagnostics drawer、`shell-runtime-diagnostics-closed`、一条 View menu flyout 和一条 disabled host-command tooltip popup，包含 host menu、drawer state、graph host、named shell parts、选中的 language/theme metadata、overlay metadata 和 artifact metadata。`SHELL_VISUAL_COVERAGE_PLANNING` 把后续 context-menu capture、additional language/theme variants 和 pixel-baseline drift measurement 保持为明确的 later candidates。 | Phase 509 只覆盖一条 tooltip popup row：no runtime UI changes、no public API changes、no retained API removal、no strict pixel baselines、no context-menu coverage，也不做 no broad visual/language/theme certification。 |

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

Phase 491 已关闭 accessibility breadth audit，同时把 live screen-reader announcement proof 保持为需要单独 tracker 的未来边界。

Phase 492 现在负责 retained migration removal roadmap，边界是 inventory now, remove later。它可以分类 retained 和 compatibility-only surfaces，但真正删除 API 必须另开后续 API-change tracker，并绑定 v1 policy 与 `eng/public-api-baseline.txt`。

Phase 493 已关闭 custom node presenter cookbook parity proof。它记录受支持的 custom-node 路线是 host-owned `NodeBodyPresenter` / `NodeVisualPresenter` 指引和既有 proof markers，不是 runtime API expansion，也不是 React Flow component/hook parity claim。

Phase 494 已关闭 localized full-window shell visual gate coverage。它新增一条 Chinese Cookbook drawer state 和 shell-state language/theme metadata，同时把 strict pixel baselines 与更广的 flyout/theme/language coverage 留给后续 tracker-backed work。

Phase 495 在 GitHub 与 Beads 都回到零 open issues 后刷新 active queue。Phase 496 已关闭 Cookbook example architecture contract。Phase 497 已通过 GitHub #117 / `avalonia-node-map-5nl` 关闭 closed-drawer shell visual gate breadth slice；它只增加一条有界 closed shell state，不是 broad shell certification。

Phase 498 已通过 GitHub #119 / `avalonia-node-map-3um` 关闭 retained migration removal execution gate。它只为后续 API-change issue 定义 gate：任何 removal PR 前都必须具备 exact symbols、blocker tests、support-window criteria、migration evidence，并通过 `eng/public-api-baseline.txt` approval。该 slice 不授权 no retained API removal、no public API baseline change、no runtime behavior change、no UI change。

Phase 499 已通过 GitHub #121 / `avalonia-node-map-9x7` 关闭 renderer virtualization execution boundary。它把当前声明继续限制为 viewport-budgeted scene projection/rendering。任何后续要扩大声明的 implementation issue，都必须引入 non-informational renderer thresholds、repeatable proof command、focused renderer tests、artifact metadata，并证明被声明的操作不会依赖 full collection scan 和 full scene rebuild。

Phase 500 已通过 GitHub #123 / `avalonia-node-map-66t` 关闭 selected runtime shell visual gate state。它新增一条有界 runtime closed-shell capture，验证 `expectedPaneOpen: false`，并继续把 runtime behavior changes、strict pixel baselines、flyouts、popups、context menus、broad language/theme certification、retained API removal 和 renderer virtualization work 排除在外。

Phase 501 已通过 GitHub #125 / `avalonia-node-map-38n` 关闭 post-Phase-500 parity follow-up queue refresh。它把当前 partial gaps 转成具体队列，同时把 broad React Flow parity claims、runtime behavior changes、public API changes、UI redesign、strict pixel baselines、retained API removal 和 renderer virtualization implementation 排除在外。

Phase 502 现在通过 GitHub #127 / `avalonia-node-map-mai` 承接 renderer virtualization execution proof。它是 Current owned slice，并保持 docs/tests only：定义任何 true renderer virtualization 声明前必须具备的 proof command 和 artifact metadata contract，同时把 implementation、runtime behavior change、public API change、UI redesign、retained API removal 和 support-claim expansion 排除在外。

Phase 503 现在通过 GitHub #129 / `avalonia-node-map-mzu` 承接 declarative API ergonomics audit。它保持 docs/tests only：记录当前 code-first、factory、hosted-builder、template 和 Demo 路线，不声明 React hook parity、`<ReactFlow>`-equivalent declarative DSL、runtime behavior change、public API change、UI redesign、retained API removal 或 public API baseline change。

Phase 504 现在通过 GitHub #131 / `avalonia-node-map-8lf` 承接 layout provider evidence expansion。它保持 docs/tests only：记录当前同步且由宿主拥有的 `IGraphLayoutProvider` / `GraphLayoutRequest` / `GraphLayoutPlan` seam，以及 preview/apply/snap command evidence；不声明 async/cancellable provider execution、runtime behavior change、public API change、UI redesign、retained API removal 或 new layout engine。

Phase 505 已通过 GitHub #133 / `avalonia-node-map-b4z` 关闭 accessibility manual assistive-technology validation plan。它保持 docs/tests only：在 `HOSTED_ACCESSIBILITY_OK:True` 已经通过后，为 Narrator、NVDA 和 VoiceOver 或平台等价检查记录 manual checklist，同时明确区分 headless automation 与 unverified live screen-reader behavior。它不授权 no live-region/runtime behavior change、no UI change、no public API change、no retained API removal 或 no broad screen-reader certification claim。

Phase 506 已通过 GitHub #135 / `avalonia-node-map-h7c` 关闭 broader shell visual coverage planning。它保持 docs/tests only：记录现有 five manifest-driven full-window shell captures 如何衔接到后续 flyout、popup、context-menu、language/theme 和 drift-measurement work；本 slice 不新增 manifest rows，也不扩大 visual certification claims。

Phase 507 已通过 GitHub #137 / `avalonia-node-map-3tw` 关闭 post-Phase-506 visual queue refresh。它是 docs/tests only：修复 stale current-owned table，并把 Phase 506 planning candidates 转成下一轮具体 visual-coverage queue；不做 no runtime UI behavior changes、no shell-state manifest rows、no strict pixel baselines、no public API changes、no retained API removal 或 no broad visual/language/theme certification。

Phase 508 现在通过 GitHub #139 / `avalonia-node-map-2nu` 承接 shell flyout visual capture。它只新增一条有界 View menu flyout state：`shell-cookbook-default-view-menu-flyout`，并记录 `PART_ViewMenu`、`full-window-shell-flyout-state` metadata 和 required header evidence；runtime behavior changes、public API changes、strict pixel baselines、popup coverage、context-menu coverage、broad language/theme certification 和 retained API removal 仍不在范围内。

Phase 509 现在通过 GitHub #140 / `avalonia-node-map-0ff` 承接 popup visual capture。它只新增一条有界 host command tooltip popup state：`shell-cookbook-default-host-command-tooltip-popup`，并记录 `PART_HostCommand_history.undo`、`full-window-shell-popup-state` metadata 和 required popup text：`Nothing to undo yet.`；runtime behavior changes、public API changes、strict pixel baselines、context-menu coverage、broad popup coverage、broad language/theme certification 和 retained API removal 仍不在范围内。

| GitHub | Bead | 标题 | 优先级 | 可能 write set | 并行边界 |
| --- | --- | --- | --- | --- | --- |
| #137 | `avalonia-node-map-3tw` | Phase 507: post-Phase-506 visual queue refresh | P3 | parity roadmap docs 和 focused docs tests | Closed slice。只修复 stale tracker wording；不做 runtime UI behavior changes、shell-state manifest rows、strict pixel baselines、public API changes、retained API removal 或 broad visual/language/theme certification。 |
| #139 | `avalonia-node-map-2nu` | Phase 508: shell flyout visual capture | P3 | shell visual gate harness、manifest/docs/tests、generated artifact metadata | Closed slice。只隔离一条 View menu flyout capture path，并证明 full-window artifact metadata，不声明 broad shell certification。 |
| #140 | `avalonia-node-map-0ff` | Phase 509: popup visual capture | P3 | shell visual gate harness、manifest/docs/tests、generated artifact metadata | Current owned slice。只隔离一条 disabled host-command tooltip popup path，并证明 full-window artifact metadata，不声明 broad popup 或 shell certification。 |
| #141 | `avalonia-node-map-8lu` | Phase 510: context-menu visual capture | P3 | context-menu visual harness/docs/tests、generated artifact metadata | popup capture 之后的候选项。复用现有 context-menu presenter route，不做 public API changes 或 retained hook removal。 |
| #142 | `avalonia-node-map-9rq` | Phase 511: additional language/theme shell variants | P3 | 有界 language/theme rows 的 shell state manifest/docs/tests | overlay capture 形状明确后的候选项。只增加明确 variants，不声明 broad visual/language/theme certification。 |
| #143 | `avalonia-node-map-1j4` | Phase 512: pixel-baseline drift measurement | P3 | drift measurement docs/tests/artifact metadata | 必须先于任何 strict pixel baseline。比较记录的 `PngSha256` 和 host metadata 作为 evidence，而不是引入 pass/fail hash policy。 |

## 推荐并行 Worktree 计划

- `docs/phase-495-roadmap-refresh`：负责 #113 / `avalonia-node-map-wzt`；刷新本 roadmap，并在不改 runtime 或 public API 的前提下给出至少三个具体 follow-up candidates。
- `docs/phase-496-cookbook-architecture-contract`：已负责 Cookbook example architecture contract row；docs/tests-only，且与 shell visual 工作独立。
- `docs/phase-497-shell-closed-drawer-gate`：已负责 #117 / `avalonia-node-map-5nl`；写集限制在 shell manifest、screenshot tests 和 screenshot docs。
- `docs/phase-498-retained-removal-gate`：负责 #119 / `avalonia-node-map-3um`；写集限制在 retained migration removal execution criteria，并必须与 v1/API-baseline policy 顺序推进。
- `perf/phase-499-renderer-virtualization-boundary`：已负责 #121 / `avalonia-node-map-9x7`；已按 docs/tests-only 关闭 renderer virtualization execution boundary proof。
- `docs/phase-500-selected-runtime-shell-state`：负责 #123 / `avalonia-node-map-66t`；写集限制在 shell state manifest、screenshot docs/tests 和 parity roadmap text。
- `docs/phase-501-post-phase500-queue`：负责 #125 / `avalonia-node-map-38n`；写集限制在 parity roadmap docs/tests，并产出下一轮具体队列。
- `perf/phase-502-renderer-proof`：负责 #127 / `avalonia-node-map-mai`；写集限制在 renderer proof docs/tests、proof-command wording 和 artifact metadata contract。
- `docs/phase-503-declarative-api-audit`：负责 #129 / `avalonia-node-map-mzu`；写集限制在 parity roadmap、Quick Start、Host Integration 和 focused docs tests。
- `docs/phase-504-layout-provider-evidence`：已负责 #131 / `avalonia-node-map-8lf`；只隔离 layout provider proof docs/tests，不修改 provider runtime behavior。
- `docs/phase-505-accessibility-manual-validation`：负责 #133 / `avalonia-node-map-b4z`；写集限制在 accessibility docs、manual validation checklist wording 和 focused docs tests。
- `docs/phase-506-shell-visual-coverage-planning`：负责 #135 / `avalonia-node-map-h7c`；写集限制在 shell visual planning docs/tests，不编辑 shell-state manifest rows。
- `docs/phase-507-visual-queue-refresh`：负责 #137 / `avalonia-node-map-3tw`；写集只限本 parity roadmap 和 focused docs tests。
- `visual/phase-508-shell-flyout-capture`：负责 #139 / `avalonia-node-map-2nu`；用于第一条 tracker-backed flyout visual capture 的候选 worktree。
- `visual/phase-509-popup-capture`：负责 #140 / `avalonia-node-map-0ff`；popup visual capture 候选 worktree，与 context-menu capture 分离。
- `visual/phase-510-context-menu-capture`：负责 #141 / `avalonia-node-map-8lu`；通过现有 context-menu presenter route 做 context-menu visual capture 的候选 worktree。
- `visual/phase-511-language-theme-shell-variants`：负责 #142 / `avalonia-node-map-9rq`；overlay capture 形状稳定后，为有界 language/theme shell-state rows 准备的候选 worktree。
- `visual/phase-512-pixel-drift-measurement`：负责 #143 / `avalonia-node-map-1j4`；任何 strict pixel-baseline gate 前，用于 drift measurement 的候选 worktree。

## UI 验证策略

未来所有触碰 `src/AsterGraph.Avalonia` 或 `src/AsterGraph.Demo` 的 UI 变更应提供：

- 针对 control state、layout contract 或 interaction state 的 focused headless Avalonia test；
- 影响 graph scene rendering 时，通过 Cookbook screenshot gate 生成 deterministic scene PNG；
- 影响 Cookbook scene 的 visual PR 需要附 before/after PNG artifacts 和 `metadata.json`；
- 每个新的 public UI component 或 interaction 都应有 Cookbook route；
- 如果 UI 变更只是 structural-only 且不改变像素，需要显式说明。

当前覆盖包含 scene-level route captures 和 seven manifest-driven full-window shell captures：five base shell states、一条有界 View menu flyout state，以及一条有界 host-command tooltip popup state。`DemoCookbookScreenshotGateTests`、`CookbookScreenshotGateRoutes.json` 和 `CookbookShellVisualGateStates.json` 能证明规范 graph scenes、route metadata、English default Cookbook open artifact、English default Cookbook closed-drawer artifact、Chinese default Cookbook shell artifact、English runtime diagnostics shell artifact、selected English runtime diagnostics closed-shell artifact、`shell-cookbook-default-view-menu-flyout`，以及 `shell-cookbook-default-host-command-tooltip-popup`；它们仍不提供严格 pixel-baseline comparisons、View menu 之外的广泛 flyout coverage、disabled undo tooltip 之外的广泛 popup coverage、context-menu coverage 或 broad theme/language coverage。

## Tracker 备注

- GitHub #79 和 Beads `avalonia-node-map-p478` 创建时把 `.planning/*` 列入 write set。由于 `.planning/` 被 ignore 且当前 worktree 中不存在，本次刷新将它记录为 tracker drift，而不是 force-add 本地 planning 文件。
- Beads 是本仓库的持久本地 tracker。Phase 491 现在通过 GitHub #105 / `avalonia-node-map-44i` 承接 accessibility breadth audit；后续 follow-up 在代码修改前也必须先拿到明确的 GitHub 与 Beads ID。
- Phase 492 现在通过 GitHub #107 / `avalonia-node-map-j8v` 承接 retained migration removal planning；它只记录分类和 gates，不删除 API。
- Phase 493 现在通过 GitHub #109 / `avalonia-node-map-8qv` 承接 custom node presenter cookbook parity proof；它只记录 route traceability，不扩大 presenter API。
- Phase 494 现在通过 GitHub #111 / `avalonia-node-map-p5z` 承接 localized full-window shell visual gate coverage；它只记录一条 localized shell visual state，不做 broad visual certification。
- Phase 495 现在通过 GitHub #113 / `avalonia-node-map-wzt` 承接 post-Phase-494 roadmap refresh；它只记录 next issue split，不做 implementation。
- Phase 497 现在通过 GitHub #117 / `avalonia-node-map-5nl` 承接 closed-drawer shell visual gate expansion；它只记录一条 closed state，不做 broad visual certification。
- Phase 498 现在通过 GitHub #119 / `avalonia-node-map-3um` 承接 retained migration removal execution-gate definition；它只记录 removal 前置证据标准，不执行 removal。
- Phase 499 现在通过 GitHub #121 / `avalonia-node-map-9x7` 承接 renderer virtualization execution-boundary proof；它只记录 proof criteria，不做 renderer implementation。
- Phase 500 已通过 GitHub #123 / `avalonia-node-map-66t` 关闭 selected runtime shell visual gate state；它只记录一条 runtime closed-shell state，不做 broad visual certification。
- Phase 501 已通过 GitHub #125 / `avalonia-node-map-38n` 关闭 post-Phase-500 parity follow-up queue refresh；它只记录 next issue split，不做 implementation。
- Phase 502 已通过 GitHub #127 / `avalonia-node-map-mai` 关闭 renderer virtualization execution proof；它只记录 proof-command 与 artifact-metadata criteria，不做 renderer implementation。
- Phase 503 已通过 GitHub #129 / `avalonia-node-map-mzu` 关闭 declarative API ergonomics audit；它只记录当前可复制 API 路线，不新增 DSL 或 public API。
- Phase 504 已通过 GitHub #131 / `avalonia-node-map-8lf` 关闭 layout provider evidence expansion；它记录当前同步且由宿主拥有的 layout provider seam。
- Phase 505 已通过 GitHub #133 / `avalonia-node-map-b4z` 关闭 accessibility manual assistive-technology validation planning；它只记录 manual AT checks，不声明 certification。
- Phase 506 已通过 GitHub #135 / `avalonia-node-map-h7c` 关闭 broader shell visual coverage planning；它只记录未来 visual candidates，不新增 manifest rows。
- Phase 507 已通过 GitHub #137 / `avalonia-node-map-3tw` 关闭 post-Phase-506 visual queue refresh；它把 Phases 508-512 分配到真实 tracker IDs。
- Phase 508 现在通过 GitHub #139 / `avalonia-node-map-2nu` 承接 shell flyout visual capture；它只新增 `shell-cookbook-default-view-menu-flyout` 和 `full-window-shell-flyout-state` metadata。
- Phase 509 现在通过 GitHub #140 / `avalonia-node-map-0ff` 承接 popup visual capture；它只为 `PART_HostCommand_history.undo` 新增 `shell-cookbook-default-host-command-tooltip-popup` 和 `full-window-shell-popup-state` metadata。
- Phase 510 / #141 / `avalonia-node-map-8lu`、Phase 511 / #142 / `avalonia-node-map-9rq` 和 Phase 512 / #143 / `avalonia-node-map-1j4` 仍是下一轮具体 visual-coverage candidates。
- Phase 478、Phase 484、Phase 490、Phase 491、Phase 492、Phase 493、Phase 494、Phase 495、Phase 497、Phase 498、Phase 499、Phase 500、Phase 501、Phase 502、Phase 503、Phase 504、Phase 505、Phase 506、Phase 507、Phase 508 和 Phase 509 都不修改产品代码；除非 focused test 证明存在具体 missing contract。
