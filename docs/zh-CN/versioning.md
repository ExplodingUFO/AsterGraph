# AsterGraph 版本说明

## 对外包版本

对外用户真正要关注的版本，是四个公开发布包在 NuGet 上的版本：

- `AsterGraph.Abstractions`
- `AsterGraph.Core`
- `AsterGraph.Editor`
- `AsterGraph.Avalonia`

当前公开基线：

- 包版本：`0.2.0-alpha.1`

如果宿主问“我该装哪个版本的 AsterGraph”，以这个包版本为准。

## 仓库 Tag 与 GitHub Release

仓库里已经存在一些历史 tag，比如 `v1.9`、`v1.10`、`v1.11`。

这些 tag 来自公开前的 milestone 式仓库检查点，适合维护者回看历史，但**不是**外部用户在 NuGet 上安装的包版本。

后续对外版本约定是：

- 公开发布包使用与包 SemVer 对齐的 tag，例如 `v0.2.0-alpha.2`
- GitHub prerelease 与 NuGet 发布包使用同一套版本号
- milestone 风格的本地规划版本可以继续私下使用，但不再作为对外包版本展示

## 实际使用规则

如果你是：

- SDK 使用者：看 NuGet 包版本
- release note 阅读者：默认认为公开 prerelease tag 与 NuGet 包版本一致
- 仓库维护者：把旧的 `v1.x` tag 当成历史 milestone 标记，不要当成当前公开包线

## 相关文档

- [Quick Start](./quick-start.md)
- [Project Status](./project-status.md)
- [Alpha Status](./alpha-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
