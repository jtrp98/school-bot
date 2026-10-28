param([switch]$SkipInstall, [string]$OutputDirectory)
. "$PSScriptRoot/common.ps1"
Assert-SchoolBotGo
if ($IsMacOS) {
    if ($OutputDirectory) { throw 'OutputDirectory is currently supported by the Windows build only.' }
    $schoolBotArguments = @("$PSScriptRoot/build-desktop.sh")
    if ($SkipInstall) { $schoolBotArguments += '--skip-install' }
    Invoke-SchoolBotCommand 'bash' $schoolBotArguments
    return
}
$schoolBotDesktopOutput = if ($OutputDirectory) {
    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
} else {
    Join-Path $SchoolBotRoot 'app/bin'
}
New-Item -ItemType Directory -Path $schoolBotDesktopOutput -Force | Out-Null
Push-Location (Join-Path $SchoolBotRoot 'app/frontend')
try {
    if (-not $SkipInstall) { Invoke-SchoolBotCommand $SchoolBotNpm @('ci','--no-audit','--no-fund') }
    Invoke-SchoolBotCommand $SchoolBotNpm @('run','build')
} finally { Pop-Location }
& "$PSScriptRoot/build-engine.ps1" -Engine zide -OutputDirectory (Join-Path $schoolBotDesktopOutput 'engines/zide')
$schoolBotManifest = Join-Path $SchoolBotRoot 'app/frontend/package.json'
$schoolBotResourceVersion = (Get-Content -LiteralPath $schoolBotManifest -Raw | ConvertFrom-Json).version
if ($schoolBotResourceVersion -cnotmatch '^\d+\.\d+\.\d+$') { throw 'package.json version must be x.y.z.' }
Invoke-SchoolBotWindowsResources -Version $schoolBotResourceVersion `
    -Output (Join-Path $SchoolBotRoot 'app/resource_windows_amd64.syso')
Push-Location (Join-Path $SchoolBotRoot 'app')
try {
    $schoolBotSuffix = if ($env:OS -eq 'Windows_NT') { '.exe' } else { '' }
    Invoke-SchoolBotCommand $SchoolBotGo @(
        'build','-trimpath','-ldflags','-s -w','-buildvcs=false',
        '-o',(Join-Path $schoolBotDesktopOutput "demo-engine$schoolBotSuffix"),'./cmd/demo-engine'
    )
    # Keep the unversioned copy because the installer and start scripts expect that name.
    $schoolBotVersionedExe = Join-Path $schoolBotDesktopOutput "school-bot-$schoolBotResourceVersion$schoolBotSuffix"
    $schoolBotPlainExe = Join-Path $schoolBotDesktopOutput "school-bot$schoolBotSuffix"
    $schoolBotBuild = @('build','-trimpath','-buildvcs=false','-tags','production','-o',$schoolBotVersionedExe)
    $schoolBotLinkFlags = if ($env:OS -eq 'Windows_NT') { '-s -w -H windowsgui' } else { '-s -w' }
    $schoolBotLinkFlags += " -X main.appVersion=$schoolBotResourceVersion"
    if ($env:SCHOOL_BOT_BUILD_API_URL) {
        $schoolBotAPIPattern = (Get-Content -LiteralPath "$PSScriptRoot/api-url-pattern.txt" -Raw).Trim()
        if ($env:SCHOOL_BOT_BUILD_API_URL -cnotmatch $schoolBotAPIPattern) {
            throw ('SCHOOL_BOT_BUILD_API_URL must be an HTTPS URL (HTTP only for localhost), ' +
                'without credentials, query, fragment or spaces.')
        }
        $schoolBotLinkFlags += " -X main.defaultAPIURL=$env:SCHOOL_BOT_BUILD_API_URL"
        Write-Host "Built-in API endpoint: $env:SCHOOL_BOT_BUILD_API_URL"
    }
    $schoolBotBuild += @('-ldflags',$schoolBotLinkFlags)
    $schoolBotBuild += '.'
    Invoke-SchoolBotCommand $SchoolBotGo $schoolBotBuild
    Invoke-SchoolBotWindowsResources -Version $schoolBotResourceVersion -Verify $schoolBotVersionedExe
    try { Copy-Item -LiteralPath $schoolBotVersionedExe -Destination $schoolBotPlainExe -Force -ErrorAction Stop }
    catch {
        $schoolBotCopyError = $_.Exception.Message
        $schoolBotCopyAdvice = 'Close the app or use -OutputDirectory.'
        throw ("Could not update $schoolBotPlainExe, a stale exe would be packaged. " +
            "$schoolBotCopyAdvice $schoolBotCopyError")
    }
    Assert-SchoolBotDesktopExecutable -Directory $schoolBotDesktopOutput -Version $schoolBotResourceVersion
    Write-Host "Desktop ready: $schoolBotVersionedExe (bundle demo-engine and engines/zide with the app)."
} finally { Pop-Location }
