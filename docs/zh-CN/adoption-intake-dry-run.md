# 采用 intake 的 synthetic dry-run 演练记录

这些 synthetic dry-run rehearsal 记录把受限 intake 字段收口在一起：route、version、proof 标记、摩擦，以及 support bundle 附件备注。它们只是 fixture，不是外部验证，也不是 adopter evidence。

## 记录 1：parameter projection 失败

- route: `ConsumerSample.Avalonia -- --proof --support-bundle <support-bundle-path>`
- version: `v0.0.0-dry-run`
- proof 标记: `CONSUMER_SAMPLE_PARAMETER_OK:False`
- 摩擦: parameter projection 没有把预期的 host-owned 值投到选中的节点上
- support bundle 附件备注: `SUPPORT_BUNDLE_PATH:synthetic/parameter-projection.bundle`

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

这条 synthetic rehearsal 记录没有 `parameterSnapshots` 小节，因为 bundle 没有被持久化。
