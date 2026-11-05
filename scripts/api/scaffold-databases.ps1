[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('SchoolBot', 'Master', 'School', 'Logs', 'PGGrade')]
    [string]$Database,
    [string]$EnvFile = (Join-Path $PSScriptRoot '../../backend/.env'),
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$serviceRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../backend'))
$infrastructure = Join-Path $serviceRoot 'src/ServiceSchoolBot.Infrastructure'
$databases = @{
    SchoolBot = @('SCHOOL_BOT_DB_CONNECTION_STRING', 'SchoolBotDb', 'SchoolBotDbContext')
    Master = @('MASTER_DB_CONNECTION_STRING', 'JabjaiMasterSingleDB', 'JabjaiMasterSingleDBPostgreSQLDBContext')
    School = @('SCHOOL_DB_CONNECTION_STRING', 'JabjaiSchoolSingleDB', 'JabjaiSchoolSingleDBPostgreSQLDBContext')
    Logs = @('LOGS_DB_CONNECTION_STRING', 'JabjaiSchoolLogs', 'JabjaiSchoolLogsPostgreSQLDBContext')
    PGGrade = @('PGGRADE_DB_CONNECTION_STRING', 'PGGradeDB', 'PGGradeDBPostgreSQLDBContext')
}
$setting, $entityDirectory, $context = $databases[$Database]
$connection = [Environment]::GetEnvironmentVariable($setting)
if ([string]::IsNullOrWhiteSpace($connection) -and (Test-Path -LiteralPath $EnvFile -PathType Leaf)) {
    $matchesInFile = @(Get-Content -LiteralPath $EnvFile | Where-Object { $_ -match "^\s*$setting\s*=" })
    if ($matchesInFile.Count -gt 1) { throw "Duplicate setting: $setting" }
    if ($matchesInFile.Count -eq 1) {
        $connection = ($matchesInFile[0] -split '=', 2)[1].Trim()
        if ($connection.Length -ge 2 -and
            (($connection.StartsWith('"') -and $connection.EndsWith('"')) -or
             ($connection.StartsWith("'") -and $connection.EndsWith("'")))) {
            $connection = $connection.Substring(1, $connection.Length - 2)
        }
    }
}
if ([string]::IsNullOrWhiteSpace($connection)) { throw "Missing setting: $setting" }
try {
    $builder = [System.Data.Common.DbConnectionStringBuilder]::new()
    $builder.set_ConnectionString($connection)
    $options = if ($builder.ContainsKey('Options')) { [string]$builder['Options'] } else { '' }
    $builder['Options'] = "$options -c default_transaction_read_only=on".Trim()
    $builder['Include Error Detail'] = 'false'
    $builder['Log Parameters'] = 'false'
    $connection = $builder.get_ConnectionString()
}
catch { throw 'Invalid database connection configuration.' }

$entityPath = Join-Path $infrastructure "Entities/$entityDirectory"
$contextPath = Join-Path $infrastructure "Common/Databases/$context.cs"
foreach ($target in @($entityPath, $contextPath)) {
    if (-not $Force -and (Test-Path -LiteralPath $target)) {
        throw 'Generated files already exist. Review changes before using -Force.'
    }
    $ancestor = $target
    while ($ancestor -and $ancestor -ne $webRoot) {
        if ((Test-Path -LiteralPath $ancestor) -and
            ((Get-Item -LiteralPath $ancestor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            throw 'Scaffold output must not traverse a symlink or junction.'
        }
        $ancestor = Split-Path $ancestor -Parent
    }
}

$temporaryEnvironment = @{
    ConnectionStrings__Scaffold = $connection
    DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    PGOPTIONS = '-c default_transaction_read_only=on'
}
$previousEnvironment = @{}
Push-Location $webRoot
try {
    foreach ($key in $temporaryEnvironment.Keys) {
        $previousEnvironment[$key] = [Environment]::GetEnvironmentVariable($key)
        [Environment]::SetEnvironmentVariable($key, $temporaryEnvironment[$key])
    }
    # EF/provider diagnostics can contain connection details, so keep raw output out of logs.
    $output = & dotnet tool run dotnet-ef -- dbcontext scaffold 'Name=ConnectionStrings:Scaffold' `
        Npgsql.EntityFrameworkCore.PostgreSQL `
        --project src/ServiceSchoolBot.Infrastructure --startup-project src/ServiceSchoolBot.API `
        --context $context --context-dir Common/Databases --output-dir "Entities/$entityDirectory" `
        --namespace "ServiceSchoolBot.Infrastructure.Entities.$entityDirectory" `
        --context-namespace ServiceSchoolBot.Infrastructure.Common.Databases --no-onconfiguring `
        --force -- --environment Development 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw 'Scaffold failed. Check tool restore, build, database access and metadata permissions; diagnostics suppressed.'
    }
    Write-Output "Scaffold completed: $Database (metadata only; read-only connection)."
}
catch {
    throw 'Scaffold failed. Check tool restore, build, database access and metadata permissions; diagnostics suppressed.'
}
finally {
    foreach ($key in $previousEnvironment.Keys) {
        [Environment]::SetEnvironmentVariable($key, $previousEnvironment[$key])
    }
    $output = $null
    $connection = $null
    Pop-Location
}
