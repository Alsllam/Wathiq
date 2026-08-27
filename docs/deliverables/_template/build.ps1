<#
.SYNOPSIS  Render docs/deliverables/<key>.md to docs/deliverables/out/<Key>.docx with Pandoc.
.EXAMPLE   .\docs\deliverables\_template\build.ps1 srs
           .\docs\deliverables\_template\build.ps1 all
#>
param([Parameter(Mandatory = $true)][string]$Key)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot           # docs/deliverables
$outDir = Join-Path $root 'out'
New-Item -ItemType Directory -Force $outDir | Out-Null

$keys = if ($Key -eq 'all') {
    Get-ChildItem $root -Filter '*.md' | Where-Object Name -ne 'README.md' | ForEach-Object BaseName
} else { @($Key) }

foreach ($k in $keys) {
    $src = Join-Path $root "$k.md"
    if (-not (Test-Path $src)) { throw "No source: $src" }
    # Output name: kebab key -> PascalCase file (ai-safety -> AiSafety.docx)
    $name = ($k -split '-' | ForEach-Object { $_.Substring(0,1).ToUpper() + $_.Substring(1) }) -join ''
    $dst = Join-Path $outDir "$name.docx"

    # Argument array instead of backtick continuations: readable and comment-safe.
    $args_ = @(
        $src, '-o', $dst,
        '--from', 'markdown+yaml_metadata_block+fenced_divs',
        '--reference-doc', (Join-Path $PSScriptRoot 'reference.docx'),
        '--resource-path', $root,          # images referenced as assets/x.png resolve
        '--toc', '--toc-depth', '3',
        '--number-sections'
    )
    & pandoc @args_
    if ($LASTEXITCODE -ne 0) { throw "pandoc failed for $k" }
    $kb = [math]::Round((Get-Item $dst).Length / 1KB)
    Write-Host "OK  $k -> out/$name.docx ($kb KB)"
}
