# Adoption Feedback Loop

这份文档定义了 public beta 的反馈收集格式，并记录了在预发布反馈循环还在收集足够真实外部反馈时的种子试用综合结果。

## Intake 格式

每条 beta 反馈都应先收口到同一套受限字段：`route`、`version`、proof 标记、摩擦点，以及 support bundle 附件。

每条反馈都应按这套受限字段记录：

- route（`HelloWorld`、`AsterGraph.Starter.Avalonia`、`HelloWorld.Avalonia`、`ConsumerSample.Avalonia`、`HostSample`、`PackageSmoke`、`ScaleSmoke`、`Demo`）
- version
- proof 标记（可用时附截图/命令输出）
- 摩擦点
- support bundle 附件（可用时）

公开反馈建议使用 GitHub 上的 `Adoption feedback` issue template。
如果你已经能跑到 `ConsumerSample.Avalonia -- --proof`，优先把 [Beta Support Bundle](./support-bundle.md) 里定义的本地证据包作为 support bundle 附件附上。

## 当前种子试用综合

在仓库还没有积累足够真实外部 issue 之前，当前建议来自四组结构化的公开前 adopter-trial 走查，它们分别覆盖了不同入口路线。

| 画像 | 尝试路线 | 主要摩擦 | 希望下一步补什么 |
| --- | --- | --- | --- |
| Avalonia 宿主集成人员 | `AsterGraph.Starter.Avalonia` -> `HelloWorld.Avalonia` -> `ConsumerSample.Avalonia` | 第一跑很顺，但在没有中等样例时，从最小样例跳到完整宿主接线跨度较大 | 更多可复制的 hosted-UI 模板和 recipe |
| 需要插件能力的 SDK 评估者 | `ConsumerSample.Avalonia` | 不往后读文档的话，trust boundary 还不够醒目 | 更前置的 trust-policy 示例和 plugin-host recipe |
| 现有 retained 宿主维护者 | `Host Integration` + retained migration 文档 | 路线选择和迁移阶段仍需要仔细阅读 | 更多 migration recipe 和路线对照示例 |
| 关注性能的评估者 | `ScaleSmoke` baseline | marker 已有，但预算数字的含义还要结合单独文档理解 | 先持续公开 baseline 数字，再决定是否升级大图 defended budget |

## 当前建议

下一条 `0.xx` alpha/beta 线继续优先做使用友好性和样例打磨，除非证据量/风险已经明显指向别处：

1. **继续优先做使用友好性和样例打磨**
2. **在真实 adopter 需求聚焦之前，不急着把 defended large-tier performance 提前成主线**
3. **更丰富的参数和元数据编辑，等下一轮 adoption 反馈确认需求后再扩**

只要积累到 3 到 5 条真实外部反馈，并且它们在同一条维护中的路线或同一个受限风险上聚焦，这份种子建议就应尽快替换成真实 adopter 报告。

## 相关文档

- [Consumer Sample](./consumer-sample.md)
- [Quick Start](./quick-start.md)
- [Project Status](./project-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
