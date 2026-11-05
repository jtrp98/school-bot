param(
    [string]$PayloadDirectory = "dist/payload-0.1.5-rc",
    [string]$ReportPath = "reports/phase6/msi-lifecycle-and-rollback-report.json"
)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../lib/common.ps1"

if ($env:OS -ne 'Windows_NT') {
    throw 'test-msi-rollback-recovery.ps1 requires Windows (msiexec.exe).'
}

$resolvedPayload = if ([IO.Path]::IsPathRooted($PayloadDirectory)) {
    $PayloadDirectory
} else {
    Join-Path $SchoolBotRoot $PayloadDirectory
}
if (-not (Test-Path -LiteralPath $resolvedPayload)) {
    $resolvedPayload = Join-Path $SchoolBotRoot 'app/bin'
}

$manifestPath = Join-Path $SchoolBotRoot 'app/frontend/package.json'
$currentVersion = (Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json).version
$prodMsiCurrent = Join-Path $SchoolBotRoot "dist/installers/school-bot-$currentVersion-windows-amd64.msi"
if (-not (Test-Path -LiteralPath $prodMsiCurrent)) {
    throw "Production MSI not found: $prodMsiCurrent"
}

# Verify production MSI static & session condition tables first
& "$PSScriptRoot/../tests/installer-package.test.ps1" -InstallerPath $prodMsiCurrent

$smokeRoot = Join-Path $SchoolBotRoot '.tools/installer-smoke'
$testInstallDir = Join-Path $smokeRoot 'School Bot'
New-Item -ItemType Directory -Path $smokeRoot -Force | Out-Null

# Clean up any leftover smoke MSI state from an interrupted run
foreach ($orphanKey in @(
    'Registry::HKEY_CURRENT_USER\Software\Microsoft\Installer\Products\B31D55DDA2C71F04A857127D62ADF31B',
    'Registry::HKEY_CURRENT_USER\Software\Microsoft\Installer\Features\B31D55DDA2C71F04A857127D62ADF31B',
    'Registry::HKEY_CURRENT_USER\Software\SchoolBotSmoke'
)) {
    if (Test-Path -LiteralPath $orphanKey) {
        Remove-Item -LiteralPath $orphanKey -Recurse -Force -ErrorAction SilentlyContinue
    }
}
if (Test-Path -LiteralPath $testInstallDir) {
    Remove-Item -LiteralPath $testInstallDir -Recurse -Force -ErrorAction SilentlyContinue
}

# Create an isolated smoke copy of Package.wxs with run-unique UpgradeCode and Component GUIDs
# so real msiexec.exe transactions never collide with %LOCALAPPDATA%\Programs\School Bot or prior interrupted runs.
$wxsOriginal = Get-Content -LiteralPath (Join-Path $SchoolBotRoot 'app/build/windows/Package.wxs') -Raw
$smokeWxsContent = $wxsOriginal `
    -replace '61D02E9C-4C33-4DA9-9510-549CA8CDD6AA', ([guid]::NewGuid().ToString().ToUpperInvariant()) `
    -replace '66E5B47A-67A0-4C33-A115-F3D104C81261', ([guid]::NewGuid().ToString().ToUpperInvariant()) `
    -replace '66E5B47A-67A0-4C33-A115-F3D104C81262', ([guid]::NewGuid().ToString().ToUpperInvariant()) `
    -replace '66E5B47A-67A0-4C33-A115-F3D104C81263', ([guid]::NewGuid().ToString().ToUpperInvariant()) `
    -replace '66E5B47A-67A0-4C33-A115-F3D104C81264', ([guid]::NewGuid().ToString().ToUpperInvariant()) `
    -replace '66E5B47A-67A0-4C33-A115-F3D104C81265', ([guid]::NewGuid().ToString().ToUpperInvariant()) `
    -replace '66E5B47A-67A0-4C33-A115-F3D104C81266', ([guid]::NewGuid().ToString().ToUpperInvariant()) `
    -replace '3F1DBC57-D0DA-4A26-9634-18AB874190A1', ([guid]::NewGuid().ToString().ToUpperInvariant()) `
    -replace '21FBC6F0-B7BE-49A9-A490-45530908339D', ([guid]::NewGuid().ToString().ToUpperInvariant()) `
    -replace 'B6C06624-21B1-4230-B845-E0E46D307014', ([guid]::NewGuid().ToString().ToUpperInvariant()) `
    -replace '0840C947-74B3-4FAD-84D7-37E975EE781A', ([guid]::NewGuid().ToString().ToUpperInvariant()) `
    -replace 'Component Id="MenuShortcut" Guid="\*"', "Component Id=`"MenuShortcut`" Guid=`"$([guid]::NewGuid().ToString().ToUpperInvariant())`"" `
    -replace 'Component Id="DesktopShortcut" Guid="\*"', "Component Id=`"DesktopShortcut`" Guid=`"$([guid]::NewGuid().ToString().ToUpperInvariant())`"" `
    -replace 'Software\\SchoolBot', 'Software\SchoolBotSmoke' `
    -replace 'Name="School Bot"', 'Name="School Bot Smoke"'
$smokeWxsPath = Join-Path $smokeRoot 'Package.smoke.wxs'
[IO.File]::WriteAllText($smokeWxsPath, $smokeWxsContent, [Text.UTF8Encoding]::new($false))

$brandDir = Join-Path $SchoolBotRoot '.tools/installer-brand/th'
function Build-SmokeMsi([string]$Version, [string]$OutPath) {
    Invoke-SchoolBotCommand (Join-Path $SchoolBotRoot '.tools/wix5/wix.exe') @(
        'build', '-arch', 'x64', '-pdbtype', 'none',
        '-ext', (Join-Path $SchoolBotRoot '.tools/wix5/.wix/extensions/WixToolset.UI.wixext/5.0.2/wixext5/WixToolset.UI.wixext.dll'),
        '-culture', 'th-TH',
        '-loc', (Join-Path $brandDir 'installer.wxl'),
        '-d', "BrandDir=$brandDir",
        '-d', "AppVersion=$Version",
        '-d', "PayloadDir=$resolvedPayload",
        '-d', "Bootstrapper=$(Join-Path $SchoolBotRoot '.tools/installer-downloads/MicrosoftEdgeWebview2Setup.exe')",
        '-d', "Notices=$(Join-Path $SchoolBotRoot 'THIRD-PARTY-NOTICES.md')",
        '-d', "AppIcon=$(Join-Path $SchoolBotRoot 'app/build/windows/appicon.ico')",
        '-out', $OutPath,
        $smokeWxsPath,
        (Join-Path $SchoolBotRoot 'app/build/windows/InstallerUI.wxs'),
        (Join-Path $SchoolBotRoot 'app/build/windows/InstallerDialogs.wxs')
    )
}

function Get-MsiProductCode([string]$MsiPath) {
    $resolvedMsi = (Resolve-Path -LiteralPath $MsiPath).Path
    $installer = New-Object -ComObject WindowsInstaller.Installer
    $db = $installer.GetType().InvokeMember('OpenDatabase', [Reflection.BindingFlags]::InvokeMethod, $null, $installer, @($resolvedMsi, 0))
    $view = $db.GetType().InvokeMember('OpenView', [Reflection.BindingFlags]::InvokeMethod, $null, $db, @('SELECT `Value` FROM `Property` WHERE `Property` = ''ProductCode'''))
    [void]$view.GetType().InvokeMember('Execute', [Reflection.BindingFlags]::InvokeMethod, $null, $view, $null)
    $rec = $view.GetType().InvokeMember('Fetch', [Reflection.BindingFlags]::InvokeMethod, $null, $view, $null)
    $code = $rec.GetType().InvokeMember('StringData', [Reflection.BindingFlags]::GetProperty, $null, $rec, @(1))
    [void]$view.GetType().InvokeMember('Close', [Reflection.BindingFlags]::InvokeMethod, $null, $view, $null)
    foreach ($comObj in @($rec, $view, $db, $installer)) {
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($comObj)
    }
    return [string]$code
}

function Get-MsiProductState([string]$ProductCode) {
    $installer = New-Object -ComObject WindowsInstaller.Installer
    try {
        return [int]$installer.GetType().InvokeMember('ProductState', [Reflection.BindingFlags]::GetProperty, $null, $installer, @($ProductCode))
    } finally {
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer)
    }
}

function Get-MsiProductInfo([string]$ProductCode, [string]$Attribute) {
    $installer = New-Object -ComObject WindowsInstaller.Installer
    try {
        return [string]$installer.GetType().InvokeMember('ProductInfo', [Reflection.BindingFlags]::GetProperty, $null, $installer, @($ProductCode, $Attribute))
    } finally {
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer)
    }
}

$msiPrev = Join-Path $smokeRoot 'school-bot-0.1.4-smoke.msi'
$msiCurr = Join-Path $smokeRoot "school-bot-$currentVersion-smoke.msi"
$msiFault = Join-Path $smokeRoot 'school-bot-0.1.6-fault-smoke.msi'

Build-SmokeMsi -Version '0.1.4' -OutPath $msiPrev
Build-SmokeMsi -Version $currentVersion -OutPath $msiCurr
Build-SmokeMsi -Version '0.1.6' -OutPath $msiFault

$codePrev = Get-MsiProductCode -MsiPath $msiPrev
$codeCurr = Get-MsiProductCode -MsiPath $msiCurr
$codeFault = Get-MsiProductCode -MsiPath $msiFault

function Invoke-MsiExec {
    param(
        [string[]]$Arguments,
        [string]$LogName,
        [int[]]$AllowedExitCodes = @(0, 3010)
    )
    $logPath = Join-Path $smokeRoot $LogName
    if (Test-Path -LiteralPath $logPath) {
        Remove-Item -LiteralPath $logPath -Force
    }
    $fullArgs = $Arguments + @('/qn', '/norestart', '/L*v', "`"$logPath`"")
    $proc = Start-Process -FilePath 'msiexec.exe' -ArgumentList $fullArgs -Wait -PassThru -WindowStyle Hidden
    if ($proc.ExitCode -notin $AllowedExitCodes) {
        throw "msiexec ($LogName) exited with $($proc.ExitCode) (expected $($AllowedExitCodes -join ',')). Log: $logPath"
    }
    return [ordered]@{
        exit_code = $proc.ExitCode
        log_path  = $logPath
    }
}

$appDataConfig = Join-Path ([Environment]::GetFolderPath('ApplicationData')) 'SchoolBotPoC'
function Get-AppDataSnapshot {
    if (Test-Path -LiteralPath $appDataConfig) {
        Get-ChildItem -LiteralPath $appDataConfig -File | Sort-Object Name | ForEach-Object {
            $h = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            "$($_.Name):$h"
        }
    }
}

$appDataBefore = @(Get-AppDataSnapshot) -join "`n"
$payloadFiles = @(
    'school-bot.exe',
    'demo-engine.exe',
    'engines/zide/zide-engine.exe',
    'engines/zide/engine.json',
    'engines/zide/THIRD-PARTY-NOTICES.md',
    'engines/zide/HIDAPI-LICENSE.txt',
    'engines/zide/JSON-LICENSE.txt',
    'engines/zide/zide-engine.exe.sha256'
)

$expectedHashes = [ordered]@{}
foreach ($rel in $payloadFiles) {
    $p = Join-Path $resolvedPayload $rel
    $expectedHashes[$rel] = (Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.ToLowerInvariant()
}

$steps = @()

try {
    # 1. Clean install of 0.1.4
    $resInstallPrev = Invoke-MsiExec -Arguments @('/i', "`"$msiPrev`"", "INSTALLFOLDER=`"$testInstallDir`"") -LogName '01-install-0.1.4.log'
    $prevState = Get-MsiProductState -ProductCode $codePrev
    $prevVer = Get-MsiProductInfo -ProductCode $codePrev -Attribute 'VersionString'
    if ($prevState -ne 5 -or $prevVer -ne '0.1.4') {
        throw "Expected 0.1.4 installed (state=5), got state=$prevState version=$prevVer"
    }
    $steps += [ordered]@{
        step        = 'install-baseline-0.1.4'
        requirement = 'AC-031-01'
        exit_code   = $resInstallPrev.exit_code
        version     = $prevVer
        status      = 'PASS'
    }

    # 2. In-place MajorUpgrade 0.1.4 -> 0.1.5
    $resInstallCurr = Invoke-MsiExec -Arguments @('/i', "`"$msiCurr`"", "INSTALLFOLDER=`"$testInstallDir`"") -LogName '02-install-upgrade-0.1.5.log'
    $currState = Get-MsiProductState -ProductCode $codeCurr
    $currVer = Get-MsiProductInfo -ProductCode $codeCurr -Attribute 'VersionString'
    $prevStateAfterUpgrade = Get-MsiProductState -ProductCode $codePrev
    if ($currState -ne 5 -or $currVer -ne $currentVersion -or $prevStateAfterUpgrade -ne -1) {
        throw "Upgrade to $currentVersion failed: currState=$currState currVer=$currVer prevState=$prevStateAfterUpgrade"
    }
    foreach ($rel in $payloadFiles) {
        $installedPath = Join-Path $testInstallDir $rel
        $actualHash = (Get-FileHash -LiteralPath $installedPath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actualHash -ne $expectedHashes[$rel]) {
            throw "Payload hash mismatch after 0.1.5 install for ${rel}: expected $($expectedHashes[$rel]), got $actualHash"
        }
    }
    $steps += [ordered]@{
        step        = 'upgrade-0.1.4-to-0.1.5'
        requirement = 'AC-031-04'
        exit_code   = $resInstallCurr.exit_code
        version     = $currVer
        status      = 'PASS'
    }

    # 3. Downgrade attempt 0.1.5 -> 0.1.4 blocked
    $resDowngrade = Invoke-MsiExec -Arguments @('/i', "`"$msiPrev`"", "INSTALLFOLDER=`"$testInstallDir`"") -LogName '03-downgrade-blocked-0.1.4.log' -AllowedExitCodes @(1603, 1638)
    $stateAfterDowngrade = Get-MsiProductState -ProductCode $codeCurr
    $verAfterDowngrade = Get-MsiProductInfo -ProductCode $codeCurr -Attribute 'VersionString'
    if ($stateAfterDowngrade -ne 5 -or $verAfterDowngrade -ne $currentVersion) {
        throw "Downgrade was not cleanly blocked; state=$stateAfterDowngrade ver=$verAfterDowngrade"
    }
    $steps += [ordered]@{
        step        = 'downgrade-blocked-0.1.5-to-0.1.4'
        requirement = 'AC-031-04'
        exit_code   = $resDowngrade.exit_code
        active_ver  = $verAfterDowngrade
        status      = 'PASS'
    }

    # 4. Delete both engines and run MSI Repair (/fa)
    Remove-Item -LiteralPath (Join-Path $testInstallDir 'demo-engine.exe') -Force
    Remove-Item -LiteralPath (Join-Path $testInstallDir 'engines/zide/zide-engine.exe') -Force
    $resRepair = Invoke-MsiExec -Arguments @('/fa', $codeCurr) -LogName '04-repair-0.1.5.log'
    foreach ($rel in @('demo-engine.exe', 'engines/zide/zide-engine.exe')) {
        $repairedPath = Join-Path $testInstallDir $rel
        $repairedHash = (Get-FileHash -LiteralPath $repairedPath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($repairedHash -ne $expectedHashes[$rel]) {
            throw "MSI repair (/fa) failed to restore $rel"
        }
    }
    $steps += [ordered]@{
        step        = 'repair-restores-engines'
        requirement = 'AC-031-03'
        exit_code   = $resRepair.exit_code
        status      = 'PASS'
    }

    # 5. AC-030-07: Mid-update failure injection during MajorUpgrade 0.1.5 -> 0.1.6 (SCHOOLBOT_INJECT_FAIL=1)
    $resFaultUpgrade = Invoke-MsiExec `
        -Arguments @('/i', "`"$msiFault`"", "INSTALLFOLDER=`"$testInstallDir`"", 'SCHOOLBOT_INJECT_FAIL=1') `
        -LogName '05-mid-update-fail-rollback-0.1.6.log' `
        -AllowedExitCodes @(1603)

    $faultLogText = Get-Content -LiteralPath $resFaultUpgrade.log_path -Raw
    $customActionTriggered = $faultLogText -match 'InjectMidUpdateFailure'
    $currStateAfterRollback = Get-MsiProductState -ProductCode $codeCurr
    $currVerAfterRollback = Get-MsiProductInfo -ProductCode $codeCurr -Attribute 'VersionString'
    $faultStateAfterRollback = Get-MsiProductState -ProductCode $codeFault
    if (-not $customActionTriggered) {
        throw "InjectMidUpdateFailure custom action was not observed in 05-mid-update-fail-rollback-0.1.6.log"
    }
    if ($currStateAfterRollback -ne 5 -or $currVerAfterRollback -ne $currentVersion -or $faultStateAfterRollback -ne -1) {
        throw "MSI transactional rollback failed to restore 0.1.5: currState=$currStateAfterRollback currVer=$currVerAfterRollback faultState=$faultStateAfterRollback"
    }
    foreach ($rel in $payloadFiles) {
        $rolledBackPath = Join-Path $testInstallDir $rel
        if (-not (Test-Path -LiteralPath $rolledBackPath)) {
            throw "File missing after MSI rollback: $rel"
        }
        $rolledBackHash = (Get-FileHash -LiteralPath $rolledBackPath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($rolledBackHash -ne $expectedHashes[$rel]) {
            throw "File hash corrupted after MSI rollback for ${rel}: expected $($expectedHashes[$rel]), got $rolledBackHash"
        }
    }

    # Execute both restored engines after rollback to verify functional runnable state
    $zideReq = @{
        protocol   = '1.0'
        request_id = 'rollback-zide-verify'
        mode       = 'robot-control'
        action     = 'robot.stop'
        data       = @{ transport = 'dry-run' }
    } | ConvertTo-Json -Compress
    $zideResp = $zideReq | & (Join-Path $testInstallDir 'engines/zide/zide-engine.exe') | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0 -or $zideResp.status -ne 'success' -or $zideResp.request_id -ne 'rollback-zide-verify') {
        throw "Restored zide-engine.exe failed execution check after MSI rollback."
    }

    $demoReq = @{
        protocol   = '1.0'
        request_id = 'rollback-demo-verify'
        mode       = 'coding-adventure'
        action     = 'program.run'
        data       = @{ source = 'hero.move_right(2)' }
    } | ConvertTo-Json -Compress
    $demoResp = $demoReq | & (Join-Path $testInstallDir 'demo-engine.exe') | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0 -or $demoResp.status -ne 'success' -or $demoResp.request_id -ne 'rollback-demo-verify') {
        throw "Restored demo-engine.exe failed execution check after MSI rollback."
    }

    $steps += [ordered]@{
        step                    = 'mid-update-failure-rollback-0.1.5-to-0.1.6'
        requirement             = 'AC-030-07'
        exit_code               = $resFaultUpgrade.exit_code
        custom_action_triggered = $customActionTriggered
        restored_version        = $currVerAfterRollback
        fault_product_state     = $faultStateAfterRollback
        zide_engine_status      = $zideResp.status
        demo_engine_status      = $demoResp.status
        status                  = 'PASS'
    }

    [IO.File]::WriteAllText(
        (Join-Path $testInstallDir 'keep-me.txt'),
        'Unrelated user file preserved across uninstall.'
    )
} finally {
    foreach ($code in @($codeCurr, $codePrev, $codeFault)) {
        if ($code -and (Get-MsiProductState -ProductCode $code) -eq 5) {
            [void](Invoke-MsiExec -Arguments @('/x', $code) -LogName "06-uninstall-$($code.Trim('{}')).log")
        }
    }
    foreach ($tempFile in @($smokeWxsPath, $msiPrev, $msiCurr, $msiFault)) {
        if (Test-Path -LiteralPath $tempFile) {
            Remove-Item -LiteralPath $tempFile -Force
        }
    }
}

foreach ($rel in $payloadFiles) {
    if (Test-Path -LiteralPath (Join-Path $testInstallDir $rel)) {
        throw "Uninstall left payload file behind: $rel"
    }
}
if (Test-Path -LiteralPath 'HKCU:\Software\SchoolBotSmoke') {
    throw "Uninstall left HKCU:\Software\SchoolBotSmoke behind"
}
if (-not (Test-Path -LiteralPath (Join-Path $testInstallDir 'keep-me.txt'))) {
    throw "Uninstall removed user file keep-me.txt"
}
$appDataAfter = @(Get-AppDataSnapshot) -join "`n"
if ($appDataAfter -ne $appDataBefore) {
    throw "Installer/uninstaller altered %APPDATA%\SchoolBotPoC snapshot."
}
Remove-Item -LiteralPath (Join-Path $testInstallDir 'keep-me.txt') -Force
Remove-Item -LiteralPath $testInstallDir -Force

$steps += [ordered]@{
    step                 = 'clean-uninstall-and-appdata-preservation'
    requirement          = 'AC-031-03'
    user_file_preserved  = $true
    appdata_unchanged    = $true
    status               = 'PASS'
}

$report = [ordered]@{
    schema_version    = 1
    verified_at       = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
    os                = [Runtime.InteropServices.RuntimeInformation]::OSDescription
    current_version   = $currentVersion
    msi_path          = "dist/installers/school-bot-$currentVersion-windows-amd64.msi"
    msi_sha256        = (Get-FileHash -LiteralPath $prodMsiCurrent -Algorithm SHA256).Hash.ToLowerInvariant()
    verified_hashes   = $expectedHashes
    steps             = $steps
    overall_status    = 'PASS'
}

$resolvedReport = if ([IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath
} else {
    Join-Path $SchoolBotRoot $ReportPath
}
New-Item -ItemType Directory -Path (Split-Path -Parent $resolvedReport) -Force | Out-Null
[IO.File]::WriteAllText($resolvedReport, ($report | ConvertTo-Json -Depth 6), [Text.UTF8Encoding]::new($false))
Write-Host "MSI lifecycle, repair, downgrade-block, and AC-030-07 mid-update rollback verification PASSED: $resolvedReport"
