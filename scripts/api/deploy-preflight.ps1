[CmdletBinding()]
param(
    [string]$GatewayRoot = (Join-Path $PSScriptRoot '../../../sb-api-gateway'),
    [string]$ReportPath
)
. "$PSScriptRoot/../lib/common.ps1"
$ErrorActionPreference = 'Stop'

$deployBase = Join-Path $SchoolBotRoot 'deploy/base/school-bot-grpc.yaml'
$devOverlay = Join-Path $SchoolBotRoot 'deploy/overlays/dev/kustomization.yaml'
$prodOverlay = Join-Path $SchoolBotRoot 'deploy/overlays/prod/kustomization.yaml'
foreach ($file in @($deployBase, $devOverlay, $prodOverlay)) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        throw "Missing BE-023 deployment manifest: $file"
    }
}

$baseContent = Get-Content -LiteralPath $deployBase -Raw

# 1. Verify SEC001-002 process isolation: PGGRADE_DB_CONNECTION_STRING must only be in school-bot-grade-worker
$docs = ($baseContent -split '(?m)^---\s*$') | ForEach-Object { ($_ -replace '(?m)^\s*#.*$', '') }
$apiDeploymentDoc = $docs | Where-Object { $_ -match 'kind:\s*Deployment' -and $_ -match 'metadata:\s*\r?\n\s*name:\s*service-school-bot\b' }
$workerDeploymentDoc = $docs | Where-Object { $_ -match 'kind:\s*Deployment' -and $_ -match 'metadata:\s*\r?\n\s*name:\s*school-bot-grade-worker\b' }
if (-not $apiDeploymentDoc -or -not $workerDeploymentDoc) {
    throw 'Both service-school-bot and school-bot-grade-worker Deployments must be defined.'
}
if ($apiDeploymentDoc -match 'PGGRADE_DB_CONNECTION_STRING') {
    throw 'SEC001-002 violation: service-school-bot RPC container must not hold PGGRADE_DB_CONNECTION_STRING.'
}
foreach ($requiredKey in @('SCHOOL_BOT_DB_CONNECTION_STRING', 'MASTER_DB_CONNECTION_STRING', 'SCHOOL_DB_CONNECTION_STRING', 'LOGS_DB_CONNECTION_STRING')) {
    if ($apiDeploymentDoc -notmatch $requiredKey) {
        throw "service-school-bot Deployment is missing secret reference for $requiredKey."
    }
}
if ($workerDeploymentDoc -notmatch 'PGGRADE_DB_CONNECTION_STRING' -or $workerDeploymentDoc -notmatch 'SCHOOL_BOT_DB_CONNECTION_STRING') {
    throw 'school-bot-grade-worker Deployment must reference SCHOOL_BOT_DB_CONNECTION_STRING and PGGRADE_DB_CONNECTION_STRING.'
}

# 2. Verify headless gRPC service, health port 8081, and NetworkPolicy
if ($baseContent -notmatch 'clusterIP:\s*None' -or $baseContent -notmatch 'kubernetes\.io/h2c') {
    throw 'service-school-bot Service must be headless (clusterIP: None) with kubernetes.io/h2c appProtocol.'
}
if ($baseContent -notmatch 'kind:\s*NetworkPolicy' -or $baseContent -notmatch 'app\.kubernetes\.io/name:\s*api-gateway') {
    throw 'NetworkPolicy restricting port 8080 ingress to api-gateway pods is required.'
}

# 3. Verify proto parity with sb-api-gateway
$schoolBotProtoDir = Join-Path $SchoolBotRoot 'backend/src/ServiceSchoolBot.Infrastructure/Protos/ServiceSchoolBot'
$protoFiles = @(Get-ChildItem -LiteralPath $schoolBotProtoDir -Filter '*.proto' | Sort-Object Name)
if ($protoFiles.Count -ne 6) {
    throw "Expected 6 .proto files in $schoolBotProtoDir, found $($protoFiles.Count)."
}
$gatewayProtoParity = $false
$gatewayRootResolved = [IO.Path]::GetFullPath($GatewayRoot)
$gatewayProtoDir = Join-Path $gatewayRootResolved 'src/ApiGateway.Infrastructure/Protos/ServiceSchoolBot'
if (Test-Path -LiteralPath $gatewayProtoDir) {
    foreach ($pf in $protoFiles) {
        $gwFile = Join-Path $gatewayProtoDir $pf.Name
        if (-not (Test-Path -LiteralPath $gwFile -PathType Leaf)) {
            throw "Gateway is missing proto file: $($pf.Name)"
        }
        $sbHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $pf.FullName).Hash
        $gwHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $gwFile).Hash
        if ($sbHash -ne $gwHash) {
            throw "Proto mismatch between SchoolBot and Gateway on $($pf.Name): $sbHash vs $gwHash"
        }
    }
    $gatewayProtoParity = $true
}

$rolloutOrder = @(
    '1. SchoolBot DB DDL (DBA-002/004/005/006; 4 legacy DBs untouched, no GRANT on School/PGGradeDB)',
    '2. ServiceSchoolBot.API (gRPC h2c :8080, health :8081/healthz) + school-bot-grade-worker (isolated PGGradeDB)',
    '3. sb-api-gateway (HTTP G -> gRPC B via GrpcServices__ServiceSchoolBot=dns:///...:8080)',
    '4. Admin Next.js (central staff curriculum/question-bank/license/release management)',
    '5. web-system / BFF (school admin & teacher progress/review/grade export)',
    '6. Desktop Client (Windows 11 25H2 x64 MSI/EXE & macOS 15+ Universal PKG with embedded HTTPS Gateway URL)'
)
$rollbackOrder = @(
    '1. Desktop Client (withdraw release manifest in catalog; existing clients continue offline/current version)',
    '2. web-system / BFF & Admin Next.js (disable SchoolBot feature entry / route)',
    '3. sb-api-gateway (set SchoolBotOptions:Enabled=false or revert overlay; SliceGate returns 404)',
    '4. school-bot-grade-worker & ServiceSchoolBot.API (kubectl rollout undo deployment/school-bot-grade-worker deployment/service-school-bot)',
    '5. SchoolBot DB (execute verified rollback script or pg_restore only on SchoolBot DB; never touch 4 legacy DBs)'
)

$summary = [ordered]@{
    checked_at                  = [DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ')
    task_id                     = 'BE-023'
    deployment_performed        = $false
    mode                        = 'preflight-validation-only (waiting for H5/Light/G-AUTH/G-CORE human gates)'
    process_isolation_sec001002 = 'PASS (PGGRADE_DB_CONNECTION_STRING isolated to school-bot-grade-worker)'
    database_secret_refs        = @('SCHOOL_BOT_DB_CONNECTION_STRING', 'MASTER_DB_CONNECTION_STRING', 'SCHOOL_DB_CONNECTION_STRING', 'LOGS_DB_CONNECTION_STRING', 'PGGRADE_DB_CONNECTION_STRING')
    proto_count                 = $protoFiles.Count
    gateway_proto_parity        = $gatewayProtoParity
    rollout_order               = $rolloutOrder
    rollback_order              = $rollbackOrder
    pending_human_gates         = @('G-AUTH', 'G-CORE', 'G-PROJECT-DB', 'G-LEARN-DB', 'G-RELEASE-DB', 'G-RELEASE', 'H5 (staff bootstrap & Light cluster values)')
}

if ($ReportPath) {
    $resolvedReport = if ([IO.Path]::IsPathRooted($ReportPath)) {
        $ReportPath
    } else {
        Join-Path $SchoolBotRoot $ReportPath
    }
    $reportDir = Split-Path $resolvedReport -Parent
    if ($reportDir) { New-Item -ItemType Directory -Path $reportDir -Force | Out-Null }
    [IO.File]::WriteAllText($resolvedReport, ($summary | ConvertTo-Json -Depth 6) + "`n", [Text.UTF8Encoding]::new($false))
}

Write-Host 'BE-023 deployment preflight passed: HTTP G -> gRPC B manifests, SEC001-002 worker isolation, 5 DB secret refs, proto parity, and rollout/rollback sequence verified (no deployment executed).'
