. "$PSScriptRoot/../lib/common.ps1"
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Install the .NET 10 SDK first.' }
Push-Location (Join-Path $SchoolBotRoot 'web')
try { Invoke-SchoolBotCommand 'dotnet' @('run','--project','src/ServiceSchoolBot.API','--no-launch-profile') } finally { Pop-Location }
