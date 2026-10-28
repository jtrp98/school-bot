param([string]$PayloadDirectory)
. "$PSScriptRoot/common.ps1"
if ($env:OS -ne 'Windows_NT') { throw 'Run the installer smoke test on Windows.' }
$schoolBotPayload = if ($PayloadDirectory) {
    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PayloadDirectory)
} else {
    Join-Path $SchoolBotRoot 'app/bin'
}
$schoolBotTestIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
$schoolBotPrincipal = [Security.Principal.WindowsPrincipal]::new($schoolBotTestIdentity)
if (-not $schoolBotPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run MSI integration tests from an elevated PowerShell or disposable CI VM. No installation was started.'
}
$schoolBotManifest = Join-Path $SchoolBotRoot 'app/frontend/package.json'
$schoolBotVersion = (Get-Content -LiteralPath $schoolBotManifest -Raw | ConvertFrom-Json).version
$schoolBotSetup = Join-Path $SchoolBotRoot "dist/installers/school-bot-$schoolBotVersion-windows-amd64.msi"
$schoolBotTestDir = Join-Path $SchoolBotRoot '.tools/installer-smoke/School Bot'
if (-not (Test-Path -LiteralPath $schoolBotSetup)) { throw 'Build the Desktop MSI first.' }
$schoolBotMsiApi = New-Object -ComObject WindowsInstaller.Installer
$schoolBotDatabase = $schoolBotMsiApi.OpenDatabase($schoolBotSetup, 0)
$schoolBotView = $schoolBotDatabase.OpenView("SELECT ``Value`` FROM ``Property`` WHERE ``Property`` = 'ProductCode'")
$schoolBotView.Execute()
$schoolBotRecord = $schoolBotView.Fetch()
$schoolBotProductCode = $schoolBotRecord.StringData(1)
$schoolBotView.Close()
foreach ($schoolBotComObject in @($schoolBotRecord,$schoolBotView,$schoolBotDatabase,$schoolBotMsiApi)) {
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($schoolBotComObject)
}
if ($schoolBotProductCode -notmatch '^\{[0-9A-Fa-f-]{36}\}$') { throw 'Invalid MSI ProductCode.' }
$schoolBotRegistration = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\$schoolBotProductCode"
$schoolBotMachineRegistration = "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\$schoolBotProductCode"
$schoolBotProductApi = New-Object -ComObject WindowsInstaller.Installer
$schoolBotSettings = 'HKCU:\Software\SchoolBot'
$schoolBotMenu = Join-Path ([Environment]::GetFolderPath('Programs')) 'School Bot'
$schoolBotShortcut = Join-Path ([Environment]::GetFolderPath('DesktopDirectory')) 'School Bot.lnk'
foreach ($schoolBotExisting in @(
    $schoolBotTestDir,$schoolBotRegistration,$schoolBotMachineRegistration,
    $schoolBotSettings,$schoolBotMenu,$schoolBotShortcut
)) {
    if (Test-Path -LiteralPath $schoolBotExisting) {
        throw "Existing School Bot state found; use a clean test account. Left untouched: $schoolBotExisting"
    }
}
New-Item -ItemType Directory -Path (Split-Path $schoolBotTestDir -Parent) -Force | Out-Null
function Invoke-SchoolBotMsi {
    param([string[]]$MsiArguments, [string]$LogName)
    $schoolBotLog = Join-Path (Split-Path $schoolBotTestDir -Parent) $LogName
    $schoolBotProcessArguments = $MsiArguments + @('/qn','/norestart','/L*v',"`"$schoolBotLog`"")
    $schoolBotProcess = Start-Process -FilePath 'msiexec.exe' -ArgumentList $schoolBotProcessArguments `
        -Wait -PassThru -WindowStyle Hidden
    if ($schoolBotProcess.ExitCode -notin @(0,3010)) {
        throw "msiexec failed: $($schoolBotProcess.ExitCode). See $schoolBotLog"
    }
}
$schoolBotConfig = Join-Path ([Environment]::GetFolderPath('ApplicationData')) 'SchoolBotPoC'
function Get-SchoolBotProjectSnapshot {
    if (Test-Path -LiteralPath $schoolBotConfig) {
        Get-ChildItem -LiteralPath $schoolBotConfig -File | Sort-Object Name | ForEach-Object {
            $schoolBotHash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
            "$($_.Name):$schoolBotHash"
        }
    }
}
$schoolBotBefore = @(Get-SchoolBotProjectSnapshot) -join "`n"
$schoolBotPayloadFiles = @(
    'school-bot.exe','demo-engine.exe','engines/zide/zide-engine.exe','engines/zide/engine.json',
    'engines/zide/THIRD-PARTY-NOTICES.md','engines/zide/HIDAPI-LICENSE.txt',
    'engines/zide/JSON-LICENSE.txt','engines/zide/zide-engine.exe.sha256'
)
try {
    Invoke-SchoolBotMsi -MsiArguments @('/i',"`"$schoolBotSetup`"","INSTALLFOLDER=`"$schoolBotTestDir`"") `
        -LogName 'install.log'
    foreach ($schoolBotFile in $schoolBotPayloadFiles) {
        $schoolBotInstalledPath = Join-Path $schoolBotTestDir $schoolBotFile
        $schoolBotOriginalPath = Join-Path $schoolBotPayload $schoolBotFile
        $schoolBotInstalledHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $schoolBotInstalledPath).Hash
        $schoolBotOriginalHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $schoolBotOriginalPath).Hash
        if ($schoolBotInstalledHash -ne $schoolBotOriginalHash) { throw "Installed payload differs: $schoolBotFile" }
    }
    $schoolBotEntries = foreach ($schoolBotRegistryPath in @($schoolBotRegistration,$schoolBotMachineRegistration)) {
        Get-ItemProperty -LiteralPath $schoolBotRegistryPath -ErrorAction SilentlyContinue
    }
    $schoolBotEntry = $schoolBotEntries | Select-Object -First 1
    $schoolBotRegisteredCorrectly = (
        $schoolBotEntry -and $schoolBotEntry.WindowsInstaller -eq 1 -and
        $schoolBotProductApi.ProductState($schoolBotProductCode) -eq 5 -and
        $schoolBotProductApi.ProductInfo($schoolBotProductCode, 'AssignmentType') -eq '0' -and
        $schoolBotProductApi.ProductInfo($schoolBotProductCode, 'InstallLocation').TrimEnd('\') -eq
            $schoolBotTestDir -and
        $schoolBotProductApi.ProductInfo($schoolBotProductCode, 'VersionString') -eq $schoolBotVersion
    )
    if (-not $schoolBotRegisteredCorrectly) { throw 'Windows Installer registration is incorrect.' }
    $schoolBotShell = New-Object -ComObject WScript.Shell
    foreach ($schoolBotLinkPath in @($schoolBotShortcut,(Join-Path $schoolBotMenu 'School Bot.lnk'))) {
        if (-not (Test-Path -LiteralPath $schoolBotLinkPath)) { throw "Shortcut missing: $schoolBotLinkPath" }
        $schoolBotLink = $schoolBotShell.CreateShortcut($schoolBotLinkPath)
        try {
            if ($schoolBotLink.TargetPath -ne (Join-Path $schoolBotTestDir 'school-bot.exe')) {
                throw 'Shortcut points outside the installed app.'
            }
        } finally { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($schoolBotLink) }
    }
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($schoolBotShell)
    Remove-Item -LiteralPath (Join-Path $schoolBotTestDir 'demo-engine.exe')
    Remove-Item -LiteralPath (Join-Path $schoolBotTestDir 'engines/zide/zide-engine.exe')
    Invoke-SchoolBotMsi -MsiArguments @('/fa',$schoolBotProductCode) -LogName 'repair.log'
    foreach ($schoolBotRestored in @(
        @{ Relative = 'demo-engine.exe'; Failure = 'MSI Repair did not restore the engine.' },
        @{ Relative = 'engines/zide/zide-engine.exe'; Failure = 'MSI Repair did not restore C++ Zide.' }
    )) {
        $schoolBotRepairedPath = Join-Path $schoolBotTestDir $schoolBotRestored.Relative
        $schoolBotPayloadPath = Join-Path $schoolBotPayload $schoolBotRestored.Relative
        $schoolBotRepairedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $schoolBotRepairedPath).Hash
        $schoolBotPayloadHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $schoolBotPayloadPath).Hash
        if ($schoolBotRepairedHash -ne $schoolBotPayloadHash) { throw $schoolBotRestored.Failure }
    }
    $schoolBotInstalledBootstrapper = Join-Path $schoolBotTestDir 'MicrosoftEdgeWebview2Setup.exe'
    $schoolBotOriginalBootstrapper = Join-Path $SchoolBotRoot `
        '.tools/installer-downloads/MicrosoftEdgeWebview2Setup.exe'
    $schoolBotInstalledBootstrapperHash = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $schoolBotInstalledBootstrapper
    ).Hash
    $schoolBotOriginalBootstrapperHash = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $schoolBotOriginalBootstrapper
    ).Hash
    if ($schoolBotInstalledBootstrapperHash -ne $schoolBotOriginalBootstrapperHash) {
        throw 'Bundled WebView2 bootstrapper differs.'
    }
    $schoolBotRequest = @{
        protocol = '1.0'
        request_id = 'installer-smoke'
        mode = 'robot-control'
        action = 'robot.stop'
        data = @{ transport = 'dry-run' }
    } | ConvertTo-Json -Compress
    $schoolBotResponse = $schoolBotRequest |
        & (Join-Path $schoolBotTestDir 'engines/zide/zide-engine.exe') | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0 -or $schoolBotResponse.status -ne 'success' -or
        $schoolBotResponse.request_id -ne 'installer-smoke') {
        throw 'Installed engine smoke test failed.'
    }
    [IO.File]::WriteAllText(
        (Join-Path $schoolBotTestDir 'keep-me.txt'),
        'Unrelated file: the uninstaller must preserve this.'
    )
} finally {
    $schoolBotTestIsInstalled = $schoolBotProductApi.ProductState($schoolBotProductCode) -eq 5
    if ($schoolBotTestIsInstalled) {
        $schoolBotInstalledLocation = $schoolBotProductApi.ProductInfo($schoolBotProductCode, 'InstallLocation')
        if ($schoolBotInstalledLocation.TrimEnd('\') -ne $schoolBotTestDir) {
            throw 'Refusing to uninstall a product outside the workspace test path.'
        }
    }
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($schoolBotProductApi)
    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
    if ($schoolBotTestIsInstalled) {
        Invoke-SchoolBotMsi -MsiArguments @('/x',$schoolBotProductCode) -LogName 'uninstall.log'
    }
}
foreach ($schoolBotRemoved in @(
    $schoolBotRegistration,$schoolBotMachineRegistration,$schoolBotSettings,$schoolBotMenu,$schoolBotShortcut,
    (Join-Path $schoolBotTestDir 'school-bot.exe'),(Join-Path $schoolBotTestDir 'demo-engine.exe'),
    (Join-Path $schoolBotTestDir 'MicrosoftEdgeWebview2Setup.exe'),
    (Join-Path $schoolBotTestDir 'THIRD-PARTY-NOTICES.md')
)) {
    if (Test-Path -LiteralPath $schoolBotRemoved) { throw "Uninstaller left installed state: $schoolBotRemoved" }
}
foreach ($schoolBotRelativeFile in $schoolBotPayloadFiles) {
    if (Test-Path -LiteralPath (Join-Path $schoolBotTestDir $schoolBotRelativeFile)) {
        throw "Uninstaller left payload: $schoolBotRelativeFile"
    }
}
if (-not (Test-Path -LiteralPath (Join-Path $schoolBotTestDir 'keep-me.txt'))) {
    throw 'Uninstaller removed an unrelated file.'
}
if ((@(Get-SchoolBotProjectSnapshot) -join "`n") -ne $schoolBotBefore) {
    throw 'Installer or uninstaller changed user projects/device ID.'
}
$schoolBotProductApi = New-Object -ComObject WindowsInstaller.Installer
try {
    if ($schoolBotProductApi.ProductState($schoolBotProductCode) -ne -1) {
        throw 'Windows Installer still reports the test product installed.'
    }
} finally { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($schoolBotProductApi) }
Remove-Item -LiteralPath (Join-Path $schoolBotTestDir 'keep-me.txt')
# Preserve unrelated files if the test directory is not empty.
Remove-Item -LiteralPath $schoolBotTestDir
Write-Host ('MSI smoke test passed: install, payload/shortcuts, Windows Installer registration, repair, ' +
    'installed engine, uninstall and project preservation. Logs: .tools/installer-smoke/')
