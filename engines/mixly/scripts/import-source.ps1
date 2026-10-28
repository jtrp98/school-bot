param([Parameter(Mandatory = $true)][string]$SourceDirectory)
$ErrorActionPreference = 'Stop'
$mixlySource = (Resolve-Path -LiteralPath $SourceDirectory).Path
$mixlyApp = Join-Path $mixlySource 'resources/app/src'
$mixlyEngine = Split-Path $PSScriptRoot -Parent
$mixlyDestination = Join-Path $mixlyEngine 'third_party/mixly'
if (-not (Test-Path -LiteralPath "$mixlyApp/board/common/blockly-core/blockly_compressed.js")) {
    throw 'SourceDirectory must contain resources/app/src from sb-app-mixly.'
}
$mixlyFiles = @(Get-ChildItem -LiteralPath "$mixlyApp/board" -File -Recurse | Where-Object {
    $mixlyRelative = [IO.Path]::GetRelativePath($mixlyApp, $_.FullName).Replace('\','/')
    $mixlyRelative -match '/(generators|others)/[^/]+\.js$' -or
    $mixlyRelative -match '/blocks/[^/]*profile\.js$' -or
    $mixlyRelative -match '/(config\.json|index[^/]*\.html)$' -or
    $mixlyRelative -match '/blockly-core/(blockly_compressed|arduino|python_compressed|names|variables|procedures)\.js$'
})
$mixlyFiles += Get-Item -LiteralPath "$mixlyApp/LICENSE", "$mixlyApp/boards.json"
$mixlyInventory = [ordered]@{}
foreach ($mixlyFile in ($mixlyFiles | Sort-Object FullName)) {
    if ($mixlyFile.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Symlink not allowed: $($mixlyFile.FullName)" }
    $mixlyRelative = [IO.Path]::GetRelativePath($mixlyApp, $mixlyFile.FullName).Replace('\','/')
    $mixlyTarget = Join-Path $mixlyDestination $mixlyRelative
    New-Item -ItemType Directory -Force -Path (Split-Path $mixlyTarget -Parent) | Out-Null
    Copy-Item -LiteralPath $mixlyFile.FullName -Destination $mixlyTarget -Force
    $mixlyText = [IO.File]::ReadAllText($mixlyTarget).Replace("`r`n", "`n")
    $mixlyHash = [Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($mixlyText))
    $mixlyInventory[$mixlyRelative] = [Convert]::ToHexString($mixlyHash).ToLowerInvariant()
}
$mixlyManifest = [ordered]@{
    source = 'sb-app-mixly/resources/app/src'
    upstream = 'https://gitee.com/mixly2/mixly2.0_src'
    version = (Get-Content -LiteralPath "$mixlySource/resources/app/package.json" -Raw | ConvertFrom-Json).version
    author = 'Mixly'
    hash_format = 'sha256 of UTF-8 text with LF line endings'
    files = $mixlyInventory
}
[IO.File]::WriteAllText((Join-Path $mixlyEngine 'source-manifest.json'), ($mixlyManifest | ConvertTo-Json -Depth 5) + "`n")
Copy-Item -LiteralPath "$mixlyApp/LICENSE" -Destination "$mixlyEngine/licenses/MIXLY-LICENSE.txt" -Force
Write-Host "Imported $($mixlyFiles.Count) Mixly files. Re-run the engine tests before building a release."
