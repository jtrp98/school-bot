param(
    [Parameter(Mandatory = $true)][string]$Engine,
    [string]$OutputDirectory,
    [switch]$Test
)
$ErrorActionPreference = 'Stop'
if ($Engine -cnotmatch '^[a-z][a-z0-9-]*$') { throw 'Engine must be a lowercase engine ID, e.g. zide.' }
$engineDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) "engines/$Engine"
$engineManifestPath = Join-Path $engineDirectory 'engine.json'
if (-not (Test-Path -LiteralPath $engineManifestPath -PathType Leaf)) { throw "Unknown engine: $Engine" }
$engineManifest = Get-Content -LiteralPath $engineManifestPath -Raw | ConvertFrom-Json
if ($engineManifest.id -cne $Engine) { throw "Engine manifest ID does not match directory: $Engine" }
$engineBuildScript = Join-Path $engineDirectory 'scripts/build.ps1'
if (-not (Test-Path -LiteralPath $engineBuildScript -PathType Leaf)) { throw "Engine $Engine does not provide a PowerShell build script." }
$engineArguments = @{ Test = $Test }
if ($OutputDirectory) { $engineArguments.OutputDirectory = $OutputDirectory }
& $engineBuildScript @engineArguments
