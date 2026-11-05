$ErrorActionPreference = 'Stop'
$script = Join-Path $PSScriptRoot '../api/scaffold-databases.ps1'
$tempFile = Join-Path ([IO.Path]::GetTempPath()) ("school-bot-scaffold-" + [guid]::NewGuid() + '.env')
$original = @{}
foreach ($key in @('MASTER_DB_CONNECTION_STRING', 'ConnectionStrings__Scaffold', 'PGOPTIONS')) {
    $original[$key] = [Environment]::GetEnvironmentVariable($key)
}
$global:schoolBotScaffoldInvocations = 0
$global:schoolBotScaffoldFail = $false
function dotnet {
    $global:schoolBotScaffoldInvocations++
    if (($args -join ' ') -notmatch 'Name=ConnectionStrings:Scaffold') { throw 'Missing named connection.' }
    if (($args -join ' ') -match 'synthetic_only') { throw 'Connection leaked to arguments.' }
    if (($args -join ' ') -notmatch '--no-onconfiguring') { throw 'Missing no-onconfiguring.' }
    $builder = [System.Data.Common.DbConnectionStringBuilder]::new()
    $builder.set_ConnectionString($env:ConnectionStrings__Scaffold)
    if ($builder['Options'] -notmatch 'default_transaction_read_only=on') { throw 'Read-only option missing.' }
    if ($env:PGOPTIONS -ne '-c default_transaction_read_only=on') { throw 'Read-only environment missing.' }
    $global:LASTEXITCODE = if ($global:schoolBotScaffoldFail) { 1 } else { 0 }
    if ($global:schoolBotScaffoldFail) { Write-Output 'synthetic_only' }
}
function Assert-Rejected([scriptblock]$action, [string]$expected) {
    $rejected = $false
    try { & $action | Out-Null }
    catch {
        if ($_.Exception.Message -notlike "*$expected*") { throw 'Unexpected rejection.' }
        if ($_.Exception.Message -match 'synthetic_only') { throw 'Secret leaked in error.' }
        $rejected = $true
    }
    if (-not $rejected) { throw 'Expected rejection did not occur.' }
}
try {
    $env:MASTER_DB_CONNECTION_STRING = $null
    Set-Content -LiteralPath $tempFile -Value 'MASTER_DB_CONNECTION_STRING="Host=localhost;Database=self_check;Password=synthetic_only"'
    & $script -Database Master -EnvFile $tempFile -Force | Out-Null
    if ($global:schoolBotScaffoldInvocations -ne 1) { throw 'Scaffold was not called.' }
    if ([string]$env:ConnectionStrings__Scaffold -ne [string]$original['ConnectionStrings__Scaffold']) {
        throw 'Temporary connection environment was not restored.'
    }
    $global:schoolBotScaffoldFail = $true
    Assert-Rejected { & $script -Database Master -EnvFile $tempFile -Force } 'Scaffold failed.'
    Assert-Rejected { & $script -Database Master -EnvFile $tempFile } 'Generated files already exist.'
    Add-Content -LiteralPath $tempFile -Value 'MASTER_DB_CONNECTION_STRING=duplicate'
    Assert-Rejected { & $script -Database Master -EnvFile $tempFile -Force } 'Duplicate setting'
    $env:MASTER_DB_CONNECTION_STRING = 'not-a-connection'
    Assert-Rejected { & $script -Database Master -EnvFile $tempFile -Force } 'Invalid database connection'
    Write-Output 'PASS: scaffold arguments, read-only options, env restoration, errors and overwrite guard (mock CLI).'
}
finally {
    foreach ($key in $original.Keys) { [Environment]::SetEnvironmentVariable($key, $original[$key]) }
    Remove-Item -LiteralPath $tempFile -Force -ErrorAction SilentlyContinue
    Remove-Item Function:dotnet
}
