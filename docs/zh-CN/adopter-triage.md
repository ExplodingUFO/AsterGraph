# 采用者分诊清单

提交公开 beta 回报时，统一使用同一组受限 intake 字段：

- 报告类型：真实外部采用报告，或维护者种子预演 / synthetic dry-run
- 采用者上下文：公开项目、公司、个人 handle，或 `private adopter`，并说明宿主上下文
- `route`（尝试的路线或样例）
- `version`（包版本或公开 tag）
- `proof 标记`（命中的 proof 标记）
- `摩擦`说明
- `support bundle` 附件备注：`SUPPORT_BUNDLE_PATH:...`（当 route 产出 bundle 时）或 `NO_SUPPORT_BUNDLE:route-cannot-produce-one`（当 route 不能产出 bundle 时）
- claim-expansion status：不请求扩大、候选扩大，或需要维护者分诊

当前 triage proof markers：`ADOPTION_INTAKE_EVIDENCE_OK:True`、`SUPPORT_BUNDLE_INTAKE_HANDOFF_OK:True` 和 `REAL_EXTERNAL_REPORT_GATE_OK:True`。
当前 refresh markers：`ADOPTER_INTAKE_REFRESH_OK:True`、`ADOPTER_SUPPORT_BUNDLE_ATTACHMENT_OK:True` 和 `ADOPTER_CLAIM_EXPANSION_GATE_OK:True`。

同一套字段应在：

- `adoption_feedback.yml` issue 模板
- Bug 报告模板
- 预发布 notes 的证据复核

这样可以让 `HelloWorld`、`Demo`、`ScaleSmoke` 这类没有 support bundle 的反馈照常进入分诊，同时保持证据合同一致。

在把某条反馈当成“应该扩大支持面”的候选之前，先看 [Project Status](./project-status.md) 里的就绪门禁。落在已证明/受边界约束之外的报告，先当 intake evidence，而不是自动扩大范围。单条报告不会扩大公开声明；扩大 support 或 capability 需要 3 到 5 条真实外部报告聚焦在同一个受限风险上。若 route 能产出 bundle，就把 `SUPPORT_BUNDLE_PATH:...` 那一行当成 support bundle 附件备注；否则记录 `NO_SUPPORT_BUNDLE:route-cannot-produce-one`。

Phase 380 refresh 把三个检查绑在一起：`ADOPTER_INTAKE_REFRESH_OK:True` 表示模板、triage 和 schema 对齐；`ADOPTER_SUPPORT_BUNDLE_ATTACHMENT_OK:True` 表示 bundle 是否可用仍记录在同一条 intake 记录上；`ADOPTER_CLAIM_EXPANSION_GATE_OK:True` 表示 support 或 capability expansion 继续等 3 到 5 条真实外部报告门禁。

## Intake 标准清单

- [ ] **报告类型和采用者上下文已记录**：真实外部报告或预演，并说明采用者/宿主上下文
- [ ] **Route 已记录**：尝试了哪个样例、路线或 artifact
- [ ] **Version 已记录**：NuGet 包版本或公开 tag
- [ ] **Proof 标记已提供**：至少包含 `CONSUMER_SAMPLE_OK` 或对应 route 的 marker
- [ ] **摩擦点已描述**：哪里拖慢或不清楚
- [ ] **Support bundle 备注已记录**：`SUPPORT_BUNDLE_PATH:...` 或 `NO_SUPPORT_BUNDLE:reason`
- [ ] **claim-expansion status 已记录**：不扩大、候选扩大，或需要维护者分诊

## 参数快照分诊分类

在这里继续沿用同一套受限证据记录词汇：`route`、`version`、`proof 标记`、`摩擦`、`support bundle` 附件备注，以及 `parameterSnapshots`。

在检查 `parameterSnapshots` 时，也要保留 `status`、`owner` 和 `priority`，这样证据仍然会绑定到选中的节点和宿主拥有的元数据。

| 失败的 proof 标记 | 分类为 | 在 `parameterSnapshots` 中检查 | 下一步动作 |
| --- | --- | --- | --- |
| `CONSUMER_SAMPLE_PARAMETER_OK` | parameter projection / 写入路径失败 | current/default/editable/valid 行 | 进行 sample/session 参数投影或写入路径调查 |
| `CONSUMER_SAMPLE_METADATA_PROJECTION_OK` | metadata projection / 元数据投影失败 | editor kind/default/constraints/options 等元数据 | 进行定义/inspector 元数据投影调查 |
| `SUPPORT_BUNDLE_PERSISTENCE_OK` | support-bundle persistence / 持久化失败 | support-bundle 路径/写入失败/环境 | 进行持久化/路径/环境调查 |

## 相关链接

- [Adoption Feedback](./adoption-feedback.md)
- [Synthetic Adoption Intake Dry-Run Fixtures](./adoption-intake-dry-run.md)
- [Beta Support Bundle](./support-bundle.md)
