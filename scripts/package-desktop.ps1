param(
    [switch]$SkipBuild,
    [switch]$SkipInstall,
    [string]$PayloadDirectory,
    [ValidateSet('th','en','zh')][string]$Language = 'th'
)
. "$PSScriptRoot/common.ps1"
if ($IsMacOS) {
    if ($PayloadDirectory) { throw 'PayloadDirectory is currently supported by the Windows package only.' }
    $schoolBotArguments = @("$PSScriptRoot/package-desktop.sh")
    if ($SkipBuild) { $schoolBotArguments += '--skip-build' }
    if ($SkipInstall) { $schoolBotArguments += '--skip-install' }
    Invoke-SchoolBotCommand 'bash' $schoolBotArguments
    return
}
if ($env:OS -ne 'Windows_NT') { throw 'Installer packaging supports Windows and macOS.' }
Invoke-SchoolBotCommand 'node' @("$PSScriptRoot/check-notices.mjs")
$schoolBotPayload = if ($PayloadDirectory) { $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PayloadDirectory) } else { Join-Path $SchoolBotRoot 'app/bin' }
if (-not $SkipBuild) { & "$PSScriptRoot/build-desktop.ps1" -SkipInstall:$SkipInstall -OutputDirectory $schoolBotPayload }
& "$PSScriptRoot/prepare-installer-tools.ps1"
$schoolBotBrandDirectory = Join-Path $SchoolBotRoot ".tools/installer-brand/$Language"
& "$PSScriptRoot/prepare-installer-brand.ps1" -OutputDirectory $schoolBotBrandDirectory -Language $Language
$schoolBotCulture = @{ th = 'th-TH'; en = 'en-US'; zh = 'zh-CN' }[$Language]
$schoolBotVersion = (Get-Content -LiteralPath (Join-Path $SchoolBotRoot 'app/frontend/package.json') -Raw | ConvertFrom-Json).version
if ($schoolBotVersion -notmatch '^\d+\.\d+\.\d+$') { throw 'Installer version must be x.y.z.' }
Assert-SchoolBotDesktopExecutable -Directory $schoolBotPayload -Version $schoolBotVersion
$schoolBotOutputDir = Join-Path $SchoolBotRoot 'dist/installers'
New-Item -ItemType Directory -Path $schoolBotOutputDir -Force | Out-Null
$schoolBotInstaller = Join-Path $schoolBotOutputDir "school-bot-$schoolBotVersion-windows-amd64.msi"
Invoke-SchoolBotCommand (Join-Path $SchoolBotRoot '.tools/wix5/wix.exe') @(
    'build', '-arch','x64','-pdbtype','none',
    '-ext',(Join-Path $SchoolBotRoot '.tools/wix5/.wix/extensions/WixToolset.UI.wixext/5.0.2/wixext5/WixToolset.UI.wixext.dll'),
    '-culture',$schoolBotCulture,
    '-loc',(Join-Path $schoolBotBrandDirectory 'installer.wxl'),
    '-d',"BrandDir=$schoolBotBrandDirectory",
    '-d',"AppVersion=$schoolBotVersion",
    '-d',"PayloadDir=$schoolBotPayload",
    '-d',"Bootstrapper=$(Join-Path $SchoolBotRoot '.tools/installer-downloads/MicrosoftEdgeWebview2Setup.exe')",
    '-d',"Notices=$(Join-Path $SchoolBotRoot 'THIRD-PARTY-NOTICES.md')",
    '-d',"AppIcon=$(Join-Path $SchoolBotRoot 'app/build/windows/appicon.ico')",
    '-out',$schoolBotInstaller,
    (Join-Path $SchoolBotRoot 'app/build/windows/Package.wxs'),
    (Join-Path $SchoolBotRoot 'app/build/windows/InstallerUI.wxs'),
    (Join-Path $SchoolBotRoot 'app/build/windows/InstallerDialogs.wxs')
)
& "$PSScriptRoot/tests/installer-package.test.ps1" -InstallerPath $schoolBotInstaller
$schoolBotInstallerHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $schoolBotInstaller).Hash.ToLowerInvariant()
[IO.File]::WriteAllText("$schoolBotInstaller.sha256", "$schoolBotInstallerHash  $([IO.Path]::GetFileName($schoolBotInstaller))`n")
$schoolBotShellOutput = Join-Path $SchoolBotRoot ".tools/installer-shell/$Language/$schoolBotVersion"
& "$PSScriptRoot/build-installer-shell.ps1" -InstallerPath $schoolBotInstaller `
    -BrandDirectory $schoolBotBrandDirectory -OutputDirectory $schoolBotShellOutput
$schoolBotSetup = Join-Path $schoolBotOutputDir "school-bot-$schoolBotVersion-windows-amd64-setup.exe"
Copy-Item -LiteralPath (Join-Path $schoolBotShellOutput 'SchoolBotSetup.exe') -Destination $schoolBotSetup -Force
Copy-Item -LiteralPath (Join-Path $schoolBotBrandDirectory 'runtime-notices.txt') `
    -Destination "$schoolBotSetup.NOTICES.txt" -Force
$schoolBotSetupHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $schoolBotSetup).Hash.ToLowerInvariant()
[IO.File]::WriteAllText("$schoolBotSetup.sha256", "$schoolBotSetupHash  $([IO.Path]::GetFileName($schoolBotSetup))`n")
Write-Host "Desktop installer ready: $schoolBotSetup (MSI also available: $schoolBotInstaller)"
Write-Host 'Development EXE/MSI are unsigned. Sign both packages before external distribution.'
