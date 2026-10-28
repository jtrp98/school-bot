$ErrorActionPreference = 'Stop'
$script:SchoolBotRoot = Split-Path $PSScriptRoot -Parent
$schoolBotGoName = if ($env:OS -eq 'Windows_NT') { 'go.exe' } else { 'go' }
$schoolBotPortable = Join-Path $script:SchoolBotRoot ".tools/go/bin/$schoolBotGoName"
if (Test-Path -LiteralPath $schoolBotPortable) {
    $script:SchoolBotGo = $schoolBotPortable
    $env:GOPATH = Join-Path $script:SchoolBotRoot '.tools/gopath'
    $env:GOCACHE = Join-Path $script:SchoolBotRoot '.tools/gocache'
} else {
    $schoolBotCommand = Get-Command go -ErrorAction SilentlyContinue
    if ($schoolBotCommand) { $script:SchoolBotGo = $schoolBotCommand.Source } else { $script:SchoolBotGo = $null }
}
$script:SchoolBotNpm = if ($IsWindows -or $env:OS -eq 'Windows_NT') { 'npm.cmd' } else { 'npm' }
function Invoke-SchoolBotCommand {
    param([string]$Executable, [string[]]$Arguments)
    & $Executable @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Executable failed (exit $LASTEXITCODE)." }
}
function Assert-SchoolBotGo {
    if (-not $script:SchoolBotGo) { throw 'Install Go (minimum version: the go line in app/go.mod), then reopen your terminal. See README.md.' }
}
function Assert-SchoolBotDesktopExecutable {
    param([string]$Directory, [string]$Version)
    Assert-SchoolBotGo
    $schoolBotSuffix = if ($env:OS -eq 'Windows_NT') { '.exe' } else { '' }
    $schoolBotVersioned = Join-Path $Directory "school-bot-$Version$schoolBotSuffix"
    $schoolBotPlain = Join-Path $Directory "school-bot$schoolBotSuffix"
    foreach ($schoolBotExecutable in @($schoolBotVersioned, $schoolBotPlain)) {
        if (-not (Test-Path -LiteralPath $schoolBotExecutable -PathType Leaf)) {
            throw "Desktop executable missing: $schoolBotExecutable. Run scripts/build-desktop.ps1 first."
        }
    }
    $schoolBotVersionedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $schoolBotVersioned).Hash
    $schoolBotPlainHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $schoolBotPlain).Hash
    if ($schoolBotVersionedHash -ne $schoolBotPlainHash) {
        throw "$schoolBotPlain is not the $Version build ($schoolBotVersioned). Rebuild before packaging."
    }
    # -trimpath drops -ldflags from the embedded build info, so the version resource that is
    # generated from package.json alongside the -X value is what identifies the build.
    Invoke-SchoolBotWindowsResources -Version $Version -Verify $schoolBotPlain
}
function Invoke-SchoolBotWindowsResources {
    param([string]$Version, [string]$Output, [string]$Verify)
    if ($env:OS -ne 'Windows_NT') { return }
    $schoolBotPreviousGoWork = $env:GOWORK
    Push-Location (Join-Path $PSScriptRoot 'windows-resources')
    try {
        $env:GOWORK = 'off'
        $schoolBotResourceArguments = @(
            'run','-mod=readonly','.',
            '-icon',(Join-Path $script:SchoolBotRoot 'app/build/windows/appicon.ico'),
            '-version',$Version
        )
        if ($Output) { $schoolBotResourceArguments += @('-output',$Output) }
        if ($Verify) { $schoolBotResourceArguments += @('-verify',$Verify) }
        Invoke-SchoolBotCommand $script:SchoolBotGo $schoolBotResourceArguments
    } finally {
        $env:GOWORK = $schoolBotPreviousGoWork
        Pop-Location
    }
}
