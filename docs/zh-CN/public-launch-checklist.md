# AsterGraph 对外发布检查清单

在把仓库切成公开可见，或者推送第一个公开 prerelease tag 之前，先跑完这份检查清单。

## 1. 可见性与分支策略

- 确认默认分支就是要对外公开的分支
- 给默认分支开启 branch protection
- 要求通过代表官方发布门禁的 `ci` workflow checks
- 确认 release 权限和 NuGet 发布权限只给维护者

## 2. 公开仓库表面

- 确认 `README.md` 和 `README.zh-CN.md` 都指向当前 public alpha 文档和入口矩阵
- 确认 `CONTRIBUTING.md`、`CODE_OF_CONDUCT.md`、`SECURITY.md` 都存在且内容仍然准确
- 确认 `.github` 里的 issue templates 和 pull request template 已启用
- 确认仓库 description、topics、homepage 和当前 alpha 叙事一致

## 3. 必跑验证

只跑维护中的官方入口，不要临时拼 ad-hoc 命令：

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane hygiene -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

## 4. 检查 Proof Artifact

公开前至少复核这些产物：

- `artifacts/proof/public-repo-hygiene.txt`
- `artifacts/proof/hostsample-packed.txt`
- `artifacts/proof/hostsample-net10-packed.txt`
- `artifacts/proof/package-smoke.txt`
- `artifacts/proof/scale-smoke.txt`
- `artifacts/proof/coverage-report.txt`
- `artifacts/coverage/release-summary.json`

重点 marker：

- `PUBLIC_REPO_HYGIENE_OK:True`
- `HOST_SAMPLE_OK:True`
- `HOST_SAMPLE_NET10_OK:True`
- `PACKAGE_SMOKE_OK:True`
- `SCALE_HISTORY_CONTRACT_OK:...`
- `COVERAGE_REPORT_OK:...`

## 5. 第一个公开 Prerelease Tag

- 确认工作区干净
- 先把要打 tag 的分支或 `master` 推上去
- 创建并推送下一个公开 `v*` tag
- 从头到尾观察 `.github/workflows/release.yml`
- 如果配置了 `NUGET_API_KEY`，确认包发布成功
- 如果没有配置 `NUGET_API_KEY`，确认 workflow 是有意跳过 NuGet publish，而不是失败

## 6. 对外入口说明

在 release note、公告、README 里把入口说清楚：

- `tools/AsterGraph.HostSample` = 最小 consumer proof
- `tools/AsterGraph.PackageSmoke` = packaged-consumption proof
- `tools/AsterGraph.ScaleSmoke` = 规模 / history / 状态连续性 proof
- `src/AsterGraph.Demo` = showcase host
- `docs/zh-CN/quick-start.md` = canonical adoption path
- `docs/zh-CN/alpha-status.md` = alpha 范围与限制
