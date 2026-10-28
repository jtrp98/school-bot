param([string]$InstallerPath)
. "$PSScriptRoot/../common.ps1"
if (-not $InstallerPath) {
    $version = (Get-Content -LiteralPath "$SchoolBotRoot/app/frontend/package.json" -Raw | ConvertFrom-Json).version
    $InstallerPath = Join-Path $SchoolBotRoot "dist/installers/school-bot-$version-windows-amd64.msi"
}
$InstallerPath = (Resolve-Path -LiteralPath $InstallerPath).Path
$installer = New-Object -ComObject WindowsInstaller.Installer
$database = $installer.OpenDatabase($InstallerPath, 0)
function Read-MsiRows([string]$Query, [string[]]$Columns) {
    $view = $database.OpenView($Query)
    try {
        [void]$view.Execute()
        while ($record = $view.Fetch()) {
            try {
                $row = @{}
                for ($index = 0; $index -lt $Columns.Count; $index++) {
                    $row[$Columns[$index]] = $record.StringData($index + 1)
                }
                [pscustomobject]$row
            } finally { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($record) }
        }
    } finally {
        [void]$view.Close()
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view)
    }
}
function Assert-Installer([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw $Message }
}
try {
    $upgrades = @(Read-MsiRows 'SELECT `Language` FROM `Upgrade`' @('Language'))
    Assert-Installer ($upgrades.Count -eq 2) 'Upgrade and downgrade detection must both be present.'
    foreach ($upgrade in $upgrades) {
        Assert-Installer ([string]::IsNullOrEmpty($upgrade.Language)) 'Changing installer language must not bypass upgrade or downgrade detection.'
    }
    $launchDefault = Read-MsiRows 'SELECT `Value` FROM `Property` WHERE `Property` = ''SCHOOLBOT_LAUNCH''' @('Value')
    Assert-Installer ($launchDefault.Value -eq '1') 'Launch checkbox must be checked by default.'
    $checkbox = Read-MsiRows 'SELECT `Type`, `Property` FROM `Control` WHERE `Dialog_` = ''SchoolBotComplete'' AND `Control` = ''Launch''' @('Type','Property')
    Assert-Installer ($checkbox.Type -eq 'CheckBox' -and $checkbox.Property -eq 'SCHOOLBOT_LAUNCH') 'Completion checkbox is not bound to the launch setting.'
    $launch = @(Read-MsiRows 'SELECT `Dialog_`, `Control_`, `Condition`, `Ordering` FROM `ControlEvent` WHERE `Event` = ''DoAction'' AND `Argument` = ''LaunchSchoolBot''' @('Dialog','Control','Condition','Ordering'))
    Assert-Installer ($launch.Count -eq 1) "There must be exactly one launch event; found $($launch.Count)."
    Assert-Installer ($launch[0].Dialog -eq 'SchoolBotComplete' -and $launch[0].Control -eq 'Finish') 'Launch must only occur from the completion button.'
    $finish = Read-MsiRows 'SELECT `Ordering` FROM `ControlEvent` WHERE `Dialog_` = ''SchoolBotComplete'' AND `Control_` = ''Finish'' AND `Event` = ''EndDialog''' @('Ordering')
    Assert-Installer ([int]$launch[0].Ordering -lt [int]$finish.Ordering) 'Launch must be evaluated before the installer UI closes.'
    $action = Read-MsiRows 'SELECT `Type`, `Source` FROM `CustomAction` WHERE `Action` = ''LaunchSchoolBot''' @('Type','Source')
    Assert-Installer (([int]$action.Type -band 63) -eq 18 -and $action.Source -eq 'DesktopExe') 'Launch must target the installed executable by file reference.'
    Assert-Installer (([int]$action.Type -band 2048) -eq 0) 'Launch must run as the current user.'
    foreach ($table in @('InstallExecuteSequence','InstallUISequence')) {
        $scheduled = @(Read-MsiRows "SELECT ``Action`` FROM ``$table`` WHERE ``Action`` = 'LaunchSchoolBot'" @('Action'))
        Assert-Installer ($scheduled.Count -eq 0) 'Launch must never run as a scheduled install or maintenance action.'
    }
    $success = Read-MsiRows 'SELECT `Sequence` FROM `InstallUISequence` WHERE `Action` = ''SchoolBotComplete''' @('Sequence')
    Assert-Installer ($success.Sequence -eq '-1') 'The completion dialog must only appear after success.'

    # A safe session can evaluate the real MSI conditions without changing the machine or running actions.
    $session = $installer.OpenPackage($InstallerPath, 1)
    try {
        $cases = @(
            @{ Name='normal installation'; Launch='1'; UI='5'; Installed=''; Remove=''; Expected=1 },
            @{ Name='unchecked'; Launch=''; UI='5'; Installed=''; Remove=''; Expected=0 },
            @{ Name='silent installation'; Launch='1'; UI='2'; Installed=''; Remove=''; Expected=0 },
            @{ Name='basic UI'; Launch='1'; UI='3'; Installed=''; Remove=''; Expected=0 },
            @{ Name='repair'; Launch='1'; UI='5'; Installed='1'; Remove=''; Expected=0 },
            @{ Name='uninstall'; Launch='1'; UI='5'; Installed='1'; Remove='ALL'; Expected=0 }
        )
        foreach ($case in $cases) {
            foreach ($property in @{
                SCHOOLBOT_LAUNCH=$case.Launch; UILevel=$case.UI; Installed=$case.Installed; REMOVE=$case.Remove
            }.GetEnumerator()) {
                $session.GetType().InvokeMember('Property', [Reflection.BindingFlags]::SetProperty, $null,
                    $session, @($property.Key, $property.Value)) | Out-Null
            }
            Assert-Installer ($session.EvaluateCondition($launch[0].Condition) -eq $case.Expected) "Incorrect launch behavior: $($case.Name)"
        }
    } finally { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($session) }

    foreach ($dialog in @('SchoolBotWelcome','SchoolBotComplete')) {
        $controls = Read-MsiRows "SELECT ``Control``, ``X``, ``Y``, ``Width``, ``Height`` FROM ``Control`` WHERE ``Dialog_`` = '$dialog'" @('Name','X','Y','Width','Height')
        foreach ($control in $controls) {
            Assert-Installer ([int]$control.X -ge 0 -and [int]$control.Y -ge 0 -and
                ([int]$control.X + [int]$control.Width) -le 370 -and
                ([int]$control.Y + [int]$control.Height) -le 270) "Control leaves the dialog: $dialog/$($control.Name)"
        }
    }
    Write-Host 'MSI package checks passed: cross-language upgrades, launch checkbox, six launch conditions, user context, success-only UI and dialog bounds. No installation or app launch was performed.'
} finally {
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($database)
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer)
}
