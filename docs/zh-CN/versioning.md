# AsterGraph 版本说明

## 对外包版本

对外用户真正要关注的版本，是四个公开发布包在 NuGet 上的版本：

- `AsterGraph.Abstractions`
- `AsterGraph.Core`
- `AsterGraph.Editor`
- `AsterGraph.Avalonia`

当前公开基线：

- 包版本：`0.11.0-beta`
- 与当前包版本配对的对外 SemVer prerelease 标签：`v0.11.0-beta`
- 历史仓库里程碑标签系列：`v1.x` 风格的公开前检查点

如果宿主问“我该装哪个版本的 AsterGraph”，以这个包版本为准。

## 仓库 Tag 与 GitHub Release

仓库现在已经开始发布和包版本一致的公开 prerelease tag，例如 `v0.11.0-beta`。

仓库里也仍保留着像 `v1.x` 风格检查点这样的历史 tag。

这些 `v1.x` 风格 tag 来自公开前的 milestone 式仓库检查点，适合维护者回看历史，但**不是**外部用户在 NuGet 上安装的包版本。

后续对外版本约定是：

- 公开发布包使用与包 SemVer 对齐的 tag，例如 `v0.11.0-beta`
- GitHub prerelease 与 NuGet 发布包使用同一套版本号
- milestone 风格的本地规划版本可以继续私下使用，但不再作为对外包版本展示

## 当前映射

| 对外概念 | 当前值 | 应该怎么理解 |
| --- | --- | --- |
| 可安装包版本 | `0.11.0-beta` | 外部用户从 nuget.org 安装的版本 |
| 与当前包版本配对的对外 prerelease tag | `v0.11.0-beta` | GitHub 上与可安装包版本严格对齐的公开 tag |
| 历史公开仓库里程碑标签系列 | `v1.x` 风格的检查点 tag | 仅用于回看公开前的旧检查点模式 |

## 实际使用规则

如果你是：

- SDK 使用者：看 NuGet 包版本
- release note 阅读者：默认认为公开 prerelease tag 与 NuGet 包版本一致
- 仓库维护者：把旧的 `v1.x` 风格 tag 当成历史 milestone 标记，不要当成当前公开包线

## 发布说明首屏规则

公开 prerelease note 的第一屏，至少要写清楚：

1. 可安装包版本
2. 与之匹配的公开 tag
3. 只有在确实需要解释旧仓库历史时，才补一行历史 milestone 引用

这几行现在由公开 prerelease workflow 自动生成并校验。除非确实需要补 legacy 历史说明，否则维护者不应该手工改 release note 的第一块内容。

例如：

- 包版本：`0.11.0-beta`
- 公开 tag：`v0.11.0-beta`
- 历史仓库检查点引用：`legacy v1.x 风格的里程碑检查点`（不是可安装版本）

自动生成的 prerelease note 还会带上 release lane 的 proof summary，让外部使用者直接看到同一组安装、兼容性、规模和覆盖率信号，而不必再去翻 workflow artifact。

## 相关文档

- [Quick Start](./quick-start.md)
- [稳定化支持矩阵](./stabilization-support-matrix.md)
- [Project Status](./project-status.md)
- [Alpha Status](./alpha-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
