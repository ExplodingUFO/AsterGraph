# Adoption Feedback Loop

这份文档定义了 public beta 的受限 intake 记录，并明确区分维护者种子预演证据和真实外部报告；在预发布反馈循环还在收集足够真实外部报告时，继续沿用这套受限 intake 词汇。

## Intake 格式

每条 beta 反馈都应先收口到同一套受限 intake 词汇：报告类型、采用者上下文、`route`、`version`、proof 标记、摩擦点、support bundle 附件备注，以及 claim-expansion status。

当前 intake proof markers：`ADOPTION_INTAKE_EVIDENCE_OK:True`、`SUPPORT_BUNDLE_INTAKE_HANDOFF_OK:True` 和 `REAL_EXTERNAL_REPORT_GATE_OK:True`。
当前 v0.61 refresh markers：`ADOPTER_INTAKE_REFRESH_OK:True`、`ADOPTER_SUPPORT_BUNDLE_ATTACHMENT_OK:True` 和 `ADOPTER_CLAIM_EXPANSION_GATE_OK:True`。

每条反馈都应按这套受限字段记录：

- 报告类型（`Real external adoption report` 或 `Maintainer-seeded rehearsal / synthetic dry-run`）
- 采用者上下文（公开项目、公司、个人 handle，或 `private adopter`，并说明尝试的宿主上下文）
- route（`HelloWorld`、`AsterGraph.Starter.Avalonia`、`HelloWorld.Avalonia`、`ConsumerSample.Avalonia`、`HostSample`、`PackageSmoke`、`ScaleSmoke`、`Demo`）
- version
- proof 标记
- 摩擦点
- support bundle 附件备注：`SUPPORT_BUNDLE_PATH:...`（当 route 产出 bundle 时）或 `NO_SUPPORT_BUNDLE:route-cannot-produce-one`（当 route 不能产出 bundle 时）
- claim-expansion status（`No support/capability expansion requested`、`Candidate support/capability expansion`，或 `Unsure / needs maintainer triage`）

截图或命令输出可以作为补充证据附上，但不替代受限 intake 字段里的 proof 标记。

真实外部报告必须来自维护者预演之外、正在评估或嵌入 AsterGraph 的采用者。维护者种子预演和 synthetic dry-run 仍可用于检查 intake 路径，但不计入扩大 support 或 capability 声明所需的 3 到 5 条真实外部报告。单条报告不会扩大公开声明；它最多成为维护者分诊的候选信号。

公开反馈建议使用 GitHub 上的 `Adoption feedback` issue template。
如果你已经能跑到 `ConsumerSample.Avalonia -- --proof --support-bundle <support-bundle-path>`，优先把 [Beta Support Bundle](./support-bundle.md) 里定义的本地证据包附上，并把 proof 输出里的 `SUPPORT_BUNDLE_PATH:...` 这一行当作 support bundle 附件备注。如果 route 不能产出 bundle，就记录 `NO_SUPPORT_BUNDLE:route-cannot-produce-one`。

## 当前种子试用综合

在仓库还没有积累足够真实外部 issue 之前，当前建议来自四组结构化的公开前 adopter-trial 走查，它们分别覆盖了不同入口路线。下面的 `Persona` 和 `Requested next capability` 两列是维护者综合出来的结论，不是公开 intake 表单里的原始字段。

| 画像 | 尝试路线 | 主要摩擦 | 希望下一步补什么 |
| --- | --- | --- | --- |
| Avalonia 宿主集成人员 | `AsterGraph.Starter.Avalonia` -> `HelloWorld.Avalonia` -> `ConsumerSample.Avalonia` | 第一跑很顺，但在没有中等样例时，从最小样例跳到完整宿主接线跨度较大 | 更多可复制的 hosted-UI 模板和 recipe |
| 需要插件能力的 SDK 评估者 | `ConsumerSample.Avalonia` | 不往后读文档的话，trust boundary 还不够醒目 | 更前置的 trust-policy 示例和 plugin-host recipe |
| 现有 retained 宿主维护者 | `Host Integration` + retained migration 文档 | 路线选择和迁移阶段仍需要仔细阅读 | 更多 migration recipe 和路线对照示例 |
| 关注性能的评估者 | `ScaleSmoke` baseline | marker 已有，但预算数字的含义还要结合单独文档理解 | 先持续公开 baseline 数字，再决定是否升级大图 defended budget |

## 当前建议

当前 `0.xx` alpha/beta hardening 线命名为 `Adoption Readiness / Release Candidate Hygiene`：先把公开推荐、support boundary、API drift 检查和 release proof 口径保持一致，再考虑扩大任何 capability 声明。之前的 `Performance / Export Hardening` 已经变成 defended evidence，不再是下一条公开推荐。

当前 proof handoff markers：`ADOPTION_RECOMMENDATION_CURRENT_OK:True` 和 `CLAIM_HYGIENE_BOUNDARY_OK:True`。
里程碑 handoff markers：`ADOPTION_READINESS_HANDOFF_OK:True`、`ADOPTION_SCOPE_BOUNDARY_OK:True` 和 `V056_MILESTONE_PROOF_OK:True`。

1. **维护者种子预演证据不计入 3 到 5 的门槛**
2. **每条真实外部报告都必须在同一套受限 schema 内保留 route、version、proof 标记、摩擦点、support bundle 附件备注和 claim-expansion status**
3. **在写 release-candidate、GA 或 `1.0` 级别语言前，必须显式保留 adoption evidence、API drift、support boundary 和 release proof gate**
4. **在 3 到 5 条真实外部报告还没有聚焦到同一个受限风险前，不要扩大 support 或 capability claim**

Phase 380 refresh proof：`ADOPTER_INTAKE_REFRESH_OK:True` 保持这页、GitHub intake 模板和 adopter triage 清单使用同一套受限 schema；`ADOPTER_SUPPORT_BUNDLE_ATTACHMENT_OK:True` 保持 `SUPPORT_BUNDLE_PATH:...` 或 `NO_SUPPORT_BUNDLE:route-cannot-produce-one` 附在同一条记录上；`ADOPTER_CLAIM_EXPANSION_GATE_OK:True` 保持 claim expansion 在 3 到 5 条真实外部报告聚焦到同一个受限风险前继续阻塞。

在达到这个阈值之前，保持这份种子建议不变，不要临时把下一条 beta 线往别处扩。

## 相关文档

- [Beta Support Bundle](./support-bundle.md)
- [Synthetic Adoption Intake Dry-Run Fixtures](./adoption-intake-dry-run.md)
- [Consumer Sample](./consumer-sample.md)
- [Quick Start](./quick-start.md)
- [Project Status](./project-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
