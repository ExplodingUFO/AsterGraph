# Quick Task 260327-pas: 继续修 Phase 6 Demo：改成方正风格；修复主编辑器节点图组件不能撑满容器高度；并支持节点图几个栏位分别隐藏 - Context

**Gathered:** 2026-03-27
**Status:** Ready for planning

<domain>
## Task Boundary

继续修 Phase 6 Demo：改成方正风格；修复主编辑器节点图组件不能撑满容器高度；并支持节点图几个栏位分别隐藏。

</domain>

<decisions>
## Implementation Decisions

### 视觉风格
- 改成“低圆角方正”风格：整体更硬朗，卡片、按钮、分区减少 Fluent 感，但不做完全直角。

### 高度策略
- 主编辑器区域里的节点图画布应始终撑满可用高度。
- 左右辅助栏各自滚动，不应再挤压或缩短中心节点图区域。

### 可隐藏栏位
- 需要支持分别隐藏：顶栏、节点库、右侧栏、底栏。
- 这些栏位应可独立控制，而不是只能使用一个粗粒度的整体模式切换。

### Claude's Discretion
- 低圆角的具体半径与边框密度
- 栏位隐藏入口放在 Demo 外壳还是主编辑器控制区，只要操作直观且不破坏现有展示结构
- 节点图填满高度时的具体 Grid / RowDefinitions / Stretch 实现方式

</decisions>

<specifics>
## Specific Ideas

- Demo 应更像专业工具界面，而不是 Fluent 风格展示页。
- 中间主编辑器必须更“满”，不要再出现节点图没有覆盖完整容器高度的观感。
- 节点图的几个栏位要能分别隐藏，便于演示不同嵌入形态。

</specifics>

<canonical_refs>
## Canonical References

- `.planning/phases/06-demo/06-CONTEXT.md` — Phase 6 已锁定的三栏单主编辑器展示边界
- `.planning/phases/06-demo/06-UI-SPEC.md` — 当前 Demo UI 合约，后续 quick 修复应在其基础上调整
- `.planning/quick/260327-oqi-phase-6-demo-ui-fluent/260327-oqi-SUMMARY.md` — 上一轮紧凑化 quick 修复结果

</canonical_refs>
