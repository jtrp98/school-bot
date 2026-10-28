. "$PSScriptRoot/common.ps1"
if ($env:OS -ne 'Windows_NT') { throw 'Run this script on Windows.' }
$schoolBotTools = Join-Path $SchoolBotRoot '.tools'
$schoolBotDownloads = Join-Path $schoolBotTools 'installer-downloads'
New-Item -ItemType Directory -Path $schoolBotDownloads -Force | Out-Null
$schoolBotWixDir = Join-Path $schoolBotTools 'wix5'
$schoolBotWix = Join-Path $schoolBotWixDir 'wix.exe'
if (-not (Test-Path -LiteralPath $schoolBotWix)) {
    Push-Location (Join-Path $SchoolBotRoot 'web')
    try {
        Invoke-SchoolBotCommand 'dotnet' @('tool','install','wix','--version','5.0.2','--allow-roll-forward','--tool-path',$schoolBotWixDir,'--add-source','https://api.nuget.org/v3/index.json')
    } finally { Pop-Location }
}
$schoolBotWixVersion = & $schoolBotWix --version
if ($LASTEXITCODE -ne 0 -or $schoolBotWixVersion -notlike '5.0.2+*') { throw 'Expected the pinned WiX 5.0.2 tool.' }
$schoolBotWixUI = Join-Path $schoolBotWixDir '.wix/extensions/WixToolset.UI.wixext/5.0.2/wixext5/WixToolset.UI.wixext.dll'
if (-not (Test-Path -LiteralPath $schoolBotWixUI)) {
    Push-Location $schoolBotWixDir
    try { Invoke-SchoolBotCommand $schoolBotWix @('extension','add','WixToolset.UI.wixext/5.0.2') } finally { Pop-Location }
}
$schoolBotBootstrapper = Join-Path $schoolBotDownloads 'MicrosoftEdgeWebview2Setup.exe'
if (-not (Test-Path -LiteralPath $schoolBotBootstrapper)) {
    Invoke-WebRequest -UseBasicParsing -Uri 'https://go.microsoft.com/fwlink/p/?LinkId=2124703' -OutFile $schoolBotBootstrapper -TimeoutSec 60
}
$schoolBotSignature = Get-AuthenticodeSignature -LiteralPath $schoolBotBootstrapper
if ($schoolBotSignature.Status -ne 'Valid' -or $schoolBotSignature.SignerCertificate.Subject -notmatch '(^|,\s*)O=Microsoft Corporation(,|$)') {
    throw 'WebView2 bootstrapper must have a valid Microsoft Authenticode signature.'
}
Write-Host 'MSI tools ready: WiX 5.0.2, WixUI and verified Microsoft WebView2 bootstrapper.'
