# Phase 6: Demo 程序排版修复、全面中文化、并用演示方式覆盖现有架构能力 - Context

**Gathered:** 2026-03-27
**Status:** Ready for planning

<domain>
## Phase Boundary

本阶段聚焦于 `AsterGraph.Demo` 的展示层重构：修复当前 Demo 的排版问题，完成面向中文用户的统一界面文案，并把现有架构能力以清晰、可感知、可验证的方式集中展示出来。范围包括默认完整壳层、独立可嵌入表面、可替换呈现能力，以及运行时与诊断能力的演示表达。它不是新增图编辑领域能力，不是引入新的 UI 技术栈，也不是新做一套独立 diagnostics workbench 产品。

</domain>

<decisions>
## Implementation Decisions

### 页面结构
- **D-01:** 新版 Demo 采用“三栏总览”主布局：左侧用于能力导航与切换，中间为主演示区，右侧为说明、状态与架构信息区。
- **D-02:** 能力呈现采用“总览 + 分区”的单页结构，不做按模式跳转的多页体验，避免用户来回切换后失去整体架构视图。
- **D-03:** 中心内容以“单主编辑器”为核心，保留一个可交互的主编辑器实例作为体验中心，其余能力通过分区说明、状态卡片、辅助面板或受控预览表达，不在首页堆叠多个同级实时编辑器视图。

### 能力展示范围
- **D-04:** 首页必须明确覆盖四类现有架构能力：完整壳层、独立表面、可替换呈现、运行时与诊断。
- **D-05:** “完整壳层”能力应直接体现 `GraphEditorView` 的默认集成价值，而不是只做文字介绍。
- **D-06:** “独立表面”能力应明确说明并可见地展示 standalone canvas、inspector、mini map 的可嵌入边界。
- **D-07:** “可替换呈现”能力应体现 stock 与 custom presenter 的替换路径，以及宿主可插拔的视觉扩展点。
- **D-08:** “运行时与诊断”能力应把会话、查询、能力快照、诊断/检查能力显式展示出来，而不是隐藏在代码或控制台里。

### 中文化策略
- **D-09:** Demo 采用“全面中文”策略：Demo 外壳、自定义说明区、默认工具栏、侧栏说明、状态文案都尽量中文化。
- **D-10:** API 名称、类型名、接口名等技术标识可以保留英文，不强行把源码级术语翻译进 UI。
- **D-11:** 中文化不应只停留在 `MainWindow` 外壳；默认 `GraphEditorView` 内当前英文文案也属于本阶段关注范围。

### 演示表达风格
- **D-12:** 整体气质采用“架构导向”的 SDK 展示台风格，而不是纯产品营销风格。
- **D-13:** 每个展示区块都应帮助用户理解“这一层属于哪一层架构、能替换什么、宿主如何接入”，而不只是展示视觉效果。

### Claude's Discretion
- 在“三栏总览”内每栏的具体比例、卡片密度、层级分组与视觉样式
- 如何在不压垮主编辑体验的前提下展示 standalone surfaces 与 diagnostics 信息
- 中文术语的最终措辞，只要保持技术语义稳定、读起来像中文软件界面

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 项目范围与阶段约束
- `.planning/ROADMAP.md` — 已完成 Phase 1-5 的整体路线，以及新增 Phase 6 的位置与依赖关系
- `.planning/PROJECT.md` — AsterGraph 的产品定位、宿主友好组件库目标、渐进式兼容约束
- `.planning/REQUIREMENTS.md` — 已存在的嵌入式表面、可替换呈现、运行时 API、诊断等需求映射
- `.planning/STATE.md` — 当前里程碑状态、已完成阶段、以及本次新增 Demo 阶段前后的上下文

### 上游阶段决策
- `.planning/phases/03-embeddable-avalonia-surfaces/03-CONTEXT.md` — 独立 canvas / inspector / mini map 与 full shell 的中粒度表面边界
- `.planning/phases/04-replaceable-presentation-kit/04-CONTEXT.md` — stock/default presenter 与 custom presenter 替换边界
- `.planning/phases/05-diagnostics-integration-inspection/05-CONTEXT.md` — runtime/session diagnostics、inspection、logger/tracing 的公开契约边界

### Demo 与可复用实现来源
- `src/AsterGraph.Demo/Views/MainWindow.axaml` — 当前 Demo 顶层布局，现有排版问题的直接来源
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` — 当前 Demo 的开关、权限、行为选项与少量本地化接入点
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` — 默认完整壳层的当前英文文案、区块结构与工具栏布局
- `tools/AsterGraph.HostSample/Program.cs` — 已验证的完整壳层、独立表面、可替换 presenter、runtime/diagnostics 能力证明点

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`：已经包含完整壳层的 header、library、canvas、inspector、mini map、status 等区块，是 Demo 进行中文化和信息重组的主要素材来源。
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`：已经具备拖拽辅助、只读模式、工作区命令、片段命令、宿主菜单扩展等演示开关，可直接复用为左侧能力控制区的数据来源。
- `tools/AsterGraph.HostSample/Program.cs`：已经覆盖 full shell、standalone surfaces、custom presenters、runtime session、inspection snapshot、recent diagnostics 等能力证明，可作为 Demo 中“架构能力说明/状态映射”的事实来源。

### Established Patterns
- 现有架构坚持 `AsterGraph.Editor` 持有运行时/状态，`AsterGraph.Avalonia` 负责可视化和宿主嵌入；Demo 应该展示这层分工，而不是模糊它。
- 完整壳层与独立表面并存是既定路径；Demo 不应只展示单一路径，而要体现 “可以直接用，也可以拆开嵌入”。
- Presenter replacement 与 diagnostics 都已经有明确的公开边界，Demo 更适合“解释并显化”这些能力，而不是新发明一套只在 Demo 生效的旁路逻辑。

### Integration Points
- 顶层排版改动主要落在 `src/AsterGraph.Demo/Views/MainWindow.axaml` 与对应 ViewModel。
- 默认壳层文案中文化和区块说明能力可能需要改动 `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`，并与现有本地化/绑定方式对齐。
- 若要把 runtime/diagnostics 能力可视化到 Demo 界面，最自然的接入点是 `MainWindowViewModel` 对 `Editor.Session` 及其 diagnostics/query surfaces 的读取与映射。

</code_context>

<specifics>
## Specific Ideas

- 新 Demo 应该一眼看上去像 “AsterGraph SDK 架构展台”，而不是只有一个能编辑节点的演示窗口。
- 用户希望首页就能看到并理解：默认完整壳层能做什么、独立表面如何拆分、Presenter 如何替换、运行时与诊断怎样暴露给宿主。
- 视觉与文案需要中文优先，但技术对象名可以保留英文，避免把 API 世界观翻译坏。

</specifics>

<deferred>
## Deferred Ideas

- 新增超出当前架构范围的图编辑器终端功能
- 为 diagnostics 单独再做一套完整工作台产品界面
- 引入 Avalonia 之外的第二套前端/展示技术栈

</deferred>

---

*Phase: 06-demo*
*Context gathered: 2026-03-27*
