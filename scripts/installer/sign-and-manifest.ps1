param(
    [string]$PayloadDirectory,
    [string]$InstallerDirectory,
    [switch]$RequireSigned
)
. "$PSScriptRoot/../lib/common.ps1"
$ErrorActionPreference = 'Stop'

$payloadDir = if ($PayloadDirectory) {
    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PayloadDirectory)
} else {
    Join-Path $SchoolBotRoot 'app/bin'
}
$installersDir = if ($InstallerDirectory) {
    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($InstallerDirectory)
} else {
    Join-Path $SchoolBotRoot 'dist/installers'
}

$version = (Get-Content -LiteralPath (Join-Path $SchoolBotRoot 'app/frontend/package.json') -Raw | ConvertFrom-Json).version
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'Installer version must be x.y.z.' }

if ($RequireSigned) {
    if (-not $env:SCHOOL_BOT_BUILD_API_URL -or $env:SCHOOL_BOT_BUILD_API_URL -notmatch '^https://') {
        throw 'Production signing requires SCHOOL_BOT_BUILD_API_URL to be an HTTPS Gateway endpoint (SEC001-008).'
    }
    if (-not $env:SCHOOL_BOT_SIGN_THUMBPRINT) {
        throw 'Production signing requires SCHOOL_BOT_SIGN_THUMBPRINT for Authenticode code signing (BE-022 / SEC001-003).'
    }
}

# 1. Verify third-party notices inventory
Invoke-SchoolBotCommand 'node' @("$PSScriptRoot/../checks/third-party-notices.mjs")

# 2. Scan payload and installer output for forbidden files (private keys, debug symbols, .env)
$forbiddenExtensions = @('.pem', '.key', '.pfx', '.p12', '.pdb', '.map', '.env')
$forbiddenFound = @()
foreach ($scanDir in @($payloadDir, $installersDir)) {
    if (Test-Path -LiteralPath $scanDir) {
        $files = Get-ChildItem -LiteralPath $scanDir -Recurse -File
        foreach ($f in $files) {
            if ($forbiddenExtensions -contains $f.Extension.ToLowerInvariant()) {
                $forbiddenFound += $f.FullName
            }
        }
    }
}
if ($forbiddenFound.Count -gt 0) {
    throw "Payload cleanliness check failed; forbidden files found: $($forbiddenFound -join ', ')"
}

# 3. Verify required payload binaries and notices
$requiredPayload = @(
    'school-bot.exe',
    'demo-engine.exe',
    'engines/zide/zide-engine.exe',
    'engines/zide/engine.json',
    'engines/zide/THIRD-PARTY-NOTICES.md',
    'engines/zide/HIDAPI-LICENSE.txt',
    'engines/zide/JSON-LICENSE.txt',
    'engines/zide/zide-engine.exe.sha256',
    'engines/mixly/mixly-engine.exe',
    'engines/mixly/engine.json',
    'engines/mixly/THIRD-PARTY-NOTICES.md',
    'engines/mixly/mixly-engine.exe.sha256'
)
foreach ($rel in $requiredPayload) {
    $full = Join-Path $payloadDir $rel
    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "Required payload asset is missing: $full"
    }
}

$msiPath = Join-Path $installersDir "school-bot-$version-windows-amd64.msi"
$setupPath = Join-Path $installersDir "school-bot-$version-windows-amd64-setup.exe"
foreach ($pkg in @($msiPath, $setupPath)) {
    if (-not (Test-Path -LiteralPath $pkg -PathType Leaf)) {
        throw "Required installer artifact is missing: $pkg"
    }
}

# 4. Sign artifacts if SCHOOL_BOT_SIGN_THUMBPRINT is provided
if ($env:SCHOOL_BOT_SIGN_THUMBPRINT) {
    $signTool = (Get-Command 'signtool.exe' -ErrorAction SilentlyContinue).Source
    if (-not $signTool) { throw 'signtool.exe is required when SCHOOL_BOT_SIGN_THUMBPRINT is set.' }
    $timestampUrl = if ($env:SCHOOL_BOT_TIMESTAMP_URL) { $env:SCHOOL_BOT_TIMESTAMP_URL } else { 'http://timestamp.digicert.com' }
    foreach ($target in @($msiPath, $setupPath)) {
        Invoke-SchoolBotCommand $signTool @(
            'sign', '/fd', 'SHA256', '/td', 'SHA256', '/tr', $timestampUrl,
            '/sha1', $env:SCHOOL_BOT_SIGN_THUMBPRINT, $target
        )
        $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $target).Hash.ToLowerInvariant()
        [IO.File]::WriteAllText("$target.sha256", "$hash  $([IO.Path]::GetFileName($target))`n")
    }
}

# 5. Copy notices alongside MSI as well as Setup EXE
$setupNotices = "$setupPath.NOTICES.txt"
$msiNotices = "$msiPath.NOTICES.txt"
if (Test-Path -LiteralPath $setupNotices) {
    Copy-Item -LiteralPath $setupNotices -Destination $msiNotices -Force
} else {
    Copy-Item -LiteralPath (Join-Path $SchoolBotRoot 'THIRD-PARTY-NOTICES.md') -Destination $msiNotices -Force
}

# 6. Inspect signatures and hashes for release-manifest.json
function Get-ArtifactSignatureMetadata([string]$FilePath) {
    $sig = Get-AuthenticodeSignature -LiteralPath $FilePath
    $subject = if ($sig.SignerCertificate) { $sig.SignerCertificate.Subject } else { $null }
    $thumbprint = if ($sig.SignerCertificate) { $sig.SignerCertificate.Thumbprint } else { $null }
    if ($RequireSigned -and $sig.Status -ne 'Valid') {
        throw "Artifact signature is not Valid ($($sig.Status)): $FilePath"
    }
    return [ordered]@{
        status            = [string]$sig.Status
        signer_subject    = $subject
        signer_thumbprint = $thumbprint
    }
}

$bootstrapperPath = Join-Path $SchoolBotRoot '.tools/installer-downloads/MicrosoftEdgeWebview2Setup.exe'
$bootstrapperSig = Get-ArtifactSignatureMetadata $bootstrapperPath
if ($bootstrapperSig.status -ne 'Valid' -or $bootstrapperSig.signer_subject -notmatch '(^|,\s*)O=Microsoft Corporation(,|$)') {
    throw 'WebView2 bootstrapper must carry a Valid Microsoft Corporation Authenticode signature.'
}

$demoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $payloadDir 'demo-engine.exe')).Hash.ToLowerInvariant()
$zideHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $payloadDir 'engines/zide/zide-engine.exe')).Hash.ToLowerInvariant()
$mixlyHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $payloadDir 'engines/mixly/mixly-engine.exe')).Hash.ToLowerInvariant()
$desktopHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $payloadDir 'school-bot.exe')).Hash.ToLowerInvariant()
$msiHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $msiPath).Hash.ToLowerInvariant()
$setupHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $setupPath).Hash.ToLowerInvariant()
$bootstrapperHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $bootstrapperPath).Hash.ToLowerInvariant()

$manifest = [ordered]@{
    schema_version = 1
    publisher      = 'SchoolBright'
    version        = $version
    channel        = 'stable'
    generated_at   = [DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ')
    target_os      = [ordered]@{
        windows_x64     = 'Windows 11 25H2 x64 (min_os 10.0.26200)'
        macos_universal = 'macOS 15.0+ Universal (Intel x86_64 + Apple Silicon arm64)'
    }
    desktop_host   = [ordered]@{
        executable = 'school-bot.exe'
        sha256     = $desktopHash
        size_bytes = (Get-Item -LiteralPath (Join-Path $payloadDir 'school-bot.exe')).Length
    }
    engine_hashes  = @(
        [ordered]@{ id = 'demo'; executable = 'demo-engine.exe'; sha256 = $demoHash },
        [ordered]@{ id = 'zide'; executable = 'engines/zide/zide-engine.exe'; sha256 = $zideHash },
        [ordered]@{ id = 'mixly'; executable = 'engines/mixly/mixly-engine.exe'; sha256 = $mixlyHash }
    )
    webview2_bootstrapper = [ordered]@{
        file       = 'MicrosoftEdgeWebview2Setup.exe'
        sha256     = $bootstrapperHash
        size_bytes = (Get-Item -LiteralPath $bootstrapperPath).Length
        signature  = $bootstrapperSig
    }
    artifacts      = @(
        [ordered]@{
            package_kind = 'windows-msi'
            platform     = 'windows-amd64'
            min_os       = '10.0.26200'
            file         = [IO.Path]::GetFileName($msiPath)
            sha256       = $msiHash
            size_bytes   = (Get-Item -LiteralPath $msiPath).Length
            notices_file = [IO.Path]::GetFileName($msiNotices)
            signature    = (Get-ArtifactSignatureMetadata $msiPath)
        },
        [ordered]@{
            package_kind = 'windows-setup'
            platform     = 'windows-amd64'
            min_os       = '10.0.26200'
            file         = [IO.Path]::GetFileName($setupPath)
            sha256       = $setupHash
            size_bytes   = (Get-Item -LiteralPath $setupPath).Length
            notices_file = [IO.Path]::GetFileName($setupNotices)
            signature    = (Get-ArtifactSignatureMetadata $setupPath)
        }
    )
    payload_cleanliness = [ordered]@{
        private_keys_found  = 0
        debug_symbols_found = 0
        notices_verified    = $true
    }
}

$manifestOut = Join-Path $installersDir 'release-manifest.json'
[IO.File]::WriteAllText($manifestOut, ($manifest | ConvertTo-Json -Depth 8) + "`n", [Text.UTF8Encoding]::new($false))
Write-Host "Release manifest and asset verification ready: $manifestOut"
