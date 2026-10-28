param([switch]$SkipBuild)
. "$PSScriptRoot/common.ps1"
if (-not $SkipBuild) { & "$PSScriptRoot/build-desktop.ps1" -SkipInstall }
if ($IsMacOS) {
    Invoke-SchoolBotCommand 'bash' @("$PSScriptRoot/start-desktop.sh", '--skip-build')
    return
}
$schoolBotSuffix = if ($env:OS -eq 'Windows_NT') { '.exe' } else { '' }
$schoolBotVersion = (Get-Content -LiteralPath (Join-Path $SchoolBotRoot 'app/frontend/package.json') -Raw | ConvertFrom-Json).version
$schoolBotExecutable = Join-Path $SchoolBotRoot "app/bin/school-bot-$schoolBotVersion$schoolBotSuffix"
if (-not (Test-Path -LiteralPath $schoolBotExecutable -PathType Leaf)) {
    throw "Desktop build missing: $schoolBotExecutable. Run without -SkipBuild."
}
Write-Host "Starting $schoolBotExecutable"
Push-Location (Join-Path $SchoolBotRoot 'app')
try { & $schoolBotExecutable } finally { Pop-Location }
