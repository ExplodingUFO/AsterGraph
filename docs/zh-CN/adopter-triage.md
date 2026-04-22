# 采用者分诊清单

提交公开 beta 回报时，统一使用一组证据字段：

- `version`（包版本或公开 tag）
- `route`（尝试的路线或样例）
- `proof` marker（命中的 proof 标记）
- `support bundle`（当 `ConsumerSample.Avalonia -- --proof` 可生成时，填写本地 JSON 路径；否则注明当前 route 无法生成）

同一套字段应在：

- `adoption_feedback.yml` issue 模板
- Bug 报告模板
- 预发布 notes 的证据复核

这样可以让 `HelloWorld`、`Demo`、`ScaleSmoke` 这类没有 support bundle 的反馈照常进入分诊，同时保持证据合同一致。

在把某条反馈当成“应该扩大支持面”的候选之前，先看 [Project Status](./project-status.md) 里的就绪门禁。落在已证明/受边界约束之外的报告，先当 intake evidence，而不是自动扩大范围。

## 相关链接

- [Adoption Feedback](./adoption-feedback.md)
- [Beta Support Bundle](./support-bundle.md)
