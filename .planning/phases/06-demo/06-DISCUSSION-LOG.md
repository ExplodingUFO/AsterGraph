# Phase 6: Demo 程序排版修复、全面中文化、并用演示方式覆盖现有架构能力 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-27
**Phase:** 06-demo
**Areas discussed:** 页面结构, 能力展示方式, 中文化范围

---

## 页面结构

| Option | Description | Selected |
|--------|-------------|----------|
| 三栏总览 | 左侧能力导航/开关，中间主演示区，右侧说明/状态区。最适合同时展示编辑器、架构能力和当前状态。 | ✓ |
| 上下分区 | 上方概览与控制，下方大画布与细节区。更像产品首页，但多能力并列展示会更拥挤。 | |
| 标签页切换 | 每个能力单独一页或一组标签。最整齐，但不容易一眼看出整体架构关系。 | |

**User's choice:** 三栏总览
**Notes:** 用户希望排版先解决清晰度问题，同时保持整体架构一眼可见。

---

## 能力展示方式

| Option | Description | Selected |
|--------|-------------|----------|
| 总览+分区 | 一个主页面内分区展示：完整壳层、独立表面、可替换呈现、运行时/诊断。最能体现现有架构全貌。 | ✓ |
| 场景切换 | 用模式切换按钮切到“完整编辑器 / 嵌入式 / 替换式 / 诊断”几个场景。更聚焦，但用户来回切换更多。 | |
| 对照展示 | 并排展示 stock 与 custom、full shell 与 standalone。信息量强，但布局压力最大。 | |

**User's choice:** 总览+分区
**Notes:** 用户明确要体现现有架构的所有功能，不希望被切页体验拆散。

---

## 中文化范围

| Option | Description | Selected |
|--------|-------------|----------|
| 全面中文 | Demo 外壳、自定义说明、默认工具栏、侧栏说明、状态文案都尽量中文化，保留 API/类型名英文。 | ✓ |
| 外壳中文 | 仅 Demo 顶层页面和说明区改中文，编辑器内部 stock 文案尽量少动。 | |
| 中英对照 | 关键说明中文为主，术语保留英文括注。 | |

**User's choice:** 全面中文
**Notes:** 中文化不只做壳层，默认 shell 内现有英文也应纳入范围。

---

## 中心演示承载方式

| Option | Description | Selected |
|--------|-------------|----------|
| 单主编辑器 | 保留一个可交互主编辑器，周围用卡片说明当前启用的架构能力，并提供少量关键切换。实现稳、排版也最容易做好。 | ✓ |
| 多实时视图 | 同页同时摆完整编辑器、独立 Canvas、独立 Inspector / MiniMap 等多个实时视图。更炫，但很容易挤压布局。 | |
| 主编辑器+预览 | 主编辑器为主，其他能力用缩略预览或说明卡片表示，不全部实时渲染。 | |

**User's choice:** 单主编辑器
**Notes:** 用户接受以一个核心编辑器承载交互，其他架构能力通过辅助展示表达。

---

## 首页必须覆盖的架构能力

| Option | Description | Selected |
|--------|-------------|----------|
| 完整壳层 | 展示 `GraphEditorView` 作为默认完整壳层能力。 | ✓ |
| 独立表面 | 明确说明并演示独立 Canvas / Inspector / MiniMap 的可嵌入能力。 | ✓ |
| 可替换呈现 | 展示 stock / custom presenter 替换能力和宿主扩展点。 | ✓ |
| 运行时与诊断 | 展示运行时会话、能力查询、诊断/检查等 Phase 2/5 成果。 | ✓ |

**User's choice:** 完整壳层、独立表面、可替换呈现、运行时与诊断
**Notes:** 用户要求首页必须把现有架构能力完整覆盖，不接受只挑一部分展示。

---

## 文案气质

| Option | Description | Selected |
|--------|-------------|----------|
| 产品化 | 界面尽量像产品，但每个区块都有简短技术说明。 | |
| 架构导向 | 界面直接强调架构分层、扩展点、宿主接入方式。更像 SDK 展示台。 | ✓ |
| 平衡型 | 在产品化和架构导向之间折中。 | |

**User's choice:** 架构导向
**Notes:** Demo 要服务于现有架构能力表达，优先做成 SDK 展示台。

---

## Claude's Discretion

- 三栏布局的具体尺寸、卡片编排、信息层级和视觉细节
- 如何把 standalone surfaces 与 runtime diagnostics 信息放进总览页而不挤压主编辑器
- 中文术语的最终措辞与统一性处理

## Deferred Ideas

- 暂无新增能力型需求被纳入本轮讨论；讨论基本保持在 Demo 展示与中文化范围内
