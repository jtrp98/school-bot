param([string]$OutputDirectory, [switch]$Test)
$ErrorActionPreference = 'Stop'
$mixlySource = Split-Path $PSScriptRoot -Parent
. "$mixlySource/../../scripts/common.ps1"
Assert-SchoolBotGo
$mixlyPreviousWorkspace = $env:GOWORK
$env:GOWORK = 'off'
Push-Location $mixlySource
try {
    $mixlyPlatform = (& $SchoolBotGo env GOOS).Trim()
    $mixlyArchitecture = (& $SchoolBotGo env GOARCH).Trim()
    if (-not $OutputDirectory) { $OutputDirectory = Join-Path $mixlySource "bin/$mixlyPlatform-$mixlyArchitecture" }
    $OutputDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
    $mixlySuffix = if ($mixlyPlatform -eq 'windows') { '.exe' } else { '' }
    $mixlyExecutable = Join-Path $OutputDirectory "mixly-engine$mixlySuffix"
    New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
    if (Test-Path -LiteralPath $mixlyExecutable -PathType Leaf) {
        try {
            $mixlyProbe = [IO.File]::Open($mixlyExecutable, 'Open', 'ReadWrite', 'None')
            $mixlyProbe.Dispose()
        } catch { throw "Engine output is in use or not writable. Use -OutputDirectory: $mixlyExecutable" }
    }
    if ($Test) {
        Invoke-SchoolBotCommand $SchoolBotGo @('test','-mod=readonly','./...')
        Invoke-SchoolBotCommand $SchoolBotGo @('vet','-mod=readonly','./...')
    }
    Invoke-SchoolBotCommand $SchoolBotGo @(
        'build','-mod=readonly','-trimpath','-buildvcs=false','-ldflags','-s -w','-o',$mixlyExecutable,'.'
    )
    Copy-Item -LiteralPath "$mixlySource/engine.json" -Destination "$OutputDirectory/engine.json" -Force
    Copy-Item -LiteralPath "$mixlySource/THIRD-PARTY-NOTICES.md" `
        -Destination "$OutputDirectory/THIRD-PARTY-NOTICES.md" -Force
    New-Item -ItemType Directory -Force -Path "$OutputDirectory/licenses" | Out-Null
    Get-ChildItem -LiteralPath "$mixlySource/licenses" -File | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination "$OutputDirectory/licenses/$($_.Name)" -Force
    }
    $mixlyHash = (Get-FileHash -LiteralPath $mixlyExecutable -Algorithm SHA256).Hash.ToLowerInvariant()
    [IO.File]::WriteAllText("$mixlyExecutable.sha256", "$mixlyHash  mixly-engine$mixlySuffix`n")
    if ($Test) {
        $mixlyPreviousEngine = $env:MIXLY_ENGINE
        try {
            $env:MIXLY_ENGINE = $mixlyExecutable
            Invoke-SchoolBotCommand 'node' @('--test',"$mixlySource/tests/ipc.test.mjs")
        } finally { $env:MIXLY_ENGINE = $mixlyPreviousEngine }
    }
    Write-Host "Mixly engine ready: $mixlyExecutable"
} finally {
    Pop-Location
    $env:GOWORK = $mixlyPreviousWorkspace
}
