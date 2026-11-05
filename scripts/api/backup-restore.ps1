[CmdletBinding()]
param(
    [ValidateSet('Validate', 'Backup', 'Restore')]
    [string]$Action = 'Validate',
    [string]$BackupFile = (Join-Path $PSScriptRoot '../../.tmp/backups/schoolbot_db.dump')
)
. "$PSScriptRoot/../lib/common.ps1"
$ErrorActionPreference = 'Stop'

# Decisions-029 / DBA-002: SchoolBot DB is strictly separated from the 4 legacy DBs (Master, School, Logs, PGGrade).
# Backup/restore and DDL rollback scripts operate ONLY on SchoolBot DB.
$forbiddenTargets = @('MASTER_DB_CONNECTION_STRING', 'SCHOOL_DB_CONNECTION_STRING', 'LOGS_DB_CONNECTION_STRING', 'PGGRADE_DB_CONNECTION_STRING')

if ($Action -eq 'Validate') {
    Write-Host 'Validating SchoolBot DB backup/restore configuration (Dry-Run)...'
    Write-Host 'Target env var: SCHOOL_BOT_DB_CONNECTION_STRING (4 legacy DBs excluded from DDL/restore writes).'
    Write-Host "Default backup artifact path: $BackupFile"
    Write-Host 'Backup command template: pg_dump --format=custom --no-owner --no-privileges --file="<BackupFile>" "$SCHOOL_BOT_DB_CONNECTION_STRING"'
    Write-Host 'Restore command template: pg_restore --clean --if-exists --no-owner --no-privileges --single-transaction --dbname="$SCHOOL_BOT_DB_CONNECTION_STRING" "<BackupFile>"'
    return
}

$conn = [Environment]::GetEnvironmentVariable('SCHOOL_BOT_DB_CONNECTION_STRING')
if ([string]::IsNullOrWhiteSpace($conn)) {
    throw 'SCHOOL_BOT_DB_CONNECTION_STRING is required for live Backup or Restore.'
}
foreach ($legacyVar in $forbiddenTargets) {
    $legacyVal = [Environment]::GetEnvironmentVariable($legacyVar)
    if (-not [string]::IsNullOrWhiteSpace($legacyVal) -and $legacyVal.Trim() -eq $conn.Trim()) {
        throw "Safety abort: SCHOOL_BOT_DB_CONNECTION_STRING matches legacy database variable $legacyVar."
    }
}

if ($Action -eq 'Backup') {
    $dir = Split-Path $BackupFile -Parent
    if ($dir) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    Invoke-SchoolBotCommand 'pg_dump' @('--format=custom', '--no-owner', '--no-privileges', "--file=$BackupFile", $conn)
    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $BackupFile).Hash.ToLowerInvariant()
    [IO.File]::WriteAllText("$BackupFile.sha256", "$hash  $([IO.Path]::GetFileName($BackupFile))`n")
    Write-Host "SchoolBot DB backup completed: $BackupFile (sha256: $hash)"
}
elseif ($Action -eq 'Restore') {
    if (-not (Test-Path -LiteralPath $BackupFile -PathType Leaf)) {
        throw "Backup file not found: $BackupFile"
    }
    Invoke-SchoolBotCommand 'pg_restore' @('--clean', '--if-exists', '--no-owner', '--no-privileges', '--single-transaction', "--dbname=$conn", $BackupFile)
    Write-Host "SchoolBot DB restore completed from: $BackupFile"
}
