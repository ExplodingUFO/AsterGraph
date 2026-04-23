# Adoption Feedback Loop

这份文档定义了 public beta 的受限 intake 记录，并在预发布反馈循环还在收集足够真实外部反馈时，保持和 issue template 相同的受限 intake 词汇。

## Intake 格式

每条 beta 反馈都应先收口到同一套受限 intake 词汇：`route`、`version`、proof 标记、摩擦点，以及 support bundle 附件备注。

每条反馈都应按这套受限字段记录：

- route（`HelloWorld`、`AsterGraph.Starter.Avalonia`、`HelloWorld.Avalonia`、`ConsumerSample.Avalonia`、`HostSample`、`PackageSmoke`、`ScaleSmoke`、`Demo`）
- version
- proof 标记
- 摩擦点
- support bundle 附件备注：`SUPPORT_BUNDLE_PATH:...`（当 route 产出 bundle 时）或 `NO_SUPPORT_BUNDLE:route-cannot-produce-one`（当 route 不能产出 bundle 时）

截图或命令输出可以作为补充证据附上，但不替代受限 intake 字段里的 proof 标记。

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

下一条 `0.xx` alpha/beta 线继续优先做可复制的 host-owned 参数/元数据打磨；只有当 3 到 5 条真实外部报告聚焦在同一个受限风险上时，才考虑把 defended large-tier performance 或更宽的参数/元数据编辑提上来：

1. **继续优先做可复制的 host-owned 参数/元数据打磨**
2. **维护者种子预演证据不计入 3 到 5 的门槛**
3. **在 3 到 5 条真实外部报告还没有在同一个受限风险上聚焦前，不要把 defended large-tier performance 当成下一步扩展方向**
4. **在 3 到 5 条真实外部报告还没有在同一个受限风险上聚焦前，不要把更宽的参数/元数据编辑当成下一步扩展方向**

在达到这个阈值之前，保持这份种子建议不变，不要临时把下一条 beta 线往别处扩。

## 相关文档

- [Consumer Sample](./consumer-sample.md)
- [Quick Start](./quick-start.md)
- [Project Status](./project-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
