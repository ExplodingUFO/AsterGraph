[CmdletBinding()]
param(
  [string]$RepoRoot,
  [string]$PublicTag
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
  $RepoRoot = Split-Path -Parent $PSScriptRoot
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$propsPath = Join-Path $resolvedRepoRoot 'Directory.Build.props'

if (-not (Test-Path -LiteralPath $propsPath)) {
  throw "Directory.Build.props was not found under repo root: $resolvedRepoRoot"
}

[xml]$props = Get-Content -LiteralPath $propsPath -Raw
$versionNode = $props.SelectSingleNode('/Project/PropertyGroup/Version')
$packageVersion = if ($null -ne $versionNode) { $versionNode.InnerText.Trim() } else { $null }

if ([string]::IsNullOrWhiteSpace($packageVersion)) {
  throw 'Directory.Build.props does not define a non-empty package version.'
}

$expectedTag = "v$packageVersion"
$effectiveTag = if ([string]::IsNullOrWhiteSpace($PublicTag)) { $expectedTag } else { $PublicTag.Trim() }

if ($effectiveTag -ne $expectedTag) {
  throw "Public tag '$effectiveTag' does not match package version '$packageVersion'. Expected '$expectedTag'."
}

function Get-RepoText {
  param([Parameter(Mandatory = $true)][string]$RelativePath)

  $path = Join-Path $resolvedRepoRoot $RelativePath
  if (-not (Test-Path -LiteralPath $path)) {
    throw "Required public versioning file is missing: $RelativePath"
  }

  return Get-Content -LiteralPath $path -Raw
}

function Assert-ContainsText {
  param(
    [Parameter(Mandatory = $true)][string]$RelativePath,
    [Parameter(Mandatory = $true)][string]$ExpectedText
  )

  $text = Get-RepoText -RelativePath $RelativePath
  if (-not $text.Contains($ExpectedText, [System.StringComparison]::Ordinal)) {
    throw "Public versioning file '$RelativePath' is missing required text: $ExpectedText"
  }
}

Assert-ContainsText 'README.md' "current installable package version: ``$packageVersion``"
Assert-ContainsText 'README.md' "matching public prerelease tag for this package line: ``$effectiveTag``"
Assert-ContainsText 'README.md' 'GitHub prerelease/Release'
Assert-ContainsText 'README.md' '[Versioning](./docs/en/versioning.md)'

Assert-ContainsText 'README.zh-CN.md' "当前可安装包版本：``$packageVersion``"
Assert-ContainsText 'README.zh-CN.md' "对外 SemVer prerelease 标签：``$effectiveTag``"
Assert-ContainsText 'README.zh-CN.md' 'GitHub prerelease/Release'
Assert-ContainsText 'README.zh-CN.md' '[Versioning](./docs/zh-CN/versioning.md)'

Assert-ContainsText 'docs/en/versioning.md' "package version: ``$packageVersion``"
Assert-ContainsText 'docs/en/versioning.md' "public tag: ``$effectiveTag``"
Assert-ContainsText 'docs/en/versioning.md' 'GitHub Releases'
Assert-ContainsText 'docs/en/versioning.md' 'local planning-only milestone labels'
Assert-ContainsText 'docs/en/versioning.md' 'not release identifiers'

Assert-ContainsText 'docs/zh-CN/versioning.md' "包版本：``$packageVersion``"
Assert-ContainsText 'docs/zh-CN/versioning.md' "公开 tag：``$effectiveTag``"
Assert-ContainsText 'docs/zh-CN/versioning.md' 'GitHub Release'
Assert-ContainsText 'docs/zh-CN/versioning.md' '本地规划专用里程碑标签'
Assert-ContainsText 'docs/zh-CN/versioning.md' '不是发布标识'

Assert-ContainsText '.github/ISSUE_TEMPLATE/config.yml' 'docs/en/versioning.md'
Assert-ContainsText 'docs/en/public-launch-checklist.md' '[Versioning](./versioning.md)'
Assert-ContainsText 'docs/zh-CN/public-launch-checklist.md' '[Versioning](./versioning.md)'

Write-Host "PUBLIC_VERSIONING_OK:${packageVersion}:${effectiveTag}"
