# 采用 intake 的 synthetic dry-run 演练记录

这些 synthetic dry-run rehearsal 记录把受限 intake 字段收口在一起：route、version、proof 标记、摩擦、support bundle 附件备注，以及 claim-expansion status。它们只是 fixture，不是外部验证，也不是 adopter evidence。

Fixture coverage marker：`ADOPTION_INTAKE_EVIDENCE_OK:True`。

## 预期分流分类

| proof 标记 | 预期分流分类 | 下一步 |
| --- | --- | --- |
| `CONSUMER_SAMPLE_PARAMETER_OK:False` | parameter projection / 写入路径失败 | 进行 sample/session 参数投影或写入路径调查 |
| `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:False` | metadata projection / 元数据投影失败 | 进行定义/inspector 元数据投影调查 |
| `SUPPORT_BUNDLE_PERSISTENCE_OK:False` | support-bundle persistence / 持久化失败 | 进行持久化/路径/环境调查 |

## 记录 1：parameter projection 失败

- route: `ConsumerSample.Avalonia -- --proof --support-bundle <support-bundle-path>`
- version: `v0.0.0-dry-run`
- proof 标记: `CONSUMER_SAMPLE_PARAMETER_OK:False`
- 摩擦: parameter projection 没有把预期的 host-owned 值投到选中的节点上
- support bundle 附件备注: `SUPPORT_BUNDLE_PATH:synthetic/parameter-projection.bundle`
- claim-expansion status: `Unsure / needs maintainer triage`

parameterSnapshots

| key | value |
| --- | --- |
| status | blocked |
| owner | host-seeded |
| priority | P2 |

## 记录 2：metadata projection 失败

- route: `ConsumerSample.Avalonia -- --proof --support-bundle <support-bundle-path>`
- version: `v0.0.0-dry-run`
- proof 标记: `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:False`
- 摩擦: metadata projection 没有暴露选中节点需要的 inspector 字段
- support bundle 附件备注: `SUPPORT_BUNDLE_PATH:synthetic/metadata-projection.bundle`
- claim-expansion status: `Unsure / needs maintainer triage`

parameterSnapshots

| key | value |
| --- | --- |
| status | draft |
| owner | maintainer-seeded |
| priority | P1 |

## 记录 3：support-bundle persistence 失败

- route: `ConsumerSample.Avalonia -- --proof`
- version: `v0.0.0-dry-run`
- proof 标记: `SUPPORT_BUNDLE_PERSISTENCE_OK:False`
- 摩擦: dry-run 在 bundle 写入之前就停止了
- support bundle 附件备注: `NO_SUPPORT_BUNDLE:route-cannot-produce-one`
- claim-expansion status: `No support/capability expansion requested`

这条 synthetic rehearsal 记录没有 `parameterSnapshots` 小节，因为 bundle 没有被持久化。

## 相关文档

- [Project Status](./project-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
- [Beta Support Bundle](./support-bundle.md)
