#requires -Version 7.4
param(
    [ValidateSet('Configure', 'Helper', 'Gateway')][string]$Target = 'Configure',
    [string]$HelperRoot = (Join-Path $PSScriptRoot '../../../sb-web-helper'),
    [string]$GatewayRoot = (Join-Path $PSScriptRoot '../../../sb-api-gateway'),
    [ValidateRange(1024, 65535)][int]$HelperPort = 3001,
    [ValidateRange(1024, 65534)][int]$GatewayPort = 5180
)
$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$directory = Join-Path $repository '.tmp/helper-auth'
$privatePath = Join-Path $directory 'helper-private.pem'
$publicPath = Join-Path $directory 'helper-public.pem'
foreach ($path in @((Join-Path $repository '.tmp'), $directory, $privatePath, $publicPath, (Join-Path $directory 'setup.lock'))) {
    if ((Test-Path -LiteralPath $path) -and ((Get-Item -LiteralPath $path -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw 'The local signing directory and key files must not be links.'
    }
}
[void][IO.Directory]::CreateDirectory($directory)
if ($IsWindows) {
    $sid = [Security.Principal.WindowsIdentity]::GetCurrent().User
    $acl = [Security.AccessControl.DirectorySecurity]::new()
    $acl.SetOwner($sid)
    $acl.SetAccessRuleProtection($true, $false)
    $acl.AddAccessRule([Security.AccessControl.FileSystemAccessRule]::new($sid, 'FullControl', 'ContainerInherit,ObjectInherit', 'None', 'Allow'))
    Set-Acl -LiteralPath $directory -AclObject $acl
} else { & chmod 700 $directory; if ($LASTEXITCODE) { throw 'Unable to protect the local signing directory.' } }
$setupLock = $null
for ($attempt = 0; $attempt -lt 50 -and $null -eq $setupLock; $attempt++) {
    try { $setupLock = [IO.File]::Open((Join-Path $directory 'setup.lock'), [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None) }
    catch [IO.IOException] { Start-Sleep -Milliseconds 100 }
}
if ($null -eq $setupLock) { throw 'Another launcher is preparing the local signing key.' }
try {
    & node (Join-Path $PSScriptRoot 'prepare-helper-auth-key.mjs') $directory
    if ($LASTEXITCODE) { throw 'Unable to prepare the local helper signing files.' }
} finally { $setupLock.Dispose() }
if (!$IsWindows) { & chmod 600 $privatePath; if ($LASTEXITCODE) { throw 'Unable to protect the local private key.' } }
if ($Target -eq 'Configure') { Write-Host "Local helper signing files prepared in $directory. No service was started."; return }
if ($HelperPort -eq $GatewayPort -or $HelperPort -eq ($GatewayPort + 1)) { throw 'Helper, Gateway and health ports must be distinct.' }
$variables = @{
    SCHOOL_BOT_GATEWAY_ORIGIN = "http://127.0.0.1:$GatewayPort"
    SCHOOL_BOT_STAFF_SIGNING_KEY_FILE = $privatePath
    SchoolBot__HelperPublicKeyFile = $publicPath
    ASPNETCORE_HTTP_PORTS = [string]$GatewayPort
    HEALTH_CHECK_PORT = [string]($GatewayPort + 1)
}
$previous = @{}
foreach ($name in $variables.Keys) { $previous[$name] = [Environment]::GetEnvironmentVariable($name); [Environment]::SetEnvironmentVariable($name, $variables[$name]) }
try {
    if ($Target -eq 'Helper') {
        Push-Location -LiteralPath ([IO.Path]::GetFullPath($HelperRoot))
        try { & bunx next dev --webpack --hostname 127.0.0.1 --port $HelperPort } finally { Pop-Location }
    } else {
        Push-Location -LiteralPath ([IO.Path]::GetFullPath($GatewayRoot))
        try { & dotnet run --project src/ApiGateway.API --no-launch-profile } finally { Pop-Location }
    }
    if ($LASTEXITCODE) { throw "$Target exited with code $LASTEXITCODE." }
} finally { foreach ($name in $previous.Keys) { [Environment]::SetEnvironmentVariable($name, $previous[$name]) } }
