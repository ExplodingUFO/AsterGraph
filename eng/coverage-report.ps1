[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [string]$RepoRoot,

  [Parameter(Mandatory = $true)]
  [string]$ResultsRoot,

  [Parameter(Mandatory = $true)]
  [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$requiredProjects = @(
  [PSCustomObject]@{ Name = 'AsterGraph.Abstractions'; Path = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot 'src/AsterGraph.Abstractions')) },
  [PSCustomObject]@{ Name = 'AsterGraph.Core'; Path = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot 'src/AsterGraph.Core')) },
  [PSCustomObject]@{ Name = 'AsterGraph.Editor'; Path = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot 'src/AsterGraph.Editor')) },
  [PSCustomObject]@{ Name = 'AsterGraph.Avalonia'; Path = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot 'src/AsterGraph.Avalonia')) }
)

function Resolve-CoveragePath {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Path,

    [string[]]$SourceRoots = @()
  )

  if ([System.IO.Path]::IsPathRooted($Path)) {
    return [System.IO.Path]::GetFullPath($Path)
  }

  foreach ($sourceRoot in $SourceRoots) {
    if ([string]::IsNullOrWhiteSpace($sourceRoot)) {
      continue
    }

    $candidatePath = [System.IO.Path]::GetFullPath((Join-Path $sourceRoot $Path))
    if (Test-Path -LiteralPath $candidatePath) {
      return $candidatePath
    }
  }

  return [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $Path))
}

$coverageFiles = @(Get-ChildItem -Path $ResultsRoot -Filter 'coverage.cobertura.xml' -Recurse -File)

if ($coverageFiles.Count -eq 0) {
  throw "No coverage.cobertura.xml files were produced under $ResultsRoot."
}

$lineMap = @{}
$projectHits = @{}

foreach ($project in $requiredProjects) {
  $projectHits[$project.Name] = $false
}

foreach ($coverageFile in $coverageFiles) {
  [xml]$document = Get-Content -LiteralPath $coverageFile.FullName -Raw
  $sourceRoots = @($document.coverage.sources.source | ForEach-Object { [string]$_ })
  $classes = @($document.coverage.packages.package.classes.class)

  foreach ($classNode in $classes) {
    $resolvedFilePath = Resolve-CoveragePath -Path $classNode.filename -SourceRoots $sourceRoots

    foreach ($project in $requiredProjects) {
      if ($resolvedFilePath.StartsWith($project.Path, [System.StringComparison]::OrdinalIgnoreCase)) {
        $projectHits[$project.Name] = $true
      }
    }

    foreach ($lineNode in @($classNode.lines.line)) {
      $lineKey = "{0}|{1}" -f $resolvedFilePath, $lineNode.number
      $lineCovered = [int]$lineNode.hits -gt 0

      if (-not $lineMap.ContainsKey($lineKey)) {
        $lineMap[$lineKey] = $lineCovered
        continue
      }

      $lineMap[$lineKey] = $lineMap[$lineKey] -or $lineCovered
    }
  }
}

$missingProjects = @($requiredProjects | Where-Object { -not $projectHits[$_.Name] })

if ($missingProjects.Count -gt 0) {
  $missingNames = ($missingProjects | ForEach-Object { $_.Name }) -join ', '
  throw "Coverage reports did not include the publishable package surface for: $missingNames"
}

$totalLines = $lineMap.Count
$coveredLines = @($lineMap.Values | Where-Object { $_ }).Count
$lineRate = if ($totalLines -eq 0) {
  0
}
else {
  [Math]::Round(($coveredLines / $totalLines) * 100, 2)
}

$report = [PSCustomObject]@{
  generatedAt = [DateTimeOffset]::UtcNow.ToString('o')
  resultsRoot = [System.IO.Path]::GetFullPath($ResultsRoot)
  coverageFiles = @($coverageFiles | ForEach-Object { $_.FullName })
  totalLines = $totalLines
  coveredLines = $coveredLines
  lineRate = $lineRate
  publishableProjects = @(
    $requiredProjects | ForEach-Object {
      [PSCustomObject]@{
        name = $_.Name
        covered = $projectHits[$_.Name]
      }
    }
  )
}

$outputDirectory = Split-Path -Parent $OutputPath

if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
  New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $OutputPath

Write-Host ("COVERAGE_REPORT_OK:{0}:{1}:{2}" -f $coveredLines, $totalLines, $lineRate) -ForegroundColor Green
